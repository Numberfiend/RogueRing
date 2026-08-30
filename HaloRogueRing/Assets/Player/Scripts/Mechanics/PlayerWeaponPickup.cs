using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerWeaponPickup : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Camera playerCamera;
    private WeaponPickup nearbyWeapon;
    private WeaponInventory inventory;

    private InputAction interactAction;
    private InputAction shootAction;
    private InputAction reloadAction;
    private InputAction swapWAction;
    private void Awake()
    {
        interactAction = GetComponent<PlayerInput>().actions["Interact"];
        shootAction = GetComponent<PlayerInput>().actions["Shoot"];
        reloadAction = GetComponent<PlayerInput>().actions["Reload"];
        swapWAction = GetComponent<PlayerInput>().actions["SwitchW"];
        inventory = GetComponent<WeaponInventory>();
    }

    private void OnEnable()
    {
        interactAction.Enable();
        shootAction.Enable();
        reloadAction.Enable();
        swapWAction.Enable();
    }
    private void OnDisable()
    {
        interactAction.Disable();
        shootAction.Disable();
        reloadAction.Disable();
        swapWAction.Disable();
    }

    private void Update()
    {
        if (swapWAction.WasPressedThisFrame())
        {
            inventory.SwapWeapons();
        }
        if (interactAction.WasPressedThisFrame())
        {
            TryPickupWeapon();
        }

        if (shootAction.WasPressedThisFrame())
        {
           StartFire();
        }
        if (shootAction.IsPressed())
        {
            ContinueFire();
        }
        if (shootAction.WasReleasedThisFrame())
        {
            ReleaseFire();
        }

       
        if (reloadAction.WasPressedThisFrame())
        {
            TryReload();
        }
    }

    private void TryReload()
    {
        Weapon weapon = weaponHolder.GetComponentInChildren<Weapon>();
        if (weapon == null)
            return;
        weapon.Reload();
    }
    /*private void TryShoot()
    {
        Weapon weapon = weaponHolder.GetComponentInChildren<Weapon>();
        if (weapon == null)
            return;
        weapon.Fire();
    }*/
    private void StartFire()
    {
        Weapon weapon = weaponHolder.GetComponentInChildren<Weapon>();
        if (weapon == null) return;
        weapon.StartFire();
    }
    private void ContinueFire()
    {
        Weapon weapon = weaponHolder.GetComponentInChildren<Weapon>();
        if (weapon == null) return;
        weapon.ContinueFire();
    }
    private void ReleaseFire()
    {
        Weapon weapon = weaponHolder.GetComponentInChildren<Weapon>();
        if (weapon == null) return;
        weapon.ReleaseFire();
    }


    private void TryPickupWeapon()
    {
        if (nearbyWeapon == null)
            return;

        GameObject weaponObject =
    Instantiate(
        nearbyWeapon.equippedPrefab,
        weaponHolder);

       // weaponObject.transform.localPosition =
          ///  Vector3.zero;

       // weaponObject.transform.localRotation =
          //  Quaternion.identity;

        Weapon weaponComponent = weaponObject.GetComponent<Weapon>();
        if (weaponComponent != null)
        {
            weaponComponent.SetCamera(playerCamera);

            if (nearbyWeapon.HasStoredState())
            {
                weaponComponent.SetState(nearbyWeapon.GetWeaponState());
            }
            
        }

        

        bool added =
            inventory.AddWeapon(weaponObject);

        if (!added)
        {
            Destroy(weaponObject);

            Debug.Log("Inventory Full");

            return;
        }

        Destroy(nearbyWeapon.gameObject);

        nearbyWeapon = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        WeaponPickup weapon = other.GetComponent<WeaponPickup>();

        if(weapon != null)
        {
            nearbyWeapon = weapon;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WeaponPickup weapon = other.GetComponent<WeaponPickup>();

        if(weapon != null && weapon == nearbyWeapon)
        {
            nearbyWeapon = null;
        }
    }

    
}
