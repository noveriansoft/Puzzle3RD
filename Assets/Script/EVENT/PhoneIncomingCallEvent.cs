using UnityEngine;

public class PhoneIncomingCallEvent : MonoBehaviour
{
    [Header("Phone")]
    [SerializeField] private PhoneContactUI phoneContactUI;

    [Header("Event")]
    [SerializeField] private int contactIndex;
    [SerializeField] private int conversationIndex;

    [Header("Player")]
    [SerializeField] private GameObject player;

    [SerializeField] private bool oneTimeOnly = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneTimeOnly)
            return;

        if (other.gameObject != player)
            return;

        phoneContactUI.StartIncomingCall(
            contactIndex,
            conversationIndex
        );

        hasTriggered = true;
    }
}