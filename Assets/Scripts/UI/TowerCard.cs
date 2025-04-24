using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Añade esta línea para IBeginDragHandler, etc.
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
    
    public TowerFactory.TowerType TowerType => towerType;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private InputController inputController;
    
    private void Awake()
    {
        // Listener for Card click
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnCardClicked);
        }
        
        // Variable initialization
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        inputController = FindObjectOfType<InputController>();
    }

    // Unity's drag and drop interface methods
    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        
        if (inputController != null)
        {
            inputController.StartTowerPlacement(towerType);
        }
    }
    
    /// <summary>
    /// Handles dragging of the card
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    
    /// <summary>
    /// Handles the end of the drag event
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        rectTransform.anchoredPosition = startPosition;
        
        if (inputController != null)
        {
            // This will try to place the tower at the mouse position
            inputController.TryPlaceTowerAtMousePosition();
        }
    }
    
    private void OnDestroy()
    {
        // Remove click listener
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnCardClicked);
        }
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
        
        // Tooltip/description functionality will be added here
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
}