using UnityEngine;

public class EliteCombat : EnemyCombat
{
    [Header("Weapon Models")]
    [SerializeField] private GameObject plasmaRifleModel;
    [SerializeField] private GameObject needlerModel;

    [Header("Weapon Fire Points")]
    [SerializeField] private Transform plasmaRifleFirePoint;
    [SerializeField] private Transform needlerFirePoint;

    [Header("Projectile Prefabs")]
    [SerializeField] private PlasmaBolts plasmaBoltPrefab;
    [SerializeField] private Needles needleProjectilePrefab;

    [Header("Testing")]
    [SerializeField] private float fireRate = 1f;

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

        if (equippedWeapon.weaponName == "PlasmaRifle")
        {
            weaponDamage = Random.Range(5f, 7.01f);
        }
        else if (equippedWeapon.weaponName == "Needler")
        {
            weaponDamage = 5f;
        }
        else
        {
            Debug.LogWarning(
                "No Elite damage value for weapon: " +
                equippedWeapon.weaponName
            );

            weaponDamage = 0f;
        }
    }

    private void SetActiveFirePoint()
    {
        if (equippedWeapon == null)
            return;

        if (equippedWeapon.weaponName == "PlasmaRifle")
        {
            SetFirePoint(plasmaRifleFirePoint);
        }
        else if (equippedWeapon.weaponName == "Needler")
        {
            SetFirePoint(needlerFirePoint);
        }
    }

    protected override void Fire()
    {
        if (equippedWeapon == null)
            return;

        if (activeFirePoint == null)
        {
            Debug.LogError(
                gameObject.name +
                " has no FirePoint for " +
                equippedWeapon.weaponName
            );

            return;
        }

        if (equippedWeapon.weaponName == "PlasmaRifle")
        {
            FirePlasmaRifle();
        }
        else if (equippedWeapon.weaponName == "Needler")
        {
            FireNeedler();
        }

        nextFireTime =
            Time.time +
            (1f / fireRate);
    }

    private void FirePlasmaRifle()
    {
        if (plasmaBoltPrefab == null)
            return;

        PlasmaBolts projectile = Instantiate(
            plasmaBoltPrefab,
            activeFirePoint.position,
            activeFirePoint.rotation
        );

        projectile.SetDamage(weaponDamage);
        projectile.SetFaction(Faction.Covenant);
    }

    private void FireNeedler()
    {
        if (needleProjectilePrefab == null)
            return;

        Needles projectile = Instantiate(
            needleProjectilePrefab,
            activeFirePoint.position,
            activeFirePoint.rotation
        );

        projectile.SetDamage(weaponDamage);
        projectile.SetFaction(Faction.Covenant);
    }

    protected override void UpdateWeaponModel()
    {
        if (plasmaRifleModel != null)
            plasmaRifleModel.SetActive(false);

        if (needlerModel != null)
            needlerModel.SetActive(false);

        if (equippedWeapon == null)
            return;

        if (equippedWeapon.weaponName == "PlasmaRifle")
        {
            if (plasmaRifleModel != null)
                plasmaRifleModel.SetActive(true);
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