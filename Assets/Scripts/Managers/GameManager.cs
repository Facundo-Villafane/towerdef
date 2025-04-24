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
        // Initialize game
        ResetGame();
    }
    
    private void Start()
    {
        // Check current scene
        if (SceneManager.GetActiveScene().name == "Gameplay")
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
        
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
        
        switch (currentState)
        {
            case GameState.Victory:
                // Handle victory
                Debug.Log("Victory!");
                ShowVictoryScreen();
                break;
                
            case GameState.Defeat:
                // Handle defeat
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
        OnLivesChanged?.Invoke(lives);
        
        // Check for game over
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
        
        // Unlock next level
        int nextLevel = GetCurrentLevel() + 1;
        if (nextLevel <= 3) // Right now we have 3 levels
        {
            UnlockLevel(nextLevel);
        }
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
    /// Show victory screen
    /// </summary>
    private void ShowVictoryScreen()
    {
        // Show victory message
        Debug.Log("¡Victoria!");
        
        // Activate victory panel if it exists
        GameObject victoryPanel = GameObject.Find("VictoryPanel");
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Show defeat screen
    /// </summary>
    private void ShowDefeatScreen()
    {
        // Show defeat message
        Debug.Log("¡Derrota!");
        
        // Activate defeat panel if it exists
        GameObject defeatPanel = GameObject.Find("DefeatPanel");
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Calculates final score for the level
    /// </summary>
    public int CalculateFinalScore()
    {
        // Add bonus for remaining lives
        int lifeBonus = lives * 10;
        
        // Add bonus for remaining gold
        int goldBonus = gold / 10;
        
        // Final score
        int finalScore = score + lifeBonus + goldBonus;
        
        // Update score
        score = finalScore;
        OnScoreChanged?.Invoke(score);
        
        return finalScore;
    }
}