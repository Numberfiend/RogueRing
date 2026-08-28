using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerWeaponPickup : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Camera playerCamera;
    private WeaponPickup nearbyWeapon;

    private InputAction interactAction;
    private InputAction shootAction;
    private InputAction reloadAction;
    private void Awake()
    {
        interactAction = GetComponent<PlayerInput>().actions["Interact"];
        shootAction = GetComponent<PlayerInput>().actions["Shoot"];
        reloadAction = GetComponent<PlayerInput>().actions["Reload"];
    }

    private void OnEnable()
    {
        interactAction.Enable();
        shootAction.Enable();
        reloadAction.Enable();
    }
    private void OnDisable()
    {
        interactAction.Disable();
        shootAction.Disable();
        reloadAction.Disable();
    }

    private void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            TryPickupWeapon();
        }

        if (shootAction.IsPressed()) 
        {
            TryShoot();
        }

        if (reloadAction.WasPressedThisFrame())
        {
            TryReload();
        }
    }

    private void TryReload()
    {
        AssaultRifle rifle = weaponHolder.GetComponentInChildren<AssaultRifle>();
        if (rifle == null)
            return;
        rifle.Reload();
    }
    private void TryShoot()
    {
        AssaultRifle rifle = weaponHolder.GetComponentInChildren<AssaultRifle>();
        if (rifle == null)
            return;
        rifle.Fire();
    }

    private void TryPickupWeapon()
    {
        if (nearbyWeapon == null)
            return;

        GameObject weapon = Instantiate(
            nearbyWeapon.equippedPrefab,
            weaponHolder);

        AssaultRifle rifle = weapon.GetComponent<AssaultRifle>();
        if (rifle != null)
        {
            rifle.SetCamera(playerCamera);
        }

        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

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
