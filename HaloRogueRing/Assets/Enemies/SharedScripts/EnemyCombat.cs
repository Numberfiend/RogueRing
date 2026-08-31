using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private float minDamage =5f;
    [SerializeField] private float mediDamage =8f;
    [SerializeField] private float maxDamage =10f;

    private float nextFireTime;

    private void Update()
    {
        if (enemyData == null) return;
        if(enemyData.startingWeapon == null) return;
        if(Time.time > nextFireTime)
        {
            Fire();
        }
    }
    private void Fire()
    {
        if(projectilePrefab == null)
        {
            return;
        }
        if (firePoint == null) return;

        float damage = GetWeaponDamage();
        GameObject projectileObject = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );
        PlasmaBolts projectile = projectileObject.GetComponent<PlasmaBolts>();

        if (projectile != null)
        {
            projectile.Intialize(damage);
            
            projectile.SetFaction(Faction.Covenant);
        }

        nextFireTime =
            Time.time +
            (1f / enemyData.startingWeapon.firerate);
    }
    public WeaponData GetWeapon()
    {
        return enemyData.startingWeapon;
    }

    private float GetWeaponDamage()
    {
        if(enemyData.startingWeapon.weaponName == "PlasmaPistol")
        {
            int damageRoll = Random.Range(0, 3);
            switch (damageRoll)
            {
                case 0:
                    return minDamage;
                case 1:
                return mediDamage;
                case 2:
                    return maxDamage;
            }
        }
        return 0f;
    }
}
