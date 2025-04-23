using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all enemies in the game
/// </summary>
public abstract class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected string enemyName;
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int goldReward = 10;
    [SerializeField] protected int damageToPlayer = 1;
    
    protected float currentHealth;
    protected bool isAlive = true;
    protected List<Vector2> pathPoints = new List<Vector2>();
    protected int currentPathIndex = 0;
    protected float pathProgress = 0f;
    
    public System.Action<Enemy> OnEnemyDeath;
    public System.Action<Enemy> OnEnemyReachedEnd;
    
    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => isAlive;
    public float PathProgress => pathProgress;
    public int GoldReward => goldReward;
    public int DamageToPlayer => damageToPlayer;
    
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }
    
    public virtual void SetPath(List<Vector2> path)
    {
        pathPoints = path;
        transform.position = pathPoints[0];
        currentPathIndex = 1;
    }
    
    protected virtual void Update()
    {
        if (!isAlive) return;
        MoveAlongPath();
    }
    
    protected virtual void MoveAlongPath()
    {
        if (pathPoints.Count < 2 || currentPathIndex >= pathPoints.Count) return;
        
        Vector2 targetPoint = pathPoints[currentPathIndex];
        Vector2 currentPosition = transform.position;
        Vector2 direction = (targetPoint - currentPosition).normalized;
        transform.position = Vector2.MoveTowards(currentPosition, targetPoint, moveSpeed * Time.deltaTime);
        
        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            currentPathIndex++;
            
            if (currentPathIndex >= pathPoints.Count)
            {
                ReachedEnd();
            }
        }
        
        UpdatePathProgress();
    }
    
    protected virtual void UpdatePathProgress()
    {
        if (pathPoints.Count < 2) return;
        
        float totalLength = 0f;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            totalLength += Vector2.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        
        float traveledLength = 0f;
        for (int i = 0; i < currentPathIndex - 1; i++)
        {
            traveledLength += Vector2.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        
        if (currentPathIndex < pathPoints.Count)
        {
            traveledLength += Vector2.Distance(pathPoints[currentPathIndex - 1], transform.position);
        }
        
        pathProgress = Mathf.Clamp01(traveledLength / totalLength);
    }
    
    protected virtual void ReachedEnd()
    {
        OnEnemyReachedEnd?.Invoke(this);
        Die(false);
    }
    
    public virtual float TakeDamage(float amount)
    {
        if (!isAlive) return 0;
        
        float actualDamage = Mathf.Min(currentHealth, amount);
        currentHealth -= actualDamage;
        
        if (currentHealth <= 0)
        {
            Die(true);
        }
        
        return actualDamage;
    }
    
    protected virtual void Die(bool killedByPlayer)
    {
        if (!isAlive) return;
        
        isAlive = false;
        
        if (killedByPlayer)
        {
            OnEnemyDeath?.Invoke(this);
        }
        
        Destroy(gameObject, 0.5f);
    }
    
    public virtual void Initialize(float healthMultiplier, float speedMultiplier)
    {
        maxHealth *= healthMultiplier;
        currentHealth = maxHealth;
        moveSpeed *= speedMultiplier;
    }
}