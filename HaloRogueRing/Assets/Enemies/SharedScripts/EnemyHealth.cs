
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    private float currentHealth;
    private float currentShield;

    [Header("Shield Recharge")]
    [SerializeField] private float shieldRechargeDelay = 5f;
    [SerializeField] private float shieldRechargeRate = 25f;

    private CommonEnemyAudio enemyAudio;
    private float shieldRechargeTimer;
    private bool isDead;

    private void Start()
    {
        enemyAudio = GetComponent<CommonEnemyAudio>();

        if (enemyData == null)
        {
            Debug.LogError(
                gameObject.name + " has no EnemyData assigned.",
                this
            );

            enabled = false;
            return;
        }

        currentHealth = enemyData.maxHealth;
        currentShield = enemyData.maxShield;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        // Reset the recharge delay whenever damage is received.
        shieldRechargeTimer = shieldRechargeDelay;

        if (currentShield > 0f)
        {
            currentShield -= damage;
            currentShield = Mathf.Max(currentShield, 0f);

            if (currentShield <= 0f && enemyAudio != null)
                enemyAudio.OnDamage();

            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0f)
        {
            Die();
        }
        else if (enemyAudio != null)
        {
            enemyAudio.OnDamage();
        }
    }

    private void Update()
    {
        if (isDead || enemyData == null)
            return;

        if (currentShield >= enemyData.maxShield)
            return;

        if (shieldRechargeTimer > 0f)
        {
            shieldRechargeTimer -= Time.deltaTime;
            return;
        }

        currentShield += shieldRechargeRate * Time.deltaTime;
        currentShield = Mathf.Min(currentShield, enemyData.maxShield);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (enemyAudio != null)
            enemyAudio.OnDeath();

        EnemyCombat enemyCombat = GetComponent<EnemyCombat>();

        if (enemyCombat != null)
            enemyCombat.DropWeapon();

        Debug.Log(enemyData.enemyName + " Died");

        Destroy(gameObject);
    }

    public EnemyData GetEnemyData()
    {
        return enemyData;
    }

    public float CurrentHealth => currentHealth;
    public float CurrentShield => currentShield;

    public float MaxHealth => enemyData != null ? enemyData.maxHealth : 0f;
    public float MaxShield => enemyData != null ? enemyData.maxShield : 0f;

    public float ShieldPercentage =>
        MaxShield > 0f ? CurrentShield / MaxShield : 0f;

    public bool IsDead => isDead;
}
