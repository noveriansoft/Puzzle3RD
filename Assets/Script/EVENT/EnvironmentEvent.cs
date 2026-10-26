using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class EnvironmentEvent : MonoBehaviour
{
    [Header("Environment Lighting")]
    [SerializeField, ColorUsage(false, true)]
    private Color targetAmbientColor = Color.gray;

    [Header("Objects To Enable")]
    [SerializeField] private GameObject[] objectsToEnable;

    [Header("Event BGM")]
    [SerializeField] private int eventBGMIndex = 1;

    private bool hasActivated;

#if UNITY_EDITOR
    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.gKey.wasPressedThisFrame)
        {
            ActivateEvent();
        }
    }

#endif

    public void ActivateEvent()
    {
        if (hasActivated) return;
        hasActivated = true;

        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayBGM(eventBGMIndex);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = targetAmbientColor;

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}