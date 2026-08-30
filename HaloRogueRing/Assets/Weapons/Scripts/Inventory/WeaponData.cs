using System.Runtime.CompilerServices;
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
    public AmmoType ammoType;
    public float batteryCost;

    [Header("Fire")]
    public float firerate;

    [Header("PlasmaValues")]
    public float maxBatteryCharge;

    [Header("Spread")]
    public float minSpread = 0f;
    public float maxSpread = 0f;
    public float spreadIncreasePerShot = 0f;
    public float spreadRecoverySpeed = 0f;

    [Header("WorldPrefab")]
    public GameObject worldPrefab;
}

