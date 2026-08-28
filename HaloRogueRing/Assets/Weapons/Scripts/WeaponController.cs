using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    private WeaponInventory inventory;

    private void Awake()
    {
        inventory = GetComponent<WeaponInventory>();
    }

    public void OnSwapWeapon(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inventory.SwapWeapon();
        }
    }
}
