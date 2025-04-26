using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DefeatScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button tryAgainButton;

    private void Start()
    {
        LoadWaveReached();
        SetupButtons();
    }

    private void LoadWaveReached()
    {
        int waveReached = PlayerPrefs.GetInt("WaveReached", 1);
        if (waveText != null)
            waveText.text = $"You reached Wave {waveReached}";
        else
            Debug.LogWarning("DefeatScreen: WaveText is not assigned.");
    }

    private void SetupButtons()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        else
            Debug.LogWarning("DefeatScreen: MainMenuButton is not assigned.");

        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(() => SceneManager.LoadScene("Nivel1"));
        else
            Debug.LogWarning("DefeatScreen: TryAgainButton is not assigned.");
    }
}
