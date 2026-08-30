using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    PlayerInput playerinput;
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;

    private Vector3 playerVelocity;
    private Transform cameraTransform;

    [SerializeField] private AssaultRifle assaultRifle;

    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jump = 5f;
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Recoil")]
    [SerializeField] private float recoilRecoverySpeed = 8f;
    
    private float recoil = 0f;;
    private float pitch = 0f;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        playerinput = GetComponent<PlayerInput>();

        moveAction = playerinput.actions.FindAction("Move");
        jumpAction = playerinput.actions.FindAction("Jump");
        lookAction = playerinput.actions.FindAction("Look");

        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {

    }
    void Update()
    {
        Move();
        look();
        jumping();
    }
    public void Move()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 move = transform.right * input.x + transform.forward * input.y;

        controller.Move(move * speed * Time.deltaTime);
    }

    public void look()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);

        pitch -= lookInput.y * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public void jumping()
    {

        if (!controller.isGrounded)
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
        else
        {
            playerVelocity.y = 0f;
        }

        controller.Move(playerVelocity * Time.deltaTime);

        if (jumpAction.IsPressed() && controller.isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jump * -2f * gravity);
        }


        // Gravity
        playerVelocity.y += gravity * Time.deltaTime;

        controller.Move(playerVelocity * Time.deltaTime);
    }
}