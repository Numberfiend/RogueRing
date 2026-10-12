using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerGrenades : MonoBehaviour
{
    [Header("Grenade Data")]
    [SerializeField] private GrenadeData grenadeData;

    [Header("Throwing")]
    [SerializeField] private Transform grenadeThrowPoint;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float upwardForce = 2f;

    [Header("Ammo")]
    [SerializeField] private int currentAmmo;

    private PlayerInput playerInput;
    private InputAction throwAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            throwAction = playerInput.actions["Throw"];

            if (throwAction != null)
                throwAction.performed += OnThrowPerformed;
            else
                Debug.LogError("PlayerGrenades: Throw action not found!", this);
        }
    }

    private void OnDisable()
    {
        if (throwAction != null)
            throwAction.performed -= OnThrowPerformed;
    }

    private void Start()
    {
        if (grenadeData == null)
        {
            Debug.LogError("PlayerGrenades: No Grenade Data assigned.", this);
            enabled = false;
            return;
        }

        currentAmmo = grenadeData.maxAmmo;

        Debug.Log("PlayerGrenades ready. Grenades: " + currentAmmo);
    }

    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("THROWiInG");
        ThrowGrenade();
    }

    private void ThrowGrenade()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("No grenades remaining!");
            return;
        }

        if (grenadeData.worldPrefab == null)
        {
            Debug.LogError("PlayerGrenades: No grenade prefab assigned.", this);
            return;
        }

        if (grenadeThrowPoint == null)
        {
            Debug.LogError("PlayerGrenades: No throw point assigned.", this);
            return;
        }

        GameObject grenadeObject = Instantiate(
            grenadeData.worldPrefab,
            grenadeThrowPoint.position,
            grenadeThrowPoint.rotation
        );

        Grenade grenade = grenadeObject.GetComponent<Grenade>();

        if (grenade == null)
        {
            Debug.LogError("The grenade prefab has no Grenade component.", grenadeObject);
            Destroy(grenadeObject);
            return;
        }

        grenade.Initialise(grenadeData);

        Vector3 throwDirection =
            grenadeThrowPoint.forward +
            Vector3.up * (upwardForce / throwForce);

        throwDirection.Normalize();

        grenade.Throw(throwDirection * throwForce);

        currentAmmo--;

        Debug.Log("Grenades remaining: " + currentAmmo);
    }

}
