using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Fast enemy implementation - Moves quickly but has less health
/// </summary>
public class FastEnemy : Enemy
{
    [SerializeField] private TrailRenderer trailEffect;
    [SerializeField] private float dodgeChance = 0.2f;
    [SerializeField] private GameObject dodgeTextPrefab;
    
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
            
            // Show text "Dodge!"
            ShowDodgeText();
            
            return 0f; // No damage taken
        }
        
        return base.TakeDamage(amount);
    }
    
    private void ShowDodgeText()
    {
        if (dodgeTextPrefab != null)
        {
            // Instantiate the dodge text prefab at the enemy's position
            Vector3 textPosition = transform.position + new Vector3(0, 0.5f, 0);
            GameObject dodgeTextObject = Instantiate(dodgeTextPrefab, textPosition, Quaternion.identity);
            
            // Make the text go upwards and destroy it after a few seconds
            StartCoroutine(AnimateText(dodgeTextObject));
        }
    }

    private IEnumerator AnimateText(GameObject textObject)
    {
        TextMeshPro textMesh = textObject.GetComponent<TextMeshPro>();
        if (textMesh == null) yield break;
        
        // Text properties
        textMesh.text = "Dodge!";
        textMesh.color = new Color(1f, 0.5f, 0f); // Orange color
        
        float duration = 1.0f;
        float timer = 0f;
        
        // Initial position
        Vector3 startPos = textObject.transform.position;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            
            // Move upwards
            textObject.transform.position = startPos + new Vector3(0, timer * 0.5f, 0);
            
            // Fade out effect
            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
            
            yield return null;
        }
        
        Destroy(textObject);
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