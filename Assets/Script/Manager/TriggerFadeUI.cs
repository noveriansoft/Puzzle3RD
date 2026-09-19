using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static EndingManager;

public class TriggerFadeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject textObject;

    [Header("Player")]
    [SerializeField] private ThirdPersonController playerController;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Ending")]
    [SerializeField] private EndingManager endingManager;
    [SerializeField] private EndingManager.EndingType endingType;

    private bool alreadyTriggered;

    private void Start()
    {
        if (uiRoot != null)
            uiRoot.SetActive(false);

        if (textObject != null)
            textObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered)
            return;

        ThirdPersonController player =
            other.GetComponent<ThirdPersonController>();

        if (player != null)
        {
            alreadyTriggered = true;

            playerController = player;

            // Lock movement + unlock cursor
            playerController.SetMovement(false);

            if (endingManager != null)
                endingManager.ShowEnding(endingType);

            StartCoroutine(ShowUI());
        }
    }

    private IEnumerator ShowUI()
    {
        uiRoot.SetActive(true);

        if (textObject != null)
            textObject.SetActive(false);

        Color color = backgroundImage.color;
        color.a = 0f;
        backgroundImage.color = color;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Clamp01(timer / fadeDuration);

            color.a = alpha;
            backgroundImage.color = color;

            yield return null;
        }

        color.a = 1f;
        backgroundImage.color = color;

        if (textObject != null)
            textObject.SetActive(true);
    }
}
