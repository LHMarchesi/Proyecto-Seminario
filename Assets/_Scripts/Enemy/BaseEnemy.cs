using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;

public abstract class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected EnemyStats stats;  //Scriptable Stats

    private float damageCooldown = 0.2f; // medio segundo de invulnerabilidad
    private float lastDamageTime = -Mathf.Infinity;

    [SerializeField] protected float currentHealth;
    protected Transform target;
    protected Vector3 spawnPosition;
    protected HandleAnimations handleAnimations;
    protected Rigidbody rb;
    protected ExperienceManager playerEXP;

    private IObjectPool<BaseEnemy> enemyPool;

    public void SetPool(IObjectPool<BaseEnemy> pool)
    {
        enemyPool = pool;
    }

    protected virtual void OnEnable()
    {
        currentHealth = stats.maxHealth;
        spawnPosition = transform.position;

        handleAnimations = GetComponent<HandleAnimations>();
        rb = GetComponent<Rigidbody>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerEXP = GameObject.Find("ExperienceManager").GetComponent<ExperienceManager>();
        if (player != null)
            target = player.transform;
    }

    protected virtual void Update()
    {
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
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
            Die();
    }
    protected virtual void Attack()
    {
    }

    protected virtual void Die(float experienceDroped = 0)
    {
        if (enemyPool != null) { enemyPool.Release(this); } else { Destroy(gameObject); }

        playerEXP.AddExperience(experienceDroped);
    }

    protected virtual void Spawn()
    {
        currentHealth = stats.maxHealth;
        transform.position = spawnPosition;
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
