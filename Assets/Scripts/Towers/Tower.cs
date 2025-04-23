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
    
    // Targeting properties
    protected List<Enemy> enemiesInRange = new List<Enemy>();
    protected Enemy currentTarget;
    protected float attackTimer;
    
    // Tower state
    protected bool isPlaced = false;
    
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
        transform.position = position;
        isPlaced = true;
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
        
        // Find all enemies in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy"));
        
        foreach (Collider2D collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                enemiesInRange.Add(enemy);
            }
        }
        
        // Select target based on targeting strategy (default: first in path)
        if (enemiesInRange.Count > 0)
        {
            currentTarget = GetFirstEnemyInPath();
        }
        else
        {
            currentTarget = null;
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
            return false;
            
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
        return distanceToTarget <= range;
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
    /// Visualizes the tower's range in the editor
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, range);
    }
}