using FMODUnity;
using FMOD.Studio;
using UnityEngine;

public class TempPlayerAudio : MonoBehaviour
{
    private EventInstance shieldDepleted;
    private EventInstance shieldLow;
    private EventInstance shieldRecharge;

    private void Start()
    {
        shieldDepleted = RuntimeManager.CreateInstance(
            "event:/ShieldEvents/ShieldDepleted"
        );

        shieldLow = RuntimeManager.CreateInstance(
            "event:/ShieldEvents/ShieldLow"
        );

        shieldRecharge = RuntimeManager.CreateInstance(
            "event:/ShieldEvents/ShieldCharging"
        );
    }

    public void PlayShieldDepleted()
    {
        // Stop any recharge sounds
        shieldLow.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        shieldRecharge.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        // Start depleted loop
        shieldDepleted.start();
    }

    public void PlayShieldLow()
    {
        // Stop depleted sound
        shieldDepleted.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        // Start low shield loop
        shieldLow.start();
    }

    public void PlayShieldRecharge()
    {
        // Stop low shield sound
        shieldLow.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        // Start charging loop
        shieldRecharge.start();
    }

    public void StopShieldAudio()
    {
        shieldDepleted.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        shieldLow.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        shieldRecharge.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    private void OnDestroy()
    {
        shieldDepleted.release();
        shieldLow.release();
        shieldRecharge.release();
    }
}
