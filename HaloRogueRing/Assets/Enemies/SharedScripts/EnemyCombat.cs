using UnityEngine;

public abstract class EnemyCombat : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] protected EnemyData enemyData;

    [Header("Weapon Selection")]
    [SerializeField] protected int activeWeaponIndex = 0;

    [SerializeField] private Transform weaponDropPoint;
    protected WeaponData equippedWeapon;
    protected Transform activeFirePoint;

    protected float nextFireTime;

    protected virtual void Start()
    {
        SetActiveWeapon();
        UpdateWeaponModel();
    }

    protected virtual void Update()
    {
        if (equippedWeapon == null)
            return;

        if (Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    protected abstract void Fire();

    protected abstract void UpdateWeaponModel();

    protected void SetActiveWeapon()
    {
        if (enemyData == null)
        {
            Debug.LogError(
                gameObject.name +
                " has no EnemyData assigned."
            );

            return;
        }

        if (enemyData.availibleWeapons == null ||
            enemyData.availibleWeapons.Length == 0)
        {
            Debug.LogError(
                enemyData.enemyName +
                " has no available weapons."
            );

            return;
        }

        activeWeaponIndex = Mathf.Clamp(
            activeWeaponIndex,
            0,
            enemyData.availibleWeapons.Length - 1
        );

        equippedWeapon =
            enemyData.availibleWeapons[activeWeaponIndex];
    }

    public void DropWeapon()
    {
        Vector3 dropPoint;
        if(weaponDropPoint != null)
        {
            dropPoint = weaponDropPoint.position;
        }
        else
        {
            dropPoint = transform.position;
        }
        Instantiate(equippedWeapon.worldPrefab,
            dropPoint, transform.rotation);
    }
    protected void SetFirePoint(Transform firePoint)
    {
        activeFirePoint = firePoint;
    }

    public WeaponData GetWeapon()
    {
        return equippedWeapon;
    }

    public Transform GetFirePoint()
    {
        return activeFirePoint;
    }
}
