using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class AmmoUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponInventory weaponInventory;

    [Header("Magazine UI")]
    [SerializeField] private GameObject magazineUI;
    [SerializeField] private TMP_Text magazineText;

    [Header("Battery UI")]
    [SerializeField] private GameObject batteryUI;
    [SerializeField] private TMP_Text batteryText;
    [SerializeField] private Slider batterySlider;

    private void Update()
    {
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        Weapon weapon = weaponInventory.GetCurrentWeapon();

        if (weapon == null)
        {
            magazineUI.SetActive(false);
            batteryUI.SetActive(false);
            return;
        }

        WeaponData data = weapon.GetWeaponData();
        WeaponState state = weapon.GetState();

        // Magazine weapon
        if (data.ammoType == AmmoType.Magazine)
        {
            magazineUI.SetActive(true);
            batteryUI.SetActive(false);

            magazineText.text =
                state.currentAmmo +
                " / " +
                state.reserveAmmo;
        }

        // Battery weapon
        else
        {
            magazineUI.SetActive(false);
            batteryUI.SetActive(true);

            float batteryPercentage =
        (state.batteryCharge / data.maxBatteryCharge) * 100f;

            // Display whole-number percentage
            batteryText.text =
                Mathf.CeilToInt(batteryPercentage) + "%";

            // Update slider
            batterySlider.value = batteryPercentage;
        }
    }
}
