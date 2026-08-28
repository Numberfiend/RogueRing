using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    public WeaponInstance primaryWeapon;
    public WeaponInstance secondaryWeapon;

    public int currentSlot;

    public WeaponInstance CurrentWeapon
    {
        get
        {
            return currentSlot == 0
                ? primaryWeapon
                : secondaryWeapon;
        }
    }
    public void SwapWeapon()
    {
        currentSlot = 1 - currentSlot;
    }
}
