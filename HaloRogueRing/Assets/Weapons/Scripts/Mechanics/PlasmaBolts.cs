using UnityEngine;

public class PlasmaBolts : MonoBehaviour
{
    [SerializeField] private Faction faction;
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 5f;

    private float damage;

    public void Intialize(float projectileDamage)
    {
        damage = projectileDamage;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    public void SetFaction( Faction newFaction)
    {
        faction = newFaction;
    }
    private void Update()
    {
        transform.position +=
            transform.forward *
            speed * Time.deltaTime;
    }

 

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            "Plasma bolt hit: " +
            other.gameObject.name +
            " | Faction: " +
            faction +
            " | Damage: " +
            damage
        );
        if (faction == Faction.Player)
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }
        else if(faction == Faction.Covenant)
        {
           
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
            if(playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
        
        Destroy(gameObject);

    }
}
