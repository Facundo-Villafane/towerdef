using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Magic projectile fired by mage towers that can deal area damage
/// </summary>
public class MagicProjectile : MonoBehaviour
{
    private Enemy target;
    private float damage;
    private float speed;
    private float splashRadius;
    private bool initialized = false;
    
    // Visual components
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem trailEffect;
    [SerializeField] private GameObject impactEffectPrefab;
    
    /// <summary>
    /// Initialize the magic projectile with target and parameters
    /// </summary>
    public void Initialize(Enemy targetEnemy, float damageAmount, float projectileSpeed, float areaRadius)
    {
        target = targetEnemy;
        damage = damageAmount;
        speed = projectileSpeed;
        splashRadius = areaRadius;
        initialized = true;
        
        // If target is destroyed immediately, destroy projectile too
        if (target == null)
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (!initialized || target == null || !target.IsAlive)
        {
            // Target died or was destroyed, explode at current position
            ExplodeAtPosition(transform.position);
            return;
        }
        
        // Move toward target
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Rotate projectile to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Check if we hit the target
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget < 0.2f)
        {
            // Hit target, create explosion at target position
            ExplodeAtPosition(target.transform.position);
        }
    }
    
    /// <summary>
    /// Creates an explosion at the specified position, damaging all enemies in splash radius
    /// </summary>
    private void ExplodeAtPosition(Vector3 position)
    {
        // Create impact effect
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, position, Quaternion.identity);
        }
        
        // Deal damage to all enemies in splash radius
        if (splashRadius > 0)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, splashRadius, LayerMask.GetMask("Enemy"));
            
            foreach (Collider2D collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    // Apply damage to enemy
                    // Calculate falloff damage based on distance from explosion center
                    float distance = Vector3.Distance(position, enemy.transform.position);
                    float damageMultiplier = 1f - (distance / splashRadius);
                    damageMultiplier = Mathf.Clamp01(damageMultiplier);
                    
                    float damageAmount = damage * damageMultiplier;
                    enemy.TakeDamage(damageAmount);
                }
            }
        }
        else
        {
            // No splash damage, just damage the direct target
            if (target != null && target.IsAlive)
            {
                target.TakeDamage(damage);
            }
        }
        
        // Destroy the projectile
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize splash radius in editor
        if (splashRadius > 0)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, splashRadius);
        }
    }
}