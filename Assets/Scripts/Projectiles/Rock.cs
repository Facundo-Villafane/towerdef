using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rock projectile fired by catapult towers with parabolic trajectory and splash damage
/// </summary>
public class Rock : MonoBehaviour
{
    private Vector3 targetPosition;
    private float damage;
    private float speed;
    private float splashRadius;
    private float arcHeight;
    private float flightTime;
    private float launchTime;
    private bool initialized = false;
    
    [SerializeField] private SpriteRenderer rockSprite;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private float rotationSpeed = 180f; // Degrees per second
    
    /// <summary>
    /// Initialize the rock projectile with target and parameters
    /// </summary>
    public void Initialize(Vector3 target, float damageAmount, float projectileSpeed, float areaRadius, float height)
    {
        targetPosition = target;
        damage = damageAmount;
        speed = projectileSpeed;
        splashRadius = areaRadius;
        arcHeight = height;
        
        // Calculate approximate flight time based on horizontal distance and speed
        float horizontalDistance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(targetPosition.x, targetPosition.y)
        );
        flightTime = horizontalDistance / speed;
        
        launchTime = Time.time;
        initialized = true;
        
        // Start rotating the rock for visual effect
        StartCoroutine(RotateRock());
    }
    
    private void Update()
    {
        if (!initialized) return;
        
        // Calculate flight progress (0 to 1)
        float elapsedTime = Time.time - launchTime;
        float progress = elapsedTime / flightTime;
        
        if (progress < 1.0f)
        {
            // Move along parabolic arc
            MoveAlongArc(progress);
        }
        else
        {
            // Reached target, create impact
            CreateImpact();
        }
    }
    
    /// <summary>
    /// Moves the rock along a parabolic arc trajectory
    /// </summary>
    private void MoveAlongArc(float progress)
    {
        // Clamp progress to prevent overshooting
        progress = Mathf.Clamp01(progress);
        
        // Calculate position along arc
        Vector3 startPosition = transform.position;
        Vector3 endPosition = targetPosition;
        
        // Interpolate position
        Vector3 currentPos = Vector3.Lerp(startPosition, endPosition, progress);
        
        // Add arc height (parabolic curve)
        // Sin curve gives nice parabolic arc when progress is 0 to 1
        float heightOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
        currentPos.y += heightOffset;
        
        // Update position
        transform.position = currentPos;
        
        // Update shadow position (if implemented)
        UpdateShadow(progress);
    }
    
    /// <summary>
    /// Updates the shadow position below the rock
    /// </summary>
    private void UpdateShadow(float progress)
    {
        // Implement shadow logic here if you have a shadow component
        // This would project a shadow directly below the rock on the ground
    }
    
    /// <summary>
    /// Creates impact effect and applies damage to enemies in splash radius
    /// </summary>
    private void CreateImpact()
    {
        // Spawn impact effect
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, targetPosition, Quaternion.identity);
        }
        
        // Deal splash damage to enemies
        if (splashRadius > 0)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(targetPosition, splashRadius, LayerMask.GetMask("Enemy"));
            
            foreach (Collider2D collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    // Calculate damage falloff based on distance from impact
                    float distance = Vector2.Distance(targetPosition, enemy.transform.position);
                    float damagePercent = 1f - (distance / splashRadius);
                    damagePercent = Mathf.Clamp01(damagePercent);
                    
                    // Apply damage with falloff
                    float actualDamage = damage * damagePercent;
                    enemy.TakeDamage(actualDamage);
                }
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
            Gizmos.DrawSphere(targetPosition, splashRadius);
        }
    }
}