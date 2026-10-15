using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class GruntAI : MonoBehaviour
{
    [Header("Suicide Charge")]
    [SerializeField] private GrenadeData plasmaGrenadeData;
    [SerializeField] private Transform leftHandGrenadePoint;
    [SerializeField] private Transform rightHandGrenadePoint;
    private bool suicideGrenadesSpawned;

    private GruntCombat gruntCombat;
    [SerializeField] private AiActions startingState = AiActions.Wait;
    private AiActions currentState;
    public AiActions CurrentState => currentState;
    private Vector3 homePosition;

    private NavMeshAgent agent;

    [SerializeField] private float searchRaidus = 20f;
    [SerializeField] private float visionCone = 120f;
    [SerializeField] private float memoryDuration = 5f;
    [SerializeField] private float wakeRadius = 15f;
    [SerializeField] private float eliteRetreatDistance = 50f;
    private Vector3 lastKnownPlayerPosition;
    private float lastSeenTime;
    [SerializeField] private float searchTime = 5f;
    private float searchTimer;
    private bool returningHome;
    private Transform player;
    private float searchStartRotation;
    
    [SerializeField] private float searchAreaRadius = 10f;
    private Vector3 searchDestination;
    private bool hasSearchDestination;
    [SerializeField] private float searchPointWaitTime = 2f;

    private float searchPointTimer;
    private bool suicideCharge;
    [SerializeField] private int searchPointCount = 3;

    private int currentSearchPoint;

    [SerializeField] private float fleeAreaRadius = 20f;
    [SerializeField] private float fleeTime = 5f;
    [SerializeField] private float fleeDestinationTime = 2f;
    
    [SerializeField] private float fleeDecisionTime = 20f;
    private float fleeDestinationTimer;
    private float fleeDecisionTimer;
    private float fleeTimer;
    private void Start()
    {
        homePosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        gruntCombat = GetComponent<GruntCombat>();
        agent = GetComponent<NavMeshAgent>();
        currentState = startingState;
    }
    private void Update()
    {
        if (currentState != AiActions.Flee && currentState!= AiActions.Charge && !EliteNearby())
        {
            SetState(AiActions.Flee);
            gruntCombat.StopFiring();
            fleeDestinationTimer = 0f;
            fleeDecisionTimer = fleeDecisionTime;
            return;
        }

        switch (currentState)
            {
                case AiActions.Sleep:
                    Sleep();
                    break;
                case AiActions.Wait:
                    Wait();
                    break;
                case AiActions.Alert:
                    Alert();
                    break;
                case AiActions.Fight:
                    Fight();
                    break;
                case AiActions.Search:
                    Search();
                    break;
                case AiActions.Flee:
                     Flee();
                    break;
                case AiActions.Charge:
                     Charge();
                    break;
        }
    }
    private void Sleep()
    {
        Collider[] nearbyObjects =
            Physics.OverlapSphere(transform.position, wakeRadius);
        foreach (Collider other in nearbyObjects)
        {
            GruntAI otherGrunt = other.GetComponentInParent<GruntAI>();
            if (otherGrunt == null) continue;
            if(otherGrunt == this) continue;
            
            if(otherGrunt.currentState == AiActions.Alert||
                otherGrunt.currentState == AiActions.Fight)
            {
                SetState(AiActions.Alert);
                return;
            }
        }
    }
    private void Wait()
    {
        if (CanSeePlayer())
        {
            SetState(AiActions.Alert);
        }
    }
    private void Alert()
    {
        if (CanSeePlayer())
        {
            RememberPlayer();
        }
        SetState(AiActions.Fight);
    }
    private void Fight()
    {
        if (!EliteNearby())
        {
            gruntCombat.StopFiring();
            SetState(AiActions.Flee);
            return;
        }
        if(CanSeePlayer())
        {
            RememberPlayer();
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0f;
            if(lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    targetRotation, 60f * Time.deltaTime);
            }
            gruntCombat.SetTarget(player);
            gruntCombat.StartFiring();
        }
        else
        {
            gruntCombat.StopFiring();
            if(Time.time - lastSeenTime >= memoryDuration)
            {
                hasSearchDestination = false;
                //searchStartRotation = transform.eulerAngles.y;
                currentSearchPoint = 0;
                searchTimer = 0f;
                SetState(AiActions.Search);
            }
        }
    }
    private void Search()
    {
        if (CanSeePlayer())
        {
            hasSearchDestination = false;
            searchTimer = 0f;
            returningHome = false;

            agent.isStopped = true;

            SetState(AiActions.Fight);

            return;
        }
        if (!returningHome)
        {
            if (!hasSearchDestination)
            {
                if (currentSearchPoint >= searchPointCount)
                {
                    returningHome = true;
                    agent.isStopped = false;
                    agent.SetDestination(homePosition);

                    Debug.Log(
                        gameObject.name +
                        " finished searching and is returning home."
                    );

                    return;
                }

                ChooseSearchPoint();

                agent.isStopped = false;
                agent.SetDestination(searchDestination);

                currentSearchPoint++;

                Debug.Log(
                    gameObject.name +
                    " searching point " +
                    currentSearchPoint
                );
            }

            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;

                hasSearchDestination = false;
            }

            return;
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            returningHome = false;
            hasSearchDestination = false;

            if (startingState == AiActions.Sleep)
            {
                SetState(AiActions.Sleep);
            }
            else
            {
                SetState(AiActions.Wait);
            }
        }
    }
    private void Flee()
    {
        fleeDecisionTimer -= Time.deltaTime;

        if (fleeDecisionTimer <= 0f)
        {
            fleeDecisionTimer = 0f;

            suicideCharge = true;
            SetState(AiActions.Charge);
            return;
        }

        if (fleeDestinationTimer <= 0f)
        {
            fleeDestinationTimer = fleeDestinationTime;

            Vector2 randomPoint =
                Random.insideUnitCircle * fleeAreaRadius;

            Vector3 randomPosition =
                transform.position +
                new Vector3(randomPoint.x, 0f, randomPoint.y);

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                randomPosition,
                out hit,
                fleeAreaRadius,
                NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }

        fleeDestinationTimer -= Time.deltaTime;
    }
    private void Charge()
    {
        Debug.Log(gameObject.name + " CHARGING!");
        if (suicideCharge)
        {
            SuicideCharge();
        }
        else
        {
            StandGroundCharge();
        }
    }
    private void StandGroundCharge()
    {
        if (player == null) return;
        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (CanSeePlayer())
        {
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0f;

            if(lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation =
                    Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        120f * Time.deltaTime
                    );
            }
            gruntCombat.SetTarget(player);
            gruntCombat.StartFiring();
        }
    }
   
    private void SuicideCharge()
    {
        if (player == null)
            return;

        gruntCombat.StopFiring();
        gruntCombat.HideWeaponModel();

        if (!suicideGrenadesSpawned)
        {
            SpawnSuicideGrenades();
            suicideGrenadesSpawned = true;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        Vector3 lookDirection =
            player.position - transform.position;

        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDirection);

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    180f * Time.deltaTime
                );
        }
    }
    private bool PlayerInSearchRadius()
    {
        if (player == null) return false;
        float distance = Vector3.Distance(
            transform.position, player.position );
        return distance <= searchRaidus;
    }
    private bool CanSeePlayer()
    {
        if (!PlayerInSearchRadius()) return false;
        Vector3 directionToPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle <= visionCone * 0.5f;
    }
    private void RememberPlayer()
    {
        lastKnownPlayerPosition = player.position;
        lastSeenTime = Time.time;

    }

    private void ChooseSearchPoint()
    {
        Vector2 randomPoint =
        Random.insideUnitCircle * searchAreaRadius;

        Vector3 randomPosition =
            lastKnownPlayerPosition +
            new Vector3(
                randomPoint.x,
                0f,
                randomPoint.y
            );

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomPosition,
            out hit,
            searchAreaRadius,
            NavMesh.AllAreas))
        {
            searchDestination = hit.position;
            hasSearchDestination = true;
        }
    }
    private void SetState(AiActions newState)
    {
        if(currentState == newState) return;
        Debug.Log(gameObject.name + " changed from " +
            currentState + " to " + newState);
        currentState = newState;
    }
    private bool EliteNearby()
    {
        Collider[] nearbyObjects =
            Physics.OverlapSphere(
                transform.position,
                eliteRetreatDistance);
        foreach (Collider other in nearbyObjects)
        {
            EliteAI elite = other.GetComponentInParent<EliteAI>();
            if(elite != null)
            {
                return true;
            }
        }
        return false;
    }
    private void SpawnSuicideGrenades()
    {
        if (plasmaGrenadeData == null)
        {
            Debug.LogError(
                gameObject.name +
                " has no Plasma Grenade Data assigned.",
                this
            );

            return;
        }

        SpawnGrenadeAtHand(leftHandGrenadePoint);
        SpawnGrenadeAtHand(rightHandGrenadePoint);
    }

    private void SpawnGrenadeAtHand(Transform handPoint)
    {
        if (handPoint == null)
        {
            Debug.LogError(
                gameObject.name +
                " has a missing grenade hand point.",
                this
            );

            return;
        }

        if (plasmaGrenadeData.worldPrefab == null)
        {
            Debug.LogError(
                "Plasma Grenade Data has no World Prefab assigned.",
                this
            );

            return;
        }

        GameObject grenadeObject = Instantiate(
            plasmaGrenadeData.worldPrefab,
            handPoint
        );

        grenadeObject.transform.localPosition = Vector3.zero;
        grenadeObject.transform.localRotation = Quaternion.identity;

        Grenade grenade = grenadeObject.GetComponent<Grenade>();

        if (grenade == null)
        {
            Debug.LogError(
                "Suicide grenade prefab has no Grenade component.",
                grenadeObject
            );

            Destroy(grenadeObject);
            return;
        }

        grenade.Initialise(plasmaGrenadeData);
    }
}