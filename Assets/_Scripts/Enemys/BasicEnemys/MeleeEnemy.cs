using System;
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
    [SerializeField] private GameObject DeathEffect;


    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = MeleeEnemyState.Idle;
        attackCooldown = 0f;
    }

    private void Start()
    {
        base.OnEnable();
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
        GetKnockback(stats.knockbackAmmount * 2);
        Invoke(nameof(EndDamageState), handleAnimations.GetCurrentAnimationLength());
    }

    private void EndDamageState()
    {
        currentState = MeleeEnemyState.Chasing;
    }

    private void ChaseTarget()
    {
        handleAnimations.ChangeAnimationState("Chasing_MeleeEnemy");

        /* if (flockManager != null)
         {
             flockManager.Register(this);
            Vector3 direction = GetFlockingDirection();
             direction.y = 0f;

             // Normalizar y mover
             direction.Normalize();

             rb.MovePosition(rb.position + direction * stats.moveSpeed * Time.fixedDeltaTime);
             FaceDirection(direction);
         }
         else
         {*/
        MoveTowardsTarget();
        FaceTarget();
        //    }
        // Mirar hacia la dirección de movimiento
    }

    private void FixedUpdate()
    {
        if (currentState == MeleeEnemyState.Chasing)
            ChaseTarget();
    }

    private void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    protected override void Attack()
    {
        FaceTarget();
        handleAnimations.ChangeAnimationState("Attack_MeleeEnemy");
        if (attackCooldown > 0f) return;
        attackCooldown = 1f / stats.attackSpeed;
        //audioSource.PlayOneShot(attackSound);
    }

    public void TryDealDamageToPlayer()
    {
        float distance;
        distance = Vector2.Distance(transform.position, target.position);
        if (distance < stats.attackRange)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            player.TakeDamage(stats.attackDamage);
        }

    }

    protected override void Die(float xpDrop)
    {
        // if (flockManager != null)
        //     flockManager.Unregister(this);

        base.Die(stats.expDrop);
        GameObject GO = Instantiate(DeathEffect, transform.position, Quaternion.identity); // Instantiate effect
        Destroy(GO, 3);
        // SoundManagerOcta.Instance.PlaySound("EnemyDeath");

    }
}