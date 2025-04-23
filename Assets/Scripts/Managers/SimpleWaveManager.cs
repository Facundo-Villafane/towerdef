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
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;
    
    // Wave state
    private int currentWave = 0;
    private int enemiesRemainingInWave = 0;
    private float waveTimer = 0f;
    private bool wavesActive = false;
    
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
    }
    
    public void StartWaves()
    {
        wavesActive = true;
        StartNextWave();
    }
    
    /// <summary>
    /// Starts the next wave of enemies
    /// </summary>
    public void StartNextWave()
    {
        currentWave++;
        
        // Check if all waves are completed
        if (currentWave > totalWaves)
        {
            OnAllWavesCompleted?.Invoke(100);
            return;
        }
        
        // Calculate enemies for this wave
        int enemyCount = 5 + (currentWave * 2);
        enemiesRemainingInWave = enemyCount;
        
        // Notify listeners
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
                GameObject enemyObj = Instantiate(enemyPrefab, path[0], Quaternion.identity, enemyParent);
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                
                if (enemy != null)
                {
                    // Setup enemy
                    enemy.SetPath(path);
                    enemy.Initialize(healthMultiplier, speedMultiplier);
                    
                    // Subscribe to events
                    enemy.OnEnemyDeath += OnEnemyDefeated;
                    enemy.OnEnemyReachedEnd += OnEnemyReachedEnd;
                }
            }
            
            // Wait before spawning next enemy
            yield return new WaitForSeconds(spawnInterval);
        }
        
        // Start timer for next wave
        waveTimer = timeBetweenWaves;
    }
    
    private GameObject SelectEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return null;
            
        // Simple selection logic based on wave progression
        float waveProgress = (float)currentWave / totalWaves;
        
        // Later waves have more chance to spawn stronger enemies
        if (Random.value < waveProgress && enemyPrefabs.Length > 1)
        {
            // Select from stronger enemies
            int index = Random.Range(1, enemyPrefabs.Length);
            return enemyPrefabs[index];
        }
        
        // Default to basic enemy
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
        CheckWaveComplete();
    }
    
    private void OnEnemyReachedEnd(Enemy enemy)
    {
        // Unsubscribe
        enemy.OnEnemyDeath -= OnEnemyDefeated;
        enemy.OnEnemyReachedEnd -= OnEnemyReachedEnd;
        
        // Damage player
        GameManager.Instance.LoseLife(enemy.DamageToPlayer);
        
        // Update counter
        enemiesRemainingInWave--;
        CheckWaveComplete();
    }
    
    private void CheckWaveComplete()
    {
        if (enemiesRemainingInWave <= 0)
        {
            // Notify wave completed
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