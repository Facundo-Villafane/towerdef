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
        // Si no se asignó el slider, intentar obtenerlo del mismo objeto
        if (slider == null)
            slider = GetComponent<Slider>();
            
        if (slider == null)
        {
            Debug.LogError("No se encontró un Slider para VolumeSlider");
            return;
        }
        
        // Cargar valor guardado
        string prefName = volumeType.ToString() + "Volume";
        float savedVolume = PlayerPrefs.GetFloat(prefName, 1f);
        slider.value = savedVolume;
        
        // Configurar listener
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
                
                // Opcional: Reproducir un sonido al ajustar el volumen de SFX
                if (value > 0.01f)
                    AudioManager.Instance.PlayButtonClick();
                break;
        }
    }
}