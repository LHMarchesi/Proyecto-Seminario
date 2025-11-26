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
    FlockingBehave flockingBehavior;
    private bool useFlocking;
    public GameObject FloatingTextPrefab;

    public bool Goblin;
    public bool Skeleton;


    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = MeleeEnemyState.Idle;
        attackCooldown = 0f;
    }

    private void Start()
    {
        base.OnEnable();
        
        flockingBehavior = GetComponent<FlockingBehave>();
        if (flockingBehavior != null)
        {
            flockingBehavior.SetPlayer(target);
            useFlocking = true;
        }

        EnemySpawner enemySpawner = GetComponentInParent<EnemySpawner>();
        if (enemySpawner != null) { Initialize(enemySpawner); }
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
        PlayHurtEffect();

        if (FloatingTextPrefab != null)
        {
            var go = Instantiate(FloatingTextPrefab, transform.position, Quaternion.identity, transform);
            go.GetComponent<TextMesh>().text = damage.ToString();
        }
    }


    private void EndDamageState()
    {
        currentState = MeleeEnemyState.Chasing;
    }
    private void ChaseTarget()
    {
        handleAnimations.ChangeAnimationState("Chasing_MeleeEnemy");

        if (useFlocking)
        {
            Vector3 direction = flockingBehavior.GetFlockingDirection();
            direction.y = 0f;

            // Normalizar y mover
            direction.Normalize();

            rb.MovePosition(rb.position + direction * stats.moveSpeed * Time.fixedDeltaTime);
            FaceTarget();
        }
        else
        {
            MoveTowardsTarget();
            FaceTarget();
        }
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
    public void PlayHurtEffect()
    {
        if(Goblin == true)
        {
            string[] attackSounds = { "GoblinDeath1", "GoblinDeath2", "GoblinDeath3" };

            int index = UnityEngine.Random.Range(0, attackSounds.Length);

            SoundManagerOcta.Instance.PlaySound(attackSounds[index]);
        }
        else if(Skeleton == true)
        {

            string[] attackSounds = { "SkeletonDeath1", "SkeletonDeath2", "SkeletonDeath3" };

            int index = UnityEngine.Random.Range(0, attackSounds.Length);

            SoundManagerOcta.Instance.PlaySound(attackSounds[index]);
        }
    }

    public void PlayDeathEffect()
    {
        if (Skeleton == true)
        {

            string[] attackSounds = { "SkeletonDeath1", "SkeletonDeath2" };

            int index = UnityEngine.Random.Range(0, attackSounds.Length);

            SoundManagerOcta.Instance.PlaySound(attackSounds[index]);
        }
    }

    protected override void Die(float xpDrop)
    {
        // if (flockManager != null)
        //     flockManager.Unregister(this);

        base.Die(stats.expDrop);
        GameObject GO = Instantiate(DeathEffect, transform.position, Quaternion.identity); // Instantiate effect
        Destroy(GO, 3);
        PlayDeathEffect();

    }
}