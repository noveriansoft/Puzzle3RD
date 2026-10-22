using UnityEngine;

[CreateAssetMenu(
    fileName = "PhoneDialogue",
    menuName = "Phone/Dialogue Data"
)]
public class PhoneDialogueData : ScriptableObject
{
    public DialogueLine[] dialogueLines;


    [Header("After Call")]
    public bool advanceToNextDialogue;
}