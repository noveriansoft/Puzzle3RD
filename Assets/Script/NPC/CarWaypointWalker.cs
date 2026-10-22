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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (customerNPC != null)
            customerNPC.SetActive(false);
    }

    private void Update()
    {
        if (finished || isWaitingForCustomer)
            return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        CarWaypointData waypoint = waypoints[currentWaypoint];

        if (waypoint.point == null)
            return;

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

        Vector3 moveDirection = direction.normalized;

        // Cek waypoint ada di depan atau belakang mobil
        float dot = Vector3.Dot(
            transform.forward,
            moveDirection
        );

        bool waypointBehind = dot < 0f;

        // Mobil tetap bergerak ke waypoint
        Vector3 movement =
            moveDirection * moveSpeed * Time.deltaTime;

        rb.MovePosition(
            rb.position + movement
        );

        // Kalau waypoint di depan:
        // moncong ngadep ke arah gerak
        //
        // Kalau waypoint di belakang:
        // moncong ngadep ke arah sebaliknya dari arah gerak
        // jadi terasa seperti mobil mundur sambil belok
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
            StartCoroutine(CustomerStopRoutine());
            return;
        }

        NextWaypoint();
    }

    private IEnumerator CustomerStopRoutine()
    {
        isWaitingForCustomer = true;

        if (customerNPC == null)
        {
            Debug.LogWarning("Customer NPC belum di-assign.");

            NextWaypoint();
            isWaitingForCustomer = false;

            yield break;
        }

        // Lepaskan NPC dari mobil
        customerNPC.transform.SetParent(null);

        // Spawn NPC di samping mobil
        if (npcSpawnPoint != null)
        {
            customerNPC.transform.position =
                npcSpawnPoint.position;

            customerNPC.transform.rotation =
                npcSpawnPoint.rotation;
        }

        customerNPC.SetActive(true);

        // Tunggu NPC menyelesaikan WaypointWalker
        // sampai NPC disable sendiri
        yield return new WaitUntil(
            () => !customerNPC.activeSelf
        );

        // Customer sudah kembali ke mobil
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

                if (disableOnFinish)
                    gameObject.SetActive(false);
            }
        }
    }
}