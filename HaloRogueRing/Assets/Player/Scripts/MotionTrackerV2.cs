using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MotionTrackerV2 : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRad = 25f;
    [SerializeField] private float movementThreshold = 0.1f;

    [Header("Radar")]
    [SerializeField] private float radarInterval = 5f;

    public List<Transform> trackedEnemies { get; private set; } = new();
    
    void Start()
    {
        StartCoroutine(RadarScan());
    }

    private IEnumerator RadarScan()
    {
        while (true)
        {
            ScanEnemies();
            yield return new WaitForSeconds(radarInterval);
        }
    }

    private void ScanEnemies()
    {
        HashSet<Transform> detectedEnemies = new();

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRad);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("enemy"))
                continue;
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == null)
                continue;
            float speed = rb.linearVelocity.magnitude;

            if(speed == movementThreshold)
            {
                detectedEnemies.Add(hit.transform);
                if (!trackedEnemies.Contains(hit.transform))
                {
                    trackedEnemies.Add(hit.transform);
                    Debug.Log($"{hit.name} on radar");
                }
            }
        }

        trackedEnemies.RemoveAll(enemy =>
        {
            bool remove = !detectedEnemies.Contains(enemy);
            if (remove)
                Debug.Log($"{enemy.name} removed from radar");
            return remove;
        });
    }

    public List<Transform> GetTrackedEnemies()
    {
        return trackedEnemies;
    }

    public float DetectionRadius => detectionRad;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRad);
    }
    void Update()
    {
        
    }
}
