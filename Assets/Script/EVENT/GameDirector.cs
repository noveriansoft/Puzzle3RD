
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameDirector : MonoBehaviour
{
    public enum StepType
    {
        IncomingCall,
        CleanAllDirt,
        SpawnNPC,
        SpawnObject,
        WaitSeconds,
        CustomEvent,
        AutomatedDialogue,
        WaitForHandPointType
    }

    [Serializable]
    public class DirectorStep
    {
        [Header("Step")]
        public string stepName;
        public StepType type;

        [TextArea]
        public string objective;

        [Header("Incoming Call")]
        public int contactIndex;
        public int conversationIndex;

        [Header("Automated Dialogue")]
        public DialogueLine[] dialogueLines;

        [Header("Wait For Item")]
        public HandPointType requiredHandPointType;

        [Header("NPC / Object")]
        public GameObject sceneObject;
        public GameObject prefab;
        public Transform spawnPoint;
        public bool waitUntilGone = true;

        [Header("Wait")]
        public float duration = 2f;

        [Header("Custom Event")]
        public string eventID;
        public bool waitForCompletion;

        [Header("Callbacks")]
        public UnityEvent onStart;
        public UnityEvent onComplete;
    }

    [Header("Settings")]
    [SerializeField] private bool autoStart = true;

    [Header("References")]
    [SerializeField] private PhoneContactUI phoneUI;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private PickupSystem pickupSystem;

    [Header("Sequence")]
    [SerializeField] private List<DirectorStep> steps = new();

    [Header("Finished")]
    [SerializeField]
    private string finishedObjective =
        "All tasks completed.";

    private int currentStepIndex = -1;
    private bool isRunning;
    private bool isCompleting;

    private GameObject activeObject;
    private DirtCleanable[] trackedDirt;
    private float timer;

    private bool automatedDialogueStarted;
    private bool automatedDialogueSawActive;
    private bool automatedDialogueConfigValid = true;

    private bool hasSeenNonMatchingItem;
    private bool itemWaitValid = true;

    private DirectorStep CurrentStep =>
        currentStepIndex >= 0 && currentStepIndex < steps.Count
            ? steps[currentStepIndex]
            : null;

    private void Start()
    {
        if (autoStart)
            StartSequence();
    }

    private void Update()
    {
        if (!isRunning || isCompleting || CurrentStep == null)
            return;

        switch (CurrentStep.type)
        {
            case StepType.CleanAllDirt:
                CheckDirt();
                break;

            case StepType.SpawnNPC:
            case StepType.SpawnObject:
                CheckObject();
                break;

            case StepType.WaitSeconds:
                timer -= Time.deltaTime;

                if (timer <= 0f)
                    CompleteCurrentStep();
                break;

            case StepType.AutomatedDialogue:
                if (automatedDialogueConfigValid)
                    ProcessAutomatedDialogue();
                break;

            case StepType.WaitForHandPointType:
                if (itemWaitValid)
                    CheckHeldItemType();
                break;
        }
    }

    public void StartSequence()
    {
        if (isRunning)
            return;

        isRunning = true;
        currentStepIndex = -1;

        NextStep();
    }

    private void NextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= steps.Count)
        {
            isRunning = false;

            if (objectiveText != null)
                objectiveText.text = finishedObjective;

            Debug.Log("GameDirector: Sequence finished.");
            return;
        }

        DirectorStep step = CurrentStep;

        activeObject = null;
        trackedDirt = null;
        timer = Mathf.Max(0f, step.duration);

        automatedDialogueStarted = false;
        automatedDialogueSawActive = false;
        automatedDialogueConfigValid = true;

        hasSeenNonMatchingItem = false;
        itemWaitValid = true;

        if (objectiveText != null)
            objectiveText.text = step.objective;

        Debug.Log("GameDirector: " + step.stepName);

        int startedIndex = currentStepIndex;

        step.onStart?.Invoke();

        if (!isRunning || currentStepIndex != startedIndex)
            return;

        switch (step.type)
        {
            case StepType.IncomingCall:
                if (phoneUI != null)
                {
                    phoneUI.StartIncomingCall(
                        step.contactIndex,
                        step.conversationIndex
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "[GameDirector] Phone UI missing!"
                    );
                }
                break;

            case StepType.CleanAllDirt:
                trackedDirt = FindObjectsByType<DirtCleanable>(
                    FindObjectsSortMode.None
                );
                break;

            case StepType.SpawnNPC:
            case StepType.SpawnObject:
                activeObject = SpawnStepObject(step);

                if (!step.waitUntilGone || activeObject == null)
                    CompleteCurrentStep();
                break;

            case StepType.WaitSeconds:
                if (timer <= 0f)
                    CompleteCurrentStep();
                break;

            case StepType.CustomEvent:
                if (!step.waitForCompletion)
                    CompleteCurrentStep();
                break;

            case StepType.AutomatedDialogue:
                if (dialogueManager == null)
                {
                    automatedDialogueConfigValid = false;
                    Debug.LogWarning(
                        "[GameDirector] Dialogue Manager missing!"
                    );
                    break;
                }

                if (step.dialogueLines == null ||
                    step.dialogueLines.Length == 0)
                {
                    automatedDialogueConfigValid = false;
                    Debug.LogWarning(
                        "[GameDirector] No dialogue lines assigned: " +
                        step.stepName
                    );
                    break;
                }

                ProcessAutomatedDialogue();
                break;

            case StepType.WaitForHandPointType:
                if (pickupSystem == null)
                {
                    itemWaitValid = false;
                    Debug.LogWarning(
                        "[GameDirector] Pickup System missing!"
                    );
                    break;
                }

                PickupItem heldItemAtStart =
                    pickupSystem.GetHeldItem();

                // If the required item is already held,
                // require the player to release it and pick it up again.
                hasSeenNonMatchingItem =
                    heldItemAtStart == null ||
                    heldItemAtStart.HandPointType !=
                    step.requiredHandPointType;

                break;
        }
    }

    private void ProcessAutomatedDialogue()
    {
        if (CurrentStep == null ||
            CurrentStep.type != StepType.AutomatedDialogue)
            return;

        // Wait until all existing dialogue and interactions finish.
        if (!automatedDialogueStarted)
        {
            if (dialogueManager.IsDialogueActive)
                return;

            if (dialoguePanel != null &&
                dialoguePanel.activeInHierarchy)
                return;

            dialogueManager.StartDialogue(
                CurrentStep.dialogueLines
            );

            automatedDialogueStarted = true;
            automatedDialogueSawActive =
                dialogueManager.IsDialogueActive;

            Debug.Log(
                "[GameDirector] Automated dialogue started: " +
                CurrentStep.stepName
            );

            return;
        }

        // Keep waiting while the dialogue is running.
        if (dialogueManager.IsDialogueActive)
        {
            automatedDialogueSawActive = true;
            return;
        }

        // Wait for the panel to close completely as well.
        if (dialoguePanel != null &&
            dialoguePanel.activeInHierarchy)
            return;

        // Do not advance before the dialogue has actually run.
        if (automatedDialogueSawActive)
        {
            Debug.Log(
                "[GameDirector] Automated dialogue finished: " +
                CurrentStep.stepName
            );

            CompleteCurrentStep();
        }
    }

    private void CheckHeldItemType()
    {
        if (CurrentStep == null || pickupSystem == null)
            return;

        PickupItem heldItem = pickupSystem.GetHeldItem();

        bool hasRequiredItem =
            heldItem != null &&
            heldItem.HandPointType ==
            CurrentStep.requiredHandPointType;

        if (!hasRequiredItem)
        {
            hasSeenNonMatchingItem = true;
            return;
        }

        if (hasSeenNonMatchingItem)
        {
            Debug.Log(
                "[GameDirector] Required item picked up: " +
                CurrentStep.requiredHandPointType
            );

            CompleteCurrentStep();
        }
    }

    private GameObject SpawnStepObject(DirectorStep step)
    {
        if (step.sceneObject != null)
        {
            step.sceneObject.SetActive(true);
            return step.sceneObject;
        }

        if (step.prefab != null)
        {
            if (step.spawnPoint != null)
            {
                return Instantiate(
                    step.prefab,
                    step.spawnPoint.position,
                    step.spawnPoint.rotation
                );
            }

            return Instantiate(step.prefab);
        }

        Debug.LogWarning(
            "GameDirector: No object assigned for " +
            step.stepName
        );

        return null;
    }

    private void CheckDirt()
    {
        if (trackedDirt == null)
            return;

        foreach (DirtCleanable dirt in trackedDirt)
        {
            if (dirt != null &&
                dirt.gameObject.activeInHierarchy)
                return;
        }

        CompleteCurrentStep();
    }

    private void CheckObject()
    {
        if (!CurrentStep.waitUntilGone)
            return;

        if (activeObject == null ||
            !activeObject.activeInHierarchy)
            CompleteCurrentStep();
    }

    public void NotifyPhoneCallFinished()
    {
        if (CurrentStep?.type == StepType.IncomingCall)
            CompleteCurrentStep();
    }

    public void CompleteCustomEvent(string eventID)
    {
        if (CurrentStep?.type != StepType.CustomEvent)
            return;

        if (CurrentStep.eventID != eventID)
            return;

        CompleteCurrentStep();
    }

    public void CompleteCurrentStep()
    {
        if (!isRunning || isCompleting || CurrentStep == null)
            return;

        isCompleting = true;

        CurrentStep.onComplete?.Invoke();

        isCompleting = false;

        NextStep();
    }
}
