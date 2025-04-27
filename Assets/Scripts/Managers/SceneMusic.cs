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
    
    // To play the music on some moments in the game ** Not implemented yet **
    public void PlaySceneMusic()
    {
        if (sceneMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.ChangeMusic(sceneMusic, useFadeTransition, fadeTime);
        }
    }
}