using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class DoorInteractable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressEUI;

    [Header("Door")]
    [SerializeField] private Transform door;
    [SerializeField] private float openZ = -120f;
    [SerializeField] private bool useOpenX;
    [SerializeField] private float openX = 0f;
    [SerializeField] private float openDuration = 1f;

    [Header("Cutscene")]
    [SerializeField] private Cutscene endCutscene;

    private bool playerInRange;
    private bool isOpening;
    private bool isOpened;

    private void Start()
    {
        if (pressEUI != null)
            pressEUI.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (isOpening || isOpened)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>() != null)
        {
            playerInRange = true;

            if (!isOpened && pressEUI != null)
                pressEUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>() != null)
        {
            playerInRange = false;

            if (pressEUI != null)
                pressEUI.SetActive(false);
        }
    }

    private void OpenDoor()
    {
        if (isOpening || isOpened)
            return;

        if (pressEUI != null)
            pressEUI.SetActive(false);

        StartCoroutine(OpenDoorRoutine());
    }

    private IEnumerator OpenDoorRoutine()
    {
        isOpening = true;

        Quaternion startRotation = door.localRotation;

        Vector3 currentRotation = door.localEulerAngles;

        float targetX = useOpenX
            ? openX
            : currentRotation.x;

        Quaternion targetRotation = Quaternion.Euler(
            targetX,
            currentRotation.y,
            openZ
        );

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
                Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        door.localRotation = targetRotation;

        isOpening = false;
        isOpened = true;

        if (endCutscene != null)
        {
            endCutscene.StartCutscene();
        }
    }
}
