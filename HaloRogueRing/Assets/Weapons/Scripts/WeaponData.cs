using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("General")]
    public string weaponName;

    [Header("Combat")]
    public float damage;
    public float range;

    [Header("Ammo")]
    public int magazineSize;
    public int maxReserveAmmo;

    [Header("Fire")]
    public float firerate;
}

