using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class WaypointWalker : MonoBehaviour
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

        [Header("Dialogue")]
        public bool startDialogue;

        [Header("Aim")]
        public bool aimWhileTalking;
    }

    [Header("Waypoints")]
    [SerializeField] private WaypointData[] waypoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float reachDistance = 0.1f;

    [Header("Settings")]
    [SerializeField] private bool loop = true;
    [SerializeField] private bool disableOnFinish = true;

    [Header("Player Controller")]
    [SerializeField] private FirstPersonController playerController;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Dialogue Interaction")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject pressEUI;
    [SerializeField] private float interactionDistance = 2f;

    [Range(-1f, 1f)]
    [SerializeField] private float frontDetection = 0.6f;

    private int currentWaypoint = 0;
    private bool npcOwnsPressE;
    private bool isWaiting;
    private bool finished;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (playerController == null && player != null)
            playerController = player.GetComponent<FirstPersonController>();
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

        Vector3 direction =
            waypoint.point.position - transform.position;

        direction.y = 0f;

        if (direction.magnitude <= reachDistance)
        {
            ReachWaypoint();
            return;
        }

        Vector3 moveDirection = direction.normalized;

        //transform.position +=
        //    moveDirection * moveSpeed * Time.deltaTime;

        rb.MovePosition(
        rb.position +
        moveDirection * moveSpeed * Time.fixedDeltaTime
        );

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

    private void ReachWaypoint()
    {
        WaypointData waypoint = waypoints[currentWaypoint];

        if (waypoint.startDialogue)
        {
            StartCoroutine(DialogueWaypointRoutine());
            return;
        }

        if (waypoint.waitHere && waypoint.waitTime > 0f)
        {
            StartCoroutine(WaitAtWaypoint(waypoint.waitTime));
            return;
        }

        NextWaypoint();
    }

    private IEnumerator WaitAtWaypoint(float duration)
    {
        isWaiting = true;

        if (animator != null)
            animator.SetFloat("Speed", 0f);

        WaypointData waypoint = waypoints[currentWaypoint];

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

        yield return new WaitForSeconds(duration);

        NextWaypoint();

        isWaiting = false;
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

                if (animator != null)
                    animator.SetFloat("Speed", 0f);

                if (pressEUI != null)
                    pressEUI.SetActive(false);

                if (disableOnFinish)
                    gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator DialogueWaypointRoutine()
    {
        isWaiting = true;

        if (animator != null)
            animator.SetFloat("Speed", 0f);

        WaypointData waypoint = waypoints[currentWaypoint];

        // Rotate NPC dulu
        if (waypoint.rotateWhileWaiting)
        {
            Quaternion targetRotation = Quaternion.Euler(
                transform.eulerAngles.x,
                waypoint.waitRotationY,
                transform.eulerAngles.z
            );

            while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.rotation = targetRotation;
        }

        // Tunggu player datang ke depan NPC dan tekan E
        bool interactionStarted = false;

        while (!interactionStarted)
        {
            bool phoneIsHeld =
                PhoneContactUI.Instance != null &&
                PhoneContactUI.Instance.IsPhoneHeld;

            bool canInteract =
                IsPlayerInFront() && !phoneIsHeld;

            SetNPCPressE(canInteract);

            if (canInteract &&
                Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                interactionStarted = true;

                SetNPCPressE(false);
            }

            yield return null;
        }

        // KUNCI PLAYER
        if (playerController != null)
            playerController.SetMovement(false);

        // Aim
        if (animator != null && waypoint.aimWhileTalking)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isAim", true);
        }

        // Mulai dialog
        dialogueManager.StartDialogue(dialogueLines);

        // Tunggu dialog selesai
        yield return new WaitUntil(
            () => !dialogueManager.IsDialogueActive
        );

        // PLAYER BISA GERAK LAGI
        if (playerController != null)
            playerController.SetMovement(true);

        // Turunkan pistol setelah dialog (kl ada)
        if (animator != null && waypoint.aimWhileTalking)
            animator.SetBool("isAim", false);

        NextWaypoint();

        isWaiting = false;
    }

    private bool IsPlayerInFront()
    {
        if (player == null)
            return false;

        Vector3 directionToPlayer =
            player.position - transform.position;

        directionToPlayer.y = 0f;

        float distance = directionToPlayer.magnitude;

        if (distance > interactionDistance)
            return false;

        if (distance <= 0.01f)
            return true;

        directionToPlayer.Normalize();

        float dot = Vector3.Dot(
            transform.forward,
            directionToPlayer
        );

        return dot >= frontDetection;
    }

    private void OnDisable()
    {
        if (pressEUI != null)
            pressEUI.SetActive(false);
    }

    private void SetNPCPressE(bool show)
    {
        if (pressEUI == null)
            return;

        if (show)
        {
            // Kalau UI belum dipakai script lain,
            // NPC boleh menyalakannya.
            if (!pressEUI.activeSelf)
            {
                pressEUI.SetActive(true);
                npcOwnsPressE = true;
            }
        }
        else
        {
            // NPC cuma boleh mematikan UI
            // kalau NPC sendiri yang menyalakannya.
            if (npcOwnsPressE)
            {
                pressEUI.SetActive(false);
                npcOwnsPressE = false;
            }
        }
    }
}