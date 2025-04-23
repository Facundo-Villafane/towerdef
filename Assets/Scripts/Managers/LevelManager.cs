using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages level-specific functionality including map loading, path management, and tower placement
/// </summary>
public class LevelManager : Singleton<LevelManager>
{
    [Header("Level References")]
    [SerializeField] private LevelMap levelMap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap pathTilemap;
    
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float difficultyMultiplier = 1f;
    
    // Level state
    private List<Vector2> currentPath = new List<Vector2>();
    private List<Tower> placedTowers = new List<Tower>();
    
    // Events
    public System.Action<int> OnLevelStart;
    public System.Action<bool> OnLevelComplete; // true = victory, false = defeat
    
    protected override void OnAwake()
    {
        // Initialize level
        LoadLevel(currentLevel);
    }
    
    private void Start()
    {
        // Initialize wave manager with level data
        SimpleWaveManager.Instance.Initialize(currentLevel, difficultyMultiplier);
    }
    
    /// <summary>
    /// Loads a specific level
    /// </summary>
    public void LoadLevel(int levelNumber)
    {
        currentLevel = levelNumber;
        
        // Adjust difficulty based on level
        difficultyMultiplier = 1f + (levelNumber - 1) * 0.2f;
        
        // Load level map
        if (levelMap != null)
        {
            // TODO: Load actual level data from resources
            Debug.Log($"Loading level {levelNumber}");
            
            // For now, just get path from current map
            currentPath = levelMap.PathPoints;
        }
        
        // Clear any existing towers
        ClearPlacedTowers();
    }
    
    /// <summary>
    /// Gets the current path for enemies to follow
    /// </summary>
    public List<Vector2> GetPath()
    {
        return currentPath;
    }
    
    /// <summary>
    /// Checks if a tower can be placed at the specified position
    /// </summary>
    public bool CanPlaceTower(Vector2 position)
    {
        // Check if position is on path
        if (IsPositionOnPath(position))
        {
            return false;
        }
        
        // Check if position is occupied by another tower
        foreach (Tower tower in placedTowers)
        {
            if (Vector2.Distance(tower.transform.position, position) < 0.5f)
            {
                return false;
            }
        }
        
        // Check if position is on valid ground tile
        if (groundTilemap != null)
        {
            Vector3Int cellPosition = groundTilemap.WorldToCell(position);
            return groundTilemap.HasTile(cellPosition) && !pathTilemap.HasTile(cellPosition);
        }
        
        return true;
    }
    
    /// <summary>
    /// Places a tower at the specified position
    /// </summary>
    public bool PlaceTower(Tower tower, Vector2 position)
    {
        if (!CanPlaceTower(position))
        {
            return false;
        }
        
        // Place tower
        tower.PlaceTower(position);
        placedTowers.Add(tower);
        
        return true;
    }
    
    /// <summary>
    /// Checks if a position is on the enemy path
    /// </summary>
    public bool IsPositionOnPath(Vector2 position, float tolerance = 0.5f)
    {
        if (currentPath.Count < 2) return false;
        
        // Check if position is close to any path segment
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector2 start = currentPath[i];
            Vector2 end = currentPath[i + 1];
            
            // Calculate distance from point to line segment
            float distanceToSegment = DistanceToLineSegment(position, start, end);
            
            if (distanceToSegment < tolerance)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Calculates the distance from a point to a line segment
    /// </summary>
    private float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        // Calculate squared length of line segment
        float lengthSq = (lineEnd - lineStart).sqrMagnitude;
        
        // If segment is a point, return distance to that point
        if (lengthSq < 0.0001f)
        {
            return Vector2.Distance(point, lineStart);
        }
        
        // Project point onto line segment
        float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, lineEnd - lineStart) / lengthSq);
        Vector2 projection = lineStart + t * (lineEnd - lineStart);
        
        // Return distance to projection
        return Vector2.Distance(point, projection);
    }
    
    /// <summary>
    /// Clears all placed towers
    /// </summary>
    private void ClearPlacedTowers()
    {
        foreach (Tower tower in placedTowers)
        {
            if (tower != null)
            {
                Destroy(tower.gameObject);
            }
        }
        
        placedTowers.Clear();
    }
    
    /// <summary>
    /// Starts the level
    /// </summary>
    public void StartLevel()
    {
        OnLevelStart?.Invoke(currentLevel);
        SimpleWaveManager.Instance.StartWaves();
    }
    
    /// <summary>
    /// Completes the level
    /// </summary>
    public void CompleteLevel(bool victory)
    {
        OnLevelComplete?.Invoke(victory);
        
        // If victory, unlock next level
        if (victory)
        {
            // TODO: Unlock next level in progression
            Debug.Log($"Level {currentLevel} completed!");
        }
    }
}