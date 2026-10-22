
using System.Collections;
using UnityEngine;
using StarterAssets;

public class AutoCloseDoubleDoorTrigger : MonoBehaviour
{
    [Header("Doors")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Left Door Closed Rotation")]
    [SerializeField] private Vector3 leftClosedRotation;

    [Header("Right Door Closed Rotation")]
    [SerializeField] private Vector3 rightClosedRotation;

    [Header("Animation")]
    [SerializeField] private float closeDuration = 1f;

    [Header("Settings")]
    [SerializeField] private bool oneTimeOnly = true;

    private bool isClosing;
    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (oneTimeOnly && hasTriggered)
            return;

        if (other.GetComponentInParent<FirstPersonController>() == null)
            return;

        if (isClosing)
            return;

        hasTriggered = true;
        StartCoroutine(CloseDoorsRoutine());
    }

    private IEnumerator CloseDoorsRoutine()
    {
        isClosing = true;

        Quaternion leftStart = leftDoor != null
            ? leftDoor.localRotation
            : Quaternion.identity;

        Quaternion rightStart = rightDoor != null
            ? rightDoor.localRotation
            : Quaternion.identity;

        Quaternion leftTarget = Quaternion.Euler(leftClosedRotation);
        Quaternion rightTarget = Quaternion.Euler(rightClosedRotation);

        float timer = 0f;
        float duration = Mathf.Max(0.01f, closeDuration);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f, 1f, Mathf.Clamp01(timer / duration)
            );

            if (leftDoor != null)
                leftDoor.localRotation = Quaternion.Slerp(
                    leftStart, leftTarget, t
                );

            if (rightDoor != null)
                rightDoor.localRotation = Quaternion.Slerp(
                    rightStart, rightTarget, t
                );

            yield return null;
        }

        if (leftDoor != null)
            leftDoor.localRotation = leftTarget;

        if (rightDoor != null)
            rightDoor.localRotation = rightTarget;

        isClosing = false;
    }
}
