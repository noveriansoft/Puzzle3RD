using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class ObjectSwapTrigger : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject[] objectsToHide;
    [SerializeField] private GameObject[] objectsToShow;

    [Header("Sound Effect")]
    [SerializeField] private AudioClip triggerSFX;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    [Header("Checkpoint")]
    [SerializeField] private string checkpointID = "ObjectSwap01";

    public string CheckpointID => checkpointID;
    public bool HasTriggered => hasTriggered;


    private AudioSource audioSource;
    private bool hasTriggered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer =
            other.CompareTag(playerTag) ||
            other.transform.root.CompareTag(playerTag);

        if (!isPlayer)
            return;

        if (triggerOnce && hasTriggered)
            return;

        hasTriggered = true;

        SetObjectsActive(objectsToHide, false);
        SetObjectsActive(objectsToShow, true);

        if (triggerSFX != null)
            audioSource.PlayOneShot(triggerSFX, volume);
    }

    private void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        foreach (GameObject target in objects)
        {
            if (target != null)
                target.SetActive(active);
        }
    }

    public void RestoreTriggeredState()
    {
        hasTriggered = true;

        SetObjectsActive(objectsToHide, false);
        SetObjectsActive(objectsToShow, true);
    }
}