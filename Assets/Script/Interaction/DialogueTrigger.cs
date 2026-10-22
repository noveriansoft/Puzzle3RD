
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using StarterAssets;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Player")]
    [SerializeField] private FirstPersonController playerController;

    [Header("Trigger")]
    [SerializeField] private bool interactOnlyOnce = false;

    [Header("After Dialogue")]
    [SerializeField] private UnityEvent onDialogueFinished;

    private bool hasInteracted;
    private bool isRunning;
    private bool lockedPlayer;
    private Coroutine dialogueEndRoutine;

    private void OnTriggerEnter(Collider other)
    {
        FirstPersonController detectedPlayer =
            other.GetComponentInParent<FirstPersonController>();

        if (detectedPlayer == null)
            return;

        if (playerController != null &&
            detectedPlayer != playerController)
            return;

        if (playerController == null)
            playerController = detectedPlayer;

        if (dialogueManager == null ||
            dialogueManager.IsDialogueActive ||
            isRunning)
            return;

        if (interactOnlyOnce && hasInteracted)
            return;

        if (PhoneContactUI.Instance != null &&
            PhoneContactUI.Instance.IsPhoneHeld)
            return;

        hasInteracted = true;
        isRunning = true;

        LockPlayer();

        dialogueManager.StartDialogue(dialogueLines);

        dialogueEndRoutine = StartCoroutine(WaitForDialogueEnd());
    }

    private IEnumerator WaitForDialogueEnd()
    {
        while (dialogueManager != null &&
               dialogueManager.IsDialogueActive)
        {
            yield return null;
        }

        UnlockPlayer();

        isRunning = false;
        dialogueEndRoutine = null;

        onDialogueFinished?.Invoke();
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
        if (playerController == null || !lockedPlayer)
            return;

        playerController.enabled = true;
        lockedPlayer = false;
    }

    private void OnDisable()
    {
        if (dialogueEndRoutine != null)
        {
            StopCoroutine(dialogueEndRoutine);
            dialogueEndRoutine = null;
        }

        isRunning = false;
        UnlockPlayer();
    }
}
