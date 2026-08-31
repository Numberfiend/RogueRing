using System.Collections.Generic;
using UnityEngine;

public class NeedlerTarget : MonoBehaviour
{
    [Header("Supercombine")]
    [SerializeField] private int supercombineThreshold = 7;
    [SerializeField] private float supercombineWindow = 0.75f;

    [Header("Explosion Damage")]
    [SerializeField] private float needleExplosionDamage = 10f;
    [SerializeField] private float minimumExplosionDamage = 40f;
    [SerializeField] private float maximumExplosionDamage = 60f;

    private float combineTimer;

    private List<Needles> embeddedNeedles =
        new List<Needles>();

    private void Update()
    {
        if (embeddedNeedles.Count == 0)
            return;

        combineTimer -= Time.deltaTime;

        if (combineTimer <= 0f)
        {
            NeedleTimeout();
        }
    }

    public void AddNeedle(Needles needle)
    {
        if (needle == null)
            return;

        if (!embeddedNeedles.Contains(needle))
        {
            embeddedNeedles.Add(needle);
        }

        Debug.Log(
            gameObject.name +
            " has " +
            embeddedNeedles.Count +
            " needles"
        );

        combineTimer = supercombineWindow;

        if (embeddedNeedles.Count >= supercombineThreshold)
        {
            Supercombine();
        }
    }

    private void NeedleTimeout()
    {
        Debug.Log(
            gameObject.name +
            " failed to supercombine."
        );

        foreach (Needles needle in embeddedNeedles)
        {
            if (needle != null)
            {
                DealDamage(needleExplosionDamage);

                Destroy(needle.gameObject);
            }
        }

        embeddedNeedles.Clear();

        combineTimer = 0f;
    }

    private void Supercombine()
    {
        float explosionDamage =
            Random.Range(
                minimumExplosionDamage,
                maximumExplosionDamage
            );

        Debug.Log(
            gameObject.name +
            " SUPERCOMBINE! Damage: " +
            explosionDamage
        );

        DealDamage(explosionDamage);

        foreach (Needles needle in embeddedNeedles)
        {
            if (needle != null)
            {
                Destroy(needle.gameObject);
            }
        }

        embeddedNeedles.Clear();

        combineTimer = 0f;
    }

    private void DealDamage(float damage)
    {
        EnemyHealth enemyHealth =
            GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            return;
        }

        PlayerHealth playerHealth =
            GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}