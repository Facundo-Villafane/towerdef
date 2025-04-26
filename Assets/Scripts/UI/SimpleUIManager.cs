using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    [SerializeField] private TextMeshProUGUI nextWaveTimerText; 
    [SerializeField] private Button skipWaveButton; // Button for skip waves
    [SerializeField] private Sprite enabledSkipSprite;
    [SerializeField] private Sprite disabledSkipSprite;
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite pauseSprite;
    private bool gameIsPaused = false;

    [Header("Additional Controls")]
    [SerializeField] private Button restartWaveButton;
    [SerializeField] private Button restartLevelButton;
    [SerializeField] private Button mainMenuButton;

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
        // **This is just for testing purposes, will remove in production**
        Invoke("StartWavesAutomatically", 1.0f);

        // To sed Other buttons
        if (restartWaveButton != null)
        restartWaveButton.onClick.AddListener(OnRestartWaveClicked);
            
        if (restartLevelButton != null)
            restartLevelButton.onClick.AddListener(OnRestartLevelClicked);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void OnRestartWaveClicked()
    {
        if (SimpleWaveManager.Instance != null)
        {
            SimpleWaveManager.Instance.RestartCurrentWave();
        }
    }

    // Method to handle restart level button click
    public void OnRestartLevelClicked()
    {
        // Simplemente recarga la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    // Method to handle main menu button click
    public void OnMainMenuClicked()
    {
        //Cargar la escena del menú principal
        SceneManager.LoadScene("MainMenu");
    }

    // Clean up event listeners!!!!!
    private void OnDestroy()
    {
        if (restartWaveButton != null)
        restartWaveButton.onClick.RemoveListener(OnRestartWaveClicked); 

        if (restartLevelButton != null)
            restartLevelButton.onClick.RemoveListener(OnRestartLevelClicked);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            
        // Clean up other listeners!!!!!
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
        gameIsPaused = !gameIsPaused;
        
        // Refresh button sprite
        Image buttonImage = playPauseButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = gameIsPaused ? playSprite : pauseSprite;
        }
        
        // Play or pause the game
        Time.timeScale = gameIsPaused ? 0f : 1f;
        
        
        if (!gameIsPaused && SimpleWaveManager.Instance != null && !SimpleWaveManager.Instance.WavesStarted)
        {
            SimpleWaveManager.Instance.StartWaves();
        }
    }
    
    /// <summary>
    /// Handles wave completion
    /// </summary>
    private void OnWaveCompletedHandler(int currentWave, int totalWaves)
    {
        // Update the wave display
        UpdateWaveDisplay(currentWave, totalWaves);
        
        // Add rewards or show message
        //Debug.Log($"Wave {currentWave} completed! {totalWaves - currentWave} waves remaining.");
        
        // Give bonus gold between waves
        //GameManager.Instance.AddGold(50);
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
            //Debug.LogError("nextWaveTimerPanel is null!");
            return;
        }

        // Verificación para depuración
        //Debug.Log($"Showing next wave panel. Current alpha: {nextWaveTimerPanel.alpha}, Active: {nextWaveTimerPanel.gameObject.activeSelf}");

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
                
        // Asegurarse de que el objeto esté activo antes de la animación
        nextWaveTimerPanel.gameObject.SetActive(true);
        
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(nextWaveTimerPanel, 0f, 1f, fadeInDuration));
        isNextWavePanelVisible = true;
    }

    private void HideNextWavePanel()
    {
        if (nextWaveTimerPanel == null) 
        {
            //Debug.LogError("nextWaveTimerPanel is null!");
            return;
        }

        // Verificación para depuración
        //Debug.Log($"Hiding next wave panel. Current alpha: {nextWaveTimerPanel.alpha}, Active: {nextWaveTimerPanel.gameObject.activeSelf}");
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
                
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(nextWaveTimerPanel, 1f, 0f, fadeOutDuration));
        isNextWavePanelVisible = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null)
        {
            //Debug.LogError("CanvasGroup is null in FadeCanvasGroup!");
            yield break;
        }
        
        // Verificación para depuración
        //Debug.Log($"Starting fade from {startAlpha} to {endAlpha} over {duration} seconds");
        
        // Asegurarse de que el objeto esté activo antes de la animación
        canvasGroup.gameObject.SetActive(true);
        
        // Establecer el alpha inicial
        canvasGroup.alpha = startAlpha;
        
        float startTime = Time.unscaledTime;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime = Time.unscaledTime - startTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            
            // Depuración ocasional
            if (elapsedTime % 0.5f < 0.02f)
            {
                //Debug.Log($"Fade progress: {normalizedTime:F2}, Current alpha: {canvasGroup.alpha:F2}");
            }
            
            yield return null;
        }
        
        // Asegurar que termine con el valor correcto
        canvasGroup.alpha = endAlpha;
        
        // Si estamos ocultando, desactivar el objeto para ahorrar recursos
        if (endAlpha <= 0.01f)
        {
            canvasGroup.gameObject.SetActive(false);
        }
        
        //Debug.Log($"Fade completed: {startAlpha} -> {endAlpha}, Panel active: {canvasGroup.gameObject.activeSelf}");
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
        if (SimpleWaveManager.Instance != null && SimpleWaveManager.Instance.IsWaitingForNextWave)
        {
            SimpleWaveManager.Instance.StartNextWave();
        }
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