using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    private float currentHealth;
    private float currentShield;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = enemyData.maxHealth;
        currentShield = enemyData.maxShield;
    }

    public void TakeDamage(float damage)
    {
        if(currentShield > 0)
        {
            float shieldDamage = Mathf.Min(currentShield, damage);
            currentShield -= shieldDamage;
            damage -= shieldDamage;
        }
        if (damage > 0)
        {
            currentHealth -= damage;
        }

        Debug.Log(enemyData.enemyName +
            "HP: " +
            currentHealth +
            "Shield: " +
            currentShield
        );
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(enemyData.enemyName + "Died");
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public EnemyData GetEnemyData()
    {
        return enemyData;
    }

    public float CurrentHealth => currentHealth;
    public float CurrentShield => currentShield;
}
