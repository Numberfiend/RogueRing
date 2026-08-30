using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class AssaultRifle : HitScanWeapon
{
    [Header("Spread")]
    [SerializeField] private float minSpread = 0.005f;
    [SerializeField] private float maxSpread = 0.05f;
    [SerializeField] private float spreadIncreasePerShot = 0.003f;
    [SerializeField] private float spreadRecoverySpeed = 0.02f;

    [Header("Recoil")]
    [SerializeField] private float recoilAmount = 1f;
    [SerializeField] private float recoilRecoverySpeed = 8f;

    private float currentSpread;
    private float currentRecoil;
    private void Start()
    {
        currentSpread = minSpread;
    }

    private void Update()
    {
        currentSpread = Mathf.MoveTowards(
            currentSpread,
            minSpread,
            spreadRecoverySpeed * Time.deltaTime
        );
    }

    public override bool Fire()
    {
        if (!base.Fire())
            return false;

        currentSpread += spreadIncreasePerShot;

        currentSpread =
            Mathf.Min(
                currentSpread,
                maxSpread
            );
        currentRecoil += recoilAmount;
        return true;
    }

    protected override Vector3 GetShotDirection()
    {
        Vector3 direction =
            playerCamera.transform.forward;

        direction +=
            Random.insideUnitSphere *
            currentSpread;

        return direction.normalized;
    }

    public float Currentrecoil => currentRecoil;
}

