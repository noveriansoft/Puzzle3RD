using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private ThirdPersonController playerController;

    [Header("Cameras")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera camera2;
    [SerializeField] private Camera camera3;

    [Header("Obj")]
    [SerializeField] private GameObject GameObj;

    [Header("Timing")]
    [SerializeField] private float camera2Duration = 2f;
    [SerializeField] private float camera3Duration = 2f;

    [Header("End UI")]
    [SerializeField] private GameObject endUI;
    [SerializeField] private Image endBackground;
    [SerializeField] private GameObject endContent;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Ending")]
    [SerializeField] private EndingManager endingManager;
    [SerializeField] private EndingManager.EndingType endingType;

    private void Start()
    {
        if (camera2 != null)
            camera2.gameObject.SetActive(false);

        if (camera3 != null)
            camera3.gameObject.SetActive(false);

        if (endUI != null)
            endUI.SetActive(false);

        if (endContent != null)
            endContent.SetActive(false);
    }

    public void StartCutscene()
    {
        StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
        // Lock player
        if (playerController != null)
            playerController.SetMovement(false);

        // Main Camera -> Camera 2
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(false);

        if (camera2 != null)
            camera2.gameObject.SetActive(true);

        if (GameObj != null)
            GameObj.gameObject.SetActive(true);

        yield return new WaitForSeconds(camera2Duration);

        // Camera 2 -> Camera 3
        if (camera2 != null)
            camera2.gameObject.SetActive(false);

        if (camera3 != null)
            camera3.gameObject.SetActive(true);

        yield return new WaitForSeconds(camera3Duration);

        // Show end UI
        if (endUI != null)
            endUI.SetActive(true);

        if (endContent != null)
            endContent.SetActive(false);

        yield return StartCoroutine(FadeEndUI());

        if (endContent != null)
            endContent.SetActive(true);

        if (endingManager != null)
            endingManager.ShowEnding(endingType);
    }

    private IEnumerator FadeEndUI()
    {
        if (endBackground == null)
            yield break;

        Color color = endBackground.color;
        color.a = 0f;
        endBackground.color = color;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);

            color.a = t;
            endBackground.color = color;

            yield return null;
        }

        color.a = 1f;
        endBackground.color = color;
    }
}
