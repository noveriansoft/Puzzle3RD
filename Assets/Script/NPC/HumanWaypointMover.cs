
using UnityEngine;
using UnityEngine.Events;

public class HumanWaypointMover : MonoBehaviour
{
    [System.Serializable]
    public class WaypointData
    {
        public Transform point;

        [Header("Movement Animation")]
        public bool playWalkAnimation = true;
    }

    [Header("Waypoints")]
    [SerializeField] private WaypointData[] waypoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float reachDistance = 0.1f;

    [Header("Settings")]
    [SerializeField] private bool loop;
    [SerializeField] private bool disableOnFinish = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("End Event")]
    [SerializeField] private UnityEvent onReachedEnd;

    private int currentWaypoint;
    private bool finished;
    private Rigidbody rb;
    private Quaternion lastHorizontalRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lastHorizontalRotation = transform.rotation;
    }

    private void Update()
    {
        if (finished || waypoints == null || waypoints.Length == 0)
            return;

        WaypointData waypoint = waypoints[currentWaypoint];

        if (waypoint == null || waypoint.point == null)
            return;

        Vector3 direction = waypoint.point.position - transform.position;

        if (direction.magnitude <= reachDistance)
        {
            ReachWaypoint();
            return;
        }

        Vector3 moveDirection = direction.normalized;

        if (rb != null)
        {
            rb.MovePosition(
                rb.position + moveDirection * moveSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        // Update facing direction only when horizontal movement exists.
        Vector3 horizontalDirection = direction;
        horizontalDirection.y = 0f;

        if (horizontalDirection.sqrMagnitude > 0.001f)
        {
            lastHorizontalRotation = Quaternion.LookRotation(horizontalDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lastHorizontalRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = lastHorizontalRotation;
        }

        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                waypoint.playWalkAnimation ? 1f : 0f
            );
        }
    }

    private void ReachWaypoint()
    {
        WaypointData waypoint = waypoints[currentWaypoint];

        if (waypoint.point != null)
            transform.position = waypoint.point.position;

        if (animator != null)
            animator.SetFloat("Speed", 0f);

        currentWaypoint++;

        if (currentWaypoint >= waypoints.Length)
        {
            if (loop)
            {
                currentWaypoint = 0;
            }
            else
            {
                finished = true;
                onReachedEnd?.Invoke();

                if (disableOnFinish)
                    gameObject.SetActive(false);
            }
        }
    }
}
