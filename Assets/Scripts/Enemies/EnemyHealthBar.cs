using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Color healthColor = Color.red;
    
    private Enemy enemyReference;
    private Canvas canvas;

    private void Awake()
    {
        // Find required components
        enemyReference = GetComponentInParent<Enemy>();
        canvas = GetComponent<Canvas>();
        
        // Configure the health image
        if (healthFillImage == null)
            healthFillImage = GetComponentInChildren<Image>();
        
        if (healthFillImage != null)
        {
            healthFillImage.type = Image.Type.Filled;
            healthFillImage.fillMethod = Image.FillMethod.Horizontal;
            healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthFillImage.color = healthColor;
            healthFillImage.fillAmount = 1f;
        }
    }

    private void Start()
    {
        if (enemyReference == null)
        {
            return;
        }
        
        // Ensure the canvas is world space and faces the camera
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
        }
        
        // Subscribe to health change events
        enemyReference.OnHealthChanged += UpdateHealthBar;
        
        // Set initial health
        UpdateHealthBar(enemyReference.Health, enemyReference.MaxHealth);
    }
    
    private void OnDestroy()
    {
        if (enemyReference != null)
            enemyReference.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFillImage != null)
        {
            // Calculate health percentage
            float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0;
            
            // Ensure it's between 0 and 1
            healthPercent = Mathf.Clamp01(healthPercent);
            
            // Set the fill amount directly
            healthFillImage.fillAmount = healthPercent;
            
            // Log for debugging
            //Debug.Log($"Health bar updated: {currentHealth}/{maxHealth} = {healthPercent:F2}, Fill: {healthFillImage.fillAmount:F2}");
        }
    }
    
    // Always face the camera
    private void LateUpdate()
    {
        if (canvas != null && Camera.main != null)
        {
            canvas.transform.forward = Camera.main.transform.forward;
        }
    }
}