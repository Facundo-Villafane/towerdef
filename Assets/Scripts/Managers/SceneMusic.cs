using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip sceneMusic;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool useFadeTransition = true;
    [SerializeField] private float fadeTime = 1.5f;
    
    private void Start()
    {
        if (playOnStart && sceneMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.ChangeMusic(sceneMusic, useFadeTransition, fadeTime);
        }
    }
    
    // Opcional: Para activar música en momentos específicos
    public void PlaySceneMusic()
    {
        if (sceneMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.ChangeMusic(sceneMusic, useFadeTransition, fadeTime);
        }
    }
}