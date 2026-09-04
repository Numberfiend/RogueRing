using UnityEngine;

public class WeaponBob : MonoBehaviour
{
    [SerializeField] private float kickbackDistance = 0.05f;
    [SerializeField] private float recoverySpeed = 8f;

    private Vector3 defaultPosition;
    private Vector3 currentOffset;

    private void Start()
    {
        defaultPosition = transform.localPosition;
    }

    private void Update()
    {
        currentOffset = Vector3.Lerp(
            currentOffset,
            Vector3.zero,
            recoverySpeed * Time.deltaTime);

        transform.localPosition =
            defaultPosition + currentOffset;
    }

    public void OnFire()
    { 
          Debug.Log("Weapon Bob Triggered");
    
        currentOffset += Vector3.back * kickbackDistance;
    }
}
