using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager for handling game state, player resources, and level progression
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("Game Settings")]
    [SerializeField] private int startingLives = 20;
    [SerializeField] private int startingGold = 200;

    // Game state
    private int lives;
    private int gold;
    private int score;
    private GameState currentState = GameState.MainMenu;
    private int currentLevel = 1;
    private bool[] levelsUnlocked = new bool[3] { true, false, false };

    // Properties
    public int Lives => lives;
    public int Gold => gold;
    public int Score => score;
    public GameState CurrentState => currentState;

    // Events
    public System.Action<int> OnLivesChanged;
    public System.Action<int> OnGoldChanged;
    public System.Action<int> OnScoreChanged;
    public System.Action<GameState> OnGameStateChanged;

    /// <summary>
    /// Game state enum
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Gameplay,
        Paused,
        Victory,
        Defeat
    }

    protected override void OnAwake()
    {
        ResetGame();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Nivel1")
        {
            StartGame();
        }
    }

    /// <summary>
    /// Resets game state to default values
    /// </summary>
    public void ResetGame()
    {
        lives = startingLives;
        gold = startingGold;
        score = 0;

        OnLivesChanged?.Invoke(lives);
        OnGoldChanged?.Invoke(gold);
        OnScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    public void StartGame()
    {
        ChangeState(GameState.Gameplay);
    }

    /// <summary>
    /// Changes the game state
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        Debug.Log("Changing state to: " + newState);
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);

        switch (currentState)
        {
            case GameState.Victory:
                Debug.Log("Victory!");
                ShowVictoryScreen();
                break;

            case GameState.Defeat:
                Debug.Log("Defeat!");
                ShowDefeatScreen();
                break;
        }
    }

    /// <summary>
    /// Adds gold to the player
    /// </summary>
    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    /// <summary>
    /// Spends gold if player has enough
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (gold < amount) return false;

        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        return true;
    }

    /// <summary>
    /// Adds score points
    /// </summary>
    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// Reduces player lives
    /// </summary>
    public void LoseLife(int amount)
    {
        lives -= amount;
        Debug.Log("Lives remaining: " + lives);
        OnLivesChanged?.Invoke(lives);

        if (lives <= 0)
        {
            ChangeState(GameState.Defeat);
        }
    }
    

    /// <summary>
    /// Victory condition
    /// </summary>
    public void Victory()
    {
        ChangeState(GameState.Victory);
    }

    /// <summary>
    /// Get current level
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    /// <summary>
    /// Unlock a level
    /// </summary>
    public void UnlockLevel(int level)
    {
        if (level >= 1 && level <= levelsUnlocked.Length)
        {
            levelsUnlocked[level - 1] = true;
        }
    }

    /// <summary>
    /// Shows victory screen
    /// </summary>
    private void ShowVictoryScreen()
    {
        // Guardar datos relevantes
        int finalScore = CalculateFinalScore();
        PlayerPrefs.SetInt("FinalScore", finalScore);
        
        // Cargar directamente la escena de victoria después de un pequeño retraso
        StartCoroutine(LoadSceneAfterDelay("VictoryScene", 1f));
    }

    /// <summary>
    /// Shows defeat screen
    /// </summary>
    private void ShowDefeatScreen()
    {
        // Guardar datos relevantes
        if (SimpleWaveManager.Instance != null)
        {
            PlayerPrefs.SetInt("WaveReached", SimpleWaveManager.Instance.GetCurrentWave());
        }
        
        // Cargar directamente la escena de derrota después de un pequeño retraso
        StartCoroutine(LoadSceneAfterDelay("DefeatScene", 1f));
    }

    /// <summary>
    /// Shows endgame panel or loads scene
    /// </summary>
    private void ShowEndGameScreen(string panelName, string sceneName, System.Action extraDataSaver)
    {
        extraDataSaver?.Invoke();

        GameObject panel = GameObject.Find(panelName);
        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            StartCoroutine(LoadSceneAfterDelay(sceneName, 1f));
        }
    }

    /// <summary>
    /// Loads a scene after a delay
    /// </summary>
    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Asegurarse de que el tiempo esté normal antes de cambiar de escena
        Time.timeScale = 1f;
        
        // Cargar la escena
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Calculates final score for the level
    /// </summary>
    public int CalculateFinalScore()
    {
        int lifeBonus = lives * 10;
        int goldBonus = gold / 10;
        int finalScore = score + lifeBonus + goldBonus;

        score = finalScore;
        OnScoreChanged?.Invoke(score);

        return finalScore;
    }
}
