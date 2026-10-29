using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Pause Blockers")]
    [SerializeField] private GameObject creditsRoot;
    [SerializeField] private GameObject gameOverRoot;

    [Header("Gameplay Scripts")]
    [Tooltip("Scripts that must stop receiving input while paused.")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    public static bool IsPaused { get; private set; }

    private bool[] previousEnabledStates;
    private float previousTimeScale;
    private bool pauseBlocked;
    private bool isLoading;

    private void Awake()
    {
        IsPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (isLoading)
            return;

        if (IsPauseBlocked())
        {
            if (IsPaused)
                ClosePause(false);

            return;
        }

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private bool IsPauseBlocked()
    {
        return pauseBlocked ||
            (creditsRoot != null && creditsRoot.activeInHierarchy) ||
            (gameOverRoot != null && gameOverRoot.activeInHierarchy);
    }

    public void PauseGame()
    {
        if (IsPaused || isLoading || IsPauseBlocked())
            return;

        if (pausePanel == null)
        {
            Debug.LogWarning("Pause panel is missing.", this);
            return;
        }

        previousTimeScale = Time.timeScale;
        PhoneContactUI.Instance?.SetCallAudioPaused(true);

        int count = scriptsToDisable != null
            ? scriptsToDisable.Length
            : 0;

        previousEnabledStates = new bool[count];

        for (int i = 0; i < count; i++)
        {
            MonoBehaviour script = scriptsToDisable[i];

            if (script == null || script == this)
                continue;

            previousEnabledStates[i] = script.enabled;
            script.enabled = false;
        }

        IsPaused = true;
        Time.timeScale = 0f;

        pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (!IsPaused || isLoading)
            return;

        ClosePause(true);
    }

    private void ClosePause(bool lockCursor)
    {
        if (!IsPaused)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (scriptsToDisable != null &&
            previousEnabledStates != null)
        {
            int count = Mathf.Min(
                scriptsToDisable.Length,
                previousEnabledStates.Length
            );

            for (int i = 0; i < count; i++)
            {
                MonoBehaviour script = scriptsToDisable[i];

                if (script != null && script != this)
                    script.enabled = previousEnabledStates[i];
            }
        }

        Time.timeScale = previousTimeScale;
        IsPaused = false;

        PhoneContactUI.Instance?.SetCallAudioPaused(false);

        Cursor.lockState = lockCursor
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !lockCursor;
    }

    public void RestartGame()
    {
        if (isLoading)
            return;

        Scene scene = SceneManager.GetActiveScene();

        if (!Application.CanStreamedLevelBeLoaded(scene.name))
        {
            Debug.LogError("Gameplay scene is unavailable.", this);
            return;
        }

        isLoading = true;

        ClosePause(false);

        CheckpointSave.ContinueRequested =
            CheckpointSave.HasCheckpoint(scene.name);

        Time.timeScale = 1f;

        SceneManager.LoadScene(scene.name);
    }

    public void BackToMenu()
    {
        if (isLoading)
            return;

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError("Main menu scene is unavailable.", this);
            return;
        }

        isLoading = true;

        ClosePause(false);

        CheckpointSave.ContinueRequested = false;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SetPauseBlocked(bool blocked)
    {
        pauseBlocked = blocked;

        if (blocked && IsPaused)
            ClosePause(false);
    }

    private void OnDisable()
    {
        if (IsPaused)
            ClosePause(false);
    }
}