using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Music List")]
    [SerializeField] private AudioClip[] bgmClips;

    [Header("Starting Music")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private int startingBGMIndex = 0;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource audioSource;
    private int currentBGMIndex = -1;

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
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
    }

    private void Start()
    {
        if (playOnStart)
            PlayBGM(startingBGMIndex);
    }

    public void PlayBGM(int index)
    {
       //BGMManager.Instance.PlayBGM(1);

        if (index < 0 || index >= bgmClips.Length)
        {
            Debug.LogWarning($"BGM index {index} tidak tersedia.", this);
            return;
        }

        AudioClip clip = bgmClips[index];

        if (clip == null)
        {
            Debug.LogWarning($"BGM pada index {index} belum diisi.", this);
            return;
        }

        // Musik yang sama tidak diulang dari awal.
        if (currentBGMIndex == index && audioSource.isPlaying)
            return;

        currentBGMIndex = index;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopBGM()
    {
        audioSource.Stop();
        currentBGMIndex = -1;

        //BGMManager.Instance.StopBGM();
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
        audioSource.volume = volume;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}