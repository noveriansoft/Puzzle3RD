
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private PlayerGameOver playerGameOver;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Game Over")]
    [Tooltip("Jarak horizontal untuk mendeteksi kontak sebagai fallback.")]
    [SerializeField] private float gameOverDistance = 0.9f;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public static event System.Action<EnemyAI> EnemyDied;

    private bool isDead;

    public bool IsDead => isDead;

    public void SetPlayer(Transform target)
    {
        player = target;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (playerGameOver == null && player != null)
            playerGameOver =
                player.GetComponentInParent<PlayerGameOver>();

        if (playerGameOver == null && player != null)
            playerGameOver =
                player.GetComponentInChildren<PlayerGameOver>(true);

        animator.applyRootMotion = false;
        animator.SetFloat("Speed", 0f);
        animator.SetBool("isDead", false);

        agent.speed = moveSpeed;
        agent.stoppingDistance = 0f;
        agent.autoBraking = false;
        agent.updateRotation = false;
    }

    private void Update()
    {
        if (isDead || player == null ||
            PlayerGameOver.IsGameOver)
            return;

        // Fallback jika callback collision tidak terpanggil.
        CheckPlayerDistance();

        if (PlayerGameOver.IsGameOver)
            return;

        if (agent == null || !agent.isOnNavMesh)
            return;

        ChasePlayer();
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        float speed = agent.velocity.magnitude;

        animator.SetFloat(
            "Speed",
            speed > 0.1f ? 1f : 0f
        );
    }

    // Deteksi collision biasa.
    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerContact(collision.transform);
    }

    // Deteksi jika menggunakan Trigger Collider.
    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerContact(other.transform);
    }

    private void HandlePlayerContact(Transform other)
    {
        if (isDead || player == null ||
            PlayerGameOver.IsGameOver)
            return;

        bool isPlayer =
            other == player ||
            other.IsChildOf(player) ||
            player.IsChildOf(other);

        if (!isPlayer)
            return;

        TriggerPlayerGameOver();
    }

    private void CheckPlayerDistance()
    {
        if (playerGameOver == null ||
            gameOverDistance <= 0f)
            return;

        Vector3 enemyPosition = transform.position;
        Vector3 playerPosition = player.position;

        // Bandingkan jarak horizontal saja.
        enemyPosition.y = 0f;
        playerPosition.y = 0f;

        float distanceSquared =
            (enemyPosition - playerPosition).sqrMagnitude;

        if (distanceSquared <=
            gameOverDistance * gameOverDistance)
        {
            TriggerPlayerGameOver();
        }
    }

    private void TriggerPlayerGameOver()
    {
        if (PlayerGameOver.IsGameOver)
            return;

        if (playerGameOver == null && player != null)
        {
            playerGameOver =
                player.GetComponentInParent<PlayerGameOver>();

            if (playerGameOver == null)
            {
                playerGameOver =
                    player.GetComponentInChildren<PlayerGameOver>(true);
            }
        }

        if (playerGameOver == null)
        {
            Debug.LogWarning(
                "[EnemyAI] PlayerGameOver tidak ditemukan!"
            );
            return;
        }

        playerGameOver.TriggerGameOver();

        // Stop enemy movement.
        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // Return enemy animation to Idle.
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        currentHealth = Mathf.Max(
            0f,
            currentHealth - damage
        );

        Debug.Log(
            $"{gameObject.name} kena damage {damage}. " +
            $"HP tersisa: {currentHealth}"
        );

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        isDead = true;

        // Play death animation.
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isDead", true);
        }

        EnemyDied?.Invoke(this);
        // Disable physics components.
        DisablePhysicsComponents();
    }


    private void DisablePhysicsComponents()
    {
        // Disable all colliders, including child objects.
        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Stop and disable all NavMeshAgents.
        NavMeshAgent[] agents =
            GetComponentsInChildren<NavMeshAgent>(true);

        foreach (NavMeshAgent navAgent in agents)
        {
            if (navAgent.enabled && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.ResetPath();
                navAgent.velocity = Vector3.zero;
            }

            navAgent.enabled = false;
        }

        // Disable Rigidbody physics.
        Rigidbody[] rigidbodies =
            GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.detectCollisions = false;
            rb.isKinematic = true;
        }
    }


    public static void StopAllEnemies()
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (EnemyAI enemy in enemies)
        {
            enemy.StopForGameOver();
        }
    }

    private void StopForGameOver()
    {
        // Stop NavMeshAgent.
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
        }

        // Return animation to Idle.
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isDead", false);
        }
    }

}
