using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Stores and manages level map data including enemy paths and tower placement areas
/// </summary>
public class LevelMap : MonoBehaviour
{
    [SerializeField] private List<Vector2> pathPoints = new List<Vector2>();
    [SerializeField] private List<Rect> towerAreas = new List<Rect>();
    [SerializeField] private string mapName = "New Map";
    [SerializeField] private int levelNumber = 1;

    public List<Vector2> PathPoints => pathPoints;
    public List<Rect> TowerAreas => towerAreas;
    public string MapName => mapName;
    public int LevelNumber => levelNumber;

    /// <summary>
    /// Adds a point to the enemy path
    /// </summary>
    public void AddPathPoint(Vector2 point)
    {
        pathPoints.Add(point);
    }

    /// <summary>
    /// Clears the current enemy path
    /// </summary>
    public void ClearPath()
    {
        pathPoints.Clear();
    }

    /// <summary>
    /// Gets a serializable representation of the map data
    /// </summary>
    public MapData GetMapData()
    {
        MapData data = new MapData();
        data.mapName = this.mapName;
        data.levelNumber = this.levelNumber;
        data.pathPoints = new List<SerializableVector2>();
        data.towerAreas = new List<SerializableRect>();

        // Convert Vector2 to SerializableVector2
        foreach (Vector2 point in pathPoints)
        {
            SerializableVector2 serPoint = new SerializableVector2();
            serPoint.x = point.x;
            serPoint.y = point.y;
            data.pathPoints.Add(serPoint);
        }

        // Convert Rect to SerializableRect
        foreach (Rect area in towerAreas)
        {
            SerializableRect serRect = new SerializableRect();
            serRect.x = area.x;
            serRect.y = area.y;
            serRect.width = area.width;
            serRect.height = area.height;
            data.towerAreas.Add(serRect);
        }

        return data;
    }

    /// <summary>
    /// Loads map data from a serializable object
    /// </summary>
    public void LoadMapData(MapData data)
    {
        mapName = data.mapName;
        levelNumber = data.levelNumber;
        
        // Clear existing data
        pathPoints.Clear();
        towerAreas.Clear();
        
        // Convert SerializableVector2 to Vector2
        foreach (SerializableVector2 point in data.pathPoints)
        {
            pathPoints.Add(new Vector2(point.x, point.y));
        }
        
        // Convert SerializableRect to Rect
        foreach (SerializableRect area in data.towerAreas)
        {
            towerAreas.Add(new Rect(area.x, area.y, area.width, area.height));
        }
    }

    /// <summary>
    /// Visualizes the path and tower areas in the game view for debugging
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw path points and lines
        Gizmos.color = Color.green;
        
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Gizmos.DrawSphere(pathPoints[i], 0.2f);
            
            if (i < pathPoints.Count - 1)
            {
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
        }
        
        // Draw tower areas
        Gizmos.color = new Color(0, 0, 1, 0.3f); // Semitransparent blue
        
        foreach (Rect area in towerAreas)
        {
            Gizmos.DrawCube(new Vector3(area.x + area.width / 2, area.y + area.height / 2, 0), 
                            new Vector3(area.width, area.height, 0.1f));
        }
    }
}

/// <summary>
/// Serializable class for storing map data
/// </summary>
[Serializable]
public class MapData
{
    public string mapName;
    public int levelNumber;
    public List<SerializableVector2> pathPoints;
    public List<SerializableRect> towerAreas;
}

/// <summary>
/// Serializable version of Vector2
/// </summary>
[Serializable]
public class SerializableVector2
{
    public float x;
    public float y;
}

/// <summary>
/// Serializable version of Rect
/// </summary>
[Serializable]
public class SerializableRect
{
    public float x;
    public float y;
    public float width;
    public float height;
}