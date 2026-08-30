using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class MotionTracker : MonoBehaviour
{
    [SerializeField] private float detectionRad = 25f;
    [SerializeField] private float velocityThreshold = 0.1f;

    private List<GameObject> trackedEnemies = new List<GameObject>();
    
    void Start()
    {
        
    }

    void Update()
    {
        UpdateTrackedEnemies();
    }

    void UpdateTrackedEnemies()
    {
        HashSet<GameObject> currentEnemies = new HashSet<GameObject>();

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRad);

        foreach (Collider hit in hits)
        {
            if(!hit.CompareTag("enemy"))
                continue;

            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if(rb != null && rb.linearVelocity.magnitude == 0)
            {
                currentEnemies.Add(hit.gameObject);
                if (!trackedEnemies.Contains(hit.gameObject))
                {
                    trackedEnemies.Add(hit.gameObject);
                    Debug.Log($"{hit.name} entered tracker");
                }
            }
        }

        trackedEnemies.RemoveAll(enemy =>
        {
            bool remove = !currentEnemies.Contains (enemy);
            if (remove)
                Debug.Log($"{enemy.name} was removed");
            return remove;
        });
    }

    public List<GameObject> GetTrackedEnemies()
    {
        return trackedEnemies;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRad);
    }
}

