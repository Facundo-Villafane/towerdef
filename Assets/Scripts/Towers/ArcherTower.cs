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
        //towerName = "Archer Tower";
        
        // Set default properties for archer tower
        //range = 1f;
        //attackSpeed = 1.5f;
        //damage = 15f;
        //cost = 100;
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
        if (arrowPrefab == null)
        {
            //Debug.LogError($"{name}: Arrow prefab is null!");
            return;
        }
        
        if (shootPoint == null)
        {
            //Debug.LogError($"{name}: Shoot point is null!");
            return;
        }
        
        if (currentTarget == null)
        {
            //Debug.LogError($"{name}: Current target is null!");
            return;
        }
        
        
        // Create arrow
        GameObject arrowObj = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        //Debug.Log($"Arrow created at {shootPoint.position} targeting {currentTarget.name}");

        // Get the SpriteRenderer component
        SpriteRenderer arrowRenderer = arrowObj.GetComponent<SpriteRenderer>();
        if (arrowRenderer != null)
        {
            //Debug.Log($"Arrow sprite renderer: Color={arrowRenderer.color}, SortingLayer={arrowRenderer.sortingLayerName}, OrderInLayer={arrowRenderer.sortingOrder}");
        }

        Arrow arrow = arrowObj.GetComponent<Arrow>();
        
        if (arrow != null)
        {
            // Initialize arrow with target and damage
            arrow.Initialize(currentTarget, damage, arrowSpeed);
            //Debug.Log("Arrow component initialized successfully");
        }
        else
        {
            //Debug.LogError("Arrow component not found on prefab!");
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