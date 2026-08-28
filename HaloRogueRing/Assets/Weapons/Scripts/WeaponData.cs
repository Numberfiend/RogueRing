using UnityEngine;

[CreateAssetMenu(menuName = "Halo/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;

    public bool usesBatttery;

    public int magazineSize;
    public int maxReserveAmmo;

    public float maxBattery = 100f;

    public float fireRate;
    public float damage;

    public GameObject worldPrefab;
    public GameObject viewModelPrefab;
}

