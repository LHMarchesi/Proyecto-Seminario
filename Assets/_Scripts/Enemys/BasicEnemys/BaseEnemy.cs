using System;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected EnemyStats stats;  //Scriptable Stats

    public float baseHealth;
    public float baseSpeed;
    public float baseDamage;
    public float baseAttackSpeed;
    public float baseExpDrop;

    private float damageCooldown = 0.2f; // medio segundo de invulnerabilidad
    private float lastDamageTime = -Mathf.Infinity;

    [SerializeField] protected float currentHealth;
    protected Transform target;
    protected Vector3 spawnPosition;
    protected HandleAnimations handleAnimations;
    protected Rigidbody rb;
    protected ExperienceManager playerEXP;
    public Action OnDeath;
    private EnemySpawner spawner;

    public EnemyStats Stats { get => stats; set => stats = value; }


    public void AddMaxHealth(float amount)
    {
        stats.maxHealth += amount;
        currentHealth += amount;
        Debug.Log($"Enemy max health increased by {amount}. New max health: {stats.maxHealth}");
    }
    public void AddMaxSpeed(float amount)
    {
        stats.moveSpeed += amount;
    }
    public void AddMaxAttackDamage(float amount)
    {
        stats.attackDamage += amount;
    }
    public void AddAttackSpeed(float amount)
    {
        stats.attackSpeed += amount;
    }
    public void AddMaxExpDrop(float amount)
    {
        stats.expDrop += amount;
    }

    protected virtual void OnEnable()
    {
        handleAnimations = GetComponent<HandleAnimations>();
        rb = GetComponent<Rigidbody>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerEXP = GameObject.Find("ExperienceManager").GetComponent<ExperienceManager>();

        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.RegisterEnemy(this);

        if (player != null)
            target = player.transform;
    }

    public virtual void Initialize(EnemySpawner spawner = null)
    {
        this.spawner = spawner;
        playerEXP = GameObject.Find("ExperienceManager").GetComponent<ExperienceManager>();


        var flock = GetComponent<FlockingBehave>();
        if (flock != null && spawner != null)
        {
            flock.Initialize(spawner, spawner.cohesionWeight, spawner.separationWeight, spawner.alignmentWeight, spawner.neighborRadius);
        }


    }

    public void ResetStatsToBase()
    {
        stats.maxHealth = baseHealth;
        stats.moveSpeed = baseSpeed;
        stats.attackDamage = baseDamage;
        stats.attackSpeed = baseAttackSpeed;
        stats.expDrop = baseExpDrop;
    }

    protected virtual void Update()
    {
    }
    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public void TakeDamage(float damage)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;
        OnDamage(damage);
    }

    protected virtual void OnDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }
    protected virtual void Attack()
    {
    }

    protected virtual void Die(float experienceDroped = 0)
    {
        if (spawner != null)
            spawner.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);

        OnDeath?.Invoke();
        //playerEXP.AddExperience(experienceDroped);
        GetComponent<EnemyDropManager>()?.DropItems();
    }

    public virtual void Spawn(Transform spawnPos)
    {
        currentHealth = stats.maxHealth;
        transform.position = spawnPos.position;
        gameObject.SetActive(true); // Para pooling
    }

    protected void FaceTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        transform.forward = direction;
    }

    protected virtual void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.Translate(direction * stats.moveSpeed * Time.deltaTime, Space.World);
    }

    protected void GetKnockback(float knockbackAmount)
    {
        rb.AddForce((transform.position - target.position).normalized * knockbackAmount, ForceMode.Impulse);
        rb.AddForce(Vector3.up * knockbackAmount, ForceMode.Impulse);
    }
}
