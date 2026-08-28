using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public bool playerNear { get; private set; }
    public GameObject equippedPrefab;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            Debug.Log("Player is near weapon");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            Debug.Log("Player left weapon");
        }
    }
}
