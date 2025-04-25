using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Factory pattern implementation for creating different types of towers
/// </summary>
public class TowerFactory : MonoBehaviour
{
    [System.Serializable]
    public class TowerPrefab
    {
        public TowerType type;
        public GameObject prefab;
        public int cost;
        public string description;
        public Sprite icon;
    }
    
    public enum TowerType
    {
        Archer,
        Mage,
        Catapult
    }
    
    [Header("Tower Prefabs")]
    [SerializeField] private List<TowerPrefab> towerPrefabs = new List<TowerPrefab>();
    
    // Cache prefabs in a dictionary for faster lookup
    private Dictionary<TowerType, TowerPrefab> towerDictionary = new Dictionary<TowerType, TowerPrefab>();
    
    private void Awake()
    {
        // Initialize dictionary
        foreach (TowerPrefab tower in towerPrefabs)
        {
            towerDictionary[tower.type] = tower;
        }
    }
    
    /// <summary>
/// Creates a tower of the specified type at the given position
/// </summary>
public Tower CreateTower(TowerType type, Vector3 position)
{
    // Check if enough gold
    if (!CanAffordTower(type))
    {
        Debug.Log("Not enough gold to build this tower!");
        return null;
    }
    
    // Get prefab from dictionary
    if (towerDictionary.TryGetValue(type, out TowerPrefab towerPrefab))
    {
        // Instantiate tower
        GameObject towerObj = Instantiate(towerPrefab.prefab, position, Quaternion.identity);
        Tower tower = towerObj.GetComponent<Tower>();
        
        if (tower != null)
        {
            // Deduct gold cost
            GameManager.Instance.SpendGold(towerPrefab.cost);
            
            return tower;
        }
    }
    
    return null;
}
    
    /// <summary>
    /// Creates a preview of the tower (for placement)
    /// </summary>
    public GameObject CreateTowerPreview(TowerType type)
    {
        if (towerDictionary.TryGetValue(type, out TowerPrefab towerPrefab))
        {
            // Create preview object
            GameObject previewObj = Instantiate(towerPrefab.prefab);
            
            // Modify preview (e.g., make translucent)
            MakeTowerPreview(previewObj);
            
            return previewObj;
        }
        
        return null;
    }
    
    /// <summary>
    /// Modifies a tower object to look like a preview
    /// </summary>
    private void MakeTowerPreview(GameObject tower)
    {
        // Make all renderers semi-transparent
        Renderer[] renderers = tower.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            // Store original materials
            Material[] originalMaterials = r.materials;
            
            // Create new materials array
            Material[] newMaterials = new Material[originalMaterials.Length];
            
            // Modify each material
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                newMaterials[i] = new Material(originalMaterials[i]);
                
                // Make semi-transparent
                Color color = newMaterials[i].color;
                color.a = 0.5f;
                newMaterials[i].color = color;
            }
            
            // Apply new materials
            r.materials = newMaterials;
        }
        
        // Disable colliders
        Collider2D[] colliders = tower.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D c in colliders)
        {
            c.enabled = false;
        }
        
        // Disable scripts
        Tower towerScript = tower.GetComponent<Tower>();
        if (towerScript != null)
        {
            towerScript.enabled = false;
        }
    }
    
    /// <summary>
    /// Gets the cost of building a specific tower type
    /// </summary>
    public int GetTowerCost(TowerType type)
    {
        if (towerDictionary.TryGetValue(type, out TowerPrefab towerPrefab))
        {
            return towerPrefab.cost;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Checks if player can afford to build a specific tower type
    /// </summary>
    public bool CanAffordTower(TowerType type)
    {
        int cost = GetTowerCost(type);
        return GameManager.Instance.Gold >= cost;
    }
    
    /// <summary>
    /// Gets the prefab for a specific tower type
    /// </summary>
    public GameObject GetTowerPrefab(TowerType type)
    {
        if (towerDictionary.TryGetValue(type, out TowerPrefab towerPrefab))
        {
            return towerPrefab.prefab;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all available tower types
    /// </summary>
    public List<TowerType> GetAvailableTowerTypes()
    {
        return new List<TowerType>(towerDictionary.Keys);
    }
    
    /// <summary>
    /// Gets data for a specific tower type
    /// </summary>
    public TowerPrefab GetTowerData(TowerType type)
    {
        if (towerDictionary.TryGetValue(type, out TowerPrefab towerPrefab))
        {
            return towerPrefab;
        }
        
        return null;
    }
}