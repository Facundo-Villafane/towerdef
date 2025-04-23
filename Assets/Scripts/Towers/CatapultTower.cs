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
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float splashRadius = 2.5f;
    [SerializeField] private Animator catapultAnimator;
    [SerializeField] private float launchHeight = 3f; // Height of projectile arc
    
    protected override void Start()
    {
        base.Start();
        towerName = "Catapult Tower";
        
        // Set default properties for catapult tower
        range = 7f;
        attackSpeed = 0.6f;
        damage = 35f;
        cost = 200;
    }
    
    /// <summary>
    /// Overridden method for catapult tower attack behavior
    /// </summary>
    protected override void Attack()
    {
        if (currentTarget == null) return;
        
        // Play attack animation
        if (catapultAnimator != null)
        {
            catapultAnimator.SetTrigger("Launch");
        }
        
        // Launch rock projectile
        LaunchRock();
    }
    
    /// <summary>
    /// Creates and launches a rock projectile at the current target
    /// </summary>
    private void LaunchRock()
    {
        if (rockPrefab == null || shootPoint == null || currentTarget == null) return;
        
        // Create projectile
        GameObject rockObj = Instantiate(rockPrefab, shootPoint.position, Quaternion.identity);
        Rock rock = rockObj.GetComponent<Rock>();
        
        if (rock != null)
        {
            // Calculate where the target will be when the rock lands
            Vector3 targetPosition = PredictTargetPosition();
            
            // Initialize rock with target position, damage and splash parameters
            rock.Initialize(targetPosition, damage, projectileSpeed, splashRadius, launchHeight);
        }
        else
        {
            // If rock component not found, destroy the object
            Destroy(rockObj);
        }
    }
    
    /// <summary>
    /// Predicts where the target will be when the projectile lands
    /// </summary>
    private Vector3 PredictTargetPosition()
    {
        // Basic prediction - could be improved with actual path prediction
        Vector3 targetPosition = currentTarget.transform.position;
        
        // Calculate time for projectile to reach target
        float distance = Vector3.Distance(transform.position, targetPosition);
        float timeToTarget = distance / projectileSpeed;
        
        // Predict target's future position based on current velocity
        Rigidbody2D targetRb = currentTarget.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetPosition += (Vector3)targetRb.velocity * timeToTarget;
        }
        
        return targetPosition;
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
            launchHeight *= 1.1f; // Higher projectile arc with upgrades
            
            // Visual upgrade (changing sprite or model could be done here)
        }
        
        return upgraded;
    }
}