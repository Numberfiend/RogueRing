using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GruntAI : MonoBehaviour
{
    
    private static readonly List<GruntAI> allGrunts = new List<GruntAI>();
    public static IReadOnlyList<GruntAI> AllGrunts => allGrunts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => allGrunts.Clear();

    [Header("Suicide Charge")]
    [SerializeField] private GrenadeData plasmaGrenadeData;
    [SerializeField] private Transform leftHandGrenadePoint;
    [SerializeField] private Transform rightHandGrenadePoint;
    [SerializeField] private float chargeSpeedMultiplier = 1.6f;
    [SerializeField] private float chargeRepathInterval = 0.2f;
    private bool suicideGrenadesSpawned;
    private float repathTimer;

    [Header("Elite Command")]
    [SerializeField] private float commandSearchRadius = 50f;
    [Tooltip("Lower is more responsive when a leader dies (it also gets notified directly on death).")]
    [SerializeField] private float commandSearchInterval = 0.5f;
    private EliteAI commandingElite;
    private float commandSearchTimer;
    public EliteAI CommandingElite => commandingElite;

    [Header("State")]
    [SerializeField] private AiActions startingState = AiActions.Wait;
    private AiActions currentState;
    public AiActions CurrentState => currentState;

    [Header("Perception")]
    [FormerlySerializedAs("searchRaidus")]
    [SerializeField] private float sightRange = 20f;
    [SerializeField] private float visionCone = 120f;
    [SerializeField] private float eyeHeight = 1.4f;
    [SerializeField] private float targetHeight = 1.0f;
    [Tooltip("Layers that block line of sight. Exclude the enemy/player layers if you can.")]
    [SerializeField] private LayerMask lineOfSightMask = ~0;
    [SerializeField] private float memoryDuration = 5f;
    [SerializeField] private float wakeRadius = 15f;

    [Header("Alert / Combat")]
    [SerializeField] private float alertDuration = 1f;
    [SerializeField] private float alertTurnSpeed = 120f;
    [SerializeField] private float combatTurnSpeed = 150f;
    [Tooltip("Closer than this and the grunt backs off.")]
    [SerializeField] private float minCombatRange = 6f;
    [Tooltip("Further than this and the grunt advances. Keep below Sight Range.")]
    [SerializeField] private float maxCombatRange = 14f;
    [Tooltip("Distance the grunt tries to settle at when no firing position is available.")]
    [SerializeField] private float preferredCombatRange = 10f;
    [SerializeField] private float combatRepathInterval = 0.3f;

    [Header("Firing Positions")]
    [Tooltip("How long a grunt holds a firing position before hopping to another.")]
    [SerializeField] private float repositionIntervalMin = 3f;
    [SerializeField] private float repositionIntervalMax = 6f;
    [SerializeField] private int firingPositionCandidates = 8;
    [Tooltip("Minimum distance between two grunts' claimed firing spots.")]
    [SerializeField] private float firingPositionSeparation = 3f;

    [Header("Search")]
    [SerializeField] private float searchAreaRadius = 10f;
    [SerializeField] private int searchPointCount = 3;
    [SerializeField] private float searchPointWaitTime = 2f;
    [SerializeField] private float searchScanSpeed = 45f;

    [Header("Flee")]
    [SerializeField] private float fleeAreaRadius = 20f;
    [Tooltip("Minimum time spent fleeing before a returning leader can calm the grunt down.")]
    [SerializeField] private float fleeTime = 5f;
    [SerializeField] private float fleeDestinationTime = 2f;
    [Tooltip("Chance (0-1), rolled each time the grunt picks a new flee destination, that it snaps and suicide charges instead.")]
    [Range(0f, 1f)]
    [SerializeField] private float suicideChance = 0.1f;
    [SerializeField] private int fleeCandidateCount = 6;
    [Tooltip("Reject flee/firing routes that pass closer than this to the player.")]
    [SerializeField] private float minPlayerPassDistance = 4f;

    private enum SearchPhase { GoToLastKnown, GoToPoint, WaitAtPoint, ReturnHome }

    private GruntCombat gruntCombat;
    private NavMeshAgent agent;
    private EnemyHealth enemyHealth;
    private Transform player;
    private Vector3 homePosition;
    private float baseSpeed;
    private bool deathHandled;

    public bool IsDead => enemyHealth != null && enemyHealth.IsDead;

    
    private Vector3 lastKnownPlayerPosition;
    private float lastSeenTime;

    
    private float alertTimer;
    private float fightRepathTimer;
    private bool hasFiringPosition;
    private float repositionTimer;

    
    private bool hadLeader;
    private bool eliteNearby;

    
    private SearchPhase searchPhase;
    private int currentSearchPoint;
    private float searchPointTimer;

    
    private float fleeTimer;
    private float fleeDestinationTimer;

    private void Awake()
    {
        gruntCombat = GetComponent<GruntCombat>();
        agent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<EnemyHealth>();
        baseSpeed = agent.speed;
    }

    private void OnEnable() => allGrunts.Add(this);

    private void OnDisable()
    {
        allGrunts.Remove(this);
        FiringPositions.Release(this);
    }

    private void Start()
    {
        homePosition = transform.position;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("GruntAI: no GameObject tagged 'Player' found.", this);
            enabled = false;
            return;
        }
        player = playerObject.transform;

        currentState = startingState;
        EnterState(currentState);
    }

    private void Update()
    {
        if (IsDead)
        {
            HandleDeath();
            return;
        }

        UpdateCommanderTracking();

        
        if (hadLeader && !eliteNearby && IsInCombatState())
        {
            SetState(AiActions.Flee);
            return;
        }

        switch (currentState)
        {
            case AiActions.Sleep: break; 
            case AiActions.Wait: Wait(); break;
            case AiActions.Alert: Alert(); break;
            case AiActions.Fight: Fight(); break;
            case AiActions.Search: Search(); break;
            case AiActions.Flee: Flee(); break;
            case AiActions.Charge: Charge(); break;
        }
    }

    private void HandleDeath()
    {
        if (deathHandled) return;
        deathHandled = true;

        gruntCombat.StopFiring();
        if (agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;

        enabled = false; 
    }

    

    private void SetState(AiActions newState)
    {
        if (currentState == newState) return;
        Debug.Log($"{gameObject.name} changed from {currentState} to {newState}");
        currentState = newState;
        EnterState(newState);
    }

    private void EnterState(AiActions state)
    {
        agent.speed = baseSpeed;
        
        agent.updateRotation = state != AiActions.Fight;

        if (state != AiActions.Fight)
        {
            FiringPositions.Release(this);
            hasFiringPosition = false;
        }

        if (state != AiActions.Fight && state != AiActions.Charge)
            gruntCombat.StopFiring();

        switch (state)
        {
            case AiActions.Sleep:
            case AiActions.Wait:
                agent.isStopped = true;
                break;

            case AiActions.Alert:
                agent.isStopped = true;
                alertTimer = alertDuration;
                AlertNearbyAllies();
                break;

            case AiActions.Fight:
                fightRepathTimer = 0f;
                hasFiringPosition = false;
                repositionTimer = 0f;
                break;

            case AiActions.Search:
                currentSearchPoint = 0;
                searchPhase = SearchPhase.GoToLastKnown;
                agent.isStopped = false;
                agent.SetDestination(lastKnownPlayerPosition);
                break;

            case AiActions.Flee:
                fleeTimer = 0f;
                fleeDestinationTimer = 0f;
                agent.isStopped = false;
                break;

            case AiActions.Charge:
                agent.isStopped = false;
                agent.speed = baseSpeed * chargeSpeedMultiplier;
                repathTimer = 0f;
                gruntCombat.StopFiring();
                gruntCombat.HideWeaponModel();
                if (!suicideGrenadesSpawned)
                {
                    SpawnSuicideGrenades();
                    suicideGrenadesSpawned = true;
                }
                break;
        }
    }

    private bool IsInCombatState()
    {
        return currentState == AiActions.Alert ||
               currentState == AiActions.Fight ||
               currentState == AiActions.Search;
    }

  

    private void Wait()
    {
        if (CanSeePlayer(false))
        {
            RememberPlayer();
            SetState(AiActions.Alert);
        }
    }

    private void Alert()
    {
        if (CanSeePlayer(true))
            RememberPlayer();

        FaceTowards(lastKnownPlayerPosition, alertTurnSpeed);

        alertTimer -= Time.deltaTime;
        if (alertTimer <= 0f)
            SetState(AiActions.Fight);
    }

    private void Fight()
    {
        fightRepathTimer -= Time.deltaTime;

        if (CanSeePlayer(true))
        {
            RememberPlayer();
            FaceTowards(player.position, combatTurnSpeed);
            gruntCombat.SetTarget(player);
            gruntCombat.StartFiring();
            UpdateCombatPosition();
        }
        else
        {
            gruntCombat.StopFiring();
            FiringPositions.Release(this);
            hasFiringPosition = false;
            ChaseLastKnownPosition();

            if (Time.time - lastSeenTime >= memoryDuration)
                SetState(AiActions.Search);
        }
    }

    
    private void UpdateCombatPosition()
    {
        repositionTimer -= Time.deltaTime;

        Vector3 fromPlayer = transform.position - player.position;
        fromPlayer.y = 0f;
        float distance = fromPlayer.magnitude;
        bool outOfBand = distance < minCombatRange || distance > maxCombatRange;

        bool wantsNewPosition = !hasFiringPosition || outOfBand || repositionTimer <= 0f;

        if (wantsNewPosition && fightRepathTimer <= 0f)
        {
            fightRepathTimer = combatRepathInterval;

            if (!TryPickFiringPosition())
            {
                
                hasFiringPosition = false;
                fightRepathTimer = 0f;
                FallbackCombatPosition();
                return;
            }
        }

        if (hasFiringPosition && HasArrived())
            agent.isStopped = true; 
    }

    private bool TryPickFiringPosition()
    {
        FiringPositions.Query query = new FiringPositions.Query
        {
            Requester = transform.position,
            Target = player,
            TargetHeight = targetHeight,
            EyeHeight = eyeHeight,
            MinRange = minCombatRange,
            MaxRange = maxCombatRange,
            Candidates = firingPositionCandidates,
            Separation = firingPositionSeparation,
            MinPassDistance = minPlayerPassDistance,
            LineOfSightMask = lineOfSightMask
        };

        if (!FiringPositions.TryClaim(this, query, out Vector3 position))
            return false;

        hasFiringPosition = true;
        repositionTimer = Random.Range(repositionIntervalMin, repositionIntervalMax);
        agent.isStopped = false;
        agent.SetDestination(position);
        return true;
    }

    
    private void FallbackCombatPosition()
    {
        Vector3 fromPlayer = transform.position - player.position;
        fromPlayer.y = 0f;
        float distance = fromPlayer.magnitude;

        if (distance >= minCombatRange && distance <= maxCombatRange)
        {
            agent.isStopped = true;
            return;
        }

        if (fightRepathTimer > 0f) return;
        fightRepathTimer = combatRepathInterval;

        Vector3 dir = distance > 0.01f ? fromPlayer / distance : transform.forward;
        MoveTo(player.position + dir * preferredCombatRange);
    }

    
    private void ChaseLastKnownPosition()
    {
        Vector3 toTarget = lastKnownPlayerPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > agent.stoppingDistance + 0.5f)
        {
            if (fightRepathTimer <= 0f)
            {
                fightRepathTimer = combatRepathInterval;
                MoveTo(lastKnownPlayerPosition);
            }
            FaceTowards(agent.steeringTarget, combatTurnSpeed);
        }
        else
        {
            agent.isStopped = true;
            FaceTowards(lastKnownPlayerPosition, combatTurnSpeed);
        }
    }

    private void MoveTo(Vector3 desired)
    {
        if (NavMesh.SamplePosition(desired, out NavMeshHit hit, 4f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    private void Search()
    {
        if (CanSeePlayer(true))
        {
            RememberPlayer();
            SetState(AiActions.Fight);
            return;
        }

        switch (searchPhase)
        {
            case SearchPhase.GoToLastKnown:
            case SearchPhase.GoToPoint:
                if (HasArrived())
                {
                    agent.isStopped = true;
                    searchPointTimer = searchPointWaitTime;
                    searchPhase = SearchPhase.WaitAtPoint;
                }
                break;

            case SearchPhase.WaitAtPoint:
                transform.Rotate(0f, searchScanSpeed * Time.deltaTime, 0f); 
                searchPointTimer -= Time.deltaTime;
                if (searchPointTimer > 0f) break;

                if (currentSearchPoint >= searchPointCount)
                {
                    searchPhase = SearchPhase.ReturnHome;
                    agent.isStopped = false;
                    agent.SetDestination(homePosition);
                }
                else
                {
                    currentSearchPoint++;
                    if (TryChooseSearchPoint(out Vector3 point))
                    {
                        searchPhase = SearchPhase.GoToPoint;
                        agent.isStopped = false;
                        agent.SetDestination(point);
                    }
                    else
                    {
                        searchPointTimer = searchPointWaitTime; 
                    }
                }
                break;

            case SearchPhase.ReturnHome:
                if (HasArrived())
                    SetState(startingState == AiActions.Sleep ? AiActions.Sleep : AiActions.Wait);
                break;
        }
    }

    private void Flee()
    {
        fleeTimer += Time.deltaTime;

        
        if (eliteNearby && fleeTimer >= fleeTime)
        {
            lastSeenTime = Time.time;
            SetState(AiActions.Fight);
            return;
        }

        fleeDestinationTimer -= Time.deltaTime;
        if (fleeDestinationTimer <= 0f)
        {
            fleeDestinationTimer = fleeDestinationTime;

            
            if (Random.value < suicideChance)
            {
                SetState(AiActions.Charge);
                return;
            }

            if (AINavigation.TryPickPointAwayFrom(
                    transform.position, player.position, fleeAreaRadius,
                    fleeCandidateCount, minPlayerPassDistance, out Vector3 destination))
            {
                agent.isStopped = false;
                agent.SetDestination(destination);
            }
            
        }
    }

    private void Charge()
    {
        
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = chargeRepathInterval;
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        FaceTowards(player.position, 180f);
    }

    

    private bool CanSeePlayer(bool alreadyEngaged)
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * targetHeight;
        Vector3 toTarget = target - eye;

        if (toTarget.sqrMagnitude > sightRange * sightRange) return false;

       
        if (!alreadyEngaged)
        {
            Vector3 flat = player.position - transform.position;
            flat.y = 0f;
            if (flat != Vector3.zero &&
                Vector3.Angle(transform.forward, flat) > visionCone * 0.5f)
                return false;
        }

        if (Physics.Linecast(eye, target, out RaycastHit hit, lineOfSightMask,
                QueryTriggerInteraction.Ignore))
        {
            Transform hitRoot = hit.transform.root;
            if (hitRoot != player.root && hitRoot != transform.root)
                return false;
        }
        return true;
    }

    private void RememberPlayer()
    {
        lastKnownPlayerPosition = player.position;
        lastSeenTime = Time.time;
    }


    private void AlertNearbyAllies()
    {
        for (int i = 0; i < allGrunts.Count; i++)
        {
            GruntAI other = allGrunts[i];
            if (other == this || other.IsDead) continue;
            if (other.currentState != AiActions.Sleep && other.currentState != AiActions.Wait) continue;
            if ((other.transform.position - transform.position).sqrMagnitude > wakeRadius * wakeRadius) continue;

            other.ReceiveAlert(lastKnownPlayerPosition);
        }

        
        if (commandingElite != null && !commandingElite.IsDead)
            commandingElite.ReceiveAlert(lastKnownPlayerPosition);
    }

    public void ReceiveAlert(Vector3 playerPosition)
    {
        if (IsDead) return;

        lastKnownPlayerPosition = playerPosition;
        lastSeenTime = Time.time;

        if (currentState == AiActions.Sleep ||
            currentState == AiActions.Wait ||
            currentState == AiActions.Search)
        {
            SetState(AiActions.Alert);
        }
    }

    public void RefreshCommander()
    {
        commandSearchTimer = 0f;
    }

    private void UpdateCommanderTracking()
    {
        commandSearchTimer -= Time.deltaTime;
        if (commandSearchTimer > 0f) return;
        commandSearchTimer = commandSearchInterval;

        
        if (commandingElite != null &&
            !commandingElite.IsDead &&
            Vector3.Distance(transform.position, commandingElite.transform.position) <= commandSearchRadius)
        {
            eliteNearby = true;
            hadLeader = true;
            return;
        }

        commandingElite = null;
        eliteNearby = false;

        float closestDistance = commandSearchRadius;
        IReadOnlyList<EliteAI> elites = EliteAI.AllElites;

        for (int i = 0; i < elites.Count; i++)
        {
            EliteAI elite = elites[i];
            if (elite == null || elite.IsDead) continue;

            float distance = Vector3.Distance(transform.position, elite.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                commandingElite = elite;
            }
        }

        if (commandingElite != null)
        {
            eliteNearby = true;
            hadLeader = true;
        }
    }

  

    private bool HasArrived() => AINavigation.HasArrived(agent);

    private void FaceTowards(Vector3 worldPoint, float degreesPerSecond)
    {
        AINavigation.FaceTowards(transform, worldPoint, degreesPerSecond);
    }

    private bool TryChooseSearchPoint(out Vector3 result)
    {
        Vector2 offset = Random.insideUnitCircle * searchAreaRadius;
        Vector3 candidate = lastKnownPlayerPosition + new Vector3(offset.x, 0f, offset.y);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, searchAreaRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }

    

    private void SpawnSuicideGrenades()
    {
        if (plasmaGrenadeData == null)
        {
            Debug.LogError(gameObject.name + " has no Plasma Grenade Data assigned.", this);
            return;
        }

        SpawnGrenadeAtHand(leftHandGrenadePoint);
        SpawnGrenadeAtHand(rightHandGrenadePoint);
    }

    private void SpawnGrenadeAtHand(Transform handPoint)
    {
        if (handPoint == null)
        {
            Debug.LogError(gameObject.name + " has a missing grenade hand point.", this);
            return;
        }

        if (plasmaGrenadeData.worldPrefab == null)
        {
            Debug.LogError("Plasma Grenade Data has no World Prefab assigned.", this);
            return;
        }

        GameObject grenadeObject = Instantiate(plasmaGrenadeData.worldPrefab, handPoint);
        grenadeObject.transform.localPosition = Vector3.zero;
        grenadeObject.transform.localRotation = Quaternion.identity;

        Grenade grenade = grenadeObject.GetComponent<Grenade>();
        if (grenade == null)
        {
            Debug.LogError("Suicide grenade prefab has no Grenade component.", grenadeObject);
            Destroy(grenadeObject);
            return;
        }

        grenade.Initialise(plasmaGrenadeData);
    }
}