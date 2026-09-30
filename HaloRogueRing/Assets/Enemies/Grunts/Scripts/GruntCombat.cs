using UnityEngine;

public class GruntCombat : EnemyCombat
{
    [Header("Weapon Models")]
    [SerializeField] public GameObject plasmaPistolModel;
    [SerializeField] public GameObject needlerModel;

    [Header("Weapon Fire Points")]
    [SerializeField] public Transform plasmaPistolFirePoint;
    [SerializeField] public Transform needlerFirePoint;

    [Header("Projectile Prefabs")]
    [SerializeField] private PlasmaBolts plasmaBoltPrefab;
    [SerializeField] private Needles needleProjectilePrefab;

    [Header("Testing")]
    [SerializeField] private float fireRate = 10f;

    private float weaponDamage;

    protected override void Start()
    {
        base.Start();

        SetWeaponDamage();
        SetActiveFirePoint();
    }

    private void SetWeaponDamage()
    {
        if (equippedWeapon == null)
        {
            Debug.LogError(
                gameObject.name +
                " has no equipped weapon."
            );

            return;
        }

        if (equippedWeapon.weaponName == "PlasmaPistol")
        {
            weaponDamage = Random.Range(5f, 10.01f);
        }
        else if (equippedWeapon.weaponName == "Needler")
        {
            weaponDamage = 5f;
        }
    }

    private void SetActiveFirePoint()
    {
        if (equippedWeapon == null)
            return;

        if (equippedWeapon.weaponName == "PlasmaPistol")
        {
            SetFirePoint(plasmaPistolFirePoint);
        }
        else if (equippedWeapon.weaponName == "Needler")
        {
            SetFirePoint(needlerFirePoint);
        }
    }

    protected override void Fire()
    {
        if (currentTarget == null) return;
        if (activeFirePoint == null)
        {
            Debug.LogError(
                gameObject.name +
                " has no FirePoint for " +
                equippedWeapon.weaponName
            );

            return;
        }

        if (equippedWeapon.weaponName == "PlasmaPistol")
        {
            FirePlasmaPistol();
        }
        else if (equippedWeapon.weaponName == "Needler")
        {
            FireNeedler();
        }

        nextFireTime =
            Time.time +
            (1f / fireRate);
    }

    private void FirePlasmaPistol()
    {
        if (plasmaBoltPrefab == null)
        {
            Debug.LogError(
                "No Plasma Bolt prefab assigned."
            );

            return;
        }
        Vector3 targetPosition = currentTarget.position + Vector3.up * 10f;
        Vector3 direction = (currentTarget.position - activeFirePoint.position).normalized;
        PlasmaBolts projectile =
            Instantiate(
                plasmaBoltPrefab,
                activeFirePoint.position,
                Quaternion.LookRotation(direction)
            );

        projectile.SetDamage(weaponDamage);
        projectile.SetFaction(Faction.Covenant);
    }

    private void FireNeedler()
    {
        if (needleProjectilePrefab == null)
        {
            Debug.LogError(
                "No Needle Projectile prefab assigned."
            );

            return;
        }
        Vector3 targetPosition = currentTarget.position + Vector3.up * 10f;
        Vector3 direction = (currentTarget.position - activeFirePoint.position).normalized;
        Needles projectile =
            Instantiate(
                needleProjectilePrefab,
                activeFirePoint.position,
                Quaternion.LookRotation(direction)
            );

        projectile.SetDamage(weaponDamage);
        projectile.SetFaction(Faction.Covenant);
    }

    protected override void UpdateWeaponModel()
    {
        if (plasmaPistolModel != null)
            plasmaPistolModel.SetActive(false);

        if (needlerModel != null)
            needlerModel.SetActive(false);

        if (equippedWeapon == null)
            return;

        if (equippedWeapon.weaponName == "PlasmaPistol")
        {
            if (plasmaPistolModel != null)
                plasmaPistolModel.SetActive(true);
        }
        else if (equippedWeapon.weaponName == "Needler")
        {
            if (needlerModel != null)
                needlerModel.SetActive(true);
        }
    }

    public float GetWeaponDamage()
    {
        return weaponDamage;
    }
}

