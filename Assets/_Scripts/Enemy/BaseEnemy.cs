using System.Collections;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected EnemyStats stats;  //Scriptable Stats

    protected float currentHealth;
    protected Transform target;
    protected Vector3 spawnPosition;
    protected HandleAnimations handleAnimations;

    protected virtual void OnEnable()
    {
        currentHealth = stats.maxHealth;
        spawnPosition = transform.position;
        handleAnimations = GetComponent<HandleAnimations>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    protected virtual void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        OnDamage(damage);
    }

    protected virtual void OnDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        transform.position = spawnPosition;
        currentHealth = stats.maxHealth;
        //gameObject.SetActive(false); // Para pooling
    }

    protected virtual void Spawn()
    {
        gameObject.SetActive(true); // Para pooling
        transform.position = spawnPosition;
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

        transform.position = Vector3.MoveTowards(transform.position, target.position, stats.moveSpeed * Time.deltaTime);
    }
}
