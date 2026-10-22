using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float lookDistance = 3f;
    [SerializeField] private float maxLookAngle = 70f;
    [SerializeField] private float lookWeightSpeed = 5f;

    [Header("Look Target")]
    [SerializeField] private float playerHeadHeight = 1.6f;

    private float currentWeight;

    private void Update()
    {
        if (player == null)
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
            lookWeightSpeed * Time.deltaTime
        );
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || player == null)
            return;

        Vector3 targetPosition =
            player.position + Vector3.up * playerHeadHeight;

        animator.SetLookAtWeight(
            currentWeight,
            0.2f,   // body
            0.8f,   // head
            0.6f,   // eyes
            0.5f    // clamp
        );

        animator.SetLookAtPosition(targetPosition);
    }
}