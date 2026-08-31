using UnityEngine;

public class Needles : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float speed = 35f;
    [SerializeField] private float lifetime = 5f;

    [Header("Homing")]
    [SerializeField] private float homingRange = 30f;
    [SerializeField] private float homingStrength = 8f;
    [SerializeField] private float maximumHomingAngle = 25f;

    [Header("Homing Hit")]
    [SerializeField] private float homingHitDistance = 0.3f;

    private float damage;
    private Faction faction;

    private bool embedded = false;

    private Transform target;

    public void Initialize(float projectileDamage, Faction projectileFaction)
    {
        damage = projectileDamage;
        faction = projectileFaction;

        FindTarget();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void SetFaction(Faction newFaction)
    {
        faction = newFaction;

        FindTarget();
    }

    private void Update()
    {
        if (embedded)
            return;

        FindTargetIfNeeded();

        if (target != null)
        {
            HomeTowardsTarget();

            float distance =
                Vector3.Distance(
                    transform.position,
                    target.position
                );

            if (distance <= homingHitDistance)
            {
                HitTarget(target);
                return;
            }
        }

        transform.position +=
            transform.forward *
            speed *
            Time.deltaTime;
    }

    private void FindTargetIfNeeded()
    {
        if (target == null)
        {
            FindTarget();
        }
    }

    private void FindTarget()
    {
        Collider[] nearbyObjects =
            Physics.OverlapSphere(
                transform.position,
                homingRange
            );

        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (Collider collider in nearbyObjects)
        {
            if (faction == Faction.Player)
            {
                EnemyHealth enemy =
                    collider.GetComponentInParent<EnemyHealth>();

                if (enemy == null)
                    continue;

                Transform enemyTransform =
                    enemy.transform;

                Vector3 directionToEnemy =
                    (
                        enemyTransform.position -
                        transform.position
                    ).normalized;

                float angle =
                    Vector3.Angle(
                        transform.forward,
                        directionToEnemy
                    );

                if (angle > maximumHomingAngle)
                    continue;

                float distance =
                    Vector3.Distance(
                        transform.position,
                        enemyTransform.position
                    );

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = enemyTransform;
                }
            }
            else if (faction == Faction.Covenant)
            {
                PlayerHealth player =
                    collider.GetComponentInParent<PlayerHealth>();

                if (player == null)
                    continue;

                Transform playerTransform =
                    player.transform;

                float distance =
                    Vector3.Distance(
                        transform.position,
                        playerTransform.position
                    );

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = playerTransform;
                }
            }
        }

        target = closestTarget;
    }

    private void HomeTowardsTarget()
    {
        if (target == null)
            return;

        Vector3 directionToTarget =
            (
                target.position -
                transform.position
            ).normalized;

        if (directionToTarget == Vector3.zero)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(directionToTarget);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                homingStrength * Time.deltaTime
            );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (embedded)
            return;

        if (faction == Faction.Player)
        {
            EnemyHealth enemy =
                other.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                HitTarget(enemy.transform);
                return;
            }
        }

        if (faction == Faction.Covenant)
        {
            PlayerHealth player =
                other.GetComponentInParent<PlayerHealth>();

            if (player != null)
            {
                HitTarget(player.transform);
                return;
            }
        }
    }

    private void HitTarget(Transform targetTransform)
    {
        if (embedded)
            return;

        if (targetTransform == null)
            return;

        if (faction == Faction.Player)
        {
            EnemyHealth enemy =
                targetTransform.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Embed(enemy.transform);
                return;
            }
        }

        if (faction == Faction.Covenant)
        {
            PlayerHealth player =
                targetTransform.GetComponentInParent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage);
                Embed(player.transform);
                return;
            }
        }
    }

    private void Embed(Transform targetTransform)
    {
        embedded = true;

        target = null;

        transform.SetParent(targetTransform);

        Collider projectileCollider =
            GetComponent<Collider>();

        if (projectileCollider != null)
        {
            Destroy(projectileCollider);
        }

        NeedlerTarget needlerTarget =
            targetTransform.GetComponentInParent<NeedlerTarget>();

        if (needlerTarget == null)
        {
            needlerTarget =
                targetTransform.GetComponentInChildren<NeedlerTarget>();
        }

        if (needlerTarget != null)
        {
            needlerTarget.AddNeedle(this);
        }
    }
}