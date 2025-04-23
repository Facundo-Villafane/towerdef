using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Data structure for enemy wave configuration
/// </summary>
[Serializable]
public class WaveData
{
    [Tooltip("Unique identifier for this wave")]
    public int waveNumber;
    
    [Tooltip("Enemy spawn count for this wave")]
    public int enemyCount;
    
    [Tooltip("Time between enemy spawns in seconds")]
    public float spawnInterval = 1f;
    
    [Tooltip("Time until next wave in seconds")]
    public float timeToNextWave = 20f;
    
    [Tooltip("Enemy types in this wave with their spawn ratios")]
    public List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    
    [Tooltip("Health multiplier for enemies in this wave")]
    public float healthMultiplier = 1f;
    
    [Tooltip("Speed multiplier for enemies in this wave")]
    public float speedMultiplier = 1f;
}

/// <summary>
/// Data structure for enemy spawn configuration within a wave
/// </summary>
[Serializable]
public class EnemySpawnData
{
    [Tooltip("Reference to enemy prefab")]
    public GameObject enemyPrefab;
    
    [Tooltip("Spawn weight relative to other enemies (higher = more common)")]
    public float spawnWeight = 1f;
}