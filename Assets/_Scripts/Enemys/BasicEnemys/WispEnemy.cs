using UnityEngine;
using UnityEngine.Rendering;

public class WispEnemy : BaseEnemy
{
    private enum WispState
    {
        Floating,
        Chasing,
        Attacking,
    }

    [Header("Wisp Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float hoverSmoothness = 2f;

    private WispState currentState;
    private float attackCooldown;
    private PoolManager<Projectile> projectilePool;
    private Vector3 initialPosition;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = WispState.Floating;
        attackCooldown = 0f;
        initialPosition = transform.position;
    }

    private void Awake()
    {
        projectilePool = new PoolManager<Projectile>(projectilePrefab.GetComponent<Projectile>(), initialSize: 5, parent: transform);
    }

    protected override void Update()
    {
        base.Update();

        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case WispState.Floating:
                Hover();
                handleAnimations.ChangeAnimationState("Idle_Wisp");

                if (distance < stats.detectionRange)
                    currentState = WispState.Chasing;
                break;

            case WispState.Chasing:
                FaceTarget();
                MoveTowardsTarget(distance);
                handleAnimations.ChangeAnimationState("Chasing_Wisp");

                if (distance > stats.detectionRange)
                    currentState = WispState.Floating;
                else if (distance <= stats.attackRange)
                    currentState = WispState.Attacking;
                break;

            case WispState.Attacking:
                FaceTarget();
                handleAnimations.ChangeAnimationState("Attack_Wisp");

                if (distance > stats.attackRange)
                    currentState = WispState.Chasing;
                else
                    Attack();
                break;
        }
    }

    private void Hover()
    {
        Vector3 targetPos = new Vector3(transform.position.x, initialPosition.y + Mathf.Sin(Time.time) * 0.5f, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * hoverSmoothness);
    }

    private new void FaceTarget()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void MoveTowardsTarget(float distance)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 move = new Vector3(direction.x, 0f, direction.z) * stats.moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);
    }

    protected override void Attack()
    {
        if (attackCooldown > 0f) return;

        ShootProjectile();
        attackCooldown = 1f / stats.attackSpeed;
    }

    private void ShootProjectile()
    {
        Projectile proj = projectilePool.Get();
        proj.transform.SetParent(null);

        Vector3 directionToTarget = (target.position - firePoint.position).normalized;

        proj.Initialize(directionToTarget * projectileSpeed, stats.attackDamage, projectilePool);
        proj.transform.position = firePoint.position;
        proj.transform.rotation = Quaternion.LookRotation(directionToTarget);
    }

    protected override void OnDamage(float damage)
    {
        base.OnDamage(damage);
        GetKnockback(stats.knockbackAmmount);
        rb.isKinematic = true;
        handleAnimations.ChangeAnimationState("Damage_Wisp");
        Invoke(nameof(RecoverFromDamage), 0.2f);
    }

    private void RecoverFromDamage()
    {
        rb.isKinematic = false;
        currentState = WispState.Chasing;
    }

    protected override void Die(float xpDrop)
    {
        base.Die(stats.expDrop);
        handleAnimations.ChangeAnimationState("Die_Wisp");
    }
}
