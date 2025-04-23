using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player input for tower placement and game controls
/// </summary>
public class InputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TowerFactory towerFactory;
    [SerializeField] private Camera mainCamera;
    
    [Header("Tower Placement")]
    [SerializeField] private Transform towerParent;
    [SerializeField] private LayerMask placementLayerMask;
    
    // Current state
    private TowerFactory.TowerType selectedTowerType;
    private GameObject towerPreview;
    private bool isPlacingTower = false;
    
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (towerParent == null)
        {
            towerParent = new GameObject("Towers").transform;
        }
    }
    
    private void Update()
    {
        // Only handle input during gameplay
        if (GameManager.Instance.CurrentState != GameManager.GameState.Gameplay)
            return;
        
        // Handle tower placement
        if (isPlacingTower)
        {
            UpdateTowerPreview();
            
            // Place tower on left click
            if (Input.GetMouseButtonDown(0))
            {
                PlaceTower();
            }
            
            // Cancel placement on right click
            if (Input.GetMouseButtonDown(1))
            {
                CancelTowerPlacement();
            }
        }
        
        // Handle undo with Ctrl+Z
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            CommandManager.Instance.Undo();
        }
    }
    
    /// <summary>
    /// Starts the tower placement process
    /// </summary>
    public void StartTowerPlacement(TowerFactory.TowerType towerType)
    {
        // Already placing a tower, cancel it first
        if (isPlacingTower)
        {
            CancelTowerPlacement();
        }
        
        selectedTowerType = towerType;
        
        // Check if player can afford this tower
        if (!towerFactory.CanAffordTower(selectedTowerType))
        {
            Debug.Log("Not enough gold to build this tower!");
            return;
        }
        
        // Create tower preview
        towerPreview = towerFactory.CreateTowerPreview(selectedTowerType);
        if (towerPreview != null)
        {
            isPlacingTower = true;
        }
    }
    
    /// <summary>
    /// Updates the tower preview position to follow mouse
    /// </summary>
    private void UpdateTowerPreview()
    {
        if (towerPreview == null) return;
        
        // Get mouse position in world coordinates
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        // Snap to grid if needed
        Vector3 snappedPos = SnapToGrid(mousePos);
        
        // Update preview position
        towerPreview.transform.position = snappedPos;
        
        // Change color based on valid placement
        bool validPlacement = LevelManager.Instance.CanPlaceTower(snappedPos);
        UpdatePreviewColor(validPlacement);
    }
    
    /// <summary>
    /// Places the tower at the current position
    /// </summary>
    private void PlaceTower()
    {
        if (towerPreview == null) return;
        
        Vector3 position = towerPreview.transform.position;
        
        // Check if position is valid for placement
        if (LevelManager.Instance.CanPlaceTower(position))
        {
            // Create a command for tower placement
            PlaceTowerCommand command = new PlaceTowerCommand(
                selectedTowerType, 
                position, 
                towerFactory, 
                LevelManager.Instance
            );
            
            // Execute the command
            if (CommandManager.Instance.ExecuteCommand(command))
            {
                // Successfully placed, clean up preview
                Destroy(towerPreview);
                towerPreview = null;
                isPlacingTower = false;
            }
        }
    }
    
    /// <summary>
    /// Cancels the current tower placement
    /// </summary>
    public void CancelTowerPlacement()
    {
        if (towerPreview != null)
        {
            Destroy(towerPreview);
            towerPreview = null;
        }
        
        isPlacingTower = false;
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
            Color color = validPlacement ? Color.green : Color.red;
            color.a = 0.5f; // Semi-transparent
            
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
        // Simple grid snapping - adjust grid size as needed
        float gridSize = 1.0f;
        
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = Mathf.Round(position.y / gridSize) * gridSize;
        
        return new Vector3(x, y, 0);
    }
}