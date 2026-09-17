using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleInteractable : MonoBehaviour
{
    [Header("Puzzle")]
    public GameObject puzzlePanel;

    [Header("Player")]
    public ThirdPersonController playerController;

    private bool playerInRange;
    private bool puzzleOpen;

    private void Start()
    {
        puzzlePanel.SetActive(false);
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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ThirdPersonController player =
            other.GetComponent<ThirdPersonController>();

        if (player != null)
        {
            playerInRange = false;
        }
    }

    public void OpenPuzzle()
    {
        puzzleOpen = true;

        puzzlePanel.SetActive(true);
        playerController.SetMovement(false);
    }

    public void ClosePuzzle()
    {
        puzzleOpen = false;

        puzzlePanel.SetActive(false);
        playerController.SetMovement(true);
    }
}
