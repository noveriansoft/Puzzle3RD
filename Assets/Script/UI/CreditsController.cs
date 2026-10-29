using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CreditsController : MonoBehaviour
{
    [Header("Credits Object")]
    [SerializeField] private GameObject creditsObject;

    [Header("Credits Fade")]
    [SerializeField] private Image creditsImage;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Credits Scroll")]
    [SerializeField] private RectTransform creditsText;
    [SerializeField] private float startY = 500f;
    [SerializeField] private float endY = -1500f;
    [SerializeField] private float scrollDuration = 20f;

    [Header("Final Fade Object")]
    [SerializeField] private GameObject finalFadeObject;
    [SerializeField] private Image finalFadeImage;
    [SerializeField] private float waitBeforeFinalFade = 3f;
    [SerializeField] private float finalFadeDuration = 1f;

    [Header("End Event")]
    [SerializeField] private UnityEvent onReachedEnd;

    private Coroutine creditsCoroutine;

    private void Start()
    {
        // Initialize alpha only when the credits system starts.
        SetCreditsAlpha(0f);
        SetFinalFadeAlpha(0f);

        if (creditsObject != null)
            creditsObject.SetActive(false);

        if (finalFadeObject != null)
            finalFadeObject.SetActive(false);
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

    // Initial fade, scroll, final fade, and event.
    public void PlayCredits()
    {
        StartCredits(true, true);
    }

    // No initial fade. Fade out credits after scrolling.
    public void PlayCreditsOnly()
    {
        StartCredits(false, false);
    }

    // Initial fade, scroll, then fade out credits.
    public void PlayCreditsWithInitialFade()
    {
        StartCredits(true, false);
    }

    // No initial fade, fade out credits, then final screen fade.
    public void PlayCreditsWithFinalFade()
    {
        StartCredits(false, true);
    }

    private void StartCredits(bool useCreditsFade, bool useFinalFade)
    {
        if (creditsCoroutine != null)
        {
            StopCoroutine(creditsCoroutine);
            creditsCoroutine = null;
        }

        if (creditsObject != null)
            creditsObject.SetActive(true);

        if (finalFadeObject != null)
            finalFadeObject.SetActive(false);

        // Do not touch credits alpha when initial fade is disabled.
        if (useCreditsFade)
            SetCreditsAlpha(0f);

        // Reset final fade only when that mode is requested.
        if (useFinalFade)
            SetFinalFadeAlpha(0f);

        creditsCoroutine = StartCoroutine(
            PlayCreditsRoutine(useCreditsFade, useFinalFade)
        );
    }

    private IEnumerator PlayCreditsRoutine(
        bool useCreditsFade,
        bool useFinalFade)
    {
        // Optional initial fade-in.
        if (useCreditsFade && creditsImage != null)
        {
            yield return FadeImage(
                creditsImage,
                0f,
                1f,
                fadeDuration
            );
        }

        // Reset and scroll credits text.
        if (creditsText != null)
        {
            Vector2 position = creditsText.anchoredPosition;
            position.y = startY;
            creditsText.anchoredPosition = position;

            if (scrollDuration <= 0f)
            {
                position.y = endY;
                creditsText.anchoredPosition = position;
            }
            else
            {
                float timer = 0f;

                while (timer < scrollDuration)
                {
                    timer += Time.deltaTime;

                    float progress = Mathf.Clamp01(
                        timer / scrollDuration
                    );

                    position = creditsText.anchoredPosition;
                    position.y = Mathf.Lerp(startY, endY, progress);
                    creditsText.anchoredPosition = position;

                    yield return null;
                }

                position = creditsText.anchoredPosition;
                position.y = endY;
                creditsText.anchoredPosition = position;
            }
        }

        // Fade out credits only AFTER scrolling has finished.
        // Read the current alpha instead of forcing it to 1.
        if (!useCreditsFade && creditsImage != null)
        {
            float currentAlpha = creditsImage.color.a;

            yield return FadeImage(
                creditsImage,
                currentAlpha,
                0f,
                fadeDuration
            );
        }

        // Optional final screen fade.
        if (useFinalFade)
        {
            yield return new WaitForSeconds(waitBeforeFinalFade);

            if (finalFadeObject != null)
                finalFadeObject.SetActive(true);

            if (finalFadeImage != null)
            {
                SetFinalFadeAlpha(0f);

                yield return FadeImage(
                    finalFadeImage,
                    0f,
                    1f,
                    finalFadeDuration
                );

                SetFinalFadeAlpha(1f);
            }
        }

        onReachedEnd?.Invoke();
        creditsCoroutine = null;
    }

    private IEnumerator FadeImage(
        Image image,
        float startAlpha,
        float targetAlpha,
        float duration)
    {
        if (image == null)
            yield break;

        SetImageAlpha(image, startAlpha);

        if (duration <= 0f)
        {
            SetImageAlpha(image, targetAlpha);
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / duration);

            SetImageAlpha(
                image,
                Mathf.Lerp(startAlpha, targetAlpha, progress)
            );

            yield return null;
        }

        SetImageAlpha(image, targetAlpha);
    }

    private void SetCreditsAlpha(float alpha)
    {
        SetImageAlpha(creditsImage, alpha);
    }

    private void SetFinalFadeAlpha(float alpha)
    {
        SetImageAlpha(finalFadeImage, alpha);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}