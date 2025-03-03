using UnityEngine;
using DG.Tweening;

/// <summary>
/// Oyun içindeki ses efektlerini yöneten basit sınıf
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip lineDrawSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip gridCreateSound;
    
    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float drawVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float completeVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float gridCreateVolume = 0.5f;
    
    [Header("Pitch Settings")]
    [SerializeField] [Range(0.5f, 1.5f)] private float startPitch = 0.7f;
    [SerializeField] [Range(1f, 2f)] private float maxPitch = 1.5f;
    
    private AudioSource[] audioSources;
    private int currentAudioSourceIndex = 0;
    private const int AUDIO_SOURCE_COUNT = 10; // Aynı anda çalabilecek maksimum ses sayısı
    private float currentGridProgress = 0f; // Grid oluşturma ilerlemesi (0-1 arası)
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        audioSources = new AudioSource[AUDIO_SOURCE_COUNT];
        for (int i = 0; i < AUDIO_SOURCE_COUNT; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
        }
    }
    
    /// <summary>
    /// Grid oluşturma ilerlemesini sıfırlar
    /// </summary>
    public void ResetGridProgress()
    {
        currentGridProgress = 0f;
    }
    
    /// <summary>
    /// Kullanılabilir bir AudioSource komponenti alır
    /// </summary>
    private AudioSource GetAvailableAudioSource()
    {
        // Sıradaki AudioSource'u al ve indeksi güncelle
        AudioSource source = audioSources[currentAudioSourceIndex];
        currentAudioSourceIndex = (currentAudioSourceIndex + 1) % AUDIO_SOURCE_COUNT;
        return source;
    }
    
    /// <summary>
    /// Belirtilen ses klibini çalar
    /// </summary>
    private void PlaySound(AudioClip clip, float volume, float pitch = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource();
        source.clip = clip;
        source.volume = volume * masterVolume;
        source.pitch = pitch;
        source.Play();
    }
    
    /// <summary>
    /// Belirtilen ses klibini gecikmeli olarak çalar
    /// </summary>
    private void PlaySoundDelayed(AudioClip clip, float volume, float delay, float pitch = 1f)
    {
        if (clip == null) return;
        
        DOVirtual.DelayedCall(delay, () => {
            PlaySound(clip, volume, pitch);
        });
    }
    
    /// <summary>
    /// X çizme sesini çalar
    /// </summary>
    public void PlayDrawSound()
    {
        PlaySound(lineDrawSound, drawVolume);
    }
    
    /// <summary>
    /// X tamamlanma sesini çalar
    /// </summary>
    public void PlayCompleteSound()
    {
        PlaySound(completeSound, completeVolume);
    }
    
    /// <summary>
    /// Grid oluşturma sesini artan pitch ile çalar
    /// </summary>
    public void PlayGridCreateSound(float delay = 0f, float progressIncrement = 0.04f)
    {
        float currentPitch = Mathf.Lerp(startPitch, maxPitch, currentGridProgress);
        PlaySoundDelayed(gridCreateSound, gridCreateVolume, delay, currentPitch);
        currentGridProgress = Mathf.Min(1f, currentGridProgress + progressIncrement);
    }
}
