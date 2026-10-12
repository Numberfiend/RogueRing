using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Grenade : MonoBehaviour
{
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

        // Stick to characters that have health components.
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

        foreach (Collider hitCollider in hitColliders)
        {
            PlayerHealth playerHealth =
                hitCollider.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                float damage = CalculateDamage(
                    playerHealth.transform.position
                );

                playerHealth.TakeDamage(damage);
                continue;
            }

            EnemyHealth enemyHealth =
                hitCollider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                float damage = CalculateDamage(
                    enemyHealth.transform.position
                );

                enemyHealth.TakeDamage(damage);
            }
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
}

