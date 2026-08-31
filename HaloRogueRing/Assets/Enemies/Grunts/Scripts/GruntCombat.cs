using UnityEngine;

public class GruntCombat : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    private float weaponDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetWeaponDamage();   
    }

    private void SetWeaponDamage()
    {
        if(enemyData == null)
        {
            Debug.Log("No EnemyData assigned");
        }
        if(enemyData.startingWeapon.weaponName == "PlasmaPistol")
        {
            weaponDamage = Random.Range(5f, 10f);
        }
        else if (enemyData.startingWeapon.weaponName == "Needler")
        {
            weaponDamage = 5f;
        }
    }
    public float GetWeaponDamage()
    {
        return weaponDamage;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
