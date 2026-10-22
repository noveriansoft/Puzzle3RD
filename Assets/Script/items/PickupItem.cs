using UnityEngine;

public enum HandPointType
{
    Default,
    Broom,
    Shotgun,
    SmallItem
}

public class PickupItem : MonoBehaviour
{
    [Header("World")]
    [SerializeField] private GameObject worldObject;

    [Header("Held")]
    [SerializeField] private GameObject heldPrefab;
    [SerializeField] private HandPointType handPointType = HandPointType.Default;

    [Header("Return Marker - Optional")]
    [SerializeField] private GameObject returnMarker;

    public bool IsPickedUp { get; private set; }

    public HandPointType HandPointType => handPointType;

    private void Start()
    {
        if (worldObject != null)
            worldObject.SetActive(true);

        if (returnMarker != null)
            returnMarker.SetActive(false);
    }

    public bool IsWorldObject(Transform hit)
    {
        if (worldObject == null)
            return false;

        return hit == worldObject.transform ||
               hit.IsChildOf(worldObject.transform);
    }

    public bool IsReturnMarker(Transform hit)
    {
        if (returnMarker == null)
            return false;

        return hit == returnMarker.transform ||
               hit.IsChildOf(returnMarker.transform);
    }

    public GameObject Pickup(Transform handPoint)
    {
        if (heldPrefab == null)
            return null;

        // Spawn sebagai child HandPoint
        // dan pertahankan local position/rotation prefab
        GameObject heldObject =
            Instantiate(heldPrefab, handPoint, false);

        worldObject.SetActive(false);

        if (returnMarker != null)
            returnMarker.SetActive(true);

        IsPickedUp = true;

        return heldObject;
    }

    public void ReturnItem()
    {
        worldObject.SetActive(true);

        if (returnMarker != null)
            returnMarker.SetActive(false);

        IsPickedUp = false;
    }
}