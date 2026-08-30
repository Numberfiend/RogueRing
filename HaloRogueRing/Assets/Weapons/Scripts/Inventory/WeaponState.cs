using UnityEngine;
[System.Serializable]
public class WeaponState
{
    public int currentAmmo;
    public int reserveAmmo;
    public float batteryCharge;

    public WeaponState()
    {

    }

    public WeaponState(WeaponState other)
    {
        currentAmmo = other.currentAmmo;
        reserveAmmo = other.reserveAmmo;
        batteryCharge = other.batteryCharge;
    }
}
