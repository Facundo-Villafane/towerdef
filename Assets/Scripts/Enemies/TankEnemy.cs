using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tank enemy implementation - Slow moving but high health and armor
/// </summary>
public class TankEnemy : Enemy
{
    [SerializeField] private float armor = 0.3f; // Damage reduction (0-1)
    [SerializeField] private GameObject armorVisual;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set specific properties for tank enemy
        enemyName = "Heavy Tank";
        maxHealth = 200f;
        moveSpeed = 1.2f;
        goldReward = 25;
    }
    
    public override float TakeDamage(float amount)
    {
        // Apply armor damage reduction
        float reducedDamage = amount * (1f - armor);
        
        // Show armor effect
        if (armorVisual != null)
        {
            StartCoroutine(FlashArmor());
        }
        
        return base.TakeDamage(reducedDamage);
    }
    
    private IEnumerator FlashArmor()
    {
        if (armorVisual != null)
        {
            armorVisual.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            armorVisual.SetActive(false);
        }
    }
    
    public override void Initialize(float healthMultiplier, float speedMultiplier)
    {
        base.Initialize(healthMultiplier, speedMultiplier);
        
        // Tanks get additional armor scaling with level
        armor = Mathf.Min(0.7f, armor + (healthMultiplier - 1f) * 0.1f);
    }
}