using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Oyun içindeki tüm ses efektlerini yöneten merkezi sınıf
/// </summary>
public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Settings")]
    [SerializeField] private int initialPoolSize = 5;
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField] private bool dontDestroyOnLoad = false; // Sahneler arası geçişte korunup korunmayacağı
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip lineDrawSound;
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip gridCreateSound;
    [SerializeField] [Range(0f, 1f)] private float drawVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float completeVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float gridCreateVolume = 0.5f;
    
    [Header("Pitch Settings")]
    [SerializeField] private float startPitch = 0.8f;
    [SerializeField] private float endPitch = 1.5f;
    [SerializeField] private float pitchStepPerCell = 0.05f;
    
    // AudioSource havuzu
    private List<AudioSource> audioSourcePool;
    private AudioSource pitchControlSource; // Pitch değişimi için özel AudioSource
    
    private void Awake()
    {
        // Singleton pattern uygulama
        if (Instance == null)
        {
            Instance = this;
            
            // Eğer dontDestroyOnLoad seçeneği aktifse, sahneler arası geçişte koru
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            InitializeAudioPool();
        }
        else
        {
            // Eğer zaten bir instance varsa, bu objeyi yok et
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// AudioSource havuzunu başlatır
    /// </summary>
    private void InitializeAudioPool()
    {
        audioSourcePool = new List<AudioSource>();
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
        
        // Pitch kontrolü için özel bir AudioSource oluştur
        pitchControlSource = gameObject.AddComponent<AudioSource>();
        pitchControlSource.playOnAwake = false;
        pitchControlSource.loop = false;
    }
    
    /// <summary>
    /// Yeni bir AudioSource oluşturur ve havuza ekler
    /// </summary>
    private AudioSource CreateNewAudioSource()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSourcePool.Add(audioSource);
        return audioSource;
    }
    
    /// <summary>
    /// Havuzdan kullanılabilir bir AudioSource alır
    /// </summary>
    private AudioSource GetAvailableAudioSource()
    {
        // Kullanılmayan bir AudioSource bul
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // Eğer tüm AudioSource'lar kullanılıyorsa, yeni bir tane oluştur
        return CreateNewAudioSource();
    }
    
    /// <summary>
    /// Belirtilen ses klibini çalar
    /// </summary>
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource();
        source.clip = clip;
        source.volume = volume * masterVolume;
        source.pitch = 1f; // Normal pitch
        source.Play();
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
    /// Grid oluşturma sesini, giderek artan pitch değeriyle çalar
    /// </summary>
    public void PlayGridCreateSoundWithPitch(int totalCells)
    {
        if (gridCreateSound == null || pitchControlSource == null) return;
        
        // Mevcut çalan sesi durdur
        if (pitchControlSource.isPlaying)
        {
            pitchControlSource.Stop();
        }
        
        // Coroutine'i başlat
        StartCoroutine(PlaySoundWithRisingPitch(totalCells));
    }
    
    /// <summary>
    /// Artan pitch değeriyle ses çalma coroutine'i
    /// </summary>
    private IEnumerator PlaySoundWithRisingPitch(int totalCells)
    {
        pitchControlSource.clip = gridCreateSound;
        pitchControlSource.volume = gridCreateVolume * masterVolume;
        pitchControlSource.loop = true;
        pitchControlSource.pitch = startPitch;
        pitchControlSource.Play();
        
        float currentPitch = startPitch;
        float targetPitch = Mathf.Min(endPitch, startPitch + (totalCells * pitchStepPerCell));
        
        while (currentPitch < targetPitch)
        {
            currentPitch += pitchStepPerCell;
            pitchControlSource.pitch = currentPitch;
            yield return new WaitForSeconds(0.05f); // Pitch değişim hızı
        }
        
        // Son pitch değerine ulaştıktan sonra biraz bekle ve sesi kapat
        yield return new WaitForSeconds(0.2f);
        
        // Sesi yavaşça kapat
        float currentVolume = pitchControlSource.volume;
        while (currentVolume > 0.05f)
        {
            currentVolume -= 0.05f;
            pitchControlSource.volume = currentVolume;
            yield return new WaitForSeconds(0.02f);
        }
        
        pitchControlSource.Stop();
        pitchControlSource.loop = false;
    }
}
