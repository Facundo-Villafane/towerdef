using UnityEngine;

/// <summary>
/// Interface for entities that can receive damage
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Applies damage to the entity
    /// </summary>
    /// <param name="amount">Amount of damage to apply</param>
    /// <returns>Actual amount of damage dealt</returns>
    float TakeDamage(float amount);
    
    /// <summary>
    /// Current health of the entity
    /// </summary>
    float Health { get; }
    
    /// <summary>
    /// Maximum health of the entity
    /// </summary>
    float MaxHealth { get; }
    
    /// <summary>
    /// Indicates if the entity is still alive
    /// </summary>
    bool IsAlive { get; }
}