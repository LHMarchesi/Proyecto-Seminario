using UnityEngine;

public class MeleeEnemy : BaseEnemy
{
    private enum MeleeEnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Damaged
    }

    private MeleeEnemyState currentState;
    private float attackCooldown;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = MeleeEnemyState.Idle;
        attackCooldown = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case MeleeEnemyState.Idle:
                handleAnimations.ChangeAnimationState("Idle_MeleeEnemy");
                if (distance < stats.detectionRange)
                    currentState = MeleeEnemyState.Chasing;
                break;

            case MeleeEnemyState.Chasing:
                if (distance > stats.detectionRange)
                {
                    currentState = MeleeEnemyState.Idle;
                }
                else if (distance <= stats.attackRange)
                {
                    currentState = MeleeEnemyState.Attacking;
                }
                else
                    ChaseTarget();
                break;

            case MeleeEnemyState.Attacking:
                if (distance > stats.attackRange)
                    currentState = MeleeEnemyState.Chasing;
                else
                    Attack();
                break;
            case MeleeEnemyState.Damaged:
                break;
        }
    }

    protected override void OnDamage(float damage)
    {
        Invoke(nameof(EndDamageState), 0.1f);
        base.OnDamage(damage);
        handleAnimations.ChangeAnimationState("TakeDamage_MeleeEnemy");
        GetKnockback(stats.knockbackAmmount);
    }

    private void EndDamageState()
    {
        currentState = MeleeEnemyState.Chasing;
    }

    private void ChaseTarget()
    {
        handleAnimations.ChangeAnimationState("Chasing_MeleeEnemy");
        MoveTowardsTarget();
        FaceTarget();
    }

    protected override void Attack()
    {
        if (attackCooldown > 0f) return;

        handleAnimations.ChangeAnimationState("Attack_MeleeEnemy");
        //audioSource.PlayOneShot(swordSwing);
        attackCooldown = 1f / stats.attackSpeed;

        Vector3 origin = transform.position + Vector3.up;
        Vector3 direction = (target.position - transform.position).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, stats.attackRange))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(stats.attackDamage);
        }
    }

    protected override void Die(int xpDrop)
    {
        base.Die(stats.expDrop);
     //   handleAnimations.ChangeAnimationState("Die_RangedEnemy");
      //  Invoke(nameof(Spawn), 2f); // Respawn after 2 seconds
    }
}