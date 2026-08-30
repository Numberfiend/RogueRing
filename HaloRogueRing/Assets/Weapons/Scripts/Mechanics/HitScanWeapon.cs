using System.Runtime.CompilerServices;
using UnityEngine;

public class HitScanWeapon : Weapon
{
    [SerializeField] private float aimDistance = 1000f;
    public override bool Fire()
    {
        if (!base.Fire())
            return false;

        if (playerCamera == null)
            return false;

        Vector3 shotDirection =
            GetShotDirection();

        Debug.DrawRay(
            playerCamera.transform.position,
            shotDirection * weaponData.range,
            Color.red,
            60f
        );

        if (Physics.Raycast(
            playerCamera.transform.position,
            shotDirection,
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

        return true;
    }

    protected virtual Vector3 GetShotDirection()
    {
        Vector3 direction = playerCamera.transform.forward;
        direction += Random.insideUnitSphere * currentSpread;
        return direction.normalized;
    }
}
