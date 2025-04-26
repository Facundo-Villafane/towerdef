using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button playAgainButton;

    private void Start()
    {
        LoadFinalScore();
        SetupButtons();
    }

    private void LoadFinalScore()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        if (scoreText != null)
            scoreText.text = $"Final Score: {finalScore}";
        else
            Debug.LogWarning("VictoryScreen: ScoreText is not assigned.");
    }

    private void SetupButtons()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        else
            Debug.LogWarning("VictoryScreen: MainMenuButton is not assigned.");

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(() => SceneManager.LoadScene("Nivel1"));
        else
            Debug.LogWarning("VictoryScreen: PlayAgainButton is not assigned.");
    }
}
