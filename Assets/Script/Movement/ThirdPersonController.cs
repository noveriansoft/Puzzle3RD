using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 3f;

    [Header("Gravity")]
    public float gravity = -20f;

    private CharacterController controller;
    private Vector3 velocity;

    private Vector3 cameraOffset;
    private Vector3 lookOffset;
    private float cameraYaw;
    private float cameraPitch;

    private bool canMove = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Keep the exact camera position from the scene
        cameraOffset = cameraTransform.position - transform.position;

        // Keep the original viewing direction
        Vector3 originalLookPoint =
            cameraTransform.position + cameraTransform.forward * 10f;

        lookOffset = transform.InverseTransformPoint(originalLookPoint);

        cameraYaw = cameraTransform.eulerAngles.y;
        cameraPitch = cameraTransform.eulerAngles.x;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleCamera();
        HandleMovement();
    }

    private void HandleCamera()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        cameraYaw += mouseDelta.x * mouseSensitivity;
        cameraPitch -= mouseDelta.y * mouseSensitivity;

        cameraPitch = Mathf.Clamp(cameraPitch, -30f, 15f);

        Quaternion rotation = Quaternion.Euler(
            cameraPitch,
            cameraYaw,
            0f
        );

        // Rotate the original camera offset around the player
        Vector3 offset = rotation * cameraOffset;

        cameraTransform.position = transform.position + offset;

        // Keep the original camera framing
        Vector3 target = transform.position + rotation * lookOffset;

        cameraTransform.LookAt(target);
    }

    private void HandleMovement()
    {
        if (!canMove)
        {
            ApplyGravity();
            return;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
                input.y += 1;

            if (Keyboard.current.sKey.isPressed)
                input.y -= 1;

            if (Keyboard.current.dKey.isPressed)
                input.x += 1;

            if (Keyboard.current.aKey.isPressed)
                input.x -= 1;
        }

        Vector3 inputDir = new Vector3(
            input.x,
            0f,
            input.y
        ).normalized;

        if (inputDir.magnitude > 0.1f)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDir =
                cameraForward * inputDir.z +
                cameraRight * inputDir.x;

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            controller.Move(
                moveDir * moveSpeed * Time.deltaTime
            );
        }

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    public void SetMovement(bool value)
    {
        canMove = value;
    }
}