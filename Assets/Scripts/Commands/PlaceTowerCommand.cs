using UnityEngine;

/// <summary>
/// Command for placing a tower on the map
/// </summary>
public class PlaceTowerCommand : ICommand
{
    private TowerFactory.TowerType towerType;
    private Vector3 position;
    private TowerFactory towerFactory;
    private LevelManager levelManager;
    private Tower placedTower; // Reference to the placed tower for undo
    
    public PlaceTowerCommand(TowerFactory.TowerType type, Vector3 pos, TowerFactory factory, LevelManager manager)
    {
        towerType = type;
        position = pos;
        towerFactory = factory;
        levelManager = manager;
    }
    
    /// <summary>
    /// Checks if the tower can be placed at the specified position
    /// </summary>
    public bool CanExecute()
    {
        // Check if player can afford the tower
        if (!towerFactory.CanAffordTower(towerType))
        {
            return false;
        }
        
        // Check if position is valid for tower placement
        if (!levelManager.CanPlaceTower(position))
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Places the tower at the specified position
    /// </summary>
    public void Execute()
    {
        if (CanExecute())
        {
            // Create the tower
            placedTower = towerFactory.CreateTower(towerType, position);
            
            // Place it on the map
            if (placedTower != null)
            {
                levelManager.PlaceTower(placedTower, position);
            }
        }
    }
    
    /// <summary>
    /// Undoes the tower placement
    /// </summary>
    public void Undo()
    {
        if (placedTower != null)
        {
            // Refund the tower cost
            GameManager.Instance.AddGold(towerFactory.GetTowerCost(towerType));
            
            // Destroy the tower
            GameObject.Destroy(placedTower.gameObject);
            placedTower = null;
        }
    }
}