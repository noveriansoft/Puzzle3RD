using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRaycastDoor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressEUI;

    [Header("Player Raycast")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float interactionDistance = 2f;

    [Header("Door")]
    [SerializeField] private Transform door;

    [Header("Open Rotation")]
    [SerializeField] private float openZ = -120f;

    [SerializeField] private bool useOpenX;
    [SerializeField] private float openX = 0f;

    [Header("Movement")]
    [SerializeField] private float openDuration = 1f;

    [Header("Lock")]
    [SerializeField] private bool isLocked = false;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool isOpen;
    private bool isMoving;

    private bool ownsPressE;

    private void Start()
    {
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
        bool lookingAtDoor = IsLookingAtDoor();

        SetPressE(
            lookingAtDoor && !isMoving
        );

        if (!lookingAtDoor || isMoving)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isLocked)
            {
                if (audioSource != null && lockedSound != null)
                    audioSource.PlayOneShot(lockedSound);

                return;
            }

            ToggleDoor();
        }
    }

    private bool IsLookingAtDoor()
    {
        if (playerCamera == null)
            return false;

        Ray ray = new Ray(
            playerCamera.position,
            playerCamera.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            ~0,
            QueryTriggerInteraction.Collide
        ))
        {
            PlayerRaycastDoor hitDoor =
                hit.collider.GetComponentInParent<PlayerRaycastDoor>();

            return hitDoor == this;
        }

        return false;
    }

    private void ToggleDoor()
    {
        if (audioSource != null)
        {
            if (isOpen)
            {
                if (closeSound != null)
                    audioSource.PlayOneShot(closeSound);
            }
            else
            {
                if (openSound != null)
                    audioSource.PlayOneShot(openSound);
            }
        }

        Quaternion targetRotation =
            isOpen
                ? closedRotation
                : openRotation;

        StartCoroutine(
            RotateDoor(targetRotation)
        );
    }

    private IEnumerator RotateDoor(
        Quaternion targetRotation
    )
    {
        isMoving = true;

        SetPressE(false);

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

    private void SetPressE(bool show)
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
            if (ownsPressE)
            {
                pressEUI.SetActive(false);
                ownsPressE = false;
            }
        }
    }

    private void OnDisable()
    {
        SetPressE(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
            return;

        Gizmos.DrawRay(
            playerCamera.position,
            playerCamera.forward * interactionDistance
        );
    }

    public void LockDoor()
    {
        isLocked = true;
    }

    public void UnlockDoor()
    {
        isLocked = false;
    }
}