using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SEManager : MonoBehaviour
{
    public static SEManager Instance { get; private set; }

    [Header("Sound Effect List")]
    [SerializeField] private AudioClip[] seClips;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    public float Volume => volume;

    private AudioSource audioSource;
    private AudioSource loopAudioSource;
    private int currentLoopIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;

        loopAudioSource = gameObject.AddComponent<AudioSource>();
        loopAudioSource.playOnAwake = false;
        loopAudioSource.loop = true;
        loopAudioSource.spatialBlend = 0f;
        loopAudioSource.volume = volume;
    }

    public void PlaySE(int index)
    {
        AudioClip clip = GetSEClip(index);

        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    public void PlayLoopSE(int index)
    {
        AudioClip clip = GetSEClip(index);

        if (clip == null)
            return;

        // Do not restart a loop that is already playing.
        if (currentLoopIndex == index && loopAudioSource.isPlaying)
            return;

        loopAudioSource.Stop();
        loopAudioSource.clip = clip;
        currentLoopIndex = index;
        loopAudioSource.Play();
    }

    public void StopLoopSE()
    {
        loopAudioSource.Stop();
        loopAudioSource.clip = null;
        currentLoopIndex = -1;
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);

        audioSource.volume = volume;
        loopAudioSource.volume = volume;
    }

    public AudioClip GetSEClip(int index)
    {
        if (seClips == null || index < 0 || index >= seClips.Length)
        {
            Debug.LogWarning($"SE index {index} is unavailable.", this);
            return null;
        }

        AudioClip clip = seClips[index];

        if (clip == null)
        {
            Debug.LogWarning($"SE clip at index {index} is missing.", this);
        }

        return clip;
    }

    private void OnDisable()
    {
        if (loopAudioSource != null)
        {
            loopAudioSource.Stop();
            loopAudioSource.clip = null;
        }

        currentLoopIndex = -1;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}