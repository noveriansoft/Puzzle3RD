
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using StarterAssets;

public class DialogueInteractable : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private bool interactOnlyOnce = false;

    [Header("Player")]
    [SerializeField] private FirstPersonController playerController;

    [Header("UI")]
    [SerializeField] private GameObject pressEUI;

    [Header("After Dialogue")]
    [SerializeField] private UnityEvent onDialogueFinished;

    private bool isLooking;
    private bool ownsPressE;
    private bool lockedPlayer;
    private bool waitingForDialogueEnd;
    private bool hasInteracted;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (dialogueManager == null || playerCamera == null)
            return;

        // Dialog yang dimulai object ini sudah selesai
        if (waitingForDialogueEnd &&
            !dialogueManager.IsDialogueActive)
        {
            waitingForDialogueEnd = false;

            UnlockPlayer();

            // Jalankan event setelah dialog
            onDialogueFinished?.Invoke();
        }

        if (dialogueManager.IsDialogueActive)
        {
            isLooking = false;
            SetPressE(false);
            return;
        }

        if (PhoneContactUI.Instance != null &&
            PhoneContactUI.Instance.IsPhoneHeld)
        {
            isLooking = false;
            SetPressE(false);
            return;
        }

        if (interactOnlyOnce && hasInteracted)
        {
            SetPressE(false);
            return;
        }

        CheckRaycast();

        if (isLooking &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void CheckRaycast()
    {
        isLooking = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                isLooking = true;
            }
        }

        SetPressE(isLooking);
    }

    private void Interact()
    {
        if (PhoneContactUI.Instance != null &&
            PhoneContactUI.Instance.IsPhoneHeld)
            return;

        if (dialogueManager.IsDialogueActive)
            return;

        SetPressE(false);

        LockPlayer();

        dialogueManager.StartDialogue(dialogueLines);

        if (dialogueManager.IsDialogueActive)
        {
            waitingForDialogueEnd = true;
            hasInteracted = true;
        }
        else
        {
            UnlockPlayer();
        }
    }

    private void LockPlayer()
    {
        if (playerController == null)
            return;

        playerController.enabled = false;
        lockedPlayer = true;
    }

    private void UnlockPlayer()
    {
        if (playerController != null && lockedPlayer)
            playerController.enabled = true;

        lockedPlayer = false;
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

        if (lockedPlayer)
            UnlockPlayer();
    }
}
