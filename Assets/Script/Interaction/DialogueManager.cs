using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;

    [TextArea]
    public string text;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerText;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.03f;

    private DialogueLine[] dialogueLines;

    public bool IsDialogueActive { get; private set; }

    private int currentLine;
    private bool isTyping;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        IsDialogueActive = false;
    }

    private void Update()
    {
        if (!IsDialogueActive)
            return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isTyping)
                FinishCurrentLine();
            else
                NextDialogue();
        }
    }

    public void StartDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            EndDialogue();
            return;
        }

        dialogueLines = lines;
        currentLine = 0;
        IsDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (speakerText != null)
            speakerText.text = dialogueLines[currentLine].speakerName;

        typingCoroutine = StartCoroutine(
            TypeLine(dialogueLines[currentLine].text)
        );
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        dialogueText.text = line;

        isTyping = false;
        typingCoroutine = null;
    }

    private void FinishCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = dialogueLines[currentLine].text;
        isTyping = false;
    }

    public void NextDialogue()
    {
        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        IsDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        currentLine = 0;
    }
}