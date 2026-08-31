using UnityEngine;

public class Needler : Weapon
{
    [SerializeField] private Needles projectilePrefab;
    [SerializeField] private Transform muzzlePoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override bool Fire()
    {
        if(!base.Fire()) return false;
        if(projectilePrefab == null) return false;
        if(muzzlePoint == null) return false;
        Needles projectile =
            Instantiate(
                projectilePrefab,
                muzzlePoint.position,
                muzzlePoint.rotation
                );
        projectile.Initialize(weaponData.damage, Faction.Player);
        return true;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
