using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    private float currentHealth;
    private float currentShield;
    [Header("Shield Recharge")]
    [SerializeField] private float shieldRechargeDelay = 5f;
    [SerializeField] private float shieldRechargeRate = 25f;

    private float shieldRechargeTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError(
                gameObject.name +
                " has no EnemyData assigned."
            );

            return;
        }

        currentHealth = enemyData.maxHealth;
        currentShield = enemyData.maxShield;

        shieldRechargeTimer = Time.time;
    }

    public void TakeDamage(float damage)
    {
        // Reset the recharge timer whenever we're hit.
        shieldRechargeTimer = shieldRechargeDelay;

        if (currentShield > 0)
        {
            currentShield -= damage;

            if (currentShield < 0)
            {
                currentShield = 0;
            }

            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void RechargeShield()
    {
        if (currentShield >= enemyData.maxShield)
            return;

        if (Time.time < shieldRechargeTimer + shieldRechargeDelay)
            return;

        currentShield += shieldRechargeRate * Time.deltaTime;

        if (currentShield >= enemyData.maxShield)
        {
            currentShield = enemyData.maxShield;
        }
    }
    private void Die()
    {
        Debug.Log(enemyData.enemyName + "Died");
        Destroy(gameObject);
    }
    // Update is called once per frame
    private void Update()
    {
        if (currentShield >= enemyData.maxShield)
            return;

        if (enemyData.maxShield <= 0)
            return;

        if (shieldRechargeTimer > 0)
        {
            shieldRechargeTimer -= Time.deltaTime;
            return;
        }

        currentShield += shieldRechargeRate * Time.deltaTime;

        if (currentShield >= enemyData.maxShield)
        {
            currentShield = enemyData.maxShield;
        }
    }
    public EnemyData GetEnemyData()
    {
        return enemyData;
    }

    public float CurrentHealth => currentHealth;
    public float CurrentShield => currentShield;
}
