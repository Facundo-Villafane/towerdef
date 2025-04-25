using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rock projectile fired by catapult towers with direct trajectory and splash damage
/// </summary>
public class Rock : MonoBehaviour
{
    // Configuración interna
    private Enemy target;
    private float damage;
    private float speed;
    private float splashRadius;
    private bool initialized = false;
    
    // Configuración visible en el Inspector
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer rockSprite;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private float rotationSpeed = 90f; // Grades per second for rotation
    [Header("Projectile Settings")]
    [SerializeField] private float minImpactDistance = 0.2f; // Distance to impact target
    [SerializeField] private float speedMultiplier = 1.5f; // Multiplier for speed
    
    /// <summary>
    /// Initialize the rock projectile with target and parameters
    /// </summary>
    public void Initialize(Enemy targetEnemy, float damageAmount, float projectileSpeed, float areaRadius)
    {
        target = targetEnemy;
        damage = damageAmount;
        speed = projectileSpeed * speedMultiplier; // Aplicar multiplicador
        splashRadius = areaRadius;
        initialized = true;
        
        // Start rotating the rock for visual effect
        StartCoroutine(RotateRock());
        
        // If target is destroyed immediately, destroy rock too
        if (target == null)
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (!initialized || target == null || !target.IsAlive)
        {
            // Target died or was destroyed, destroy rock
            Destroy(gameObject);
            return;
        }
        
        // Move toward target
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Check if we hit the target
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget < minImpactDistance)
        {
            CreateImpact();
        }
    }
    
    /// <summary>
    /// Creates impact effect and applies damage to enemies in splash radius
    /// </summary>
    private void CreateImpact()
    {
        Vector3 impactPosition = transform.position;
        
        // Spawn impact effect
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, impactPosition, Quaternion.identity);
        }
        
        // Deal splash damage to enemies
        if (splashRadius > 0)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(impactPosition, splashRadius, LayerMask.GetMask("Enemy"));
            
            foreach (Collider2D collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    // Calculate damage falloff based on distance from impact
                    float distance = Vector2.Distance(impactPosition, enemy.transform.position);
                    float damagePercent = 1f - (distance / splashRadius);
                    damagePercent = Mathf.Clamp01(damagePercent);
                    
                    // Apply damage with falloff
                    float actualDamage = damage * damagePercent;
                    enemy.TakeDamage(actualDamage);
                }
            }
        }
        else
        {
            // If no splash radius, just damage the direct target
            if (target != null && target.IsAlive)
            {
                target.TakeDamage(damage);
            }
        }
        
        // Destroy rock after impact
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Coroutine to rotate the rock during flight for visual effect
    /// </summary>
    private IEnumerator RotateRock()
    {
        while (true)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize splash radius in editor
        if (splashRadius > 0 && Application.isEditor)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, splashRadius);
        }
    }
}