using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType
    {
        Master,
        Music,
        SFX
    }
    
    [SerializeField] private VolumeType volumeType;
    [SerializeField] private Slider slider;
    
    private void Start()
    {
        // If ther no slider assigned, try to get it from the GameObject
        if (slider == null)
            slider = GetComponent<Slider>();
            
        if (slider == null)
        {
            Debug.LogError("No se encontró un Slider para VolumeSlider");
            return;
        }
        
        // Load saved volume value
        string prefName = volumeType.ToString() + "Volume";
        float savedVolume = PlayerPrefs.GetFloat(prefName, 1f);
        slider.value = savedVolume;
        
        // Configure listeners
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    private void OnSliderValueChanged(float value)
    {
        if (AudioManager.Instance == null)
            return;
            
        switch (volumeType)
        {
            case VolumeType.Master:
                AudioManager.Instance.SetMasterVolume(value);
                break;
            case VolumeType.Music:
                AudioManager.Instance.SetMusicVolume(value);
                break;
            case VolumeType.SFX:
                AudioManager.Instance.SetSFXVolume(value);
                
                
                if (value > 0.01f)
                    AudioManager.Instance.PlayButtonClick();
                break;
        }
    }
}