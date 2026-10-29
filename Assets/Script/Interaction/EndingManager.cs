using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isLoading;

    public void BackToMenu()
    {
        if (isLoading)
            return;

        isLoading = true;

        CheckpointSave.ContinueRequested = false;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartGame()
    {
        if (isLoading)
            return;

        isLoading = true;

        Scene currentScene = SceneManager.GetActiveScene();

        CheckpointSave.ContinueRequested =
            CheckpointSave.HasCheckpoint(currentScene.name);

        Time.timeScale = 1f;

        SceneManager.LoadScene(currentScene.name);
    }
}