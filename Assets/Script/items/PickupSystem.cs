using UnityEngine;
using UnityEngine.InputSystem;


[System.Serializable]
public class HandPointSlot
{
    public HandPointType type;
    public Transform handPoint;
}


public class PickupSystem : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 2.5f;

    [Header("Hand Points")]
    [SerializeField] private HandPointSlot[] handPoints;

    [Header("UI")]
    [SerializeField] private GameObject pressEUI;

    [Header("Sound Effects")]
    [SerializeField] private int disposeSEIndex = 1;

    private PickupItem currentTarget;
    private Transform currentHit;

    private PickupItem heldItem;
    private GameObject heldObject;

    // Biar tidak ganggu Press E milik interaction lain
    private bool ownsPressE;

    private void Update()
    {
        CheckInteraction();

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentTarget == null)
                return;

            // Belum pegang barang → ambil
            if (heldItem == null &&
                currentTarget.IsWorldObject(currentHit))
            {
                TakeItem(currentTarget);
            }

            // Sedang lihat return marker barang yang dipegang
            else if (heldItem == currentTarget &&
                     currentTarget.IsReturnMarker(currentHit))
            {
                ReturnItem();
            }
        }
    }

    private void CheckInteraction()
    {
        currentTarget = null;
        currentHit = null;

        if (PhoneContactUI.Instance != null && PhoneContactUI.Instance.IsPhoneHeld)
        {
            SetPressE(false);
            return;
        }

        bool canInteract = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            PickupItem item =
                hit.collider.GetComponentInParent<PickupItem>();

            if (item != null)
            {
                // Mau ambil barang
                if (heldItem == null &&
                    !item.IsPickedUp &&
                    item.IsWorldObject(hit.transform))
                {
                    currentTarget = item;
                    currentHit = hit.transform;
                    canInteract = true;
                }

                // Mau mengembalikan barang
                else if (heldItem == item &&
                         item.IsReturnMarker(hit.transform))
                {
                    currentTarget = item;
                    currentHit = hit.transform;
                    canInteract = true;
                }
            }
        }

        SetPressE(canInteract);
    }

    private Transform GetHandPoint(PickupItem item)
    {
        foreach (HandPointSlot slot in handPoints)
        {
            if (slot.type == item.HandPointType)
                return slot.handPoint;
        }

        return null;
    }

    private void TakeItem(PickupItem item)
    {
        Transform handPoint = GetHandPoint(item);

        if (handPoint == null)
            return;

        GameObject newHeldObject = item.Pickup(handPoint);

        if (newHeldObject == null)
            return;

        heldItem = item;
        heldObject = newHeldObject;

        SEManager.Instance?.PlaySE(item.PickupSEIndex);

        SetPressE(false);
    }

    private void ReturnItem()
    {
        SetPressE(false);

        if (heldObject != null)
            Destroy(heldObject);

        heldItem.ReturnItem();

        heldItem = null;
        heldObject = null;
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
        else
        {
            // Jangan matikan Press E milik interaction lain
            if (ownsPressE)
            {
                pressEUI.SetActive(false);
                ownsPressE = false;
            }
        }
    }

    public bool HasItem()
    {
        return heldItem != null;
    }

    public PickupItem GetHeldItem()
    {
        return heldItem;
    }

    public bool IsHoldingTrash()
    {
        return heldItem != null && heldItem.IsTrash;
    }

    public void DisposeHeldTrash()
    {
        if (!IsHoldingTrash())
            return;

        SetPressE(false);

        heldItem.DisposeItem();

        if (heldObject != null)
            Destroy(heldObject);

        heldItem = null;
        heldObject = null;

        SEManager.Instance?.PlaySE(disposeSEIndex);
    }
}