using System.Collections;
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
        base.OnDamage(damage);

        currentState = MeleeEnemyState.Damaged;
        handleAnimations.ChangeAnimationState("TakeDamage_MeleeEnemy");
        GetKnockback(stats.knockbackAmmount);
        Invoke(nameof(EndDamageState), 0.5f);
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
        FaceTarget();
        handleAnimations.ChangeAnimationState("Attack_MeleeEnemy");
        if (attackCooldown > 0f) return;
        attackCooldown = 1f / stats.attackSpeed;
    }

    public void TryDealDamageToPlayer()
    {
        float distance;
        distance = Vector2.Distance(transform.position, target.position);
        if (distance < stats.attackRange)
        {
            GameObject player = target.gameObject;
            Debug.Log(player);
            PlayerController controller = player.GetComponent<PlayerController>();
            Debug.Log(controller);
            controller.TakeDamage(stats.attackDamage);
        }

    }

    protected override void Die(float xpDrop)
    {
        base.Die(stats.expDrop);
        //   handleAnimations.ChangeAnimationState("Die_RangedEnemy");
        //  Invoke(nameof(Spawn), 2f); // Respawn after 2 seconds
    }
}