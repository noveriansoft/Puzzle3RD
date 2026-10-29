using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Buttons")]
    [SerializeField] private Button continueButton;

    [Header("Fade")]
    [SerializeField] private GameObject fadeParent;
    [SerializeField] private Image fadeImage;
    [SerializeField, Min(0f)] private float fadeDuration = 1f;

    private bool isStarting;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (continueButton != null)
        {
            continueButton.interactable =
                CheckpointSave.HasCheckpoint(gameplaySceneName);
        }
    }

    public void StartGame()
    {
        BeginGame(false);
    }

    public void ContinueGame()
    {
        if (isStarting)
            return;

        if (!CheckpointSave.HasCheckpoint(gameplaySceneName))
        {
            Debug.Log("No saved checkpoint is available.", this);
            return;
        }

        BeginGame(true);
    }

    private void BeginGame(bool loadCheckpoint)
    {
        if (isStarting)
            return;

        if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            Debug.LogError(
                $"Scene '{gameplaySceneName}' is not available in the build.",
                this
            );
            return;
        }

        if (fadeParent == null || fadeImage == null)
        {
            Debug.LogError("Fade references are missing.", this);
            return;
        }

        isStarting = true;
        StartCoroutine(StartGameRoutine(loadCheckpoint));
    }

    private IEnumerator StartGameRoutine(bool loadCheckpoint)
    {
        fadeParent.SetActive(true);
        fadeImage.gameObject.SetActive(true);
        fadeImage.raycastTarget = true;

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        // Render the fully faded screen before loading.
        yield return null;

        if (!loadCheckpoint)
            CheckpointSave.Clear(gameplaySceneName);

        CheckpointSave.ContinueRequested = loadCheckpoint;

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ExitGame()
    {
        if (isStarting)
            return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ClearSave()
    {
        if (isStarting)
            return;

        CheckpointSave.Clear(gameplaySceneName);
        CheckpointSave.ContinueRequested = false;

        if (continueButton != null)
            continueButton.interactable = false;

        Debug.Log("Save data cleared.");
    }
}