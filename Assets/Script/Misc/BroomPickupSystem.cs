using UnityEngine;
using UnityEngine.InputSystem;

public class BroomPickupSystem : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 2.5f;

    [Header("Broom")]
    [SerializeField] private GameObject heldBroom;
    [SerializeField] private GameObject worldBroom;
    [SerializeField] private GameObject broomReturnMarker;

    [Header("UI")]
    [SerializeField] private GameObject pressEUI;

    private bool hasBroom;
    private bool lookingAtBroom;
    private bool lookingAtReturnMarker;

    // Supaya tidak mematikan Press E milik interaction lain
    private bool broomOwnsPressE;

    private void Start()
    {
        heldBroom.SetActive(false);
        worldBroom.SetActive(true);
        broomReturnMarker.SetActive(false);
    }

    private void Update()
    {
        CheckInteraction();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!hasBroom && lookingAtBroom)
            {
                TakeBroom();
            }
            else if (hasBroom && lookingAtReturnMarker)
            {
                ReturnBroom();
            }
        }
    }

    private void CheckInteraction()
    {
        lookingAtBroom = false;
        lookingAtReturnMarker = false;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        // Ignore trigger supaya trigger sapu di tangan tidak menghalangi raycast
        if (Physics.Raycast(
            ray,
            out hit,
            interactDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            // Ambil sapu
            if (!hasBroom && IsObjectOrChild(hit.transform, worldBroom))
            {
                lookingAtBroom = true;
            }

            // Taruh sapu kembali
            if (hasBroom && IsObjectOrChild(hit.transform, broomReturnMarker))
            {
                lookingAtReturnMarker = true;
            }
        }

        bool shouldShow =
            (!hasBroom && lookingAtBroom) ||
            (hasBroom && lookingAtReturnMarker);

        SetBroomPressE(shouldShow);
    }

    private bool IsObjectOrChild(Transform target, GameObject objectToCheck)
    {
        if (target == null || objectToCheck == null)
            return false;

        return target == objectToCheck.transform ||
               target.IsChildOf(objectToCheck.transform);
    }

    private void TakeBroom()
    {
        SetBroomPressE(false);

        hasBroom = true;

        worldBroom.SetActive(false);
        broomReturnMarker.SetActive(true);
        heldBroom.SetActive(true);
    }

    private void ReturnBroom()
    {
        SetBroomPressE(false);

        hasBroom = false;

        heldBroom.SetActive(false);
        broomReturnMarker.SetActive(false);
        worldBroom.SetActive(true);
    }

    private void SetBroomPressE(bool show)
    {
        if (pressEUI == null)
            return;

        if (show)
        {
            // Hanya claim UI kalau belum sedang dipakai object lain
            if (!pressEUI.activeSelf)
            {
                pressEUI.SetActive(true);
                broomOwnsPressE = true;
            }
        }
        else
        {
            // Hanya boleh matikan kalau broom yang menyalakannya
            if (broomOwnsPressE)
            {
                pressEUI.SetActive(false);
                broomOwnsPressE = false;
            }
        }
    }

    public bool HasBroom()
    {
        return hasBroom;
    }
}