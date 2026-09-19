using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    public enum EndingType
    {
        Ending1,
        Ending2
    }

    [Header("Ending Text")]
    [SerializeField] private TMP_Text endingDescription;

    [TextArea]
    [SerializeField] private string ending1Text;

    [TextArea]
    [SerializeField] private string ending2Text;

    public void ShowEnding(EndingType endingType)
    {
        if (endingDescription == null)
            return;

        switch (endingType)
        {
            case EndingType.Ending1:
                endingDescription.text = ending1Text;
                break;

            case EndingType.Ending2:
                endingDescription.text = ending2Text;
                break;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
