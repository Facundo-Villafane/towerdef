using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all towers in the game
/// Provides common functionality and properties for tower types
/// </summary>
public abstract class Tower : MonoBehaviour, IUpgradeable
{
    [Header("Base Tower Properties")]
    [SerializeField] protected string towerName;
    [SerializeField] protected float range = 5f;
    [SerializeField] protected float attackSpeed = 1f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected int cost = 100;
    [SerializeField] protected int upgradeLevel = 1;
    [SerializeField] protected int maxUpgradeLevel = 3;
    [SerializeField] protected GameObject rangeIndicator;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Targeting properties
    protected List<Enemy> enemiesInRange = new List<Enemy>();
    protected Enemy currentTarget;
    protected float attackTimer;
    
    // Tower state
    protected bool isPlaced = false;
    protected bool isFacingRight = false; // Used for sprite flipping
    
    // Properties
    public float Range => range;
    public float AttackSpeed => attackSpeed;
    public float Damage => damage;
    public int Cost => cost;
    public int UpgradeLevel => upgradeLevel;
    public int MaxUpgradeLevel => maxUpgradeLevel;
    public string TowerName => towerName;
    public bool IsPlaced => isPlaced;
    
    // Events
    public System.Action<Tower> OnTowerPlaced;
    public System.Action<Tower> OnTowerUpgraded;
    public System.Action<Tower> OnTowerSold;
    
    protected virtual void Start()
    {
        attackTimer = 0f;
        UpdateRangeIndicator();
        
        Debug.Log($"Tower {name} started. Range: {range}, Attack Speed: {attackSpeed}, isPlaced: {isPlaced}");
    }
    
    protected virtual void Update()
    {
        if (!isPlaced) return;
        
        attackTimer += Time.deltaTime;
        
        // Find target if we don't have one
        if (currentTarget == null || !IsTargetValid())
        {
            FindTarget();
        }
        else
        {
            // Update the orientation to face the target
            UpdateModelOrientation();
        }
        
        // Attack if we have a target and timer is ready
        if (currentTarget != null && attackTimer >= 1f / attackSpeed)
        {
            Attack();
            attackTimer = 0f;
        }
    }
    
    /// <summary>
    /// Places the tower at the specified position
    /// </summary>
    public virtual void PlaceTower(Vector3 position)
    {
        Debug.Log($"Tower.PlaceTower called with position: {position}");
        transform.position = position;
        Debug.Log($"Tower position after setting: {transform.position}");
        isPlaced = true;
        Debug.Log($"Tower {name} placed at {position}, isPlaced = {isPlaced}");
        OnTowerPlaced?.Invoke(this);
    }
    
    /// <summary>
    /// Checks if the tower can be upgraded
    /// </summary>
    public virtual bool CanUpgrade()
    {
        return upgradeLevel < maxUpgradeLevel;
    }
    
    /// <summary>
    /// Upgrades the tower if possible
    /// </summary>
    public virtual bool Upgrade()
    {
        if (!CanUpgrade()) return false;
        
        upgradeLevel++;
        
        // Apply stat improvements
        range *= 1.2f;
        attackSpeed *= 1.15f;
        damage *= 1.25f;
        
        UpdateRangeIndicator();
        OnTowerUpgraded?.Invoke(this);
        
        return true;
    }
    
    /// <summary>
    /// Sells the tower and returns the refund amount
    /// </summary>
    public virtual int Sell()
    {
        int refundAmount = Mathf.RoundToInt(cost * 0.7f);
        OnTowerSold?.Invoke(this);
        
        // Destroy the tower
        Destroy(gameObject);
        
        return refundAmount;
    }
    
    /// <summary>
    /// Updates the size of the range indicator
    /// </summary>
    protected virtual void UpdateRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.transform.localScale = new Vector3(range * 2, range * 2, 1);
        }
    }
    
    /// <summary>
    /// Finds a target from enemies in range
    /// </summary>
    protected virtual void FindTarget()
    {
        // Clear and repopulate enemies in range
        enemiesInRange.Clear();

        // Verificar la capa Enemy
        int enemyLayerMask = LayerMask.GetMask("Enemy");
        Debug.Log($"Enemy layer mask value: {enemyLayerMask}");
        
        if (enemyLayerMask == 0)
        {
            Debug.LogError("La capa 'Enemy' no existe o no está correctamente configurada!");
        }

        Debug.Log($"Tower {name}: Buscando enemigos en rango {range}");
        
        // Find all enemies in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, enemyLayerMask);
        Debug.Log($"Tower {name}: Encontrados {colliders.Length} colliders dentro del rango");
        
        // Intenta detectar enemigos sin filtro de capa
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(transform.position, range);
        Debug.Log($"Tower {name}: Total de colliders (cualquier capa): {allColliders.Length}");
        
        foreach (var col in allColliders)
        {
            Debug.Log($"- Collider: {col.name}, Layer: {LayerMask.LayerToName(col.gameObject.layer)}");
        }
        
        foreach (Collider2D collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                enemiesInRange.Add(enemy);
                Debug.Log($"Tower {name}: Enemigo añadido a la lista: {enemy.name}");
            }
            else
            {
                Debug.Log($"Tower {name}: Collider no es un enemigo válido: {collider.name}");
            }
        }
        
        // Select target based on targeting strategy (default: first in path)
        if (enemiesInRange.Count > 0)
        {
            currentTarget = GetFirstEnemyInPath();
            Debug.Log($"Tower {name}: Objetivo seleccionado: {currentTarget.name}");
        }
        else
        {
            currentTarget = null;
            Debug.Log($"Tower {name}: No hay enemigos en rango");
            
            // Buscar todos los enemigos en la escena
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            Debug.Log($"Total de enemigos en la escena: {allEnemies.Length}");
            
            foreach (var enemyObj in allEnemies)
            {
                float distance = Vector2.Distance(transform.position, enemyObj.transform.position);
                Debug.Log($"- Enemigo: {enemyObj.name}, Distancia: {distance}, Layer: {LayerMask.LayerToName(enemyObj.layer)}");
            }
        }
    }
    
    /// <summary>
    /// Gets the enemy that is furthest along the path
    /// </summary>
    protected virtual Enemy GetFirstEnemyInPath()
    {
        Enemy firstEnemy = null;
        float highestProgress = -1f;
        
        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy.PathProgress > highestProgress)
            {
                highestProgress = enemy.PathProgress;
                firstEnemy = enemy;
                Debug.Log($"Tower {name}: Encontrado enemigo con mayor progreso: {enemy.name}, Progreso: {enemy.PathProgress}");
            }
        }
        
        return firstEnemy;
    }
    
    /// <summary>
    /// Checks if the current target is still valid
    /// </summary>
    protected virtual bool IsTargetValid()
    {
        if (currentTarget == null || !currentTarget.IsAlive)
        {
            Debug.Log($"Tower {name}: Target inválido o muerto");
            return false;
        }
            
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
        bool inRange = distanceToTarget <= range;
        
        if (!inRange)
        {
            Debug.Log($"Tower {name}: Target fuera de rango. Distancia: {distanceToTarget}, Rango: {range}");
        }
        
        return inRange;
    }
    
    /// <summary>
    /// Updates the orientation of the tower model to face the target
    /// </summary>
    protected virtual void UpdateModelOrientation()
{
    if (currentTarget == null) return;
    
    // Calculate direction to target
    Vector3 direction = currentTarget.transform.position - transform.position;
    bool shouldFaceRight = direction.x > 0;
    
    // Only update if direction changed
    if (shouldFaceRight != isFacingRight)
    {
        isFacingRight = shouldFaceRight;
        
        // Since sprites face LEFT by default, flip when they should face RIGHT
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer.transform == transform) continue;
            
            // For sprites facing left by default
            renderer.flipX = shouldFaceRight;
            Debug.Log($"Tower {name}: Flipped sprite to flipX={renderer.flipX} (shouldFaceRight={shouldFaceRight})");
        }
        
        
    }
}

    /// <summary>
    /// Attacks the current target
    /// </summary>
    protected abstract void Attack();
    
    /// <summary>
    /// Shows the tower's range
    /// </summary>
    public virtual void ShowRange(bool show)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(show);
        }
    }
    
    /// <summary>
    /// Test method to manually check detection
    /// </summary>
    [ContextMenu("Test Detection")]
    public void TestDetection()
    {
        Debug.Log("======= PRUEBA DE DETECCIÓN MANUAL =======");
        Debug.Log($"Tower position: {transform.position}, Range: {range}");
        
        // Detectar TODOS los colliders sin filtro de capa
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(transform.position, range);
        Debug.Log($"Total colliders detected (any layer): {allColliders.Length}");
        
        foreach (var col in allColliders)
        {
            Debug.Log($"- Collider: {col.name}, Layer: {LayerMask.LayerToName(col.gameObject.layer)}");
        }
        
        // Detectar solo en capa Enemy
        int enemyMask = LayerMask.GetMask("Enemy");
        Debug.Log($"Enemy layer mask: {enemyMask}");
        
        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(transform.position, range, enemyMask);
        Debug.Log($"Enemy colliders detected: {enemyColliders.Length}");
        
        // Verificar todos los GameObjects con Tag "Enemy"
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"GameObjects with Enemy tag: {enemyObjects.Length}");
        
        foreach (var enemy in enemyObjects)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            Debug.Log($"- Enemy: {enemy.name}, Distance: {distance}, Layer: {LayerMask.LayerToName(enemy.layer)}");
        }
        
        Debug.Log("======= FIN DE PRUEBA =======");
    }
    
    /// <summary>
    /// Método para validar parámetros desde el Inspector
    /// </summary>
    public void OnValidate()
    {
        if (debugMode)
        {
            debugMode = false;
            TestDetection();
        }
    }
    
    /// <summary>
    /// Visualizes the tower's range in the editor
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, range);
    }

    private void OnDrawGizmos()
    {
        // Draw the range of the tower
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        
        // If there is a current target, draw a line to it
        if (currentTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}