using System;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour, IDamageable
{

    [SerializeField] public EnemyStats baseStats;
    [SerializeField] public EnemyStats currentStats;

    [Header("Runtime Stats (copiados del template)")]
    public float maxHealth;
    public float moveSpeed;
    public float attackDamage;
    public float attackSpeed;
    public float expDrop;

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

    protected virtual void OnEnable()
    {
        handleAnimations = GetComponent<HandleAnimations>();
        rb = GetComponent<Rigidbody>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerEXP = GameObject.Find("ExperienceManager").GetComponent<ExperienceManager>();

        if (player != null)
            target = player.transform;
    }

    public virtual void Initialize(EnemySpawner spawner = null)
    {
        currentStats = ScriptableObject.Instantiate(baseStats); 

        this.spawner = spawner;
        playerEXP = GameObject.Find("ExperienceManager").GetComponent<ExperienceManager>();


        //maxHealth = currentStats.maxHealth;
        //moveSpeed = currentStats.moveSpeed;
        //attackDamage = currentStats.attackDamage;
        //attackSpeed = currentStats.attackSpeed;
        //expDrop = currentStats.expDrop;

        currentHealth = maxHealth;


        var flock = GetComponent<FlockingBehave>();
        if (flock != null && spawner != null)
        {
            flock.Initialize(spawner, spawner.cohesionWeight, spawner.separationWeight, spawner.alignmentWeight, spawner.neighborRadius);
        }


    }

    public void ApplyDifficulty(float difficulty)
    {
        Debug.Log("Se Aplico dificultad");

        attackDamage += 2f * difficulty;
        maxHealth += 5f * difficulty;
        moveSpeed += 0.05f * difficulty;

        currentHealth = maxHealth;
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
        currentHealth = baseStats.maxHealth;
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
        transform.Translate(direction * baseStats.moveSpeed * Time.deltaTime, Space.World);
    }

    protected void GetKnockback(float knockbackAmount)
    {
        rb.AddForce((transform.position - target.position).normalized * knockbackAmount, ForceMode.Impulse);
        rb.AddForce(Vector3.up * knockbackAmount, ForceMode.Impulse);
    }
}
