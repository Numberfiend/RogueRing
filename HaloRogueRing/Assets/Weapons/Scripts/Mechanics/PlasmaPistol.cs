using UnityEngine;

public class PlasmaPistol : ProjectileWeapon
{
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float chargedDamageMultiplier = 7f;
    [SerializeField] private float chargedBatteryCost = 11f;

    private float chargeTimer;
    private bool charging;

    public override void StartFire()
    {
        if (Time.time < nextFireTime) return;
        if (state.batteryCharge < weaponData.batteryCost)
        {
            if(weaponAudio != null)
            {
                weaponAudio.PlayDryFire();
            }
        }
        charging = true;
        chargeTimer = 0f;
        if (weaponAudio != null)
        {
            weaponAudio.StartCharge();
        }
        Debug.Log("PlasmaPsitol charging...");
    }
    public override void ContinueFire()
    {
        if (!charging) return;
        chargeTimer += Time.deltaTime;

        if(chargeTimer >= maxChargeTime)
        {
            chargeTimer = maxChargeTime;
        }
    }

    public override void ReleaseFire()
    {
        if(!charging) return;
        charging = false;

        if (weaponAudio != null)
        {
            weaponAudio.StopCharge();
        }

        if (chargeTimer >= maxChargeTime)
        {
            FireChargedShot();
        }
        else
        {
            FireNormalShot();
        }
        chargeTimer = 0f;
    }

    private void FireNormalShot()
    {
        base.Fire();
    }
    private void FireChargedShot()
    {
        if (Time.time < nextFireTime) return;
        if(state.batteryCharge < chargedBatteryCost)
        {
            Debug.Log("Not enough battery");
            return;
        }

        state.batteryCharge -= chargedBatteryCost;

        nextFireTime = Time.time + (1f / weaponData.firerate);

        PlasmaBolts projectile =
            Instantiate(
                projectilePrefab,
                muzzlePoint.position,
                muzzlePoint.rotation);
        projectile.Intialize(
            weaponData.damage *
            chargedDamageMultiplier);

        if (weaponAudio != null)
        {
            weaponAudio.FireCharge();
        }
        Debug.Log("Plasma Pistol Cahrged Shot!" + state.batteryCharge);
    }
}
