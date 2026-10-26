
using UnityEngine;

public class TeleportToTransform : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform objectToMove;
    [SerializeField] private Transform targetPoint;

    public void Teleport()
    {
        if (objectToMove == null || targetPoint == null)
            return;

        CharacterController characterController =
            objectToMove.GetComponent<CharacterController>();

        if (characterController != null)
            characterController.enabled = false;

        objectToMove.SetPositionAndRotation(
            targetPoint.position,
            targetPoint.rotation
        );

        if (characterController != null)
            characterController.enabled = true;
    }
}