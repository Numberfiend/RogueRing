using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class GruntAI : MonoBehaviour
{
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
        if (currentState != AiActions.Flee && !EliteNearby())
        {
            SetState(AiActions.Flee);
            gruntCombat.StopFiring();
            fleeDestinationTimer = 0f;
            fleeDecisionTimer = fleeDestinationTime;
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

            if (Random.value < 0.5f)
            {
                // Continue fleeing
                fleeDecisionTimer = fleeDecisionTime;
            }
            else
            {
                suicideCharge = Random.value < 0.5f;
                SetState(AiActions.Charge);
                return;
            }
        }

        if (fleeDestinationTimer <= 0f)
        {
            fleeDestinationTimer = fleeDestinationTime;

            Vector2 randomPoint =
                Random.insideUnitCircle * fleeAreaRadius;

            Vector3 randomPosition =
                transform.position +
                new Vector3(
                    randomPoint.x,
                    0f,
                    randomPoint.y
                );

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
}

/*public class GruntAI : MonoBehaviour
{
    [Header("Starting State")]
    [SerializeField] private AiActions startingAction = AiActions.Wait;

    [SerializeField] private float sightRange = 20f;
    [SerializeField] private float sightAngle = 120f;
    private Transform player;

    [Header("Memory")]
    [SerializeField] private float memoryDuration = 10f;
    private Vector3 lastKnownPlayerPosition;
    private float lastSeenTime;
    [Header("Combat")]
    [SerializeField] private float minFightTime = 3f;
    [SerializeField] private float maxFightTime = 5f;
    [SerializeField] private float turnSpeed = 60f;
    private float actionTimer;

    private GruntCombat combatscript;
    private NavMeshAgent agent;
    private bool searchingForPlayer;
    private float searchTurnDirection;
    private Vector3 repositionPoint;
    public AiActions CurrentAction
    {
        get;
        private set;
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        combatscript = GetComponent<GruntCombat>();
        CurrentAction = startingAction;   
    }
    private bool CanSeePlayer()
    {
        if(player == null) return false;
        Vector3 direction = player.position - transform.position;
        if(direction.magnitude > sightRange) return false;
        float angle = Vector3.Angle(transform.forward,direction);
        if(angle > sightAngle * 0.5f) return false;
        return true;
    }
    private void RememberPlayer()
    {
        lastKnownPlayerPosition = player.position;
        lastSeenTime = Time.time;
    }
    private bool HasPlayerMemory()
    {
        return Time.time - lastSeenTime < memoryDuration;
    }
    // Update is called once per frame
    void Update()
    {
        UpdateAction();
    }

    public void SetActions(AiActions newAction)
    {
        if (CurrentAction == newAction) return;
        Debug.Log(gameObject.name + " changed from " + CurrentAction + " to " + newAction);
        CurrentAction = newAction;
    }

    private void UpdateAction()
    {
        switch (CurrentAction)
        {
            case AiActions.Sleep:
                Sleep();
                break;
            case AiActions.Alert:
                Alert();
                break;

            case AiActions.Fight:
                Fight();
                break;
            case AiActions.Uncover:
                Uncover();
                break;

            case AiActions.Flee:
                Flee();
                break;

            case AiActions.Search:
                Search();
                break;

            case AiActions.Wait:
                Wait();
                break;
            case AiActions.Guard:
                Guard();
                break;
        }
    }
    private void Sleep()
    {

    }
    private void Alert()
    {
        actionTimer = Random.Range(minFightTime, maxFightTime);
        SetActions(AiActions.Fight);
    }

    private void Fight()
    {
        combatscript.StartFiring();

        if (CanSeePlayer())
        {
            RememberPlayer();

            Vector3 lookTarget = player.position;
            lookTarget.y = transform.position.y;

            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime);
            

            actionTimer -= Time.deltaTime;

            if (actionTimer <= 0)
            {
                SetActions(AiActions.Uncover);
            }

            return;
        }

        combatscript.StopFiring();

        searchingForPlayer = true;
        actionTimer = 3f;
        searchTurnDirection = Random.value < 0.5f ? -1f : 1f;
        SetActions(AiActions.Search);
    }

    private void Flee()
    {

    }
    private void Search()
    {
        combatscript.StopFiring();

        if (CanSeePlayer())
        {
            SetActions(AiActions.Fight);
            return;
        }

        if (!HasPlayerMemory())
        {
            searchingForPlayer = false;
            SetActions(AiActions.Wait);
            return;
        }

        agent.SetDestination(lastKnownPlayerPosition);

        if (!agent.pathPending &&
    agent.remainingDistance <= 1f)
        {
            agent.isStopped = true;

            transform.Rotate(
                Vector3.up,
                turnSpeed * searchTurnDirection * Time.deltaTime
            );

            actionTimer -= Time.deltaTime;

            if (actionTimer <= 0f)
            {
                agent.isStopped = false;
                searchingForPlayer = false;
                SetActions(AiActions.Wait);
            }
        }
    }
    private void Wait()
    {
        combatscript.StopFiring();
        transform.Rotate(
        Vector3.up,
        turnSpeed * Time.deltaTime);

        if (CanSeePlayer())
        {
            SetActions(AiActions.Alert);
        }
    }
    private void Uncover()
    {
        combatscript.StopFiring();
        PickRepositionPoint();
        agent.SetDestination(repositionPoint);
        SetActions(AiActions.Guard);
    }
    private void Guard()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance > 1f)
            return;

        actionTimer =
            Random.Range(
                minFightTime,
                maxFightTime);

        SetActions(AiActions.Fight);
    }
    private void PickRepositionPoint()
    {
        Vector3 directionFromPlayer =
        (transform.position -
         player.position).normalized;

        Vector3 sideOffset =
            Vector3.Cross(
                Vector3.up,
                directionFromPlayer) *
            Random.Range(-5f, 5f);

        Vector3 distanceOffset =
            directionFromPlayer *
            Random.Range(2f, 6f);

        repositionPoint =
            transform.position +
            sideOffset +
            distanceOffset;

        Debug.Log(
            gameObject.name +
            " repositioning");
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Vector3 leftBoundary =
            Quaternion.Euler(0, -sightAngle * 0.5f, 0) *
            transform.forward;

        Vector3 rightBoundary =
            Quaternion.Euler(0, sightAngle * 0.5f, 0) *
            transform.forward;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position,
            leftBoundary * sightRange);

        Gizmos.DrawRay(
            transform.position,
            rightBoundary * sightRange);
    }
}*/
