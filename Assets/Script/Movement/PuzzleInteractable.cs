using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleInteractable : MonoBehaviour
{
    [Header("Puzzle")]
    public GameObject puzzlePanel;

    [Header("Player")]
    public ThirdPersonController playerController;

    [Header("UI")]
    public GameObject pressEUI;

    [Header("Trigger")]
    public Collider triggerCollider;

    private bool playerInRange;
    private bool puzzleOpen;

    private void Start()
    {
        puzzlePanel.SetActive(false);

        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (puzzleOpen)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenPuzzle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ThirdPersonController player =
            other.GetComponent<ThirdPersonController>();

        if (player != null)
        {
            playerInRange = true;
            playerController = player;

            if (pressEUI != null)
                pressEUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ThirdPersonController player =
            other.GetComponent<ThirdPersonController>();

        if (player != null)
        {
            playerInRange = false;

            if (pressEUI != null)
                pressEUI.SetActive(false);
        }
    }

    public void OpenPuzzle()
    {
        puzzleOpen = true;

        if (pressEUI != null)
            pressEUI.SetActive(false);

        puzzlePanel.SetActive(true);
        playerController.SetMovement(false);
    }

    public void ClosePuzzle()
    {
        puzzleOpen = false;

        puzzlePanel.SetActive(false);
        playerController.SetMovement(true);

        if (playerInRange && pressEUI != null)
            pressEUI.SetActive(true);
    }

    public void DisableInteraction()
    {
        puzzleOpen = false;
        playerInRange = false;

        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (triggerCollider != null)
            triggerCollider.enabled = false;
    }
}
