using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// UI component for tower selection cards
/// </summary>
public class TowerCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image backgroundImage;
    
    [Header("Colors")]
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = Color.gray;
    
    private TowerFactory.TowerType towerType;
    private Action<TowerFactory.TowerType> onClickCallback;
    
    // Drag & drop variables
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private InputController inputController;
    private TowerFactory towerFactory;
    private Camera mainCamera;
    
    // Tower preview
    private GameObject towerPreview;
    
    public TowerFactory.TowerType TowerType => towerType;
    
    private void Awake()
    {
        // Get references
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        inputController = FindObjectOfType<InputController>();
        towerFactory = FindObjectOfType<TowerFactory>();
        mainCamera = Camera.main;
        
        // Listener for Card click
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnCardClicked);
        }
    }
    
    /// <summary>
    /// Handles the start of drag
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store original position
        startPosition = rectTransform.anchoredPosition;
        
        // Make the card semi-transparent
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        
        // Check if player can afford this tower
        if (towerFactory != null && !towerFactory.CanAffordTower(towerType))
        {
            Debug.Log("Not enough gold to build this tower!");
            return;
        }
        
        // Create tower preview directly
        if (towerFactory != null)
        {
            towerPreview = towerFactory.CreateTowerPreview(towerType);
            
            if (towerPreview != null)
            {
                // Set initial position to mouse
                UpdateTowerPreviewPosition(eventData);
            }
        }
    }
    
    /// <summary>
    /// Handles the drag
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // Update tower preview position
        if (towerPreview != null)
        {
            UpdateTowerPreviewPosition(eventData);
        }
    }
    
    /// <summary>
    /// Handles the end of drag
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // Reset the card appearance
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        rectTransform.anchoredPosition = startPosition;
        
        // Try to place the tower at current mouse position
        if (towerPreview != null)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            // Snap to grid
            Vector3 position = SnapToGrid(mousePos);
            
            // Check if placement is valid
            if (LevelManager.Instance != null && LevelManager.Instance.CanPlaceTower(position))
            {
                // Create the tower
                PlaceTowerCommand command = new PlaceTowerCommand(
                    towerType,
                    position,
                    towerFactory,
                    LevelManager.Instance
                );
                
                if (CommandManager.Instance != null)
                {
                    CommandManager.Instance.ExecuteCommand(command);
                }
            }
            
            // Clean up preview
            Destroy(towerPreview);
            towerPreview = null;
        }
    }
    
    /// <summary>
    /// Updates the tower preview position
    /// </summary>
    private void UpdateTowerPreviewPosition(PointerEventData eventData)
    {
        if (towerPreview != null && mainCamera != null)
        {
            // Get mouse position in world coordinates
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            // Snap position to grid
            Vector3 snappedPos = SnapToGrid(mousePos);
            
            // Update preview position
            towerPreview.transform.position = snappedPos;
            
            // Update color based on placement validity
            bool validPlacement = LevelManager.Instance != null && 
                                 LevelManager.Instance.CanPlaceTower(snappedPos);
            
            UpdatePreviewColor(validPlacement);
        }
    }
    
    /// <summary>
    /// Updates the preview color based on placement validity
    /// </summary>
    private void UpdatePreviewColor(bool validPlacement)
    {
        if (towerPreview == null) return;
        
        // Get all renderers in preview
        Renderer[] renderers = towerPreview.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer r in renderers)
        {
            // Apply color based on validity
            Color color = validPlacement ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            
            // Apply to all materials
            foreach (Material mat in r.materials)
            {
                mat.color = color;
            }
        }
    }
    
    /// <summary>
    /// Snaps a position to the grid
    /// </summary>
    private Vector3 SnapToGrid(Vector3 position)
    {
        // Grid size
        float gridSize = 0.32f;
        
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = Mathf.Round(position.y / gridSize) * gridSize;
        
        return new Vector3(x, y, 0);
    }
    
    /// <summary>
    /// Initializes the tower card
    /// </summary>
    public void Initialize(TowerFactory.TowerType type, Sprite icon, int cost, string description, Action<TowerFactory.TowerType> callback)
    {
        towerType = type;
        onClickCallback = callback;
        
        // Set icon
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }
        
        // Set name
        if (nameText != null)
        {
            nameText.text = type.ToString();
        }
        
        // Set cost
        if (costText != null)
        {
            costText.text = cost.ToString();
        }
    }
    
    /// <summary>
    /// Sets whether the tower is affordable
    /// </summary>
    public void SetAffordable(bool affordable)
    {
        // Update button interactability
        if (selectButton != null)
        {
            selectButton.interactable = affordable;
        }
        
        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = affordable ? affordableColor : unaffordableColor;
        }
    }
    
    /// <summary>
    /// Handles card button click
    /// </summary>
    private void OnCardClicked()
    {
        onClickCallback?.Invoke(towerType);
    }
    
    private void OnDestroy()
    {
        // Remove click listener
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnCardClicked);
        }
        
        // Clean up tower preview
        if (towerPreview != null)
        {
            Destroy(towerPreview);
            towerPreview = null;
        }
    }
}