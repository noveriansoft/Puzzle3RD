using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class WaypointWalkerWithEvent : MonoBehaviour
{
    [System.Serializable]
    public class WaypointData
    {
        public Transform point;

        [Header("Wait")]
        public bool waitHere;
        public float waitTime = 2f;

        [Header("Wait Rotation")]
        public bool rotateWhileWaiting;
        public float waitRotationY;

        [Header("Aim")]
        public bool aimOnReach;
    }

    [Header("Waypoints")]
    [SerializeField] private WaypointData[] waypoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float reachDistance = 0.1f;

    [Header("Settings")]
    [SerializeField] private bool loop = false;
    [SerializeField] private bool disableOnFinish = true;

    [Header("End Event")]
    [SerializeField] private UnityEvent onReachedEnd;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private int currentWaypoint;
    private bool isWaiting;
    private bool finished;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (finished || isWaiting)
            return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        WaypointData waypoint = waypoints[currentWaypoint];

        if (waypoint.point == null)
            return;

        Vector3 direction = waypoint.point.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= reachDistance)
        {
            ReachWaypoint();
            return;
        }

        Vector3 moveDirection = direction.normalized;

        if (rb != null)
        {
            rb.MovePosition(
                rb.position +
                moveDirection * moveSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position +=
                moveDirection * moveSpeed * Time.deltaTime;
        }

        if (moveDirection.sqrMagnitude > 0.001f)
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

    private void ReachWaypoint()
    {
        WaypointData waypoint = waypoints[currentWaypoint];

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);

            // Aim automatically when reaching this waypoint.
            animator.SetBool("isAim", waypoint.aimOnReach);
        }

        if (waypoint.waitHere && waypoint.waitTime > 0f)
        {
            StartCoroutine(WaitAtWaypoint(waypoint));
            return;
        }

        NextWaypoint();
    }

    private IEnumerator WaitAtWaypoint(WaypointData waypoint)
    {
        isWaiting = true;

        if (waypoint.rotateWhileWaiting)
        {
            Quaternion targetRotation = Quaternion.Euler(
                transform.eulerAngles.x,
                waypoint.waitRotationY,
                transform.eulerAngles.z
            );

            float rotateTimer = 0f;

            while (rotateTimer < 0.5f)
            {
                rotateTimer += Time.deltaTime;

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotateTimer / 0.5f
                );

                yield return null;
            }

            transform.rotation = targetRotation;
        }

        yield return new WaitForSeconds(waypoint.waitTime);

        isWaiting = false;
        NextWaypoint();
    }

    private void NextWaypoint()
    {
        currentWaypoint++;

        if (currentWaypoint >= waypoints.Length)
        {
            if (loop)
            {
                currentWaypoint = 0;
            }
            else
            {
                if (finished)
                    return;

                finished = true;

                if (animator != null)
                    animator.SetFloat("Speed", 0f);

                onReachedEnd?.Invoke();

                if (disableOnFinish)
                    gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}