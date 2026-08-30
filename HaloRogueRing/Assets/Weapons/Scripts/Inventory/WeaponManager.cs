using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;

    private GameObject currentWeapon;

    public void EquipWeapon(GameObject weaponPrefab)
    {
        if(currentWeapon != null)
        {
            Destroy(currentWeapon);
        }
        currentWeapon = Instantiate(weaponPrefab, weaponHolder);

        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
    }
    
}
