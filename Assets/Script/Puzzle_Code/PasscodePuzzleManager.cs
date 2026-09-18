using UnityEngine;
using TMPro;

public class PasscodePuzzleManager : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private int maxDigits = 4;

    [Header("UI")]
    [SerializeField] private TMP_Text inputText;
    [SerializeField] private TMP_Text resultText;

    [Header("Interaction")]
    [SerializeField] private PuzzleInteractable puzzleInteractable;

    private string currentInput = "";

    private void Start()
    {
        ResetPuzzle();
    }

    public void PressNumber(string number)
    {
        if (currentInput.Length >= maxDigits)
            return;

        currentInput += number;
        UpdateInputText();
    }

    public void ClearInput()
    {
        currentInput = "";
        UpdateInputText();

        if (resultText != null)
            resultText.text = "";
    }

    public void SubmitCode()
    {
        if (currentInput == correctCode)
        {
            PuzzleCompleted();
        }
        else
        {
            WrongCode();
        }
    }

    private void WrongCode()
    {
        if (resultText != null)
            resultText.text = "WRONG!";

        currentInput = "";
        UpdateInputText();
    }

    private void PuzzleCompleted()
    {
        if (resultText != null)
            resultText.text = "CORRECT!";

        Debug.Log("Passcode Puzzle Solved");

        puzzleInteractable.ClosePuzzle();
    }

    public void ExitPuzzle()
    {
        ResetPuzzle();
        puzzleInteractable.ClosePuzzle();
    }

    public void ResetPuzzle()
    {
        currentInput = "";

        UpdateInputText();

        if (resultText != null)
            resultText.text = "";
    }

    private void UpdateInputText()
    {
        if (inputText == null)
            return;

        inputText.text = "";

        for (int i = 0; i < maxDigits; i++)
        {
            if (i < currentInput.Length)
                inputText.text += currentInput[i];
            else
                inputText.text += "_";

            if (i < maxDigits - 1)
                inputText.text += " ";
        }
    }
}
