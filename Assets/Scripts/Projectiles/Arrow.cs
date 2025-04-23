using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Arrow projectile fired by archer towers
/// </summary>
public class Arrow : MonoBehaviour
{
    private Enemy target;
    private float damage;
    private float speed;
    private bool initialized = false;
    
    // Visual components
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trailRenderer;
    
    /// <summary>
    /// Initialize the arrow with target and damage values
    /// </summary>
    public void Initialize(Enemy targetEnemy, float damageAmount, float arrowSpeed)
    {
        target = targetEnemy;
        damage = damageAmount;
        speed = arrowSpeed;
        initialized = true;
        
        // If target is destroyed immediately, destroy arrow too
        if (target == null)
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (!initialized || target == null || !target.IsAlive)
        {
            // Target died or was destroyed, destroy arrow
            Destroy(gameObject);
            return;
        }
        
        // Move toward target
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Rotate arrow to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Check if we hit the target
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget < 0.2f)
        {
            HitTarget();
        }
    }
    
    /// <summary>
    /// Handles logic when arrow hits its target
    /// </summary>
    private void HitTarget()
    {
        if (target != null && target.IsAlive)
        {
            // Apply damage to target
            target.TakeDamage(damage);
            
            // Create hit effect (could be implemented here)
            
            // Destroy arrow
            Destroy(gameObject);
        }
    }
}