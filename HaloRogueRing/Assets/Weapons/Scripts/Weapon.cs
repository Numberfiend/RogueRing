using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponData weaponData;

    protected WeaponState state;

    protected Camera playerCamera;

    protected float nextFireTime;

    protected virtual void Awake()
    {
        state = new WeaponState();
        state.currentAmmo = weaponData.magazineSize;
        state.reserveAmmo = weaponData.maxReserveAmmo;
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

    public virtual void Fire()
    {
        if (Time.time < nextFireTime)
            return;

        if (state.currentAmmo <= 0)
            return;

        state.currentAmmo--;

        nextFireTime =
            Time.time +
            (1f / weaponData.firerate);
        Debug.Log("Ammo: " + state.currentAmmo);
        if (playerCamera == null)
            return;

        if (Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out RaycastHit hit,
            weaponData.range))
        {
            TempEnemyHealth health =
                hit.collider.GetComponent<TempEnemyHealth>();

            if (health != null)
            {
                health.TakeDamage(
                    weaponData.damage);
            }
        }
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
}
