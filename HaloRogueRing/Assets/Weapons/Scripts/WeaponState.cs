using UnityEngine;
[System.Serializable]
public class WeaponState
{
    public int currentAmmo;
    public int reserveAmmo;

    public WeaponState()
    {

    }

    public WeaponState(WeaponState other)
    {
        currentAmmo = other.currentAmmo;
        reserveAmmo = other.reserveAmmo;
    }
}
