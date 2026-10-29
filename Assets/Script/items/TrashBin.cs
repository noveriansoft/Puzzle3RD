using UnityEngine;
using UnityEngine.InputSystem;

public class TrashBin : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PickupSystem pickupSystem;
    [SerializeField] private Collider binCollider;
    [SerializeField] private GameObject pressEUI;

    [Header("Interaction")]
    [SerializeField, Min(0f)] private float interactDistance = 2f;

    private bool ownsPressE;

    private void Reset()
    {
        binCollider = GetComponent<Collider>();
    }

    private void LateUpdate()
    {
        bool canDispose = CanDispose();

        SetPressE(canDispose);

        if (!canDispose || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            pickupSystem.DisposeHeldTrash();
            SetPressE(false);
        }
    }

    private bool CanDispose()
    {
        if (player == null || pickupSystem == null)
            return false;

        if (!pickupSystem.IsHoldingTrash())
            return false;

        if (PhoneContactUI.Instance != null &&
            PhoneContactUI.Instance.IsPhoneHeld)
            return false;

        Vector3 closestPoint = binCollider != null
            ? binCollider.ClosestPoint(player.position)
            : transform.position;

        return (player.position - closestPoint).sqrMagnitude
            <= interactDistance * interactDistance;
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
        else if (ownsPressE)
        {
            pressEUI.SetActive(false);
            ownsPressE = false;
        }
    }

    private void OnDisable()
    {
        SetPressE(false);
    }
}