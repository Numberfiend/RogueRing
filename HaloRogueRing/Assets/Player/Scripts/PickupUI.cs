using TMPro;
using UnityEngine;

public class PickupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text pickupText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string button, string weaponName)
    {
        pickupText.text = $"Press {button} to pick up {weaponName}";
        pickupText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        pickupText.gameObject.SetActive(false);
    }
}
