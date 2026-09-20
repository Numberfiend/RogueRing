using System.Runtime.CompilerServices;
using UnityEngine;

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
    private float actionTimer;

    private Vector3 repositionPoint;
    public AiActions CurrentAction
    {
        get;
        private set;
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
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
        if(CanSeePlayer())
        {
            RememberPlayer();
        }
        else
        {
            SetActions(AiActions.Search);
            return;
        }
        
        actionTimer -= Time.deltaTime;
        
        if(actionTimer < 0)
        {
            SetActions(AiActions.Uncover);
        }
    }

    private void Flee()
    {

    }
    private void Search()
    {
        if (CanSeePlayer())
        {
            SetActions(AiActions.Fight);
            return;
        }
        if (!HasPlayerMemory())
        {
            SetActions(AiActions.Wait);
        }
    }
    private void Wait()
    {
        if (CanSeePlayer())
        {
            SetActions(AiActions.Alert);
        }
    }
    private void Uncover()
    {
        PickRepositionPoint();
        actionTimer = Random.Range(minFightTime, maxFightTime);
        SetActions(AiActions.Fight);
    }
    private void PickRepositionPoint()
    {
        Vector3 right =
            transform.right *
            Random.Range(-5f, 5f);

        Vector3 forward =
            transform.forward *
            Random.Range(-3f, 3f);

        repositionPoint =
            transform.position +
            right +
            forward;

        Debug.DrawLine(
            transform.position,
            repositionPoint,
            Color.blue,
            2f);

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
