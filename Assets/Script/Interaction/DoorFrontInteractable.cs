using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorFrontInteractable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressEUI;

    [Header("Interaction")]
    [SerializeField] private Transform player;

    // Empty GameObject untuk menentukan posisi + arah depan pintu
    [SerializeField] private Transform interactionPoint;

    [SerializeField] private float interactionDistance = 2f;

    [Range(0f, 1f)]
    [SerializeField] private float frontDetection = 0.5f;

    [SerializeField] private bool detectBothSides = true;

    [Header("Door")]
    [SerializeField] private Transform door;
    [SerializeField] private float openZ = -120f;

    [Header("Optional Open X")]
    [SerializeField] private bool useOpenX;
    [SerializeField] private float openX = 0f;

    [Header("Movement")]
    [SerializeField] private float openDuration = 1f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool isOpen;
    private bool isMoving;

    private bool ownsPressE;

    private void Start()
    {
        if (interactionPoint == null)
            interactionPoint = transform;

        if (door == null)
            door = transform;

        closedRotation = door.localRotation;

        Vector3 currentRotation = door.localEulerAngles;

        float targetX = useOpenX
            ? openX
            : currentRotation.x;

        openRotation = Quaternion.Euler(
            targetX,
            currentRotation.y,
            openZ
        );
    }

    private void Update()
    {
        bool canInteract = CanPlayerInteract();

        SetDoorPressE(
            canInteract && !isMoving
        );

        if (!canInteract || isMoving)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleDoor();
        }
    }

    private bool CanPlayerInteract()
    {
        if (player == null || interactionPoint == null)
            return false;

        Vector3 directionToPlayer =
            player.position - interactionPoint.position;

        directionToPlayer.y = 0f;

        float distance = directionToPlayer.magnitude;

        if (distance > interactionDistance)
            return false;

        if (distance <= 0.01f)
            return true;

        directionToPlayer.Normalize();

        Vector3 forward =
            interactionPoint.forward;

        forward.y = 0f;
        forward.Normalize();

        float dot = Vector3.Dot(
            forward,
            directionToPlayer
        );

        if (detectBothSides)
        {
            return Mathf.Abs(dot) >= frontDetection;
        }

        return dot >= frontDetection;
    }

    private void ToggleDoor()
    {
        StartCoroutine(
            RotateDoor(
                isOpen
                    ? closedRotation
                    : openRotation
            )
        );
    }

    private IEnumerator RotateDoor(
        Quaternion targetRotation
    )
    {
        isMoving = true;

        SetDoorPressE(false);

        Quaternion startRotation =
            door.localRotation;

        float timer = 0f;

        while (timer < openDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                timer / openDuration
            );

            door.localRotation =
                Quaternion.Lerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        door.localRotation = targetRotation;

        isOpen = !isOpen;
        isMoving = false;
    }

    private void SetDoorPressE(bool show)
    {
        if (pressEUI == null)
            return;

        if (show)
        {
            if (!pressEUI.activeSelf)
            {
                pressEUI.SetActive(true);
                ownsPressE = true;
            }
        }
        else
        {
            // Jangan matikan Press E
            // kalau UI sedang dipakai script lain
            if (ownsPressE)
            {
                pressEUI.SetActive(false);
                ownsPressE = false;
            }
        }
    }

    private void OnDisable()
    {
        SetDoorPressE(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionPoint == null)
            return;

        Gizmos.DrawWireSphere(
            interactionPoint.position,
            interactionDistance
        );

        Gizmos.DrawRay(
            interactionPoint.position,
            interactionPoint.forward * interactionDistance
        );

        if (detectBothSides)
        {
            Gizmos.DrawRay(
                interactionPoint.position,
                -interactionPoint.forward * interactionDistance
            );
        }
    }
}