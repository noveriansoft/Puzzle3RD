using UnityEngine;
using TMPro;

public class RevealPasscode : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PasscodePuzzleManager passcodeManager;
    [SerializeField] private TMP_Text codeText;

    private void Start()
    {
        if (codeText != null)
            codeText.text = "Password : ----";
    }

    public void RevealCode()
    {
        if (passcodeManager == null || codeText == null)
            return;

        codeText.text = "Password : " + passcodeManager.GetCorrectCode();
    }
}
