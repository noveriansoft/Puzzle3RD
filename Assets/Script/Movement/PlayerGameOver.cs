using System.Collections;
using UnityEngine;

public class PlayerGameOver : MonoBehaviour
{
    public static bool IsGameOver { get; private set; }

    [Header("Camera")]
    [SerializeField] private Transform playerCamera;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private float gameOverUIDelay = 2f;

    [Header("Disable Player Controls")]
    [Tooltip("Masukkan semua script yang menerima input player.")]
    [SerializeField] private MonoBehaviour[] controlsToDisable;

    [Header("Specific Controls")]
    [SerializeField] private PhoneContactUI phoneContactUI;

    [Header("Held Item")]
    [Tooltip("Transform tempat item yang sedang dipegang menjadi child.")]
    [SerializeField] private Transform heldItemRoot;

    private bool hasGameOver;

    private void Awake()
    {
        IsGameOver = false;

        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    public void TriggerGameOver()
    {
        if (hasGameOver)
            return;

        hasGameOver = true;
        IsGameOver = true;

        EnemyAI.StopAllEnemies();

        // CAMERA POSITION
        if (playerCamera != null)
        {
            Vector3 localPosition = playerCamera.localPosition;

            localPosition.x = 0.686f;
            localPosition.y = -0.725f;

            // Z position tetap seperti semula.
            playerCamera.localPosition = localPosition;

            // CAMERA ROTATION
            Vector3 localRotation = playerCamera.localEulerAngles;

            localRotation.z = -54.886f;

            // Rotasi X dan Y tetap seperti semula.
            playerCamera.localEulerAngles = localRotation;
        }
        else
        {
            Debug.LogWarning(
                "[GameOver] Player Camera belum diisi!"
            );
        }

        // DISABLE PLAYER INPUT
        foreach (MonoBehaviour control in controlsToDisable)
        {
            if (control == null || control == this)
                continue;

            control.enabled = false;
        }

        if (phoneContactUI != null)
            phoneContactUI.enabled = false;

        if (heldItemRoot != null)
        {
            MonoBehaviour[] itemScripts =
                heldItemRoot.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour itemScript in itemScripts)
            {
                if (itemScript != null && itemScript != this)
                    itemScript.enabled = false;
            }
        }

        // SHOW GAME OVER UI
        StartCoroutine(ShowGameOverAfterDelay());

        Debug.Log("[GameOver] Player has been defeated.");
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(gameOverUIDelay);

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
