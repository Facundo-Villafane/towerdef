using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Simplified UI manager for the tower defense game
/// </summary>
public class SimpleUIManager : Singleton<SimpleUIManager>
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI nextWaveTimerText; 
    [SerializeField] private Button skipWaveButton; 
    [SerializeField] private Sprite enabledSkipSprite;
    [SerializeField] private Sprite disabledSkipSprite;
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite pauseSprite;
    private bool gameIsPaused = false;

    [Header("Additional Controls")]
    [SerializeField] private Button previousWaveButton; // Button to go to the previous wave
    [SerializeField] private Button restartWaveButton; // Button to restart the current wave
    [SerializeField] private Button restartLevelButton; // Button to restart the level
    [SerializeField] private Button mainMenuButton; // Button to go to the main menu

    [Header("Notification Elements")]
    [SerializeField] private CanvasGroup nextWaveTimerPanel;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private Coroutine fadeCoroutine;
    private bool isNextWavePanelVisible = false;
    
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

        // Hidde the next wave timer panel at start
        if (nextWaveTimerPanel != null)
        {
            nextWaveTimerPanel.alpha = 0f;
            nextWaveTimerPanel.gameObject.SetActive(false);
            isNextWavePanelVisible = false;
        }
        
        // Create tower cards
        CreateTowerCards();

        // Initialize play/pause button
        if (playPauseButton != null)
        {
            Image buttonImage = playPauseButton.GetComponent<Image>();
            if (buttonImage != null && pauseSprite != null)
            {
                // Initialize with pause sprite cuz the game starts not paused
                buttonImage.sprite = pauseSprite;
            }
        }
        // Start the game automatically after a short delay
        Invoke("StartWavesAutomatically", 1.0f);

        // Setup all control buttons
        if (previousWaveButton != null)
            previousWaveButton.onClick.AddListener(OnPreviousWaveClicked);
            
        if (restartWaveButton != null)
            restartWaveButton.onClick.AddListener(OnRestartWaveClicked);
            
        if (restartLevelButton != null)
            restartLevelButton.onClick.AddListener(OnRestartLevelClicked);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            
        if (skipWaveButton != null)
            skipWaveButton.onClick.AddListener(OnSkipWaveClicked);
            
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(OnPlayPauseButtonClicked);
    }

    // Method to handle restart wave button click
    public void OnRestartWaveClicked()
    {
        if (SimpleWaveManager.Instance != null)
        {
            SimpleWaveManager.Instance.RestartCurrentWave();
        }
    }

    // Method to handle previous wave button click
    public void OnPreviousWaveClicked()
    {
        if (SimpleWaveManager.Instance != null)
        {
            SimpleWaveManager.Instance.PreviousWave();
        }
    }

    // Method to handle restart level button click
    public void OnRestartLevelClicked()
    {
        // Restart actual level
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
            
            // Delete all towers in the scene
            Tower[] allTowers = FindObjectsOfType<Tower>();
            foreach (Tower tower in allTowers)
            {
                Destroy(tower.gameObject);
            }
            
            // Restart the waves too
            if (SimpleWaveManager.Instance != null)
            {
                SimpleWaveManager.Instance.ResetWaves();
                
                // Start the waves after a short delay
                Invoke("StartWavesAfterReset", 1.0f);
            }
        }
    }

    private void StartWavesAfterReset()
    {
        if (SimpleWaveManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Gameplay)
        {
            SimpleWaveManager.Instance.StartWaves();
        }
    }

    // Method to handle main menu button click
    public void OnMainMenuClicked()
    {
        // Load the main menu scene
        // Reset the game state and player data if necessary ****
        SceneManager.LoadScene("MainMenu");
    }

    // Clean up event listeners!!!!!
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
            GameManager.Instance.OnLivesChanged -= UpdateLivesDisplay;
        }
        
        if (SimpleWaveManager.Instance != null)
        {
            SimpleWaveManager.Instance.OnWaveStarted -= UpdateWaveDisplay;
            SimpleWaveManager.Instance.OnWaveCompleted -= OnWaveCompletedHandler;
            SimpleWaveManager.Instance.OnAllWavesCompleted -= OnAllWavesCompletedHandler;
        }
        
        if (previousWaveButton != null)
            previousWaveButton.onClick.RemoveListener(OnPreviousWaveClicked);
            
        if (restartWaveButton != null)
            restartWaveButton.onClick.RemoveListener(OnRestartWaveClicked);
            
        if (restartLevelButton != null)
            restartLevelButton.onClick.RemoveListener(OnRestartLevelClicked);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            
        if (skipWaveButton != null)
            skipWaveButton.onClick.RemoveListener(OnSkipWaveClicked);
            
        if (playPauseButton != null)
            playPauseButton.onClick.RemoveListener(OnPlayPauseButtonClicked);
    }

    // Method to handle Start waves automatically
    private void StartWavesAutomatically()
    {
        if (SimpleWaveManager.Instance != null && !SimpleWaveManager.Instance.WavesStarted)
        {
            SimpleWaveManager.Instance.StartWaves();
            Debug.Log("Waves started automatically");
        }
    }

    public void OnPlayPauseButtonClicked()
    {
        Debug.Log("Play/Pause button clicked!");
        
        gameIsPaused = !gameIsPaused;
        Debug.Log($"Game is now {(gameIsPaused ? "paused" : "unpaused")}");
        
        // Refresh button sprite
        Image buttonImage = playPauseButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = gameIsPaused ? playSprite : pauseSprite;
            Debug.Log("Button sprite updated");
        }
        else
        {
            Debug.LogError("No Image component found on playPauseButton!");
        }
        
        // Play or pause the game
        Time.timeScale = gameIsPaused ? 0f : 1f;
        Debug.Log($"Time.timeScale set to {Time.timeScale}");
        
        if (!gameIsPaused && SimpleWaveManager.Instance != null && !SimpleWaveManager.Instance.WavesStarted)
        {
            SimpleWaveManager.Instance.StartWaves();
            Debug.Log("Waves started");
        }
    }
    
    /// <summary>
    /// Handles wave completion
    /// </summary>
    private void OnWaveCompletedHandler(int currentWave, int totalWaves)
    {
        // Update the wave display
        UpdateWaveDisplay(currentWave, totalWaves);
    }

    private void Update()
    {
        // Update the next wave timer and skip button state
        bool isWaiting = SimpleWaveManager.Instance != null && SimpleWaveManager.Instance.IsWaitingForNextWave;
    
        // Show/hide the next wave timer panel based on waiting state
        if (isWaiting && !isNextWavePanelVisible)
        {
            ShowNextWavePanel();
        }
        else if (!isWaiting && isNextWavePanelVisible)
        {
            HideNextWavePanel();
        }
        
        // Update the next wave timer text
        if (isWaiting && nextWaveTimerText != null)
        {
            float timeRemaining = SimpleWaveManager.Instance.TimeUntilNextWave;
            nextWaveTimerText.text = $"NEXT WAVE IN {timeRemaining:0}s";
        }

        
        // Update skip wave button state
        if (skipWaveButton != null)
        {
            // Keep the button interactable only if waiting for the next wave
            skipWaveButton.interactable = isWaiting;
            
            // Handle button sprite change based on waiting state
            Image skipButtonImage = skipWaveButton.GetComponent<Image>();
            if (skipButtonImage != null && enabledSkipSprite != null && disabledSkipSprite != null)
            {
                skipButtonImage.sprite = isWaiting ? enabledSkipSprite : disabledSkipSprite;
                
                // Keep the button color at full alpha
                Color buttonColor = skipButtonImage.color;
                buttonColor.a = 1f;  
                skipButtonImage.color = buttonColor;
            }
            
            // Update button text color to full alpha
            TextMeshProUGUI buttonText = skipWaveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                Color textColor = buttonText.color;
                textColor.a = 1f;  
                buttonText.color = textColor;
            }
        }
    }

    private void ShowNextWavePanel()
    {
        if (nextWaveTimerPanel == null) 
        {
            Debug.LogError("nextWaveTimerPanel is null!");
            return;
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
                
        // Check if the onbject is active before showing it
        nextWaveTimerPanel.gameObject.SetActive(true);
        
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(nextWaveTimerPanel, 0f, 1f, fadeInDuration));
        isNextWavePanelVisible = true;
    }

    private void HideNextWavePanel()
    {
        if (nextWaveTimerPanel == null) 
        {
            Debug.LogError("nextWaveTimerPanel is null!");
            return;
        }
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
                
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(nextWaveTimerPanel, 1f, 0f, fadeOutDuration));
        isNextWavePanelVisible = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null in FadeCanvasGroup!");
            yield break;
        }
        
        // Check if the canvas group is already active
        canvasGroup.gameObject.SetActive(true);
        
        // Set initial alpha
        canvasGroup.alpha = startAlpha;
        
        float startTime = Time.unscaledTime;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime = Time.unscaledTime - startTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            
            yield return null;
        }
        
        // Check if the coroutine was stopped before completion
        canvasGroup.alpha = endAlpha;
        
        // If we are hiding the panel and the alpha is very low, deactivate the game object
        if (endAlpha <= 0.01f)
        {
            canvasGroup.gameObject.SetActive(false);
        }
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
            goldText.text = $"{gold}";
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
            livesText.text = $"{lives}";
        }
    }
    
    /// <summary>
    /// Updates the wave display
    /// </summary>
    private void UpdateWaveDisplay(int currentWave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {currentWave}/{totalWaves}";
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
    
    /// <summary>
    /// Handles skip wave button click
    /// </summary>
    public void OnSkipWaveClicked()
    {
        if (SimpleWaveManager.Instance != null && SimpleWaveManager.Instance.IsWaitingForNextWave)
        {
            SimpleWaveManager.Instance.StartNextWave();
        }
    }
    
    /// <summary>
    /// Shows the victory panel
    /// </summary>
    public void ShowVictoryPanel()
    {
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
        // Find and activate defeat panel
        Transform defeatPanel = transform.Find("DefeatPanel");
        if (defeatPanel != null)
        {
            defeatPanel.gameObject.SetActive(true);
        }
    }
}