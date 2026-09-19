using UnityEngine;
using System.Collections;
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

    [Header("Doors")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private float doorOpenDuration = 1f;

    [SerializeField] private float leftDoorOpenZ = -120f;
    [SerializeField] private float rightDoorOpenZ = 120f;

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

        OpenDoors();
        puzzleInteractable.DisableInteraction();

    }

    #region DOOR COUROUTINE
    private void OpenDoors()
    {
        //if (leftDoor != null)
        //{
        //    Vector3 rot = leftDoor.localEulerAngles;
        //    rot.z = leftDoorOpenZ;
        //    leftDoor.localEulerAngles = rot;
        //}

        //if (rightDoor != null)
        //{
        //    Vector3 rot = rightDoor.localEulerAngles;
        //    rot.z = rightDoorOpenZ;
        //    rightDoor.localEulerAngles = rot;
        //}

        StartCoroutine(OpenDoorsRoutine());
    }

    private IEnumerator OpenDoorsRoutine()
    {
        Quaternion leftStart = leftDoor.localRotation;
        Quaternion rightStart = rightDoor.localRotation;

        Quaternion leftTarget = Quaternion.Euler(
            leftDoor.localEulerAngles.x,
            leftDoor.localEulerAngles.y,
            leftDoorOpenZ
        );

        Quaternion rightTarget = Quaternion.Euler(
            rightDoor.localEulerAngles.x,
            rightDoor.localEulerAngles.y,
            rightDoorOpenZ
        );

        float timer = 0f;

        while (timer < doorOpenDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / doorOpenDuration);

            if (leftDoor != null)
                leftDoor.localRotation = Quaternion.Lerp(leftStart, leftTarget, t);

            if (rightDoor != null)
                rightDoor.localRotation = Quaternion.Lerp(rightStart, rightTarget, t);

            yield return null;
        }

        if (leftDoor != null)
            leftDoor.localRotation = leftTarget;

        if (rightDoor != null)
            rightDoor.localRotation = rightTarget;

        ObjectiveManager.Instance.CompleteDoorObjective();
        puzzleInteractable.ClosePuzzle();
        
    }
    #endregion

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

    public string GetCorrectCode()
    {
        return correctCode;
    }
}
