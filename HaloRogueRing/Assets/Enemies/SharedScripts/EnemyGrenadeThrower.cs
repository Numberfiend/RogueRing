using UnityEngine;
using UnityEngine.Events;

public class EnemyGrenadeThrower : MonoBehaviour
{
    [Header("Grenade")]
    [SerializeField] private GrenadeData grenadeData;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private int grenadeCount = 2;

    [Header("Timing")]
    [SerializeField] private float initialDelay = 4f;
    [SerializeField] private float cooldownMin = 8f;
    [SerializeField] private float cooldownMax = 14f;
    [SerializeField] private float windUpTime = 0.6f;
    [SerializeField] private float decisionInterval = 1f;

    [Header("Targeting")]
    [SerializeField] private float minThrowDistance = 7f;
    [SerializeField] private float maxThrowDistance = 25f;
    [SerializeField] private float scatterRadius = 1.5f;
    [SerializeField] private float throwSpeed = 14f;
    [SerializeField] private float friendlyClearRadius = 5f;

    [Header("When to throw")]
    [SerializeField] private float coverFlushWindow = 4f;
    [SerializeField] private float campTime = 3f;
    [SerializeField] private float campDistance = 1.5f;
    [Range(0f, 1f)]
    [SerializeField] private float randomThrowChance = 0.1f;

    [Header("Audio and Animation Events")]
    public UnityEvent onWindUpStarted;
    public UnityEvent onGrenadeReleased;

    private EnemyHealth enemyHealth;

    private float cooldownTimer;
    private float decisionTimer;
    private bool isWindingUp;
    private float windUpTimer;
    private Vector3 pendingTarget;

    private Vector3 campAnchor;
    private float campTimer;

    public bool IsThrowing => isWindingUp;
    public Vector3 PendingTarget => pendingTarget;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        cooldownTimer = initialDelay;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (!isWindingUp) return;

        if (enemyHealth != null && enemyHealth.IsDead)
        {
            CancelThrow();
            return;
        }

        windUpTimer -= Time.deltaTime;
        if (windUpTimer <= 0f)
            Release();
    }

    public void Evaluate(Transform target, bool canSeeTarget, Vector3 lastKnownPosition, float lastSeenTime)
    {
        if (target == null || grenadeData == null || isWindingUp) return;

      
        if (canSeeTarget)
        {
            if ((target.position - campAnchor).sqrMagnitude > campDistance * campDistance)
            {
                campAnchor = target.position;
                campTimer = 0f;
            }
            else
            {
                campTimer += Time.deltaTime;
            }
        }
        else
        {
            campTimer = 0f;
        }

        if (grenadeCount <= 0 || cooldownTimer > 0f) return;

        decisionTimer -= Time.deltaTime;
        if (decisionTimer > 0f) return;
        decisionTimer = decisionInterval;

        Vector3 aimPoint;

        if (canSeeTarget)
        {
            bool camping = campTimer >= campTime;
            if (!camping && Random.value >= randomThrowChance) return;
            aimPoint = target.position;
        }
        else if (Time.time - lastSeenTime <= coverFlushWindow)
        {
            aimPoint = lastKnownPosition; 
        }
        else
        {
            return;
        }

        if (!IsValidTarget(aimPoint)) return;

        pendingTarget = aimPoint;
        isWindingUp = true;
        windUpTimer = windUpTime;
        onWindUpStarted.Invoke();
    }

    public void CancelThrow()
    {
        isWindingUp = false;
    }

    private bool IsValidTarget(Vector3 point)
    {
        Vector3 flat = point - transform.position;
        flat.y = 0f;
        float distance = flat.magnitude;

        float minDistance = Mathf.Max(minThrowDistance, grenadeData.explosionRadius + 2f);
        if (distance < minDistance || distance > maxThrowDistance) return false;

        float clearSqr = friendlyClearRadius * friendlyClearRadius;

        var grunts = GruntAI.AllGrunts;
        for (int i = 0; i < grunts.Count; i++)
        {
            GruntAI g = grunts[i];
            if (g == null || g.IsDead || g.gameObject == gameObject) continue;
            if ((g.transform.position - point).sqrMagnitude < clearSqr) return false;
        }

        var elites = EliteAI.AllElites;
        for (int i = 0; i < elites.Count; i++)
        {
            EliteAI e = elites[i];
            if (e == null || e.IsDead || e.gameObject == gameObject) continue;
            if ((e.transform.position - point).sqrMagnitude < clearSqr) return false;
        }

        return true;
    }

    private void Release()
    {
        isWindingUp = false;
        cooldownTimer = Random.Range(cooldownMin, cooldownMax);

        if (grenadeData == null || grenadeData.worldPrefab == null)
        {
            Debug.LogError(gameObject.name + ": grenade data or its World Prefab is missing.", this);
            return;
        }

        Vector3 origin = throwPoint != null
            ? throwPoint.position
            : transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;

        Vector2 scatter = Random.insideUnitCircle * scatterRadius;
        Vector3 target = pendingTarget + new Vector3(scatter.x, 0f, scatter.y);

        if (!TryComputeLaunchVelocity(origin, target, out Vector3 velocity))
            return;

        GameObject grenadeObject = Instantiate(grenadeData.worldPrefab, origin, Quaternion.identity);
        Grenade grenade = grenadeObject.GetComponent<Grenade>();

        if (grenade == null)
        {
            Debug.LogError("Enemy grenade prefab has no Grenade component.", grenadeObject);
            Destroy(grenadeObject);
            return;
        }

        grenade.Initialise(grenadeData);
        IgnoreEnemyColliders(grenadeObject);
        grenade.ThrowWithVelocity(velocity);

        grenadeCount--;
        onGrenadeReleased.Invoke();
    }

  
    private bool TryComputeLaunchVelocity(Vector3 origin, Vector3 target, out Vector3 velocity)
    {
        velocity = Vector3.zero;

        Vector3 delta = target - origin;
        Vector3 flat = new Vector3(delta.x, 0f, delta.z);
        if (flat.magnitude < 0.1f) return false;

        float flightTime = Mathf.Clamp(flat.magnitude / throwSpeed, 0.4f, 2.5f);

        velocity = delta / flightTime - 0.5f * Physics.gravity * flightTime;
        return true;
    }


    private void IgnoreEnemyColliders(GameObject grenadeObject)
    {
        Collider grenadeCollider = grenadeObject.GetComponent<Collider>();
        if (grenadeCollider == null) return;

        IgnoreCollidersOf(transform, grenadeCollider);

        var grunts = GruntAI.AllGrunts;
        for (int i = 0; i < grunts.Count; i++)
            if (grunts[i] != null) IgnoreCollidersOf(grunts[i].transform, grenadeCollider);

        var elites = EliteAI.AllElites;
        for (int i = 0; i < elites.Count; i++)
            if (elites[i] != null) IgnoreCollidersOf(elites[i].transform, grenadeCollider);
    }

    private static void IgnoreCollidersOf(Transform root, Collider grenadeCollider)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            Physics.IgnoreCollision(grenadeCollider, colliders[i]);
    }
}

