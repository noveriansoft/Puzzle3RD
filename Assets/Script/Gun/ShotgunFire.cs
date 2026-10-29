
using UnityEngine;
using UnityEngine.InputSystem;

public class ShotgunFire : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 50f;
    [SerializeField] private LayerMask hitLayers = ~0;

    [Header("Shotgun Spread")]
    [SerializeField] private int pelletCount = 8;
    [SerializeField] private float spreadAngle = 4f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private Light muzzleLight;
    [SerializeField] private float muzzleLightDuration = 0.08f;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.15f;
    [SerializeField] private float recoilReturnSpeed = 8f;

    private bool waitForMouseRelease = true;
    private float nextFireTime;
    private Vector3 originalLocalPosition;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (muzzleLight != null)
            muzzleLight.enabled = false;

        originalLocalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        bool dialogueActive =
            dialogueManager != null &&
            dialogueManager.IsDialogueActive;

        if (PauseManager.IsPaused || dialogueActive)
        {
            waitForMouseRelease = true;
            return;
        }

        // Restore the weapon position after recoil.
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            originalLocalPosition,
            recoilReturnSpeed * Time.deltaTime
        );

        if (Mouse.current == null || playerCamera == null)
        {
            waitForMouseRelease = true;
            return;
        }

        // Ignore the click used to close dialogue or resume gameplay.
        if (waitForMouseRelease)
        {
            if (!Mouse.current.leftButton.isPressed)
                waitForMouseRelease = false;

            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (PauseManager.IsPaused ||
            (dialogueManager != null && dialogueManager.IsDialogueActive))
        {
            return;
        }

        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);

        // Muzzle Flash
        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (muzzleLight != null)
        {
            StopCoroutine(nameof(FlashLight));
            StartCoroutine(nameof(FlashLight));
        }

        // Sound
        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        // Recoil
        Vector3 recoilDirection =
            transform.parent.InverseTransformDirection(
                -playerCamera.transform.forward
            );

        transform.localPosition =
            originalLocalPosition + recoilDirection * recoilDistance;

        // Shotgun pellets
        for (int i = 0; i < pelletCount; i++)
        {
            FirePellet();
        }
    }

    private System.Collections.IEnumerator FlashLight()
    {
        muzzleLight.enabled = true;

        yield return new WaitForSeconds(muzzleLightDuration);

        muzzleLight.enabled = false;
    }


    private void FirePellet()
    {
        Vector2 spread = Random.insideUnitCircle *
            Mathf.Tan(spreadAngle * Mathf.Deg2Rad);

        Vector3 direction =
            playerCamera.transform.forward +
            playerCamera.transform.right * spread.x +
            playerCamera.transform.up * spread.y;

        direction.Normalize();

        Ray ray = new Ray(
            playerCamera.transform.position,
            direction
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            range,
            ~0,
            QueryTriggerInteraction.Collide))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green, 2f);

            Debug.Log(
                $"Hit: {hit.collider.name} | " +
                $"Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}"
            );

            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Damage applied: {damage}");
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, direction * range, Color.red, 2f);
            Debug.Log("Nothing hit on any layer.");
        }
    }

    private void OnEnable()
    {
        waitForMouseRelease = true;
    }
}
