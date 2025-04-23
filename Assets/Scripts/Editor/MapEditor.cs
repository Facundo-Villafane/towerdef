using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor tool for creating tower defense maps
/// This editor tool allows designers to:
/// - Draw paths for enemies
/// - Mark areas where towers can be placed
/// - Save/load map configurations
/// </summary>
[CustomEditor(typeof(LevelMap))]
public class MapEditor : Editor
{
    private LevelMap levelMap;
    private Tool lastTool = Tool.None;
    private enum EditMode { DrawPath, MarkTowerArea, None }
    private EditMode currentMode = EditMode.None;
    
    // Path drawing variables
    private List<Vector2> tempPathPoints = new List<Vector2>();
    
    private void OnEnable()
    {
        levelMap = (LevelMap)target;
        lastTool = Tools.current;
        Tools.current = Tool.None;
    }
    
    private void OnDisable()
    {
        Tools.current = lastTool;
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Editor Tools", EditorStyles.boldLabel);
        
        // Mode selection buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Draw Path"))
        {
            currentMode = EditMode.DrawPath;
        }
        
        if (GUILayout.Button("Mark Tower Areas"))
        {
            currentMode = EditMode.MarkTowerArea;
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Action buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Clear Path"))
        {
            levelMap.ClearPath();
            EditorUtility.SetDirty(levelMap);
        }
        
        if (GUILayout.Button("Save Map"))
        {
            SaveMap();
        }
        
        if (GUILayout.Button("Load Map"))
        {
            LoadMap();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Current mode display
        EditorGUILayout.LabelField($"Current Mode: {currentMode}", EditorStyles.boldLabel);
    }
    
    private void OnSceneGUI()
    {
        if (currentMode == EditMode.None) return;
        
        Event e = Event.current;
        
        if (currentMode == EditMode.DrawPath)
        {
            HandlePathDrawing(e);
        }
        else if (currentMode == EditMode.MarkTowerArea)
        {
            HandleTowerAreaMarking(e);
        }
    }
    
    private void HandlePathDrawing(Event e)
    {
        // Handle path point addition on left click
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector2 mousePosition = ray.origin;
            
            // Add point to the temporary path
            tempPathPoints.Add(mousePosition);
            
            // If it's the first point or we've confirmed the path, update the level map
            if (tempPathPoints.Count > 1)
            {
                levelMap.AddPathPoint(mousePosition);
                EditorUtility.SetDirty(levelMap);
            }
            
            e.Use();
        }
        
        // Draw the current path
        Handles.color = Color.green;
        
        // Draw existing path points
        if (levelMap.PathPoints.Count > 0)
        {
            for (int i = 0; i < levelMap.PathPoints.Count; i++)
            {
                Handles.DrawSolidDisc(levelMap.PathPoints[i], Vector3.forward, 0.2f);
                
                if (i < levelMap.PathPoints.Count - 1)
                {
                    Handles.DrawLine(levelMap.PathPoints[i], levelMap.PathPoints[i + 1]);
                }
            }
        }
        
        // If we have temporary points, draw them too
        if (tempPathPoints.Count > 0)
        {
            Handles.DrawSolidDisc(tempPathPoints[tempPathPoints.Count - 1], Vector3.forward, 0.2f);
            
            if (levelMap.PathPoints.Count > 0 && tempPathPoints.Count > 0)
            {
                Handles.DrawLine(levelMap.PathPoints[levelMap.PathPoints.Count - 1], tempPathPoints[0]);
            }
        }
        
        // Always repaint when in this mode
        SceneView.RepaintAll();
    }
    
    private void HandleTowerAreaMarking(Event e)
    {
        // Implement tower area marking logic similar to path drawing
        // This would allow marking rectangular areas where towers can be placed
        
        // Example implementation would be similar to path drawing but with rectangles
        // For brevity, detailed implementation is omitted here
    }
    
    private void SaveMap()
    {
        string path = EditorUtility.SaveFilePanel("Save Map", "Assets/Resources/Maps", "NewMap", "json");
        if (string.IsNullOrEmpty(path)) return;
        
        // Convert path to a relative path
        if (path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }
        
        // Save map data
        string json = JsonUtility.ToJson(levelMap.GetMapData(), true);
        System.IO.File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }
    
    private void LoadMap()
    {
        string path = EditorUtility.OpenFilePanel("Load Map", "Assets/Resources/Maps", "json");
        if (string.IsNullOrEmpty(path)) return;
        
        // Load map data
        string json = System.IO.File.ReadAllText(path);
        MapData mapData = JsonUtility.FromJson<MapData>(json);
        
        levelMap.LoadMapData(mapData);
        EditorUtility.SetDirty(levelMap);
    }
}
