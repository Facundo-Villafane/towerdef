using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for tower selection cards
/// </summary>
public class TowerCard : MonoBehaviour
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
    
    private void Awake()
    {
        // Add click listener to button
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnCardClicked);
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
        
        // Set tooltip/description if needed
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