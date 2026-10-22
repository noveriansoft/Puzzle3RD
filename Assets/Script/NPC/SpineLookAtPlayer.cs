using UnityEngine;

public class SpineLookAtPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spine1;
    [SerializeField] private Transform player;

    [Header("Detection")]
    [SerializeField] private float lookDistance = 3f;
    [SerializeField] private float maxLookAngle = 70f;

    [Header("Spine Rotation")]
    [SerializeField] private float maxYaw = 35f;
    [SerializeField] private float maxPitch = 20f;
    [SerializeField] private float lookSpeed = 5f;
    [SerializeField] private float playerHeadHeight = 1.6f;

    private float currentWeight;

    private void Update()
    {
        if (spine1 == null || player == null)
            return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        float angle = Vector3.Angle(
            transform.forward,
            direction
        );

        bool shouldLook =
            distance <= lookDistance &&
            angle <= maxLookAngle;

        float targetWeight = shouldLook ? 1f : 0f;

        currentWeight = Mathf.MoveTowards(
            currentWeight,
            targetWeight,
            lookSpeed * Time.deltaTime
        );
    }

    private void LateUpdate()
    {
        if (spine1 == null || player == null)
            return;

        if (currentWeight <= 0f)
            return;

        Vector3 targetPosition =
            player.position + Vector3.up * playerHeadHeight;

        Vector3 direction =
            (targetPosition - spine1.position).normalized;

        // Arah player relatif terhadap NPC
        Vector3 localDirection =
            transform.InverseTransformDirection(direction);

        float yaw = Mathf.Atan2(
            localDirection.x,
            localDirection.z
        ) * Mathf.Rad2Deg;

        float horizontalDistance = new Vector2(
            localDirection.x,
            localDirection.z
        ).magnitude;

        float pitch = -Mathf.Atan2(
            localDirection.y,
            horizontalDistance
        ) * Mathf.Rad2Deg;

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        yaw *= currentWeight;
        pitch *= currentWeight;

        Quaternion yawRotation =
            Quaternion.AngleAxis(yaw, transform.up);

        Quaternion pitchRotation =
            Quaternion.AngleAxis(pitch, transform.right);

        spine1.rotation =
            yawRotation *
            pitchRotation *
            spine1.rotation;
    }
}