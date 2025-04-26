using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    // Instancia única (Singleton)
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Música")]
    [SerializeField] private AudioSource musicSource;

    [Header("Efectos de Sonido")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips de Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip menuOpenSound;

    private Coroutine musicTransitionCoroutine; // ToHandle music transitions

    // Nombres de los parámetros expuestos en el AudioMixer
    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";

    // Para evitar llamadas repetidas
    private float lastMasterVolume;
    private float lastMusicVolume;
    private float lastSfxVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Inicializar valores
        lastMasterVolume = 1f;
        lastMusicVolume = 1f;
        lastSfxVolume = 1f;
        
        // Cargar volúmenes guardados al iniciar
        LoadVolume();
    }

    private void LoadVolume()
    {
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME, 1f);
        
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
    
    public void SetMasterVolume(float volume)
    {
        // Evitar procesamiento innecesario
        if (Mathf.Approximately(lastMasterVolume, volume)) return;
        lastMasterVolume = volume;
        
        // Convertir de escala lineal (0-1) a logarítmica (dB)
        float volumeDb = ConvertToDecibel(volume);
        
        bool success = audioMixer.SetFloat(MASTER_VOLUME, volumeDb);
        Debug.Log($"Setting Master Volume: {volume} (linear) -> {volumeDb} dB, Success: {success}");
        
        PlayerPrefs.SetFloat(MASTER_VOLUME, volume);
        PlayerPrefs.Save();
    }
    
    public void SetMusicVolume(float volume)
    {
        // Evitar procesamiento innecesario
        if (Mathf.Approximately(lastMusicVolume, volume)) return;
        lastMusicVolume = volume;
        
        // Convertir de escala lineal (0-1) a logarítmica (dB)
        float volumeDb = ConvertToDecibel(volume);
        
        bool success = audioMixer.SetFloat(MUSIC_VOLUME, volumeDb);
        Debug.Log($"Setting Music Volume: {volume} (linear) -> {volumeDb} dB, Success: {success}");
        
        PlayerPrefs.SetFloat(MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
        
        // También ajustar el volumen del AudioSource directamente como respaldo
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        // Evitar procesamiento innecesario
        if (Mathf.Approximately(lastSfxVolume, volume)) return;
        lastSfxVolume = volume;
        
        // Convertir de escala lineal (0-1) a logarítmica (dB)
        float volumeDb = ConvertToDecibel(volume);
        
        bool success = audioMixer.SetFloat(SFX_VOLUME, volumeDb);
        Debug.Log($"Setting SFX Volume: {volume} (linear) -> {volumeDb} dB, Success: {success}");
        
        PlayerPrefs.SetFloat(SFX_VOLUME, volume);
        PlayerPrefs.Save();
        
        // También ajustar el volumen del AudioSource directamente como respaldo
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
        
        // Opcional: Reproducir un sonido al ajustar el volumen para que el usuario pueda oír el efecto
        if (volume > 0.05f && buttonClickSound != null)
        {
            PlayButtonClick();
        }
    }
    
    // Convierte un valor entre 0 y 1 a decibeles para el mixer
    private float ConvertToDecibel(float volume)
    {
        // Evitar log(0) que es -infinito
        if (volume <= 0.001f)
            return -80f; // -80dB es prácticamente silencio
            
        // Convertir a escala logarítmica
        return Mathf.Log10(volume) * 20f;
    }

    // Reproducir sonido de botón
    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }

    // Reproducir sonido de abrir menú
    public void PlayMenuOpen()
    {
        PlaySound(menuOpenSound);
    }

    // Reproducir un sonido cualquiera
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.clip = clip;
            sfxSource.Play();
        }
    }

    // Cambiar música
    public void ChangeMusic(AudioClip newMusic, bool fadeTransition = true, float fadeTime = 1.0f)
    {
        if (musicSource == null)
            return;
            
        // Si no hay cambio real de música, no hacer nada
        if (musicSource.clip == newMusic && musicSource.isPlaying)
            return;
            
        // Detener cualquier transición en progreso
        if (musicTransitionCoroutine != null)
            StopCoroutine(musicTransitionCoroutine);
        
        // Si queremos transición suave y hay música reproduciéndose
        if (fadeTransition && musicSource.isPlaying)
        {
            musicTransitionCoroutine = StartCoroutine(CrossFadeMusic(newMusic, fadeTime));
        }
        else
        {
            // Cambio directo sin transición
            musicSource.clip = newMusic;
            if (newMusic != null)
                musicSource.Play();
            else
                musicSource.Stop();
        }
    }

    private IEnumerator CrossFadeMusic(AudioClip newMusic, float fadeTime)
    {
        // Guarda el volumen original
        float startVolume = musicSource.volume;
        
        // Fade out
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }
        
        // Cambia la música
        musicSource.clip = newMusic;
        
        // Si hay nueva música, iniciamos el fade in
        if (newMusic != null)
        {
            musicSource.Play();
            
            // Fade in
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, startVolume, t / fadeTime);
                yield return null;
            }
        }
        
        // Asegurarse de que el volumen quede exactamente como estaba
        musicSource.volume = startVolume;
        
        musicTransitionCoroutine = null;
    }
}