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
    
    // Ses kaynağı havuzu ayarları
    private const int AUDIO_SOURCE_COUNT = 10;
    private const float DEFAULT_PITCH = 1f;
    private const float DEFAULT_GRID_PROGRESS_INCREMENT = 0.04f;
    
    private AudioSource[] audioSources;
    private int currentAudioSourceIndex = 0;
    private float currentGridProgress = 0f;
    
    private void Awake()
    {
        Instance = this;
        InitializeAudioSources();
    }

    /// <summary>
    /// Ses kaynaklarını oluşturur ve yapılandırır
    /// </summary>
    private void InitializeAudioSources()
    {
        audioSources = new AudioSource[AUDIO_SOURCE_COUNT];
        
        for (int i = 0; i < AUDIO_SOURCE_COUNT; i++)
        {
            CreateAudioSource(i);
        }
    }

    /// <summary>
    /// Yeni bir ses kaynağı oluşturur ve yapılandırır
    /// </summary>
    private void CreateAudioSource(int index)
    {
        audioSources[index] = gameObject.AddComponent<AudioSource>();
        ConfigureAudioSource(audioSources[index]);
    }

    /// <summary>
    /// Ses kaynağının temel ayarlarını yapar
    /// </summary>
    private void ConfigureAudioSource(AudioSource source)
    {
        source.playOnAwake = false;
    }
    
    /// <summary>
    /// Grid oluşturma ilerlemesini sıfırlar
    /// </summary>
    public void ResetGridProgress()
    {
        currentGridProgress = 0f;
    }
    
    /// <summary>
    /// Kullanılabilir bir ses kaynağı alır
    /// </summary>
    private AudioSource GetNextAudioSource()
    {
        AudioSource source = audioSources[currentAudioSourceIndex];
        currentAudioSourceIndex = (currentAudioSourceIndex + 1) % AUDIO_SOURCE_COUNT;
        return source;
    }
    
    /// <summary>
    /// Ses klibini hemen çalar
    /// </summary>
    private void PlaySound(AudioClip clip, float volume, float pitch = DEFAULT_PITCH)
    {
        if (clip == null) return;
        
        var source = GetNextAudioSource();
        ConfigureAndPlaySound(source, clip, volume, pitch);
    }

    /// <summary>
    /// Ses kaynağını yapılandırır ve çalar
    /// </summary>
    private void ConfigureAndPlaySound(AudioSource source, AudioClip clip, float volume, float pitch)
    {
        source.clip = clip;
        source.volume = volume * masterVolume;
        source.pitch = pitch;
        source.Play();
    }
    
    /// <summary>
    /// Ses klibini gecikmeli olarak çalar
    /// </summary>
    private void PlaySoundDelayed(AudioClip clip, float volume, float delay, float pitch = DEFAULT_PITCH)
    {
        if (clip == null) return;
        DOVirtual.DelayedCall(delay, () => PlaySound(clip, volume, pitch));
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
    public void PlayGridCreateSound(float delay = 0f, float progressIncrement = DEFAULT_GRID_PROGRESS_INCREMENT)
    {
        float currentPitch = CalculateGridPitch();
        PlaySoundDelayed(gridCreateSound, gridCreateVolume, delay, currentPitch);
        UpdateGridProgress(progressIncrement);
    }

    /// <summary>
    /// Mevcut ilerlemeye göre pitch değerini hesaplar
    /// </summary>
    private float CalculateGridPitch()
    {
        return Mathf.Lerp(startPitch, maxPitch, currentGridProgress);
    }

    /// <summary>
    /// Grid oluşturma ilerlemesini günceller
    /// </summary>
    private void UpdateGridProgress(float increment)
    {
        currentGridProgress = Mathf.Min(1f, currentGridProgress + increment);
    }
}
