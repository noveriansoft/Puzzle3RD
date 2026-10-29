
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    [Header("Fade Object")]
    [SerializeField] private GameObject fadeObject;
    [SerializeField] private Image fadeImage;

    [Header("Default Duration")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;

    [Header("Settings")]
    [SerializeField] private bool disableAfterFadeIn = true;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        SetAlpha(0f);

        if (fadeObject != null)
            fadeObject.SetActive(false);
    }

    // Uses the default Fade Out duration.
    public void FadeOut()
    {
        StartFade(0f, 1f, fadeOutDuration, false);
    }

    // Uses a custom duration from UnityEvent or another script.
    public void FadeOutWithDuration(float duration)
    {
        StartFade(0f, 1f, duration, false);
    }

    // Uses the default Fade In duration.
    public void FadeIn()
    {
        StartFade(1f, 0f, fadeInDuration, disableAfterFadeIn);
    }

    // Uses a custom duration from UnityEvent or another script.
    public void FadeInWithDuration(float duration)
    {
        StartFade(1f, 0f, duration, disableAfterFadeIn);
    }

    private void StartFade(
        float startAlpha,
        float targetAlpha,
        float duration,
        bool disableAtEnd)
    {
        if (fadeObject == null || fadeImage == null)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeObject.SetActive(true);

        fadeCoroutine = StartCoroutine(
            FadeRoutine(
                startAlpha,
                targetAlpha,
                duration,
                disableAtEnd
            )
        );
    }

    private IEnumerator FadeRoutine(
        float startAlpha,
        float targetAlpha,
        float duration,
        bool disableAtEnd)
    {
        SetAlpha(startAlpha);

        if (duration <= 0f)
        {
            SetAlpha(targetAlpha);
        }
        else
        {
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / duration);

                SetAlpha(Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    progress
                ));

                yield return null;
            }

            SetAlpha(targetAlpha);
        }

        fadeCoroutine = null;

        if (disableAtEnd && fadeObject != null)
            fadeObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
