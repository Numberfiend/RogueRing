using UnityEngine;

public class ProjectileWeapon : Weapon
{
    [SerializeField] protected PlasmaBolts projectilePrefab;
    [SerializeField] protected Transform muzzlePoint;

    public override bool Fire()
    {
        if(!base.Fire()) return false;

        PlasmaBolts projectile = Instantiate(
            projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);

        projectile.Intialize(weaponData.damage);
        projectile.SetFaction(Faction.Player);
        return true;
    }
}
