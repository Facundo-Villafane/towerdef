using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton; 
    [SerializeField] private Button creditsButton;  
    [SerializeField] private Button exitButton;

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainPanel;     
    [SerializeField] private GameObject optionsPanel;   
    [SerializeField] private GameObject creditsPanel;   

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private AudioClip mainTheme;

    private void Start()
    {
        // Check that only the main menu panel is active at the start
        
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Listener for button clicks
        SetupButtons();

        // Play main theme music
        if (AudioManager.Instance != null && mainTheme != null)
        {
            AudioManager.Instance.ChangeMusic(mainTheme);
        }

        // Load audio settings
        LoadAudioSettings();
    }

    private void LoadAudioSettings()
    {
        // Loading saved volume settings from PlayerPrefs
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        
        // Appying loaded settings to sliders
        if (masterVolumeSlider != null) 
        {
            masterVolumeSlider.value = masterVolume;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        
        // Apply settings to AudioMixer
        ApplyAudioSettings();
    }

    public void SetMasterVolume(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        ApplyAudioSettings();
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        ApplyAudioSettings();
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(masterVolume);
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }
    }

    private void SetupButtons()
    {
        // Play button
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        else
            Debug.LogWarning("Play Button not assigned!");

        // Options button
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OnOptionsClicked);

        //  Credit button
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);

        // Exit button
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
        else
            Debug.LogWarning("Exit Button not assigned!");
    }

    private void OnPlayClicked()
    {
        // Load the game scene
        SceneManager.LoadScene("Nivel1");
    }

    private void OnOptionsClicked()
    {
        // Only deactivate the main panel, NOT the entire Canvas
        if (mainPanel != null) mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        
        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuOpen();
    }

    private void OnCreditsClicked()
    {
        // Only deactivate the main panel, NOT the entire Canvas
        if (mainPanel != null) mainPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
        
        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuOpen();
    }

    public void OnBackClicked()
    {
        // Go back to the main menu
        if (mainPanel != null) mainPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        
        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    private void OnExitClicked()
    {
#if UNITY_EDITOR
        // If developer mode, stop playing in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // On build, quit the application
        Application.Quit();
#endif
    }
}