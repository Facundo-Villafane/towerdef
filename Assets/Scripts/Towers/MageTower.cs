using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mage tower specialization - Medium range, high damage, area effect
/// </summary>
public class MageTower : Tower
{
    [Header("Mage Tower Properties")]
    [SerializeField] private GameObject magicProjectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float splashRadius = 2f;
    [SerializeField] private Animator mageAnimator;
    
    protected override void Start()
    {
        base.Start();
        towerName = "Mage Tower";
        
        // Set default properties for mage tower
        range = 4.5f;
        attackSpeed = 0.8f;
        damage = 25f;
        cost = 150;
    }
    
    /// <summary>
    /// Overridden method for mage tower attack behavior
    /// </summary>
    protected override void Attack()
    {
        if (currentTarget == null) return;
        
        // Play attack animation
        if (mageAnimator != null)
        {
            mageAnimator.SetTrigger("Cast");
        }
        
        // Shoot magic projectile
        ShootMagicProjectile();
    }
    
    /// <summary>
    /// Creates and shoots a magic projectile at the current target
    /// </summary>
    private void ShootMagicProjectile()
    {
        if (magicProjectilePrefab == null || shootPoint == null || currentTarget == null) return;
        
        // Create projectile
        GameObject projectileObj = Instantiate(magicProjectilePrefab, shootPoint.position, Quaternion.identity);
        MagicProjectile projectile = projectileObj.GetComponent<MagicProjectile>();
        
        if (projectile != null)
        {
            // Initialize projectile with target, damage and splash radius
            projectile.Initialize(currentTarget, damage, projectileSpeed, splashRadius);
        }
        else
        {
            // If projectile component not found, destroy the object
            Destroy(projectileObj);
        }
    }
    
    /// <summary>
    /// Override upgrade method to add mage-specific upgrades
    /// </summary>
    public override bool Upgrade()
    {
        bool upgraded = base.Upgrade();
        
        if (upgraded)
        {
            // Mage-specific upgrades
            splashRadius *= 1.15f; // Splash area increases with upgrades
            
            // Visual upgrade (changing sprite or effects could be done here)
        }
        
        return upgraded;
    }
}