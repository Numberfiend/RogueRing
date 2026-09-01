using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 75f;
    [SerializeField] private float currentHealth;

    [Header("Shield")]
    [SerializeField] private float maxShield = 75f;
    [SerializeField] private float currentShield;
    [SerializeField] private float shieldRechargeDelay = 5f;
    [SerializeField] private float shieldRechargeRate = 25f;
    public UnityEvent OnShieldDepleted;
    public UnityEvent OnShieldRechargeStart;
    public UnityEvent OnShieldRecovered;
    public UnityEvent OnShieldDamaged;
    private bool shieldDepletedTriggered;
    private bool rechargeStartedTriggered;
    private bool shieldRecoveredTriggered;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider shieldSlider;

    private float lastDamageTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;

        UpdateUI();
    }
    

    // Update is called once per frame
    void Update()
    {
       
        RechargeShield();    
    }

    public void TakeDamage(float damage)
    {
        OnShieldDamaged?.Invoke();
        lastDamageTime = Time.time;

        rechargeStartedTriggered = false;
        shieldRecoveredTriggered = false;

        if (currentShield > 0f)
        {
            currentShield -= damage;
            currentShield = Mathf.Max(currentShield, 0f);
            if (currentShield <= 0f && !shieldDepletedTriggered)
            {
                shieldDepletedTriggered = true;
                OnShieldDepleted?.Invoke();
            }
        }
        else
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0f);
        }
        UpdateUI();
        Debug.Log("Player Health: " + currentHealth +
            " / " + maxHealth +
            " | Shield: " + currentShield +
            " / " + maxShield);
        if(currentHealth <= 0f)
        {
            Die();
        }
    }

    private void RechargeShield()
    {
        if (currentShield >= maxShield)
            return;

        if (Time.time < lastDamageTime + shieldRechargeDelay)
            return;

        if (!rechargeStartedTriggered)
        {
            rechargeStartedTriggered = true;
            OnShieldRechargeStart?.Invoke();
        }

        currentShield += shieldRechargeRate * Time.deltaTime;
        currentShield = Mathf.Min(currentShield, maxShield);

        float shieldPercent =
            currentShield / maxShield;

        if (shieldPercent >= 0.25f &&
            !shieldRecoveredTriggered)
        {
            shieldRecoveredTriggered = true;
            shieldDepletedTriggered = false;

            OnShieldRecovered?.Invoke();
        }

        UpdateUI();
    }
    private void UpdateUI()
    {
        if(healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        if(shieldSlider != null)
        {
            shieldSlider.value = currentShield;
        }
    }
    private void Die()
    {
        Debug.Log("PlayerDied");
    }
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    public float GetCurrentShield()
    {
        return currentShield;
    }

}
