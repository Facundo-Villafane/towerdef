using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fast enemy implementation - Moves quickly but has less health
/// </summary>
public class FastEnemy : Enemy
{
    [SerializeField] private TrailRenderer trailEffect;
    [SerializeField] private float dodgeChance = 0.2f;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set specific properties for fast enemy
        //enemyName = "Fast Runner";
        //maxHealth = 70f;
        //moveSpeed = 3.5f;
        //goldReward = 15;
    }
    
    public override float TakeDamage(float amount)
    {
        // Fast enemies have a chance to dodge attacks
        if (Random.value < dodgeChance)
        {
            // Show dodge effect or animation
            ShowDodgeEffect();
            return 0f; // No damage taken
        }
        
        return base.TakeDamage(amount);
    }
    
    private void ShowDodgeEffect()
    {
        // Show a dodge effect (could be a text popup, animation, etc.)
        Debug.Log($"{enemyName} dodged the attack!");
        
        // Example: Flash the sprite
        StartCoroutine(FlashSprite());
    }
    
    private IEnumerator FlashSprite()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.color;
            renderer.color = Color.blue; // Dodge color
            yield return new WaitForSeconds(0.1f);
            renderer.color = originalColor;
        }
    }
    
    protected override void MoveAlongPath()
    {
        base.MoveAlongPath();
        
        // Enhance trail effect based on speed
        if (trailEffect != null)
        {
            trailEffect.time = moveSpeed * 0.2f;
        }
    }
}