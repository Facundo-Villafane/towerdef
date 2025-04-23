using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Archer tower specialization - Long range, moderate damage, fast attack speed
/// </summary>
public class ArcherTower : Tower
{
    [Header("Archer Tower Properties")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private Animator archerAnimator;
    
    protected override void Start()
    {
        base.Start();
        towerName = "Archer Tower";
        
        // Set default properties for archer tower
        range = 6f;
        attackSpeed = 1.5f;
        damage = 15f;
        cost = 100;
    }
    
    /// <summary>
    /// Overridden method for archer tower attack behavior
    /// </summary>
    protected override void Attack()
    {
        if (currentTarget == null) return;
        
        // Play attack animation
        if (archerAnimator != null)
        {
            archerAnimator.SetTrigger("Attack");
        }
        
        // Shoot arrow
        ShootArrow();
    }
    
    /// <summary>
    /// Creates and shoots an arrow at the current target
    /// </summary>
    private void ShootArrow()
    {
        if (arrowPrefab == null || shootPoint == null || currentTarget == null) return;
        
        // Create arrow
        GameObject arrowObj = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        Arrow arrow = arrowObj.GetComponent<Arrow>();
        
        if (arrow != null)
        {
            // Initialize arrow with target and damage
            arrow.Initialize(currentTarget, damage, arrowSpeed);
        }
        else
        {
            // If arrow component not found, destroy the object
            Destroy(arrowObj);
        }
    }
    
    /// <summary>
    /// Override upgrade method to add archer-specific upgrades
    /// </summary>
    public override bool Upgrade()
    {
        bool upgraded = base.Upgrade();
        
        if (upgraded)
        {
            // Archer-specific upgrades
            arrowSpeed *= 1.1f; // Arrows get faster with upgrades
            
            // Visual upgrade (changing sprite or model could be done here)
        }
        
        return upgraded;
    }
}