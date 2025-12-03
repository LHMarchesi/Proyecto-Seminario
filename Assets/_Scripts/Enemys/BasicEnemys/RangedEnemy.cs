using UnityEngine;
using static UnityEngine.GridBrushBase;

public class RangedEnemy : BaseEnemy
{
    private enum RangedEnemyState
    {
        Idle,
        Chasing,
        Attacking,
    }

    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 15f;

    private RangedEnemyState currentState;
    private float attackCooldown;
    private PoolManager<Projectile> projectilePool;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = RangedEnemyState.Idle;
        attackCooldown = 0f;
    }

    private void Awake()
    {
        projectilePool = new PoolManager<Projectile>(projectilePrefab.GetComponent<Projectile>(), initialSize: 5, parent: transform);
    }

    protected override void Update()
    {
        base.Update();

        // Cooldown timer
        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case RangedEnemyState.Idle:
                rb.angularVelocity = Vector3.zero;
                handleAnimations.ChangeAnimationState("Idle_RangedEnemy");
                if (distance < baseStats.detectionRange)
                    currentState = RangedEnemyState.Chasing;
                break;

            case RangedEnemyState.Chasing:
                if (distance > baseStats.detectionRange)
                {
                    currentState = RangedEnemyState.Idle;
                }
                else if (distance <= baseStats.attackRange)
                {
                    currentState = RangedEnemyState.Attacking;
                }
                else
                {
                    FaceTarget();
                    MoveTowardsTarget();
                }
                break;

            case RangedEnemyState.Attacking:
                if (distance > baseStats.attackRange)
                {
                    currentState = RangedEnemyState.Chasing;
                }
                else
                {
                    Attack();
                }
                break;
        }
    }

    protected override void OnDamage(float damage)
    {
        Invoke(nameof(EndDamageState), 0.1f);
        base.OnDamage(damage);
        //  handleAnimations.ChangeAnimationState("TakeDamage_RangedEnemy");
        GetKnockback(baseStats.knockbackAmmount);
    }

    private void EndDamageState()
    {
        currentState = RangedEnemyState.Chasing;
    }

    protected override void Die(float xpDrop)
    {
        base.Die(baseStats.expDrop);
        handleAnimations.ChangeAnimationState("Die_RangedEnemy");
      //  Invoke(nameof(Spawn), 2f); // Respawn after 2 seconds
    }


    protected override void MoveTowardsTarget()
    {
        base.MoveTowardsTarget();
        handleAnimations.ChangeAnimationState("Chasing_RangedEnemy");
    }

    protected override void Attack()
    {
        if (attackCooldown > 0f) return;
        FaceTarget();
        handleAnimations.ChangeAnimationState("Shoot_RangedEnemy");
        ShootProjectile();
        attackCooldown = 1f / baseStats.attackSpeed; 
    }

    private void ShootProjectile()
    {
        Vector3 directionToTarget = (target.position - firePoint.position).normalized;

        GameObject project = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(directionToTarget));
        Projectile projectileScript = project.GetComponent<Projectile>();
        Rigidbody projectilRB = project.GetComponent<Rigidbody>();
        projectilRB.velocity = directionToTarget * projectileSpeed;
        projectileScript.SetDamage(attackDamage);
    }
}
