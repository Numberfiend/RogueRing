using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class CommonEnemyAudio : MonoBehaviour
{
    [Header("Common enemy sounds")]
    [SerializeField] private EventReference deathEvent;
    [SerializeField] private EventReference damageEvent;

    private float nextDamageSoundTime;
    private float damageSoundCooldown = 0.75f;
    [SerializeField] private float damageSoundChance = 0.3f;
    public void OnDamage()
    {
        if (Time.time < nextDamageSoundTime)
            return;

        if (Random.value > damageSoundChance)
            return;

        nextDamageSoundTime =
            Time.time + damageSoundCooldown;

        RuntimeManager.PlayOneShot(
            damageEvent,
            transform.position
        );
    }

    public void OnDeath()
    {
        if (!deathEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(
                deathEvent,
                transform.position
            );
        }
    }
}
