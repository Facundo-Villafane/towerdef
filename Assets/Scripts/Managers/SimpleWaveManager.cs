using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages enemy wave spawning with a simpler implementation
/// </summary>
public class SimpleWaveManager : Singleton<SimpleWaveManager>, IWaveManager
{
    [Header("Wave Settings")]
    [SerializeField] private int totalWaves = 10;
    [SerializeField] private float timeBetweenWaves = 20f;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private Transform enemyParent;
    public bool WavesStarted { get; private set; } = false;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;
    
    // Wave state
    private int currentWave = 0;
    private int enemiesRemainingInWave = 0;
    private float waveTimer = 0f;
    private bool wavesActive = false;
    public bool IsWaitingForNextWave => enemiesRemainingInWave <= 0 && wavesActive && currentWave < totalWaves;
    public float TimeUntilNextWave => waveTimer;
    
    // Events
    public System.Action<int, int> OnWaveStarted;
    public System.Action<int, int> OnWaveCompleted;
    public System.Action<int> OnAllWavesCompleted;
    
    
    protected override void OnAwake()
    {
        if (enemyParent == null)
        {
            enemyParent = new GameObject("Enemies").transform;
        }
    }
    
    // For debugging purposes only!!
    private void Start()
    {
        List<Vector2> path = FindObjectOfType<LevelManager>().GetPath();
        Debug.Log($"Path points: {path.Count}");
        foreach (var point in path)
        {
            Debug.Log($"Path point: {point}");
        }
    }
    
    private void Update()
    {
        if (!wavesActive) return;
        
        // Handle wave timer
        if (enemiesRemainingInWave <= 0)
        {
            waveTimer -= Time.deltaTime;
            
            if (waveTimer <= 0)
            {
                StartNextWave();
            }
        }
    }
    
    public void Initialize(int level, float difficulty)
    {
        // Reset state
        currentWave = 0;
        enemiesRemainingInWave = 0;
        wavesActive = false;
        
        Debug.Log($"SimpleWaveManager initialized with level {level} and difficulty {difficulty}");
    }
    
    public void StartWaves()
    {
        Debug.Log("StartWaves called - beginning wave spawning");
        wavesActive = true;
        WavesStarted = true; 
        StartNextWave();
    }

    /// <summary>
    /// Restart the current wave
    /// </summary>
    public void RestartCurrentWave()
    {
    
        CleanupCurrentWave();
        
        // If we are in a valid wave, just restart it
        if (currentWave > 0 && currentWave <= totalWaves)
        {
            
            currentWave--;
            StartNextWave();
        }
        else if (currentWave == 0)
        {
            // If we are at the beginning, just start the first wave
            StartNextWave();
        }
    }

    /// <summary>
    /// Go back to the previous wave
    /// </summary>
    public void PreviousWave()
    {
        // It only allows going back if we are not on the first wave
        if (currentWave > 1)
        {
            // Stop coroutines and clean up enemies
            CleanupCurrentWave();
            
            // Go back to the previous wave
            currentWave -= 2;
            
            // Start the next wave
            StartNextWave();
        }
    }

    /// <summary>
    /// Reset all waves to the initial state
    /// </summary>
    public void ResetWaves()
    {
        // Stop all coroutines and clean up current wave
        CleanupCurrentWave();
        
        // Restart current wave
        currentWave = 0;
        enemiesRemainingInWave = 0;
        wavesActive = false;
        waveTimer = 0f;
        WavesStarted = false;
    }
    
    /// <summary>
    /// Clean up the current wave by destroying all active enemies and stopping coroutines
    /// </summary>
    private void CleanupCurrentWave()
    {
        // Destroy all active enemies
        GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in activeEnemies)
        {
            Destroy(enemy);
        }
        
        // Stop all coroutines to prevent spawning new enemies
        StopAllCoroutines();
        
        // Reset enemies count
        enemiesRemainingInWave = 0;
    }
    
    /// <summary>
    /// Starts the next wave of enemies
    /// </summary>
    public void StartNextWave()
    {
        currentWave++;
        Debug.Log($"Starting wave {currentWave} of {totalWaves}");
        
        // Check if all waves are completed
        if (currentWave > totalWaves)
        {
            Debug.Log("All waves completed!");
            OnAllWavesCompleted?.Invoke(100);
            return;
        }
        
        // Calculate enemies for this wave
        int enemyCount = 5 + (currentWave * 2);
        enemiesRemainingInWave = enemyCount;
        
        // Notify listeners
        Debug.Log($"Invoking OnWaveStarted event with {currentWave}, {totalWaves}");
        OnWaveStarted?.Invoke(currentWave, totalWaves);
        
        // Start spawning enemies
        StartCoroutine(SpawnEnemies(enemyCount));
    }
    
    private IEnumerator SpawnEnemies(int count)
    {
        List<Vector2> path = FindObjectOfType<LevelManager>().GetPath();
        
        if (path == null || path.Count < 2)
        {
            Debug.LogError("No valid path found for enemies");
            yield break;
        }
        
        Debug.Log($"Spawning {count} enemies on path with {path.Count} points");
        Debug.Log($"First path point: {path[0]}, Last path point: {path[path.Count - 1]}");
        
        // Health and speed increase with wave number
        float healthMultiplier = 1f + (currentWave - 1) * 0.1f;
        float speedMultiplier = 1f + (currentWave - 1) * 0.05f;
        
        for (int i = 0; i < count; i++)
        {
            // Select enemy type based on current wave
            GameObject enemyPrefab = SelectEnemyPrefab();
            
            if (enemyPrefab != null)
            {
                // Spawn enemy
                Vector3 spawnPosition = new Vector3(path[0].x, path[0].y, -1f);
                GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, enemyParent);
                Debug.Log($"Enemy spawned at {path[0]}");
                
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                
                if (enemy != null)
                {
                    // Setup enemy
                    enemy.SetPath(path);
                    enemy.Initialize(healthMultiplier, speedMultiplier);
                    
                    // Subscribe to events
                    enemy.OnEnemyDeath += OnEnemyDefeated;
                    enemy.OnEnemyReachedEnd += OnEnemyReachedEnd;
                    
                    Debug.Log($"Enemy initialized with health multiplier {healthMultiplier} and speed multiplier {speedMultiplier}");
                }
                else
                {
                    Debug.LogError("Enemy component not found on prefab!");
                }
            }
            else
            {
                Debug.LogError("No enemy prefab selected!");
            }
            
            // Wait before spawning next enemy
            yield return new WaitForSeconds(spawnInterval);
        }
        
        // Start timer for next wave
        waveTimer = timeBetweenWaves;
        Debug.Log($"All enemies spawned. Next wave in {timeBetweenWaves} seconds");
    }
    
    private GameObject SelectEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned!");
            return null;
        }
            
        // Simple selection logic based on wave progression
        float waveProgress = (float)currentWave / totalWaves;
        
        // Later waves have more chance to spawn stronger enemies
        if (Random.value < waveProgress && enemyPrefabs.Length > 1)
        {
            // Select from stronger enemies
            int index = Random.Range(1, enemyPrefabs.Length);
            Debug.Log($"Selected stronger enemy type index: {index}");
            return enemyPrefabs[index];
        }
        
        // Default to basic enemy
        Debug.Log("Selected basic enemy type");
        return enemyPrefabs[0];
    }
    
    private void OnEnemyDefeated(Enemy enemy)
    {
        // Unsubscribe
        enemy.OnEnemyDeath -= OnEnemyDefeated;
        enemy.OnEnemyReachedEnd -= OnEnemyReachedEnd;
        
        // Add gold and score
        GameManager.Instance.AddGold(enemy.GoldReward);
        GameManager.Instance.AddScore(enemy.GoldReward * 2);
        
        // Update counter
        enemiesRemainingInWave--;
        Debug.Log($"Enemy defeated. Remaining: {enemiesRemainingInWave}");
        CheckWaveComplete();
    }
    
    private void OnEnemyReachedEnd(Enemy enemy)
    {
        // Unsubscribe
        enemy.OnEnemyDeath -= OnEnemyDefeated;
        enemy.OnEnemyReachedEnd -= OnEnemyReachedEnd;
        
        // Damage player
        GameManager.Instance.LoseLife(enemy.DamageToPlayer);
        Debug.Log($"Enemy reached end. Player lost {enemy.DamageToPlayer} lives");
        
        // Update counter
        enemiesRemainingInWave--;
        CheckWaveComplete();
    }
    
    private void CheckWaveComplete()
    {
        if (enemiesRemainingInWave <= 0)
        {
            // Notify wave completed
            Debug.Log($"Wave {currentWave} completed!");
            OnWaveCompleted?.Invoke(currentWave, totalWaves);
        }
    }
    
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    public int GetTotalWaves()
    {
        return totalWaves;
    }
}