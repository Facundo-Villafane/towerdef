using UnityEngine;

/// <summary>
/// Interface for wave management system
/// </summary>
public interface IWaveManager
{
    /// <summary>
    /// Initializes wave manager with level data
    /// </summary>
    void Initialize(int level, float difficulty);
    
    /// <summary>
    /// Starts spawning waves
    /// </summary>
    void StartWaves();
    
    /// <summary>
    /// Gets the current wave number
    /// </summary>
    int GetCurrentWave();
    
    /// <summary>
    /// Gets the total number of waves
    /// </summary>
    int GetTotalWaves();
}