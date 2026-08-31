using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemies/EnemyData")]
public class EnemyData: ScriptableObject
{
    [Header("Name")]
    public string enemyName;

    [Header("Stats")]
    public float maxHealth;
    public float maxShield;

    public WeaponData startingWeapon;
}