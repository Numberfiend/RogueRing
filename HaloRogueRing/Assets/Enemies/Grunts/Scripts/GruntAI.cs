using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class GruntAI : MonoBehaviour
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
}
