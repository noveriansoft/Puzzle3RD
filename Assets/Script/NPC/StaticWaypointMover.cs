
using UnityEngine;
using UnityEngine.Events;

public class StaticWaypointMover : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float reachDistance = 0.05f;

    [Header("Continuous Rotation")]
    [SerializeField] private bool rotateYContinuously = true;
    [SerializeField] private float rotationSpeedY = 45f;

    [Header("End Event")]
    [SerializeField] private UnityEvent onReachedEnd;

    private int currentWaypoint;
    private bool finished;

    private void Update()
    {
        // Keep rotating even after movement has finished.
        if (rotateYContinuously)
        {
            transform.Rotate(
                0f,
                rotationSpeedY * Time.deltaTime,
                0f,
                Space.Self
            );
        }

        if (finished)
            return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[currentWaypoint];

        if (target == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            target.position
        );

        if (distance <= reachDistance)
        {
            transform.position = target.position;
            ReachWaypoint();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );
    }

    private void ReachWaypoint()
    {
        if (currentWaypoint < waypoints.Length - 1)
        {
            currentWaypoint++;
            return;
        }

        // Stop movement and invoke the event once.
        finished = true;
        onReachedEnd?.Invoke();
    }
}
