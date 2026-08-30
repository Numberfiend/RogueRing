using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponData weaponData;

    protected float currentSpread;
    
    protected WeaponState state;

    protected Camera playerCamera;

    protected PlayerMovement playerMovement;

    protected float nextFireTime;

    protected virtual void Awake()
    {
        state = new WeaponState();
        state.currentAmmo = weaponData.magazineSize;
        state.reserveAmmo = weaponData.maxReserveAmmo;
        state.batteryCharge = weaponData.maxBatteryCharge;
        currentSpread = weaponData.minSpread;
    }

    public void SetCamera(Camera camera)
    {
        playerCamera = camera;
    }
    public WeaponData GetWeaponData()
    {
        return weaponData;
    }

    public WeaponState GetState()
    {
        return new WeaponState(state);
    }
    public void SetState(WeaponState newState)
    {
        state = new WeaponState(newState);
    }

    protected virtual void Update()
    {
        currentSpread = Mathf.MoveTowards(
            currentSpread, weaponData.minSpread,
            weaponData.spreadRecoverySpeed * Time.deltaTime);
    }

    public virtual bool Fire()
    {
        // Fire-rate check
        if (Time.time < nextFireTime)
            return false;

        // Magazine weapon
        if (weaponData.ammoType == AmmoType.Magazine)
        {
            if (state.currentAmmo <= 0)
                return false;

            state.currentAmmo--;
        }
        // Battery weapon
        else
        {
            if (state.batteryCharge < weaponData.batteryCost)
                return false;

            state.batteryCharge -= weaponData.batteryCost;
        }

        // Debug ammo
        if (weaponData.ammoType == AmmoType.Magazine)
        {
            Debug.Log(
                weaponData.weaponName +
                " Ammo: " +
                state.currentAmmo);
        }
        else
        {
            Debug.Log(
                weaponData.weaponName +
                " Battery: " +
                state.batteryCharge);
        }

        // Set next allowed fire time
        nextFireTime =
            Time.time +
            (1f / weaponData.firerate);

        currentSpread += weaponData.spreadIncreasePerShot;

        currentSpread = Mathf.Min(
            currentSpread,
            weaponData.maxSpread
        );


        return true;
    }
    public virtual void Reload()
    {
        if (state.currentAmmo >= weaponData.magazineSize) return;
        if(state.reserveAmmo <= 0) return;

        int ammoNeeded =
            weaponData.magazineSize -
            state.currentAmmo;

        int ammoToLoad =
            Mathf.Min(
                ammoNeeded,
                state.reserveAmmo);
        state.currentAmmo += ammoToLoad;
        state.reserveAmmo -= ammoToLoad;
        Debug.Log("Reloaded. Magazine: " +
                  state.currentAmmo +
                  "Reserve: " +
                  state.reserveAmmo);
    }

    public virtual void StartFire()
    {

    }
    public virtual void ContinueFire()
    {
        Fire();
    }
    public virtual void ReleaseFire()
    {

    } 
    
}
