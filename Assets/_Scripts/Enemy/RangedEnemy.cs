using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    private enum RangedEnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Damaged
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
                handleAnimations.ChangeAnimationState("Idle_RangedEnemy");
                if (distance < stats.detectionRange)
                    currentState = RangedEnemyState.Chasing;
                break;

            case RangedEnemyState.Chasing:
                if (distance > stats.detectionRange)
                {
                    currentState = RangedEnemyState.Idle;
                }
                else if (distance <= stats.attackRange)
                {
                    currentState = RangedEnemyState.Attacking;
                }
                else
                {
                    ChaseTarget();
                }
                break;

            case RangedEnemyState.Attacking:
                if (distance > stats.attackRange)
                {
                    currentState = RangedEnemyState.Chasing;
                }
                else
                {
                    FaceTarget();
                    Attack();
                }
                break;

            case RangedEnemyState.Damaged:
                // Remain in damaged until animation/event exits
                break;
        }
    }

    protected override void OnDamage(float damage)
    {
        handleAnimations.ChangeAnimationState("TakeDamage_RangedEnemy");
        base.OnDamage(damage);
        currentState = RangedEnemyState.Damaged;
        Invoke(nameof(EndDamageState), 0.3f);
    }

    private void EndDamageState()
    {
        currentState = RangedEnemyState.Chasing;
    }

    private void ChaseTarget()
    {
        handleAnimations.ChangeAnimationState("Chasing_RangedEnemy");
        MoveTowardsTarget();
        FaceTarget();
    }

    private void Attack()
    {
        // Only shoot when cooldown elapsed
        if (attackCooldown > 0f) return;

        // Trigger shoot animation
        handleAnimations.ChangeAnimationState("Shoot_RangedEnemy");

        ShootProjectile();
        // Reset cooldown based on attackSpeed (shots per second)
        attackCooldown = 1f / stats.attackSpeed;
    }

    private void ShootProjectile()
    {
        // Instantiate and launch
        Projectile proj = projectilePool.Get();
        proj.Initialize(firePoint.forward * projectileSpeed, stats.attackDamage, projectilePool);
        proj.transform.position = firePoint.position;
        proj.transform.rotation = firePoint.rotation;
    }
}

