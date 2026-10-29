using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarWaypointWalker : MonoBehaviour
{
    [System.Serializable]
    public class CarWaypointData
    {
        public Transform point;

        [Header("Customer Stop")]
        public bool customerStop;
    }

    [Header("Waypoints")]
    [SerializeField] private CarWaypointData[] waypoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float reachDistance = 0.3f;

    [Header("Driving Audio")]
    [SerializeField] private AudioSource drivingAudioSource;

    [Header("Customer")]
    [SerializeField] private GameObject customerNPC;
    [SerializeField] private Transform npcSpawnPoint;

    [Header("Settings")]
    [SerializeField] private bool loop;
    [SerializeField] private bool disableOnFinish;

    private Rigidbody rb;

    private int currentWaypoint;
    private bool isWaitingForCustomer;
    private bool finished;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (drivingAudioSource != null)
        {
            drivingAudioSource.playOnAwake = false;
            drivingAudioSource.loop = true;
            drivingAudioSource.Stop();
        }
    }

    private void Start()
    {
        if (customerNPC != null)
            customerNPC.SetActive(false);
    }

    private void Update()
    {
        if (finished || isWaitingForCustomer)
        {
            StopDrivingSound();
            return;
        }

        if (waypoints == null ||
            waypoints.Length == 0 ||
            currentWaypoint >= waypoints.Length)
        {
            StopDrivingSound();
            return;
        }

        CarWaypointData waypoint = waypoints[currentWaypoint];

        if (waypoint == null || waypoint.point == null)
        {
            StopDrivingSound();
            return;
        }

        MoveToWaypoint(waypoint);
    }

    private void MoveToWaypoint(CarWaypointData waypoint)
    {
        Vector3 direction =
            waypoint.point.position - transform.position;

        direction.y = 0f;

        if (direction.magnitude <= reachDistance)
        {
            ReachWaypoint(waypoint);
            return;
        }

        if (moveSpeed <= 0f || Time.deltaTime <= 0f)
        {
            StopDrivingSound();
            return;
        }

        PlayDrivingSound();

        Vector3 moveDirection = direction.normalized;

        // Determine whether the waypoint is behind the car.
        float dot = Vector3.Dot(
            transform.forward,
            moveDirection
        );

        bool waypointBehind = dot < 0f;

        Vector3 movement =
            moveDirection * moveSpeed * Time.deltaTime;

        rb.MovePosition(rb.position + movement);

        // Face backward when reversing toward the waypoint.
        Vector3 facingDirection =
            waypointBehind ? -moveDirection : moveDirection;

        if (facingDirection != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(facingDirection);

            Quaternion newRotation =
                Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );

            rb.MoveRotation(newRotation);
        }
    }

    private void ReachWaypoint(CarWaypointData waypoint)
    {
        if (waypoint.customerStop)
        {
            StopDrivingSound();
            StartCoroutine(CustomerStopRoutine());
            return;
        }

        NextWaypoint();
    }

    private IEnumerator CustomerStopRoutine()
    {
        isWaitingForCustomer = true;
        StopDrivingSound();

        if (customerNPC == null)
        {
            Debug.LogWarning("Customer NPC is not assigned.", this);

            NextWaypoint();
            isWaitingForCustomer = false;
            yield break;
        }

        // Detach the customer from the car.
        customerNPC.transform.SetParent(null);

        if (npcSpawnPoint != null)
        {
            customerNPC.transform.position =
                npcSpawnPoint.position;

            customerNPC.transform.rotation =
                npcSpawnPoint.rotation;
        }

        customerNPC.SetActive(true);

        // Wait until the customer finishes and becomes inactive.
        yield return new WaitUntil(
            () => customerNPC == null || !customerNPC.activeSelf
        );

        NextWaypoint();
        isWaitingForCustomer = false;
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
                finished = true;
                StopDrivingSound();

                if (disableOnFinish)
                    gameObject.SetActive(false);
            }
        }
    }

    private void PlayDrivingSound()
    {
        if (drivingAudioSource == null ||
            drivingAudioSource.clip == null ||
            !drivingAudioSource.isActiveAndEnabled)
        {
            return;
        }

        if (!drivingAudioSource.isPlaying)
            drivingAudioSource.Play();
    }

    private void StopDrivingSound()
    {
        if (drivingAudioSource != null &&
            drivingAudioSource.isPlaying)
        {
            drivingAudioSource.Stop();
        }
    }

    private void OnDisable()
    {
        StopDrivingSound();
    }
}