using UnityEngine;

public class ProximityWaypointNPC : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float triggerDistance = 5f;

    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float reachDistance = 0.1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private bool disableOnFinish = true;

    private int currentWaypoint;
    private bool started;
    private bool finished;

    private void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.SetFloat("Speed", 0f);
    }

    private void Update()
    {
        if (finished)
            return;

        // Belum mulai, tunggu player mendekat
        if (!started)
        {
            CheckPlayerDistance();
            return;
        }

        MoveToWaypoint();
    }

    private void CheckPlayerDistance()
    {
        if (player == null)
            return;

        Vector3 playerPos = player.position;
        Vector3 npcPos = transform.position;

        // Abaikan beda ketinggian
        playerPos.y = 0f;
        npcPos.y = 0f;

        float distance = Vector3.Distance(
            npcPos,
            playerPos
        );

        if (distance <= triggerDistance)
        {
            started = true;
        }
    }

    private void MoveToWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[currentWaypoint];

        if (target == null)
            return;

        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.magnitude <= reachDistance)
        {
            NextWaypoint();
            return;
        }

        Vector3 moveDirection = direction.normalized;

        transform.position +=
            moveDirection * moveSpeed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        if (animator != null)
            animator.SetFloat("Speed", 1f);
    }

    private void NextWaypoint()
    {
        currentWaypoint++;

        if (currentWaypoint >= waypoints.Length)
        {
            FinishRoute();
        }
    }

    private void FinishRoute()
    {
        finished = true;

        if (animator != null)
            animator.SetFloat("Speed", 0f);

        if (disableOnFinish)
            gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            triggerDistance
        );
    }
}