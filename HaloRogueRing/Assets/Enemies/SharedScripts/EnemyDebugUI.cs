using TMPro;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class EnemyDebugUI : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private TextMeshProUGUI debugText;

    private Camera playerCam;
    private void Start()
    {
        playerCam = Camera.main;
    }

    private void Update()
    {
        if (enemyHealth == null) return;
        UpdateText();
        FaceCamera();
    }
    private void UpdateText()
    {
        EnemyData data = enemyHealth.GetEnemyData();

        debugText.text =
            data.enemyName +
            "\nShield: " +
            enemyHealth.CurrentShield +
            " / " +
            data.maxShield +
            "\nHP: " +
            enemyHealth.CurrentHealth +
            " / " +
            data.maxHealth;
    }

    private void FaceCamera()
    {
        if(playerCam == null) return;

        transform.LookAt(
            transform.position +
            playerCam.transform.rotation * Vector3.forward,
            playerCam.transform.rotation * Vector3.up
        );
    }
}
