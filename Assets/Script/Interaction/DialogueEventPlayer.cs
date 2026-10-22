
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using StarterAssets;

public class DialogueEventPlayer : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Player")]
    [SerializeField] private FirstPersonController playerController;

    [Header("After Dialogue")]
    [SerializeField] private UnityEvent onDialogueFinished;

    private Coroutine dialogueRoutine;
    private bool wasPlayerControllerEnabled;
    private bool isPlaying;

    public void PlayDialogue()
    {
        if (isPlaying || dialogueManager == null)
            return;

        if (dialogueManager.IsDialogueActive)
            return;

        isPlaying = true;

        if (playerController != null)
        {
            wasPlayerControllerEnabled = playerController.enabled;
            playerController.enabled = false;
        }

        dialogueManager.StartDialogue(dialogueLines);
        dialogueRoutine = StartCoroutine(WaitForDialogueEnd());
    }

    private IEnumerator WaitForDialogueEnd()
    {
        while (dialogueManager != null &&
               dialogueManager.IsDialogueActive)
        {
            yield return null;
        }

        if (playerController != null)
            playerController.enabled = wasPlayerControllerEnabled;

        isPlaying = false;
        dialogueRoutine = null;

        onDialogueFinished?.Invoke();
    }

    private void OnDisable()
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }

        if (playerController != null && isPlaying)
            playerController.enabled = wasPlayerControllerEnabled;

        isPlaying = false;
    }
}
