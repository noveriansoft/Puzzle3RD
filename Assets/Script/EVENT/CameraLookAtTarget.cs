using UnityEngine;

public class CameraLookAtTarget : MonoBehaviour
{
    [System.Serializable]
    public class LookTarget
    {
        public string targetName;
        public Transform target;
    }

    [Header("Targets")]
    [SerializeField] private LookTarget[] targets;
    [SerializeField] private int startingTargetIndex = 0;

    [Header("Rotation")]
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSpeed = 5f;

    private Transform currentTarget;

    private void Start()
    {
        LookAtTarget(startingTargetIndex);
    }

    private void LateUpdate()
    {
        if (currentTarget == null)
            return;

        Vector3 direction = currentTarget.position - transform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        if (smoothRotation)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    public void LookAtTarget(int index)
    {
        if (targets == null || index < 0 || index >= targets.Length)
        {
            Debug.LogWarning($"Target index {index} tidak tersedia.", this);
            return;
        }

        if (targets[index].target == null)
        {
            Debug.LogWarning($"Target pada index {index} belum diisi.", this);
            return;
        }

        currentTarget = targets[index].target;
    }

    public void LookAtTargetByName(string targetName)
    {
        if (targets == null)
            return;

        foreach (LookTarget item in targets)
        {
            if (item.targetName == targetName && item.target != null)
            {
                currentTarget = item.target;
                return;
            }
        }

        Debug.LogWarning($"Target bernama '{targetName}' tidak ditemukan.", this);
    }

    public void StopLooking()
    {
        currentTarget = null;
    }

    public void LookAtTarget1()
    {
        LookAtTarget(0);
    }

    public void LookAtTarget2()
    {
        LookAtTarget(1);
    }

    public void LookAtTarget3()
    {
        LookAtTarget(2);
    }
}