using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class AssaultRifle : MonoBehaviour
{
    [SerializeField]private WeaponData weaponData;
    [SerializeField] private Camera playerCamera;

    private WeaponState state;
    private float nextFireTime;
    private void Awake()
    {
        state = new WeaponState();
        state.currentAmmo = weaponData.magazineSize;
        state.reserveAmmo = weaponData.maxReserveAmmo;
    }
    public void SetCamera(Camera camera)
    {
        playerCamera = camera;
    }
    public void Fire()
    {
        if (Time.time < nextFireTime)
            return;
        if (state.currentAmmo <= 0)
            return;
        state.currentAmmo--;
        nextFireTime = Time.time + (1f/weaponData.firerate);
        
        Debug.Log("Ammo: " + state.currentAmmo);
        
        if (Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out RaycastHit hit,
            weaponData.range))
        {
            Debug.Log("Hit: " + hit.collider.name);
        }
    }

    public void Reload()
    {
        if (state.currentAmmo >= weaponData.magazineSize)
            return;
        if (state.reserveAmmo <= 0)
            return;
        int ammoNeeded = weaponData.magazineSize - state.currentAmmo;

        int ammoToLoad = Mathf.Min(ammoNeeded, state.reserveAmmo);

        state.currentAmmo += ammoToLoad;
        state.reserveAmmo -= ammoToLoad;

        Debug.Log("Reloaded. Magazine: "  +
                  state.currentAmmo +
                  "Reserve: " + 
                  state.reserveAmmo);
    }
}
