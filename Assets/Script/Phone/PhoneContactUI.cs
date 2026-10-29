using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PhoneContactUI : MonoBehaviour
{
    [System.Serializable]
    public class ContactData
    {
        public string contactName;
        public string phoneNumber;

        [Header("Availability")]
        public bool showInContacts = true;

        [Header("Phone Dialogues")]
        public PhoneDialogueData[] dialogues;

        public bool advanceDialogueAfterCall = false;

        [HideInInspector]
        public int currentDialogueIndex;
    }

    private enum PhoneState
    {
        ContactList,
        IncomingCall,
        Calling,
        InCall
    }

    private int incomingContactIndex = -1;
    private int incomingDialogueIndex = 0;

    [Header("Contacts")]
    [SerializeField] private List<ContactData> contacts = new List<ContactData>();

    [Header("Contact List UI")]
    [SerializeField] private GameObject contactListPanel;
    [SerializeField] private TMP_Text txt_Contacts;

    [Header("Call UI")]
    [SerializeField] private GameObject callPanel;
    [SerializeField] private TMP_Text txt_CallStatus;
    [SerializeField] private TMP_Text txt_CallName;
    [SerializeField] private TMP_Text txt_CallNumber;

    [Header("Display")]
    [SerializeField] private int visibleContacts = 2;

    [Header("Call Settings")]
    [SerializeField] private float connectDelay = 2f;

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Pickup")]
    [SerializeField] private PickupSystem pickupSystem;

    private bool callAudioPausedByGame;
    private int selectedIndex = 0;
    private int firstVisibleIndex = 0;
    private List<int> visibleContactIndices = new List<int>();

    private PhoneState currentState = PhoneState.ContactList;

    [Header("Phone Hold")]
    [SerializeField] private Transform phoneTransform;
    [SerializeField] private Key togglePhoneKey = Key.Tab;

    [SerializeField] private float pocketY = -0.450f;
    [SerializeField] private float heldY = -0.004015446f;

    [SerializeField] private float moveSpeed = 2f;

    private bool isPhoneHeld = false;
    private Vector3 phoneTargetPosition;

    [Header("Phone Audio")]
    [SerializeField] private AudioSource callAudioSource;
    [SerializeField] private AudioSource uiAudioSource;

    [SerializeField] private AudioClip incomingCallSFX;
    [SerializeField] private AudioClip outgoingCallSFX;
    [SerializeField] private AudioClip connectedSFX;
    [SerializeField] private AudioClip clickSFX;

    [SerializeField] private GameDirector gameDirector;

    public static PhoneContactUI Instance { get; private set; }
    public bool IsPhoneHeld => isPhoneHeld;

    private void Awake()
    {
        Instance = this;

        // INITIAL PHONE STATE
        currentState = PhoneState.ContactList;

        contactListPanel.SetActive(true);
        callPanel.SetActive(false);

        // HP awalnya dikantongin
        isPhoneHeld = false;

        Vector3 pos = phoneTransform.localPosition;
        pos.y = pocketY;
        phoneTransform.localPosition = pos;

        phoneTargetPosition = pos;

        RefreshVisibleContacts();
    }

    private void Start()
    {
        //contactListPanel.SetActive(true);
        //callPanel.SetActive(false);

        //// HP awalnya dikantongin
        //isPhoneHeld = false;

        //Vector3 pos = phoneTransform.localPosition;
        //pos.y = pocketY;
        //phoneTransform.localPosition = pos;

        //phoneTargetPosition = pos;

        ////UpdateUI();
        //RefreshVisibleContacts();
    }

    private void Update()
    {
        if (PauseManager.IsPaused)
            return;

        phoneTransform.localPosition = Vector3.MoveTowards(
            phoneTransform.localPosition,
            phoneTargetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Keyboard.current != null &&
            Keyboard.current[togglePhoneKey].wasPressedThisFrame)
        {
            TogglePhone();
        }

        // HP harus sedang dipegang
        if (!isPhoneHeld)
            return;

        if (Mouse.current == null)
            return;


        // =========================
        // INCOMING CALL
        // =========================

        if (currentState == PhoneState.IncomingCall)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                AnswerIncomingCall();
            }

            return;
        }


        // =========================
        // CONTACT LIST
        // =========================

        if (currentState != PhoneState.ContactList)
            return;

        //if (contacts.Count == 0)
        //    return;

        if (visibleContactIndices.Count == 0)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll > 0)
        {
            MoveSelection(-1);
        }
        else if (scroll < 0)
        {
            MoveSelection(1);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SelectContact();
        }
    }

    private void TogglePhone()
    {
        if (PauseManager.IsPaused)
            return;

        if (currentState != PhoneState.ContactList &&
            currentState != PhoneState.IncomingCall)
            return;

        // Hanya cek saat mau mengeluarkan HP
        if (!isPhoneHeld)
        {
            // Tidak boleh mengeluarkan HP saat memegang barang
            if (pickupSystem != null && pickupSystem.HasItem())
                return;

            // Tidak boleh mengeluarkan HP saat dialog aktif
            if (dialogueManager != null &&
                dialogueManager.IsDialogueActive)
                return;
        }

        isPhoneHeld = !isPhoneHeld;

        Vector3 target = phoneTransform.localPosition;

        if (isPhoneHeld)
        {
            target.y = heldY;
        }
        else
        {
            target.y = pocketY;
        }

        phoneTargetPosition = target;
    }

    private void MoveSelection(int direction)
    {
        if (visibleContactIndices.Count == 0)
            return;

        selectedIndex += direction;

        if (selectedIndex < 0)
            selectedIndex = visibleContactIndices.Count - 1;

        if (selectedIndex >= visibleContactIndices.Count)
            selectedIndex = 0;

        PlayClickSFX();

        UpdateScroll();
        UpdateUI();
    }

    private void UpdateScroll()
    {
        if (visibleContactIndices.Count == 0)
            return;

        if (selectedIndex < firstVisibleIndex)
        {
            firstVisibleIndex = selectedIndex;
        }

        if (selectedIndex >= firstVisibleIndex + visibleContacts)
        {
            firstVisibleIndex = selectedIndex - visibleContacts + 1;
        }

        if (selectedIndex == 0)
        {
            firstVisibleIndex = 0;
        }

        if (selectedIndex == visibleContactIndices.Count - 1)
        {
            firstVisibleIndex =
                Mathf.Max(0, visibleContactIndices.Count - visibleContacts);
        }
    }

    private void UpdateUI()
    {
        StringBuilder text = new StringBuilder();

        if (visibleContactIndices.Count == 0)
        {
            txt_Contacts.text = "";
            return;
        }

        int endIndex = Mathf.Min(
            firstVisibleIndex + visibleContacts,
            visibleContactIndices.Count
        );

        for (int i = firstVisibleIndex; i < endIndex; i++)
        {
            int contactIndex = visibleContactIndices[i];
            ContactData contact = contacts[contactIndex];

            if (i == selectedIndex)
                text.Append("> ");
            else
                text.Append("  ");

            text.Append(contact.contactName);

            if (i < endIndex - 1)
                text.AppendLine();
        }

        txt_Contacts.text = text.ToString();
    }

    private void SelectContact()
    {
        if (visibleContactIndices.Count == 0)
            return;

        PlayClickSFX();

        int contactIndex = visibleContactIndices[selectedIndex];
        ContactData contact = contacts[contactIndex];

        StartCoroutine(CallRoutine(contact));
    }

    private IEnumerator CallRoutine(ContactData contact)
    {
        currentState = PhoneState.Calling;

        contactListPanel.SetActive(false);
        callPanel.SetActive(true);

        txt_CallStatus.text = "Calling...";
        txt_CallName.text = contact.contactName;
        txt_CallNumber.text = contact.phoneNumber;

        StartCallLoop(outgoingCallSFX);

        yield return new WaitForSeconds(connectDelay);

        StopCallLoop();

        currentState = PhoneState.InCall;
        txt_CallStatus.text = "Connected";

        if (dialogueManager != null &&
            contact.dialogues != null &&
            contact.dialogues.Length > 0)
        {
            int playedIndex = Mathf.Clamp(
                contact.currentDialogueIndex,
                0,
                contact.dialogues.Length - 1
            );

            contact.currentDialogueIndex = playedIndex;

            PhoneDialogueData dialogue = contact.dialogues[playedIndex];

            if (dialogue != null &&
                dialogue.dialogueLines != null &&
                dialogue.dialogueLines.Length > 0)
            {
                dialogueManager.StartDialogue(dialogue.dialogueLines);

                yield return new WaitUntil(
                    () => !dialogueManager.IsDialogueActive
                );

                // Maju setelah percakapan selesai, mentok di dialog terakhir.
                if (dialogue.advanceToNextDialogue)
                {
                    contact.currentDialogueIndex = Mathf.Min(
                        playedIndex + 1,
                        contact.dialogues.Length - 1
                    );
                }
            }
        }

        ReturnToContactList();
    }

    private void ReturnToContactList()
    {
        currentState = PhoneState.ContactList;

        callPanel.SetActive(false);
        contactListPanel.SetActive(true);

        UpdateUI();
    }

    public void StartIncomingCall(int contactIndex, int dialogueIndex)
    {
        if (contactIndex < 0 || contactIndex >= contacts.Count)
        {
            Debug.LogWarning(
                "[Phone] Invalid incoming contact index: " + contactIndex
            );
            return;
        }

        // Jangan mulai incoming call kalau HP sedang dalam call lain
        if (currentState != PhoneState.ContactList)
        {
            Debug.LogWarning(
                "[Phone] Incoming call ignored. Current state: " +
                currentState
            );
            return;
        }

        ContactData contact = contacts[contactIndex];

        if (contact.dialogues == null ||
            contact.dialogues.Length == 0)
        {
            Debug.LogWarning(
                "[Phone] Contact has no dialogues: " +
                contact.contactName
            );
            return;
        }

        incomingContactIndex = contactIndex;

        incomingDialogueIndex = Mathf.Clamp(
            dialogueIndex,
            0,
            contact.dialogues.Length - 1
        );

        currentState = PhoneState.IncomingCall;

        contactListPanel.SetActive(false);
        callPanel.SetActive(true);

        txt_CallStatus.text = "Incoming Call";
        txt_CallName.text = contact.contactName;
        txt_CallNumber.text = contact.phoneNumber;

        StartCallLoop(incomingCallSFX);

        Debug.Log(
            "[Phone] Incoming call started: " +
            contact.contactName
        );
    }

    private void AnswerIncomingCall()
    {
        if (incomingContactIndex < 0 ||
            incomingContactIndex >= contacts.Count)
            return;

        StopCallLoop();
        PlayClickSFX();

        ContactData contact = contacts[incomingContactIndex];

        StartCoroutine(IncomingCallRoutine(contact));
    }

    private IEnumerator IncomingCallRoutine(ContactData contact)
    {
        currentState = PhoneState.InCall;

        txt_CallStatus.text = "Connected";

        //PlayConnectedSFX();

        // Tunggu klik yang dipakai untuk angkat telepon dilepas
        if (Mouse.current != null)
        {
            yield return new WaitUntil(
                () => !Mouse.current.leftButton.isPressed
            );
        }

        // Tunggu 1 frame tambahan supaya input klik sebelumnya benar-benar selesai
        yield return null;

        // Incoming call
        PhoneDialogueData dialogue =
            contact.dialogues[incomingDialogueIndex];

        if (dialogue != null)
        {
            dialogueManager.StartDialogue(
                dialogue.dialogueLines
            );

            yield return new WaitUntil(
                () => !dialogueManager.IsDialogueActive
            );
        }

        // Setelah incoming call selesai,
        // kalau player menelepon balik → dialogue kedua
        if (contact.dialogues != null &&
            contact.dialogues.Length > 0)
        {
            contact.currentDialogueIndex = Mathf.Min(
                incomingDialogueIndex + 1,
                contact.dialogues.Length - 1
            );
        }

        incomingContactIndex = -1;

        ReturnToContactList();

        gameDirector?.NotifyPhoneCallFinished();
    }

    public void SetDialogueStage(int contactIndex, int dialogueIndex)
    {
        //phoneContactUI.SetDialogueStage(0, 1);
        //0 = Contact Name
        //1 = Dialogue

        if (contactIndex < 0 || contactIndex >= contacts.Count)
            return;

        ContactData contact = contacts[contactIndex];

        contact.currentDialogueIndex = Mathf.Clamp(
            dialogueIndex,
            0,
            contact.dialogues.Length - 1
        );
    }

    #region Audio Helper
    private void PlayClickSFX()
    {
        if (uiAudioSource != null && clickSFX != null)
            uiAudioSource.PlayOneShot(clickSFX);
    }

    private void PlayConnectedSFX()
    {
        if (uiAudioSource != null && connectedSFX != null)
            uiAudioSource.PlayOneShot(connectedSFX);
    }

    private void StartCallLoop(AudioClip clip)
    {
        if (callAudioSource == null || clip == null)
            return;

        callAudioSource.Stop();

        callAudioSource.clip = clip;
        callAudioSource.loop = true;
        callAudioSource.Play();
    }

    private void StopCallLoop()
    {
        callAudioPausedByGame = false;

        if (callAudioSource == null)
            return;

        callAudioSource.Stop();
        callAudioSource.loop = false;
        callAudioSource.clip = null;
    }
    #endregion

    private void RefreshVisibleContacts()
    {
        visibleContactIndices.Clear();

        for (int i = 0; i < contacts.Count; i++)
        {
            if (contacts[i].showInContacts)
            {
                visibleContactIndices.Add(i);
            }
        }

        if (visibleContactIndices.Count == 0)
        {
            selectedIndex = 0;
            firstVisibleIndex = 0;
        }
        else
        {
            selectedIndex = Mathf.Clamp(
                selectedIndex,
                0,
                visibleContactIndices.Count - 1
            );

            firstVisibleIndex = Mathf.Clamp(
                firstVisibleIndex,
                0,
                Mathf.Max(0, visibleContactIndices.Count - visibleContacts)
            );
        }

        UpdateUI();
    }

    public void SetContactVisible(int contactIndex, bool visible)
    {
        if (contactIndex < 0 || contactIndex >= contacts.Count)
            return;

        contacts[contactIndex].showInContacts = visible;

        RefreshVisibleContacts();
    }

    public void UnlockContact(int contactIndex)
    {
        SetContactVisible(contactIndex, true);
    }

    public void LockContact(int contactIndex)
    {
        SetContactVisible(contactIndex, false);
    }

    public void AdvanceContactDialogue(int contactIndex)
    {
        if (contactIndex < 0 || contactIndex >= contacts.Count)
            return;

        ContactData contact = contacts[contactIndex];

        if (contact.dialogues == null || contact.dialogues.Length == 0)
            return;

        contact.currentDialogueIndex = Mathf.Clamp(
            contact.currentDialogueIndex + 1,
            0,
            contact.dialogues.Length - 1
        );
    }

    public void SetCallAudioPaused(bool paused)
    {
        if (callAudioSource == null)
            return;

        if (paused)
        {
            if (callAudioPausedByGame)
                return;

            callAudioPausedByGame = callAudioSource.isPlaying;

            if (callAudioPausedByGame)
                callAudioSource.Pause();
        }
        else
        {
            if (!callAudioPausedByGame)
                return;

            callAudioSource.UnPause();
            callAudioPausedByGame = false;
        }
    }

    #region checkpoint save/load
    [System.Serializable]
    private class ContactCheckpointState
    {
        public string phoneNumber;
        public int dialogueIndex;
        public bool showInContacts;
    }

    [System.Serializable]
    private class PhoneCheckpointData
    {
        public List<ContactCheckpointState> contacts =
            new List<ContactCheckpointState>();
    }

    public string CaptureCheckpointData()
    {
        PhoneCheckpointData data = new PhoneCheckpointData();
        HashSet<string> usedNumbers = new HashSet<string>();

        foreach (ContactData contact in contacts)
        {
            if (contact == null)
                continue;

            if (string.IsNullOrWhiteSpace(contact.phoneNumber))
            {
                Debug.LogWarning(
                    $"Phone number is missing for contact: {contact.contactName}",
                    this
                );
                continue;
            }

            if (!usedNumbers.Add(contact.phoneNumber))
            {
                Debug.LogError(
                    $"Duplicate contact phone number: {contact.phoneNumber}",
                    this
                );
                continue;
            }

            data.contacts.Add(new ContactCheckpointState
            {
                phoneNumber = contact.phoneNumber,
                dialogueIndex = contact.currentDialogueIndex,
                showInContacts = contact.showInContacts
            });
        }

        return JsonUtility.ToJson(data);
    }

    public void RestoreCheckpointData(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        PhoneCheckpointData data =
            JsonUtility.FromJson<PhoneCheckpointData>(json);

        if (data == null || data.contacts == null)
            return;

        foreach (ContactData contact in contacts)
        {
            if (contact == null ||
                string.IsNullOrWhiteSpace(contact.phoneNumber))
            {
                continue;
            }

            foreach (ContactCheckpointState saved in data.contacts)
            {
                if (saved.phoneNumber != contact.phoneNumber)
                    continue;

                int dialogueCount = contact.dialogues != null
                    ? contact.dialogues.Length
                    : 0;

                contact.currentDialogueIndex = dialogueCount > 0
                    ? Mathf.Clamp(saved.dialogueIndex, 0, dialogueCount - 1)
                    : 0;

                contact.showInContacts = saved.showInContacts;
                break;
            }
        }

        RefreshVisibleContacts();
    }
    #endregion
}