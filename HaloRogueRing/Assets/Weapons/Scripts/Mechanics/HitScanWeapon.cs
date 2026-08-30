using UnityEngine;

public class HitScanWeapon : Weapon
{
    public override bool Fire()
    {
        if (!base.Fire())
            return false;
        if (playerCamera == null)
            return false;

        if(Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out RaycastHit hit,
            weaponData.range))
        {
            TempEnemyHealth health = hit.collider.GetComponent<TempEnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(weaponData.damage);
            }
        }
        return true;
    }
}
