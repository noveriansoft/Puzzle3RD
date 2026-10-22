using UnityEngine;
using UnityEngine.InputSystem;

public class MopSwing : MonoBehaviour
{
    [Header("Broom")]
    [SerializeField] private GameObject broomObject;
    [SerializeField] private Transform broomTransform;

    [Header("Rotation")]
    [SerializeField] private float startAngle = -78f;
    [SerializeField] private float endAngle = -84f;
    [SerializeField] private float speed = 8f;

    [SerializeField] private DirtCleanable currentDirt;
    private float currentAngle;

    private void Start()
    {
        currentAngle = startAngle;
    }

    private void Update()
    {
        // Sapunya harus ada / aktif
        if (broomObject == null || !broomObject.activeInHierarchy)
            return;

        if (Mouse.current.leftButton.isPressed)
        {
            float t = Mathf.PingPong(Time.time * speed, 1f);

            currentAngle = Mathf.Lerp(startAngle, endAngle, t);

            Vector3 rot = broomTransform.localEulerAngles;
            rot.x = currentAngle;

            broomTransform.localEulerAngles = rot;

            // Bersihkan dirt kalau sapu sedang menyentuhnya
            if (currentDirt != null)
            {
                currentDirt.Clean(Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DirtCleanable dirt = other.GetComponent<DirtCleanable>();

        if (dirt != null)
        {
            currentDirt = dirt;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DirtCleanable dirt = other.GetComponent<DirtCleanable>();

        if (dirt != null && dirt == currentDirt)
        {
            currentDirt = null;
        }
    }
}