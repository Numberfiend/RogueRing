using UnityEngine;
using System;
public class WeaponInstance
{
    public WeaponData data;

    public int currentMagazine;
    public int reserveAmmo;

    public float battery;

    public WeaponInstance(WeaponData weapon)
    {
        data = weapon;

        currentMagazine = weapon.magazineSize;
        reserveAmmo = weapon.maxReserveAmmo;

        battery = weapon.maxBattery;
    }
}

