using UnityEngine;

public class ProjectileWeapon : Weapon
{
    [SerializeField] protected PlasmaBolts projectilePrefab;
    [SerializeField] protected Transform muzzlePoint;
    protected WeaponAudio weaponAudio;

    protected override void Awake()
    {
        base.Awake();

        weaponAudio =
            GetComponent<WeaponAudio>();
    }

    public override bool Fire()
    {
        if(!base.Fire()) return false;

        PlasmaBolts projectile = Instantiate(
            projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);

        projectile.Intialize(weaponData.damage);
        projectile.SetFaction(Faction.Player);
        if (weaponAudio != null)
        {
            weaponAudio.PlayFire();
        }

        return true;
    }
    public override void StartFire()
    {
        if (IsEmpty())
        {
            if (weaponAudio != null)
            {
                weaponAudio.PlayDryFire();
            }

            return;
        }

        Fire();
    }
}
