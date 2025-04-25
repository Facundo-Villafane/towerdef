using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catapult tower specialization - Long range, high damage, slow attack speed, splash damage
/// </summary>
public class CatapultTower : Tower
{
    [Header("Catapult Tower Properties")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 10f; // Speed of the rock projectile
    [SerializeField] private float splashRadius = 2.5f;
    [SerializeField] private Animator catapultAnimator;
    
    [Header("Advanced Settings")]
    [SerializeField] private bool useAnimation = false; // Toggle animation usage
    [SerializeField] private string animationTriggerName = "Launch"; // Animation trigger name
    protected override void Start()
    {
        base.Start();
    }
    
    /// <summary>
    /// Overridden method for catapult tower attack behavior
    /// </summary>
    protected override void Attack()
    {
        if (currentTarget == null) return;
        
        // Play attack animation if enabled
        if (useAnimation && catapultAnimator != null)
        {
            catapultAnimator.SetTrigger(animationTriggerName);
        }
        
        // Launch rock projectile
        LaunchRock();
    }
    
    /// <summary>
    /// Creates and launches a rock projectile at the current target
    /// </summary>
    private void LaunchRock()
    {
        if (rockPrefab == null)
        {
            Debug.LogError("Rock prefab not assigned!");
            return;
        }
        
        if (shootPoint == null)
        {
            Debug.LogError("Shoot point not assigned!");
            return;
        }
        
        if (currentTarget == null)
        {
            Debug.LogError("No target to shoot at!");
            return;
        }
        
        // Create projectile
        GameObject rockObj = Instantiate(rockPrefab, shootPoint.position, Quaternion.identity);
        Rock rock = rockObj.GetComponent<Rock>();
        
        if (rock != null)
        {
            // Initialize rock with target enemy, damage and splash radius
            rock.Initialize(currentTarget, damage, projectileSpeed, splashRadius);
            Debug.Log($"Rock launched with speed: {projectileSpeed}");
        }
        else
        {
            Debug.LogError("Rock component not found on prefab!");
            Destroy(rockObj);
        }
    }
    
    /// <summary>
    /// Override upgrade method to add catapult-specific upgrades
    /// </summary>
    public override bool Upgrade()
    {
        bool upgraded = base.Upgrade();
        
        if (upgraded)
        {
            // Catapult-specific upgrades
            splashRadius *= 1.2f; // Bigger splash area with upgrades
            projectileSpeed *= 1.1f; // Faster projectiles with upgrades
            
            // Visual upgrade (changing sprite or model could be done here)
        }
        
        return upgraded;
    }
}