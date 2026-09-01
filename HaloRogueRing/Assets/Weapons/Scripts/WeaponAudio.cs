using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;

public class WeaponAudio : MonoBehaviour
{
    [Header("Common weapon sounds")]
    [SerializeField] private EventReference fireEvent;
    [SerializeField] private EventReference reloadEvent;
    [SerializeField] private EventReference dryFireEvent;

    [Header("Plasma Rifle Unique Sounds")]
    [SerializeField] private EventReference overheatEvent;

    [Header("Plasma Pistol Unique Sounds")]
    [SerializeField] private EventReference chargeEvent;
    [SerializeField] private EventReference chargeShot;
    private EventInstance chargeInstance;

    public void PlayFire()
    {
        if (!fireEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(
                fireEvent,
                transform.position
            );
        }
    }

    public void PlayReload()
    {
        if (!reloadEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(
                reloadEvent,
                transform.position
            );
        }
    }

    public void PlayDryFire()
    {
        if (!dryFireEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(
                dryFireEvent,
                transform.position
            );
        }
    }

    public void PlayOverheat()
    {
        if (!overheatEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(
                overheatEvent, transform.position);
        }
    }

    public void FireCharge()
    {
        if (!chargeShot.IsNull)
        {
            RuntimeManager.PlayOneShot(chargeShot, transform.position );
        }
    }

    public void StartCharge()
    {
        if (chargeEvent.IsNull)
            return;

        // Create the instance
        chargeInstance =
            RuntimeManager.CreateInstance(chargeEvent);

        // Attach it to the weapon
        RuntimeManager.AttachInstanceToGameObject(
            chargeInstance,
            gameObject
        );

        chargeInstance.start();
    }

    public void StopCharge()
    {
        if (!chargeInstance.isValid())
            return;

        chargeInstance.stop(
            FMOD.Studio.STOP_MODE.IMMEDIATE
        );

        chargeInstance.release();
    }


    private void OnDestroy()
    {
        StopCharge();
    }
}