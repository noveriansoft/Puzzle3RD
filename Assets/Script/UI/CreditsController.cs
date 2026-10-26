
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CreditsController : MonoBehaviour
{
    [Header("Object")]
    [SerializeField] private GameObject creditsObject;

    [Header("Fade")]
    [SerializeField] private Image creditsImage;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Credits Scroll")]
    [SerializeField] private RectTransform creditsText;
    [SerializeField] private float startY = 500f;
    [SerializeField] private float endY = -1500f;
    [SerializeField] private float scrollDuration = 20f;

    private Coroutine creditsCoroutine;

    private void Start()
    {
        if (creditsObject != null)
            creditsObject.SetActive(false);
    }

#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            PlayCredits();
        }
    }
#endif

    public void PlayCredits()
    {
        if (creditsCoroutine != null)
            StopCoroutine(creditsCoroutine);

        if (creditsObject != null)
            creditsObject.SetActive(true);

        SetImageAlpha(0f);

        creditsCoroutine = StartCoroutine(PlayCreditsRoutine());
    }

    private IEnumerator PlayCreditsRoutine()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            SetImageAlpha(Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }

        SetImageAlpha(1f);

        if (creditsText == null)
            yield break;

        Vector2 position = creditsText.anchoredPosition;
        position.y = startY;
        creditsText.anchoredPosition = position;

        float scrollTimer = 0f;

        while (scrollTimer < scrollDuration)
        {
            scrollTimer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                scrollTimer / scrollDuration
            );

            position = creditsText.anchoredPosition;
            position.y = Mathf.Lerp(startY, endY, progress);
            creditsText.anchoredPosition = position;

            yield return null;
        }

        position = creditsText.anchoredPosition;
        position.y = endY;
        creditsText.anchoredPosition = position;

        creditsCoroutine = null;
    }

    private void SetImageAlpha(float alpha)
    {
        if (creditsImage == null)
            return;

        Color color = creditsImage.color;
        color.a = alpha;
        creditsImage.color = color;
    }
}
