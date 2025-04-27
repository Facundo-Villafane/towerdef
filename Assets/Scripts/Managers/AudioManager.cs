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

    // Names of Audio Mixer parameters
    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";

    // To avoid unnecessary processing
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
        
        // Start with default values
        lastMasterVolume = 1f;
        lastMusicVolume = 1f;
        lastSfxVolume = 1f;
        
        // Load saved volume settings
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
        // To avoid unnecessary processing
        if (Mathf.Approximately(lastMasterVolume, volume)) return;
        lastMasterVolume = volume;
        
        // Convert from linear scale (0-1) to logarithmic (dB)
        float volumeDb = ConvertToDecibel(volume);
        
        bool success = audioMixer.SetFloat(MASTER_VOLUME, volumeDb);
        //Debug.Log($"Setting Master Volume: {volume} (linear) -> {volumeDb} dB, Success: {success}");
        
        PlayerPrefs.SetFloat(MASTER_VOLUME, volume);
        PlayerPrefs.Save();
    }
    
    public void SetMusicVolume(float volume)
    {
        // Avoid unnecessary processing
        if (Mathf.Approximately(lastMusicVolume, volume)) return;
        lastMusicVolume = volume;
        
        // Convert from linear scale (0-1) to logarithmic (dB)
        float volumeDb = ConvertToDecibel(volume);
        
        bool success = audioMixer.SetFloat(MUSIC_VOLUME, volumeDb);
        //Debug.Log($"Setting Music Volume: {volume} (linear) -> {volumeDb} dB, Success: {success}");
        
        PlayerPrefs.SetFloat(MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
        
        // Adjust the volume of the AudioSource directly as a backup
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        // Avoid unnecessary processing
        if (Mathf.Approximately(lastSfxVolume, volume)) return;
        lastSfxVolume = volume;
        
        // Convert from linear scale (0-1) to logarithmic (dB)
        float volumeDb = ConvertToDecibel(volume);
        
        bool success = audioMixer.SetFloat(SFX_VOLUME, volumeDb);
        //Debug.Log($"Setting SFX Volume: {volume} (linear) -> {volumeDb} dB, Success: {success}");
        
        PlayerPrefs.SetFloat(SFX_VOLUME, volume);
        PlayerPrefs.Save();
        
        // Adjust the volume of the AudioSource directly as a backup
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
        
        // Play Button Click sound to give feedback to the user
        if (volume > 0.05f && buttonClickSound != null)
        {
            PlayButtonClick();
        }
    }
    
    // Convert linear volume (0-1) to decibel scale
    private float ConvertToDecibel(float volume)
    {
        // Evitar log(0) que es -infinito
        if (volume <= 0.001f)
            return -80f; // -80dB es prácticamente silencio
            
        // Convertir a escala logarítmica
        return Mathf.Log10(volume) * 20f;
    }

    // Play sound for button click
    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }

    // Play sound for menu open
    public void PlayMenuOpen()
    {
        PlaySound(menuOpenSound);
    }

    // Play sound for any AudioClip
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.clip = clip;
            sfxSource.Play();
        }
    }

    // Change music with optional fade transition
    public void ChangeMusic(AudioClip newMusic, bool fadeTransition = true, float fadeTime = 1.0f)
    {
        if (musicSource == null)
            return;
            
        // If the new music is null, stop the current music
        if (musicSource.clip == newMusic && musicSource.isPlaying)
            return;
            
        // Stop any ongoing music transition
        if (musicTransitionCoroutine != null)
            StopCoroutine(musicTransitionCoroutine);
        
        // Smooth transition between music clips
        if (fadeTransition && musicSource.isPlaying)
        {
            musicTransitionCoroutine = StartCoroutine(CrossFadeMusic(newMusic, fadeTime));
        }
        else
        {
            // Directly change the music without fading
            musicSource.clip = newMusic;
            if (newMusic != null)
                musicSource.Play();
            else
                musicSource.Stop();
        }
    }

    private IEnumerator CrossFadeMusic(AudioClip newMusic, float fadeTime)
    {
        // Save the current volume to restore it later
        float startVolume = musicSource.volume;
        
        // Fade out
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }
        
        // Change the music clip
        musicSource.clip = newMusic;
        
        // If there new music, play it
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
        
        // Check the volume to ensure it's restored
        musicSource.volume = startVolume;
        
        musicTransitionCoroutine = null;
    }
}