using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EliteAI : MonoBehaviour
{
    private static readonly List<EliteAI> allElites = new List<EliteAI>();
    public static IReadOnlyList<EliteAI> AllElites => allElites;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => allElites.Clear();

    private enum EliteState { Wait, Alert, Fight, Search, Retreat }
    private enum SearchPhase { GoToLastKnown, GoToPoint, WaitAtPoint, ReturnHome }

    [Header("State")]
    [SerializeField] private EliteState startingState = EliteState.Wait;
    private EliteState currentState;

    [Header("Perception")]
    [SerializeField] private float sightRange = 30f;
    [SerializeField] private float visionCone = 150f;
    [SerializeField] private float eyeHeight = 1.7f;
    [SerializeField] private float targetHeight = 1f;
    [SerializeField] private LayerMask lineOfSightMask = ~0;
    [SerializeField] private float memoryDuration = 12f;

    [Header("Squad Command")]
    [SerializeField] private float commandRadius = 50f;
    [SerializeField] private float squadAlertRadius = 45f;

    [Header("Combat")]
    [SerializeField] private float alertDuration = 1f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float combatRepathInterval = 0.3f;
    [Tooltip("Preferred range when commanded Grunts are alive.")]
    [SerializeField] private float supportCombatRange = 18f;
    [Tooltip("Preferred range when fighting without Grunts.")]
    [SerializeField] private float soloCombatRange = 10f;
    [SerializeField] private float minCombatRange = 7f;
    [SerializeField] private float maxCombatRange = 22f;

    [Header("Retreat")]
    [SerializeField] private float retreatDistance = 25f;
    [SerializeField] private float retreatRepathInterval = 1f;
    [Tooltip("How often the Elite pauses to fire back while retreating.")]
    [SerializeField] private float retreatFireInterval = 1.5f;
    [Tooltip("How long each return-fire burst lasts.")]
    [SerializeField] private float retreatFireDuration = 0.6f;
    [SerializeField] private int retreatCandidateCount = 6;
    [SerializeField] private float minPlayerPassDistance = 4f;
    [Tooltip("Shield fraction (0-1) the Elite must recover before it re-engages.")]
    [SerializeField] private float shieldRecoveryThreshold = 0.25f;

    [Header("Search")]
    [SerializeField] private float searchRadius = 12f;
    [SerializeField] private float searchWaitTime = 3f;
    [SerializeField] private int searchPointCount = 3;
    [SerializeField] private float searchScanSpeed = 45f;

    private NavMeshAgent agent;
    private EnemyHealth enemyHealth;
    private EliteCombat eliteCombat;
    private Transform player;

    private Vector3 homePosition;
    private Vector3 lastKnownPlayerPosition;
    private float baseSpeed;
    private bool deathHandled;

    private float lastSeenTime;
    private float alertTimer;
    private float combatTimer;

    
    private int commandedGruntCount;
    private float commandCountTimer;

    
    private float retreatTimer;
    private float retreatFireTimer;
    private float retreatFireUntil;

    
    private SearchPhase searchPhase;
    private int searchPointsVisited;
    private float searchTimer;

    public bool IsDead => enemyHealth == null || enemyHealth.IsDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<EnemyHealth>();
        eliteCombat = GetComponent<EliteCombat>();

        if (agent != null)
            baseSpeed = agent.speed;
    }

    private void OnEnable()
    {
        if (!allElites.Contains(this))
            allElites.Add(this);
    }

    private void OnDisable()
    {
        allElites.Remove(this);
    }

    private void Start()
    {
        if (agent == null || enemyHealth == null || eliteCombat == null)
        {
            Debug.LogError(gameObject.name + " is missing NavMeshAgent, EnemyHealth or EliteCombat.", this);
            enabled = false;
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("EliteAI could not find the Player.", this);
            enabled = false;
            return;
        }

        player = playerObject.transform;
        homePosition = transform.position;

        currentState = startingState;
        EnterState(currentState);
    }

    private void Update()
    {
        if (player == null) return;

        if (IsDead)
        {
            HandleDeath();
            return;
        }

        RefreshCommandCount();
        UpdateShieldState();

        switch (currentState)
        {
            case EliteState.Wait: Wait(); break;
            case EliteState.Alert: Alert(); break;
            case EliteState.Fight: Fight(); break;
            case EliteState.Search: Search(); break;
            case EliteState.Retreat: Retreat(); break;
        }
    }

    private void HandleDeath()
    {
        if (deathHandled) return;
        deathHandled = true;

        eliteCombat.StopFiring();
        if (agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;

        
        IReadOnlyList<GruntAI> grunts = GruntAI.AllGrunts;
        for (int i = 0; i < grunts.Count; i++)
        {
            if (grunts[i] != null)
                grunts[i].RefreshCommander();
        }

        enabled = false;
    }

    private void UpdateShieldState()
    {
        if (currentState == EliteState.Retreat)
        {
            if (enemyHealth.ShieldPercentage >= shieldRecoveryThreshold)
            {
                lastSeenTime = Time.time; 
                SetState(EliteState.Fight);
            }
        }
        else if (enemyHealth.CurrentShield <= 0f)
        {
            SetState(EliteState.Retreat);
        }
    }

   

    private void SetState(EliteState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log(gameObject.name + " Elite State: " + newState);
        EnterState(newState);
    }

    private void EnterState(EliteState state)
    {
        agent.speed = baseSpeed;

        
        agent.updateRotation =
            state != EliteState.Alert &&
            state != EliteState.Fight &&
            state != EliteState.Retreat;

        eliteCombat.StopFiring();

        switch (state)
        {
            case EliteState.Wait:
                agent.isStopped = true;
                break;

            case EliteState.Alert:
                agent.isStopped = true;
                alertTimer = alertDuration;
                break;

            case EliteState.Fight:
                combatTimer = 0f;
                agent.isStopped = false;
                break;

            case EliteState.Search:
                searchPhase = SearchPhase.GoToLastKnown;
                searchPointsVisited = 0;
                searchTimer = 0f;
                agent.isStopped = false;
                agent.SetDestination(lastKnownPlayerPosition);
                break;

            case EliteState.Retreat:
                
                RememberPlayer();
                retreatTimer = 0f;
                retreatFireTimer = 0f;
                retreatFireUntil = 0f;
                agent.isStopped = false;
                break;
        }
    }

    

    private void Wait()
    {
        if (!CanSeePlayer(false))
            return;

        RememberPlayer();
        SetState(EliteState.Alert);
        BroadcastPlayerLocation();
    }

    private void Alert()
    {
        if (CanSeePlayer(true))
            RememberPlayer();

        FaceTowards(lastKnownPlayerPosition, turnSpeed);

        alertTimer -= Time.deltaTime;
        if (alertTimer <= 0f)
            SetState(EliteState.Fight);
    }

    

    private void Fight()
    {
        combatTimer -= Time.deltaTime;

        if (CanSeePlayer(true))
        {
            RememberPlayer();

            FaceTowards(player.position, turnSpeed);

            eliteCombat.SetTarget(player);
            eliteCombat.StartFiring();

            UpdateCombatPosition();
        }
        else
        {
            eliteCombat.StopFiring();
            ChaseLastKnownPosition();

            if (Time.time - lastSeenTime >= memoryDuration)
                SetState(EliteState.Search);
        }
    }

    private void UpdateCombatPosition()
    {
        bool hasSupport = commandedGruntCount > 0;
        float preferredRange = hasSupport ? supportCombatRange : soloCombatRange;

        Vector3 fromPlayer = transform.position - player.position;
        fromPlayer.y = 0f;
        float distance = fromPlayer.magnitude;

       
        bool inGoodRange = hasSupport
            ? Mathf.Abs(distance - preferredRange) <= 3f
            : distance >= minCombatRange && distance <= maxCombatRange;

        if (inGoodRange)
        {
            agent.isStopped = true;
            return;
        }

        if (combatTimer > 0f) return;
        combatTimer = combatRepathInterval;

        Vector3 direction = distance > 0.01f ? fromPlayer / distance : transform.forward;
        MoveTo(player.position + direction * preferredRange);
    }

    private void ChaseLastKnownPosition()
    {
        Vector3 toTarget = lastKnownPlayerPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > agent.stoppingDistance + 0.5f)
        {
            if (combatTimer <= 0f)
            {
                combatTimer = combatRepathInterval;
                MoveTo(lastKnownPlayerPosition);
            }
            FaceTowards(agent.steeringTarget, turnSpeed);
        }
        else
        {
            agent.isStopped = true;
            FaceTowards(lastKnownPlayerPosition, turnSpeed);
        }
    }

    

    private void Retreat()
    {
        retreatTimer -= Time.deltaTime;
        retreatFireTimer -= Time.deltaTime;

        
        FaceTowards(player.position, turnSpeed);

        
        if (retreatFireTimer <= 0f)
        {
            retreatFireTimer = retreatFireInterval;
            retreatFireUntil = Time.time + retreatFireDuration;
        }

        if (Time.time < retreatFireUntil && CanSeePlayer(true))
        {
            eliteCombat.SetTarget(player);
            eliteCombat.StartFiring();
        }
        else
        {
            eliteCombat.StopFiring();
        }

        if (retreatTimer > 0f) return;
        retreatTimer = retreatRepathInterval;

        if (AINavigation.TryPickPointAwayFrom(
                transform.position, player.position, retreatDistance,
                retreatCandidateCount, minPlayerPassDistance, out Vector3 destination))
        {
            agent.isStopped = false;
            agent.SetDestination(destination);
        }
        else
        {
           
            agent.isStopped = true;
            retreatFireUntil = Time.time + retreatRepathInterval;
        }
    }

    

    private void Search()
    {
        if (CanSeePlayer(true))
        {
            RememberPlayer();
            SetState(EliteState.Fight);
            return;
        }

        switch (searchPhase)
        {
            case SearchPhase.GoToLastKnown:
            case SearchPhase.GoToPoint:
                if (AINavigation.HasArrived(agent))
                {
                    agent.isStopped = true;
                    searchTimer = searchWaitTime;
                    searchPhase = SearchPhase.WaitAtPoint;
                }
                break;

            case SearchPhase.WaitAtPoint:
                transform.Rotate(0f, searchScanSpeed * Time.deltaTime, 0f); 
                searchTimer -= Time.deltaTime;
                if (searchTimer > 0f) break;

                if (searchPointsVisited >= searchPointCount)
                {
                    searchPhase = SearchPhase.ReturnHome;
                    agent.isStopped = false;
                    agent.SetDestination(homePosition);
                }
                else
                {
                    searchPointsVisited++;
                    if (TryChooseSearchPoint(out Vector3 point))
                    {
                        searchPhase = SearchPhase.GoToPoint;
                        agent.isStopped = false;
                        agent.SetDestination(point);
                    }
                    else
                    {
                        searchTimer = searchWaitTime;
                    }
                }
                break;

            case SearchPhase.ReturnHome:
                if (AINavigation.HasArrived(agent))
                    SetState(EliteState.Wait);
                break;
        }
    }

    private bool TryChooseSearchPoint(out Vector3 result)
    {
        Vector2 offset = Random.insideUnitCircle * searchRadius;
        Vector3 candidate = lastKnownPlayerPosition + new Vector3(offset.x, 0f, offset.y);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }

    

    private void RefreshCommandCount()
    {
        commandCountTimer -= Time.deltaTime;
        if (commandCountTimer > 0f) return;
        commandCountTimer = 0.5f;

        int count = 0;
        IReadOnlyList<GruntAI> grunts = GruntAI.AllGrunts;

        for (int i = 0; i < grunts.Count; i++)
        {
            GruntAI grunt = grunts[i];
            if (grunt == null || grunt.IsDead) continue;
            if (grunt.CommandingElite != this) continue;

            if (Vector3.Distance(transform.position, grunt.transform.position) <= commandRadius)
                count++;
        }

        commandedGruntCount = count;
    }

    private void BroadcastPlayerLocation()
    {
        
        IReadOnlyList<GruntAI> grunts = GruntAI.AllGrunts;
        for (int i = 0; i < grunts.Count; i++)
        {
            GruntAI grunt = grunts[i];
            if (grunt == null || grunt.IsDead) continue;
            if (grunt.CommandingElite != this) continue;

            grunt.ReceiveAlert(lastKnownPlayerPosition);
        }

        
        for (int i = 0; i < allElites.Count; i++)
        {
            EliteAI other = allElites[i];
            if (other == null || other == this || other.IsDead) continue;

            if (Vector3.Distance(transform.position, other.transform.position) <= squadAlertRadius)
                other.ReceiveAlert(lastKnownPlayerPosition);
        }
    }

    public void ReceiveAlert(Vector3 playerPosition)
    {
        if (IsDead) return;

        lastKnownPlayerPosition = playerPosition;
        lastSeenTime = Time.time;

        if (currentState == EliteState.Wait ||
            currentState == EliteState.Search)
        {
            SetState(EliteState.Alert);
            BroadcastPlayerLocation();
        }
    }


    private bool CanSeePlayer(bool alreadyEngaged)
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * targetHeight;
        Vector3 toTarget = target - eye;

        if (toTarget.sqrMagnitude > sightRange * sightRange)
            return false;

        if (!alreadyEngaged)
        {
            Vector3 flat = player.position - transform.position;
            flat.y = 0f;

            if (flat.sqrMagnitude > 0.001f &&
                Vector3.Angle(transform.forward, flat) > visionCone * 0.5f)
                return false;
        }

        if (Physics.Linecast(eye, target, out RaycastHit hit, lineOfSightMask,
                QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.root != player.root &&
                hit.transform.root != transform.root)
                return false;
        }

        return true;
    }

    private void RememberPlayer()
    {
        lastKnownPlayerPosition = player.position;
        lastSeenTime = Time.time;
    }

    

    private void MoveTo(Vector3 destination)
    {
        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            return;

        agent.isStopped = false;
        agent.SetDestination(hit.position);
    }

    private void FaceTowards(Vector3 worldPosition, float degreesPerSecond)
    {
        AINavigation.FaceTowards(transform, worldPosition, degreesPerSecond);
    }
}