using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simplified UI manager for the tower defense game
/// </summary>
public class SimpleUIManager : Singleton<SimpleUIManager>
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;
    
    [Header("Tower Selection")]
    [SerializeField] private Transform towerCardsContainer;
    [SerializeField] private GameObject towerCardPrefab;
    
    // Game references
    private TowerFactory towerFactory;
    private InputController inputController;
    
    // Active tower cards
    private List<TowerCard> towerCards = new List<TowerCard>();
    
    protected override void OnAwake()
    {
        // Get references
        towerFactory = FindObjectOfType<TowerFactory>();
        inputController = FindObjectOfType<InputController>();
    }
    
    private void Start()
    {
        // Subscribe to events
        GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
        GameManager.Instance.OnLivesChanged += UpdateLivesDisplay;
        SimpleWaveManager.Instance.OnWaveStarted += UpdateWaveDisplay;              
        SimpleWaveManager.Instance.OnWaveCompleted += OnWaveCompletedHandler;
        SimpleWaveManager.Instance.OnAllWavesCompleted += OnAllWavesCompletedHandler;
        
        // Initial display
        UpdateGoldDisplay(GameManager.Instance.Gold);
        UpdateLivesDisplay(GameManager.Instance.Lives);
        UpdateWaveDisplay(0, SimpleWaveManager.Instance.GetTotalWaves());
        
        // Create tower cards
        CreateTowerCards();
    }

    public void StartWaves()
    {
        if (SimpleWaveManager.Instance != null)
        {
            SimpleWaveManager.Instance.StartWaves();
            Debug.Log("Waves started!");
        }
    }
    
    /// <summary>
    /// Handles wave completion
    /// </summary>
    private void OnWaveCompletedHandler(int currentWave, int totalWaves)
    {
        // Update the wave display
        UpdateWaveDisplay(currentWave, totalWaves);
        
        // Optionally add rewards or show message
        Debug.Log($"Wave {currentWave} completed! {totalWaves - currentWave} waves remaining.");
        
        // Example: Give bonus gold between waves
        GameManager.Instance.AddGold(50);
    }
    
    /// <summary>
    /// Handles when all waves are completed
    /// </summary>
    private void OnAllWavesCompletedHandler(int finalScore)
    {
        Debug.Log($"All waves completed! Final score: {finalScore}");
        
        // Trigger victory condition
        GameManager.Instance.Victory();
    }
    
    /// <summary>
    /// Creates cards for all available tower types
    /// </summary>
    private void CreateTowerCards()
    {
        if (towerFactory == null || towerCardPrefab == null) return;
        
        // Get available tower types
        List<TowerFactory.TowerType> towerTypes = towerFactory.GetAvailableTowerTypes();
        
        foreach (TowerFactory.TowerType type in towerTypes)
        {
            // Get tower data
            TowerFactory.TowerPrefab towerData = towerFactory.GetTowerData(type);
            if (towerData == null) continue;
            
            // Create card
            GameObject cardObj = Instantiate(towerCardPrefab, towerCardsContainer);
            TowerCard card = cardObj.GetComponent<TowerCard>();
            
            if (card != null)
            {
                // Initialize card
                card.Initialize(
                    towerData.type,
                    towerData.icon,
                    towerData.cost,
                    towerData.description,
                    OnTowerCardClicked
                );
                
                towerCards.Add(card);
            }
        }
    }
    
    /// <summary>
    /// Handles tower card click
    /// </summary>
    private void OnTowerCardClicked(TowerFactory.TowerType towerType)
    {
        if (inputController != null)
        {
            inputController.StartTowerPlacement(towerType);
        }
    }
    
    /// <summary>
    /// Updates the gold display
    /// </summary>
    private void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"Gold: {gold}";
        }
        
        // Update tower card affordability
        UpdateTowerCardAffordability();
    }
    
    /// <summary>
    /// Updates the lives display
    /// </summary>
    private void UpdateLivesDisplay(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }
    
    /// <summary>
    /// Updates the wave display
    /// </summary>
    private void UpdateWaveDisplay(int currentWave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {currentWave}/{totalWaves}";
        }
    }
    
    /// <summary>
    /// Updates tower card affordability based on player's gold
    /// </summary>
    private void UpdateTowerCardAffordability()
    {
        foreach (TowerCard card in towerCards)
        {
            if (card != null)
            {
                bool canAfford = towerFactory.CanAffordTower(card.TowerType);
                card.SetAffordable(canAfford);
            }
        }
    }
    
    #region Button Handlers
    
    /// <summary>
    /// Handles start game button click
    /// </summary>
    public void OnStartGameClicked()
    {
        // Reset and start the game
        GameManager.Instance.ResetGame();
        GameManager.Instance.StartGame();
    }
    
    /// <summary>
    /// Handles speed up button click
    /// </summary>
    public void OnSpeedUpClicked()
    {
        // Toggle game speed
        if (Time.timeScale == 1f)
        {
            Time.timeScale = 2f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
    
    /// <summary>
    /// Handles skip wave button click
    /// </summary>
    public void OnSkipWaveClicked()
    {
        SimpleWaveManager.Instance.StartNextWave();
    }
    
    #endregion
    
    /// <summary>
    /// Shows the victory panel
    /// </summary>
    public void ShowVictoryPanel()
    {
        // Implement victory panel logic
        Debug.Log("Showing victory panel");
        
        // Find and activate victory panel
        Transform victoryPanel = transform.Find("VictoryPanel");
        if (victoryPanel != null)
        {
            victoryPanel.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Shows the defeat panel
    /// </summary>
    public void ShowDefeatPanel()
    {
        // Implement defeat panel logic
        Debug.Log("Showing defeat panel");
        
        // Find and activate defeat panel
        Transform defeatPanel = transform.Find("DefeatPanel");
        if (defeatPanel != null)
        {
            defeatPanel.gameObject.SetActive(true);
        }
    }
}