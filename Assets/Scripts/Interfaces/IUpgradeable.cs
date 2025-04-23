using UnityEngine;

/// <summary>
/// Interface for entities that can be upgraded, such as towers
/// </summary>
public interface IUpgradeable
{
    /// <summary>
    /// Checks if the entity can be upgraded
    /// </summary>
    bool CanUpgrade();
    
    /// <summary>
    /// Upgrades the entity
    /// </summary>
    /// <returns>True if upgrade was successful, False otherwise</returns>
    bool Upgrade();
    
    /// <summary>
    /// Current upgrade level of the entity
    /// </summary>
    int UpgradeLevel { get; }
    
    /// <summary>
    /// Maximum possible upgrade level for this entity
    /// </summary>
    int MaxUpgradeLevel { get; }
}