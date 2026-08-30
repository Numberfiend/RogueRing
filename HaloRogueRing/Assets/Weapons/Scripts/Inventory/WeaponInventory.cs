using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private Transform weaponDropPoint;
    public GameObject[] weaponSlots = new GameObject[2];

    public int currentSlot = -1;

    public bool AddWeapon(GameObject weapon)
    {
        for (int i = 0; i <weaponSlots.Length; i++)
        {
            if(weaponSlots[i] == null)
            {
                weaponSlots[i] = weapon;

                EquipSlot(i);

                return true;
            }
        }

        if(currentSlot >= 0)
        {
            int slotToReplace = currentSlot;
            DropCurrentWeapon();
            weaponSlots[slotToReplace] = weapon;
            EquipSlot(slotToReplace);
            return true;
        }
        return false;
    }

    public void EquipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length)
            return;
        
        for(int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
            {
                weaponSlots[i].SetActive(false);
            }
        }

        weaponSlots[slotIndex].SetActive(true);
        currentSlot = slotIndex;
    }
    public void SwapWeapons()
    {
        if (weaponSlots[0] == null || weaponSlots[1] == null)
        {
            return;
        }

        if(currentSlot == 0)
        {
            EquipSlot(1);
        }
        else
        {
            EquipSlot(0);
        }
    }
    public GameObject DropCurrentWeapon()
    {
        if (currentSlot < 0)
            return null;

        GameObject equippedWeapon =
            weaponSlots[currentSlot];

        if (equippedWeapon == null)
            return null;

        Weapon weaponComponent =
            equippedWeapon.GetComponent<Weapon>();

        if (weaponComponent == null)
            return null;

        GameObject worldWeapon =
            Instantiate(
                weaponComponent.GetWeaponData().worldPrefab,
                weaponDropPoint.position,
                weaponDropPoint.rotation);

        WeaponPickup pickup =
            worldWeapon.GetComponent<WeaponPickup>();

        if (pickup != null)
        {
            pickup.SetWeaponState(
                weaponComponent.GetState());
        }

        Destroy(equippedWeapon);

        weaponSlots[currentSlot] = null;

        return worldWeapon;
    }
    public Weapon GetCurrentWeapon()
    {
        if(currentSlot < 0) return null;
        if(weaponSlots[currentSlot] == null) return null;
        return weaponSlots[currentSlot].GetComponent<Weapon>();
    }
}
