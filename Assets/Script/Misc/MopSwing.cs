using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class MopSwing : MonoBehaviour
{
    [Header("Broom")]
    [SerializeField] private GameObject broomObject;
    [SerializeField] private Transform broomTransform;

    [Header("Rotation")]
    [SerializeField] private float startAngle = -78f;
    [SerializeField] private float endAngle = -84f;
    [SerializeField] private float speed = 8f;

    [Header("Sound Effects")]
    [SerializeField] private int mopSEIndex = 0;

    [Header("Cleaning")]
    [SerializeField] private DirtCleanable currentDirt;

    private AudioSource mopAudioSource;

    private void Awake()
    {
        mopAudioSource = GetComponent<AudioSource>();
        mopAudioSource.playOnAwake = false;
        mopAudioSource.loop = true;
        mopAudioSource.spatialBlend = 0f;
        mopAudioSource.Stop();
    }

    private void Update()
    {
        if (PauseManager.IsPaused)
        {
            StopMopSound();
            return;
        }

        bool isMopping =
            broomObject != null &&
            broomObject.activeInHierarchy &&
            broomTransform != null &&
            Mouse.current != null &&
            Mouse.current.leftButton.isPressed;

        if (!isMopping)
        {
            StopMopSound();
            return;
        }

        PlayMopSound();

        float t = Mathf.PingPong(Time.time * speed, 1f);
        float angle = Mathf.Lerp(startAngle, endAngle, t);

        Vector3 rotation = broomTransform.localEulerAngles;
        rotation.x = angle;
        broomTransform.localEulerAngles = rotation;

        if (currentDirt != null &&
            currentDirt.gameObject.activeInHierarchy)
        {
            currentDirt.Clean(Time.deltaTime);
        }
    }

    private void PlayMopSound()
    {      
        if (SEManager.Instance == null)
        {
            StopMopSound();
            return;
        }

        mopAudioSource.volume = SEManager.Instance.Volume;

        if (mopAudioSource.isPlaying)
            return;

        AudioClip clip = SEManager.Instance.GetSEClip(mopSEIndex);

        if (clip == null)
            return;

        mopAudioSource.clip = clip;
        mopAudioSource.Play();
    }

    private void StopMopSound()
    {
        if (mopAudioSource != null && mopAudioSource.isPlaying)
            mopAudioSource.Stop();
    }

    private void OnDisable()
    {
        StopMopSound();
        currentDirt = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        DirtCleanable dirt = other.GetComponent<DirtCleanable>();

        if (dirt != null)
            currentDirt = dirt;
    }

    private void OnTriggerExit(Collider other)
    {
        DirtCleanable dirt = other.GetComponent<DirtCleanable>();

        if (dirt != null && dirt == currentDirt)
            currentDirt = null;
    }
}