using UnityEngine;

public class PlasmaBolts : MonoBehaviour
{
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

    private void Update()
    {
        transform.position +=
            transform.forward *
            speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        TempEnemyHealth health = other.GetComponent<TempEnemyHealth>();
        if (health != null) 
        {
            health.TakeDamage(damage);
        }
        Destroy(gameObject);

    }
}
