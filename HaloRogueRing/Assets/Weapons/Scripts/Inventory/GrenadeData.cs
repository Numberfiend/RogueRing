using UnityEngine;
public enum GrenadeType
{
    HumanFrag,
    Plasma
}
[CreateAssetMenu(fileName = "New Grenade Data", menuName = "Grenades/GrenadeData")]

public class GrenadeData : ScriptableObject
{
    [Header("General")]
    public string grenadeName;
    public GrenadeType grenadeType;

    [Header("Combat")]
    public float minDamage = 80f;
    public float maxDamage = 120f;
    public float explosionRadius = 5f;

    [Header("Ammo")]
    public int maxAmmo = 4;

    [Header("Throwing")]
    public float throwForce = 15f;

    [Header("Fuse")]
    public float minTimeAfterThrow = 1.5f;
    public float timeAfterRest = 0.5f;
    public float plasmaFuseTime = 3f;
    
    public GameObject worldPrefab;
}

