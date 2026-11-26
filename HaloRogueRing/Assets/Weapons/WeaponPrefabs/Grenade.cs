using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Grenade : MonoBehaviour
{
    private static readonly List<Grenade> activeGrenades = new List<Grenade>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => activeGrenades.Clear();

    private bool isCarried;

    public bool IsLive => hasBeenThrown && !hasExploded && !isCarried;
    public float ExplosionRadius => grenadeData != null ? grenadeData.explosionRadius : 0f;
    public Transform StuckTarget => isStuck ? stuckTarget : null;

    private void OnEnable() => activeGrenades.Add(this);
    private void OnDisable() => activeGrenades.Remove(this);

    
    
    [Header("Grenade Data")]
    [SerializeField] private GrenadeData grenadeData;

    private Rigidbody rb;
    private bool hasExploded;
    private bool hasCollided;
    private bool hasBeenThrown;
    private bool isStuck;

    private float throwTime;
    private float restTime;
    private float collisionTime;

    private Transform stuckTarget;
    private Vector3 stuckLocalPosition;
    private Quaternion stuckLocalRotation;

    private const float RestSpeed = 0.15f;
    private const float RestCheckDelay = 0.1f;

    private bool isGrounded;
    private float groundTime;

    [SerializeField] private float groundDrag = 4f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialise(GrenadeData data)
    {
        grenadeData = data;
    }

    public void Throw(Vector3 force)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        hasBeenThrown = true;
        throwTime = Time.time;

        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        if (!hasBeenThrown || hasExploded)
            return;

        if (isGrounded)
        {
            Vector3 velocity = rb.linearVelocity;

            Vector3 horizontalVelocity = new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

            horizontalVelocity = Vector3.Lerp(
                horizontalVelocity,
                Vector3.zero,
                groundDrag * Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector3(
                horizontalVelocity.x,
                velocity.y,
                horizontalVelocity.z
            );
        }
    }


    private void Update()
    {
        if (hasExploded || !hasBeenThrown || grenadeData == null)
            return;

        if (grenadeData.grenadeType == GrenadeType.HumanFrag)
        {
            UpdateHumanFrag();
        }
        else if (grenadeData.grenadeType == GrenadeType.Plasma)
        {
            UpdatePlasma();
        }
    }

    private void UpdateHumanFrag()
    {
        if (Time.time - throwTime < grenadeData.minTimeAfterThrow)
            return;

        if (rb.linearVelocity.magnitude <= RestSpeed &&
            Time.time - throwTime >= RestCheckDelay)
        {
            if (restTime <= 0f)
                restTime = Time.time;

            if (Time.time - restTime >= grenadeData.timeAfterRest)
            {
                Explode();
            }
        }
        else
        {
            restTime = 0f;
        }
    }

    private void UpdatePlasma()
    {
        if (!hasCollided)
            return;

        if (isStuck && stuckTarget != null)
        {
            transform.position =
                stuckTarget.TransformPoint(stuckLocalPosition);

            transform.rotation =
                stuckTarget.rotation * stuckLocalRotation;
        }

        if (Time.time - collisionTime >= grenadeData.plasmaFuseTime)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown || hasExploded || grenadeData == null)
            return;

        if (grenadeData.grenadeType == GrenadeType.HumanFrag)
            return;

        if (hasCollided)
            return;

        hasCollided = true;
        collisionTime = Time.time;

        PlayerHealth playerHealth =
            collision.collider.GetComponentInParent<PlayerHealth>();

        EnemyHealth enemyHealth =
            collision.collider.GetComponentInParent<EnemyHealth>();

        if (playerHealth != null || enemyHealth != null)
        {
            StickToTarget(collision.transform);
        }
    }

    private void StickToTarget(Transform target)
    {
        stuckTarget = target;
        isStuck = true;

        stuckLocalPosition =
            target.InverseTransformPoint(transform.position);

        stuckLocalRotation =
            Quaternion.Inverse(target.rotation) * transform.rotation;

        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Collider grenadeCollider = GetComponent<Collider>();

        if (grenadeCollider != null)
            grenadeCollider.enabled = false;

        transform.SetParent(target, true);
    }

    private void Explode()
    {
        if (hasExploded || grenadeData == null)
            return;

        hasExploded = true;

        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            grenadeData.explosionRadius
        );

        HashSet<PlayerHealth> damagedPlayers = new HashSet<PlayerHealth>();
        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();

        foreach (Collider hitCollider in hitColliders)
        {
            PlayerHealth playerHealth = hitCollider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (damagedPlayers.Add(playerHealth))
                    playerHealth.TakeDamage(CalculateDamage(playerHealth.transform.position));
                continue;
            }

            EnemyHealth enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null && damagedEnemies.Add(enemyHealth))
                enemyHealth.TakeDamage(CalculateDamage(enemyHealth.transform.position));
        }

        Debug.Log(grenadeData.grenadeName + " exploded!");

        Destroy(gameObject);
    }

    private float CalculateDamage(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(
            transform.position,
            targetPosition
        );

        float distancePercent = Mathf.Clamp01(
            distance / grenadeData.explosionRadius
        );

        return Mathf.Lerp(
            grenadeData.maxDamage,
            grenadeData.minDamage,
            distancePercent
        );
    }
    private void OnCollisionStay(Collision collision)
    {
        if (!hasBeenThrown || hasExploded)
            return;

        if (collision.contacts.Length == 0)
            return;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }

        isGrounded = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    public static Grenade FindThreat(Vector3 position, float margin, Transform ignoreStuckTo)
    {
        Grenade nearest = null;
        float nearestSqr = float.MaxValue;

        for (int i = 0; i < activeGrenades.Count; i++)
        {
            Grenade g = activeGrenades[i];
            if (g == null || !g.IsLive) continue;

            
            if (ignoreStuckTo != null && g.StuckTarget != null &&
                g.StuckTarget.root == ignoreStuckTo.root)
                continue;

            float range = g.ExplosionRadius + margin;
            float sqr = (g.transform.position - position).sqrMagnitude;
            if (sqr <= range * range && sqr < nearestSqr)
            {
                nearest = g;
                nearestSqr = sqr;
            }
        }
        return nearest;
    }

    public void ThrowWithVelocity(Vector3 velocity)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        hasBeenThrown = true;
        throwTime = Time.time;

        rb.isKinematic = false;
        rb.linearVelocity = velocity;
    }

    
    public void PrepareAsCarried()
    {
        isCarried = true;
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    public void Detonate() => Explode();
}

