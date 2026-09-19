using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TriggerUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressEUI;
    [SerializeField] private GameObject UItoShow;

    [Header("Player")]
    [SerializeField] private ThirdPersonController playerController;

    private bool playerInRange;
    private bool hintOpen;

    private void Start()
    {
        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (UItoShow != null)
            UItoShow.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!hintOpen)
                OpenUI();
            else
                OpenUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ThirdPersonController player = other.GetComponent<ThirdPersonController>();

        if (other.GetComponent<ThirdPersonController>() != null)
        {
            playerInRange = true;
            playerController = player;

            if (!hintOpen && pressEUI != null)
                pressEUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ThirdPersonController player = other.GetComponent<ThirdPersonController>();

        if (other.GetComponent<ThirdPersonController>() != null)
        {
            playerInRange = false;

            if (pressEUI != null)
                pressEUI.SetActive(false);

            CloseUI();
        }
    }

    private void OpenUI()
    {
        hintOpen = true;

        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (UItoShow != null)
            UItoShow.SetActive(true);

        if (playerController != null)
            playerController.SetMovement(false);
    }

    public void CloseUI()
    {
        if (playerController != null)
            playerController.SetMovement(true);

        hintOpen = false;

        if (UItoShow != null)
            UItoShow.SetActive(false);

        if (playerInRange && pressEUI != null)
            pressEUI.SetActive(true);
    }
}
