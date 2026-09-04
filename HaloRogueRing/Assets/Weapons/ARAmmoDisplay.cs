using TMPro;
using UnityEngine;

public class ARAmmoDisplay : MonoBehaviour
{
    [SerializeField] private Weapon weapon;
    [SerializeField] private TMP_Text ammoText;

    private void Update()
    {
        if (weapon == null)
            return;

        WeaponState state = weapon.GetState();

        ammoText.text = state.currentAmmo.ToString();
    }
}
