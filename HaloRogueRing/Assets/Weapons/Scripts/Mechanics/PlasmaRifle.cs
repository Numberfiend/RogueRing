using UnityEngine;

public class PlasmaRifle : ProjectileWeapon
{
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float heatperShot = 5f;
    [SerializeField] private float cooldownRate = 20f;

    private float currentHeat;
    private bool overheated;

    private void Update()
    {
        if(currentHeat > 0)
        {
            currentHeat -=
                cooldownRate *
                Time.deltaTime;
            currentHeat = Mathf.Max(currentHeat, 0);
        }

        if(overheated && currentHeat <= 0)
        {
            overheated = false;

            Debug.Log("Plasma Rifle cooled");
        }
    }

    public override void ContinueFire()
    {
        if (overheated) return;
        if (Fire())
        {
            currentHeat += heatperShot;

            if(currentHeat >= maxHeat)
            {
                currentHeat = maxHeat;
                overheated = true;
                Debug.Log("OVERHEAT");
                if(weaponAudio != null)
                {
                    weaponAudio.PlayOverheat();
                }
            }
        }
    }
}
