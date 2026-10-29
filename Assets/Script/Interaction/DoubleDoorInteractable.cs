using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class DoubleDoorInteractable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressEUI;

    [Header("Left Door")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private float leftOpenZ = -120f;

    [Header("Right Door")]
    [SerializeField] private Transform rightDoor;
    [SerializeField] private float rightOpenZ = 120f;

    [Header("Optional X Rotation")]
    [SerializeField] private bool useOpenX;
    [SerializeField] private float openX = 0f;

    [Header("Animation")]
    [SerializeField] private float openDuration = 1f;

    [Header("Door Audio")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip doorOpenedClip;
    [SerializeField, Range(0f, 1f)] private float doorVolume = 1f;

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
            OpenDoors();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() != null)
        {
            playerInRange = true;

            if (!isOpened && pressEUI != null)
                pressEUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() != null)
        {
            playerInRange = false;

            if (pressEUI != null)
                pressEUI.SetActive(false);
        }
    }

    private void OpenDoors()
    {
        if (isOpening || isOpened)
            return;

        if (pressEUI != null)
            pressEUI.SetActive(false);

        StartCoroutine(OpenDoorsRoutine());
    }

    private IEnumerator OpenDoorsRoutine()
    {
        isOpening = true;

        if (doorAudioSource != null && doorOpenedClip != null)
        {
            doorAudioSource.PlayOneShot(doorOpenedClip, doorVolume);
        }

        Quaternion leftStartRotation = leftDoor.localRotation;
        Quaternion rightStartRotation = rightDoor.localRotation;

        Vector3 leftCurrentRotation = leftDoor.localEulerAngles;
        Vector3 rightCurrentRotation = rightDoor.localEulerAngles;

        float leftTargetX = useOpenX
            ? openX
            : leftCurrentRotation.x;

        float rightTargetX = useOpenX
            ? openX
            : rightCurrentRotation.x;

        Quaternion leftTargetRotation = Quaternion.Euler(
            leftTargetX,
            leftCurrentRotation.y,
            leftOpenZ
        );

        Quaternion rightTargetRotation = Quaternion.Euler(
            rightTargetX,
            rightCurrentRotation.y,
            rightOpenZ
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

            if (leftDoor != null)
            {
                leftDoor.localRotation =
                    Quaternion.Lerp(
                        leftStartRotation,
                        leftTargetRotation,
                        t
                    );
            }

            if (rightDoor != null)
            {
                rightDoor.localRotation =
                    Quaternion.Lerp(
                        rightStartRotation,
                        rightTargetRotation,
                        t
                    );
            }

            yield return null;
        }

        if (leftDoor != null)
            leftDoor.localRotation = leftTargetRotation;

        if (rightDoor != null)
            rightDoor.localRotation = rightTargetRotation;

        isOpening = false;
        isOpened = true;

        if (endCutscene != null)
        {
            endCutscene.StartCutscene();
        }

        if (endCutscene != null)
        {
            endCutscene.StartCutscene();
        }
    }

    public void SetOpenedForCheckpoint()
    {
        StopAllCoroutines();

        isOpening = false;
        isOpened = true;

        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (leftDoor != null)
        {
            Vector3 rotation = leftDoor.localEulerAngles;

            if (useOpenX)
                rotation.x = openX;

            rotation.z = leftOpenZ;
            leftDoor.localRotation = Quaternion.Euler(rotation);
        }

        if (rightDoor != null)
        {
            Vector3 rotation = rightDoor.localEulerAngles;

            if (useOpenX)
                rotation.x = openX;

            rotation.z = rightOpenZ;
            rightDoor.localRotation = Quaternion.Euler(rotation);
        }
    }
}