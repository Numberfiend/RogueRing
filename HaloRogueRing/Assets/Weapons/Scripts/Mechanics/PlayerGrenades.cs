using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerGrenades : MonoBehaviour
{
    [Header("Grenade Data")]
    [SerializeField] private GrenadeData humanFragData;
    [SerializeField] private GrenadeData plasmaGrenadeData;

    [Header("Throwing")]
    [SerializeField] private Transform grenadeThrowPoint;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float upwardForce = 2f;

    [Header("Starting Ammo")]
    [SerializeField] private int startingHumanFragAmmo = 4;
    [SerializeField] private int startingPlasmaAmmo = 4;

    private int currentHumanFragAmmo;
    private int currentPlasmaAmmo;

    private GrenadeData currentGrenadeData;
    private bool usingPlasmaGrenade;

    private PlayerInput playerInput;
    private InputAction throwAction;
    private InputAction switchGrenadeAction;

    private int CurrentAmmo
    {
        get
        {
            return usingPlasmaGrenade
                ? currentPlasmaAmmo
                : currentHumanFragAmmo;
        }
        set
        {
            if (usingPlasmaGrenade)
                currentPlasmaAmmo = value;
            else
                currentHumanFragAmmo = value;
        }
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
            return;

        throwAction = playerInput.actions["Throw"];
        switchGrenadeAction = playerInput.actions["SwitchG"];

        if (throwAction != null)
            throwAction.performed += OnThrowPerformed;
        else
            Debug.LogError("PlayerGrenades: Throw action not found!", this);

        if (switchGrenadeAction != null)
            switchGrenadeAction.performed += OnSwitchGrenadePerformed;
        else
            Debug.LogError("PlayerGrenades: SwitchGrenade action not found!", this);
    }

    private void OnDisable()
    {
        if (throwAction != null)
            throwAction.performed -= OnThrowPerformed;

        if (switchGrenadeAction != null)
            switchGrenadeAction.performed -= OnSwitchGrenadePerformed;
    }

    private void Start()
    {
        currentHumanFragAmmo = Mathf.Max(0, startingHumanFragAmmo);
        currentPlasmaAmmo = Mathf.Max(0, startingPlasmaAmmo);

        SelectGrenadeType();

        Debug.Log(
            "Grenade inventory ready. M9: " +
            currentHumanFragAmmo +
            " | Plasma: " +
            currentPlasmaAmmo
        );
    }

    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        ThrowGrenade();
    }

    private void OnSwitchGrenadePerformed(InputAction.CallbackContext context)
    {
        usingPlasmaGrenade = !usingPlasmaGrenade;

        SelectGrenadeType();

        Debug.Log(
            "Selected grenade: " +
            currentGrenadeData.grenadeName +
            " | Ammo: " +
            CurrentAmmo
        );
    }

    private void SelectGrenadeType()
    {
        currentGrenadeData = usingPlasmaGrenade
            ? plasmaGrenadeData
            : humanFragData;

        if (currentGrenadeData == null)
        {
            Debug.LogError("PlayerGrenades: Selected grenade data is missing.", this);
        }
    }

    private void ThrowGrenade()
    {
        if (currentGrenadeData == null)
        {
            Debug.LogWarning("No grenade data selected.");
            return;
        }

        if (CurrentAmmo <= 0)
        {
            Debug.Log("No grenades of this type remaining!");
            return;
        }

        if (currentGrenadeData.worldPrefab == null)
        {
            Debug.LogError("Selected grenade has no World Prefab.", this);
            return;
        }

        if (grenadeThrowPoint == null)
        {
            Debug.LogError("No Grenade Throw Point assigned.", this);
            return;
        }

        GameObject grenadeObject = Instantiate(
            currentGrenadeData.worldPrefab,
            grenadeThrowPoint.position,
            grenadeThrowPoint.rotation
        );

        Grenade grenade = grenadeObject.GetComponent<Grenade>();

        if (grenade == null)
        {
            Debug.LogError("Grenade prefab has no Grenade component.", grenadeObject);
            Destroy(grenadeObject);
            return;
        }

        grenade.Initialise(currentGrenadeData);

        Vector3 throwDirection =
            grenadeThrowPoint.forward +
            Vector3.up * (upwardForce / throwForce);

        throwDirection.Normalize();

        grenade.Throw(throwDirection * throwForce);

        CurrentAmmo--;

        Debug.Log(
            "Threw " + currentGrenadeData.grenadeName +
            ". Remaining: " + CurrentAmmo
        );
    }

    public int GetHumanFragAmmo()
    {
        return currentHumanFragAmmo;
    }

    public int GetPlasmaAmmo()
    {
        return currentPlasmaAmmo;
    }

    public bool IsUsingPlasmaGrenade()
    {
        return usingPlasmaGrenade;
    }
}