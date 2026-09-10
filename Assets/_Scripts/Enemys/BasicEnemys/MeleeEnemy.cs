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

    [Header("Horde behaviour")]
    [SerializeField] private bool alwaysChasePlayer = true;

    private FlockingBehave flockingBehavior;
    private bool useFlocking;

    public bool Goblin;
    public bool Skeleton;

    protected override void OnEnable()
    {
        base.OnEnable();

        CancelInvoke(nameof(EndDamageState));
        attackCooldown = 0f;

        currentState =
            alwaysChasePlayer && target != null
                ? MeleeEnemyState.Chasing
                : MeleeEnemyState.Idle;
    }

    private void Start()
    {
        // Unity ya llamó OnEnable. No hay que llamar base.OnEnable() otra vez.
        flockingBehavior = GetComponent<FlockingBehave>();

        if (flockingBehavior != null)
        {
            if (target != null)
                flockingBehavior.SetPlayer(target);

            useFlocking = true;
        }

        // Compatibilidad con el spawner viejo.
        EnemySpawner enemySpawner = GetComponentInParent<EnemySpawner>();
        if (enemySpawner != null && currentStats == null)
            Initialize(enemySpawner);

        if (alwaysChasePlayer && target != null)
            currentState = MeleeEnemyState.Chasing;
    }

    protected override void Update()
    {
        base.Update();

        if (target == null || CurrentStats == null)
            return;

        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        float distance =
            Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case MeleeEnemyState.Idle:
                handleAnimations.ChangeAnimationState("Idle_MeleeEnemy");

                if (alwaysChasePlayer ||
                    distance < CurrentStats.detectionRange)
                {
                    currentState = MeleeEnemyState.Chasing;
                }
                break;

            case MeleeEnemyState.Chasing:
                if (!alwaysChasePlayer &&
                    distance > CurrentStats.detectionRange)
                {
                    currentState = MeleeEnemyState.Idle;
                }
                else if (distance <= CurrentStats.attackRange)
                {
                    currentState = MeleeEnemyState.Attacking;
                }
                break;

            case MeleeEnemyState.Attacking:
                if (distance > CurrentStats.attackRange)
                    currentState = MeleeEnemyState.Chasing;
                else
                    Attack();
                break;

            case MeleeEnemyState.Damaged:
                break;
        }
    }

    protected override void OnDamage(float damage, DamageFeedbackType feedbackType)
    {
        base.OnDamage(damage, feedbackType);

        currentState = MeleeEnemyState.Damaged;
        handleAnimations.ChangeAnimationState("TakeDamage_MeleeEnemy");

        if (CurrentStats != null)
            GetKnockback(CurrentStats.knockbackAmmount * 2f);

        CancelInvoke(nameof(EndDamageState));
        Invoke(nameof(EndDamageState), handleAnimations.GetCurrentAnimationLength());

        PlayHurtEffect();
        ShowDamageNumber(damage, feedbackType);
    }

    private void EndDamageState()
    {
        if (IsDead())
            return;

        currentState =
            target != null
                ? MeleeEnemyState.Chasing
                : MeleeEnemyState.Idle;
    }

    private void ChaseTarget()
    {
        if (target == null || CurrentStats == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (useFlocking && flockingBehavior != null)
        {
            Vector3 flockDirection = flockingBehavior.GetFlockingDirection();
            flockDirection.y = 0f;

            // Fallback: si flocking devuelve cero, perseguimos directo al jugador.
            if (flockDirection.sqrMagnitude > 0.001f)
                direction = flockDirection;
        }

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        if (rb != null)
        {
            rb.MovePosition(
                rb.position +
                direction * CurrentStats.moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.position +=
                direction * CurrentStats.moveSpeed * Time.fixedDeltaTime;
        }

        FaceDirection(direction);
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
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                Time.deltaTime * 5f);
        }
    }

    protected override void Attack()
    {
        if (target == null || CurrentStats == null)
            return;

        FaceTarget();
        handleAnimations.ChangeAnimationState("Attack_MeleeEnemy");

        if (attackCooldown > 0f)
            return;

        attackCooldown = 1f / Mathf.Max(0.01f, CurrentStats.attackSpeed);
    }

    public void TryDealDamageToPlayer()
    {
        if (target == null || CurrentStats == null)
            return;

        float distance =
            Vector2.Distance(transform.position, target.position);

        if (distance < CurrentStats.attackRange)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage(CurrentStats.attackDamage);
        }
    }

    public void PlayHurtEffect()
    {
        if (Goblin)
        {
            string[] sounds = { "GoblinDeath1", "GoblinDeath2", "GoblinDeath3" };
            int index = UnityEngine.Random.Range(0, sounds.Length);
            SoundManagerOcta.Instance.PlaySound(sounds[index]);
        }
        else if (Skeleton)
        {
            string[] sounds = { "SkeletonDeath1", "SkeletonDeath2", "SkeletonDeath3" };
            int index = UnityEngine.Random.Range(0, sounds.Length);
            SoundManagerOcta.Instance.PlaySound(sounds[index]);
        }
    }

    public void PlayDeathEffect()
    {
        if (Skeleton)
        {
            string[] sounds = { "SkeletonDeath1", "SkeletonDeath2" };
            int index = UnityEngine.Random.Range(0, sounds.Length);
            SoundManagerOcta.Instance.PlaySound(sounds[index]);
        }
    }

    protected override void Die(float xpDrop)
    {
        if (DeathEffect != null)
        {
            GameObject effect =
                Instantiate(DeathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        PlayDeathEffect();

        float finalXP =
            CurrentStats != null
                ? CurrentStats.expDrop
                : xpDrop;

        base.Die(finalXP);
    }
}
