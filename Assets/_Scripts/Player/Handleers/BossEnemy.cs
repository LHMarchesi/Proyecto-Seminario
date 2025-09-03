using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    private enum BossState
    {
        Idle,
        Chasing,
        Attacking,
        Damaged
    }

    [Header("Boss Attacks by Phase")]
    public BossAttackStats[] phase1Attacks;
    public BossAttackStats[] phase2Attacks;
    public BossAttackStats[] phase3Attacks;

    public BossHitbox meleeHitBox;

    private BossState currentState;
    private int currentPhase = 1;
    private float attackCooldown;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = BossState.Chasing;
        attackCooldown = 0f;
    }

    protected override void Update()
    {
        base.Update();

        if (attackCooldown > 0f)
            attackCooldown -= Time.deltaTime;

        if (target == null) return;

        HandlePhases();

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case BossState.Idle:
           //     handleAnimations.ChangeAnimationState("Idle_Boss");
                if (distance < stats.detectionRange)
                    currentState = BossState.Chasing;
                break;

            case BossState.Chasing:
                if (distance > stats.detectionRange)
                    currentState = BossState.Idle;
                else if (distance <= GetCurrentAttackRange())
                    currentState = BossState.Attacking;
                else
                    ChaseTarget();
                break;

            case BossState.Attacking:
                if (distance > GetCurrentAttackRange())
                {
                    currentState = BossState.Chasing;
                }
                else
                {
                    PerformAttack();
                }
                break;

            case BossState.Damaged:
                // stun o knockback
                break;
        }
    }

    private void HandlePhases()
    {
        float healthPercent = currentHealth / stats.maxHealth;

        if (healthPercent <= 0.2f && currentPhase < 3)
            currentPhase = 3;
        else if (healthPercent <= 0.5f && currentPhase < 2)
            currentPhase = 2;
    }

    private void ChaseTarget()
    {
 //       handleAnimations.ChangeAnimationState("Chasing_Boss");
        MoveTowardsTarget();
        FaceTarget();
    }

    private void PerformAttack()
    {
        if (attackCooldown > 0f) return;

        FaceTarget();

        BossAttackStats[] attacks = GetAttacksForPhase();
        if (attacks == null || attacks.Length == 0) return;

        BossAttackStats chosenAttack = null;

        if (attacks.Length == 1)
        {
            // si solo hay un ataque, lo usamos directo
            chosenAttack = attacks[0];
        }
        else { 
            // Intentamos varios ataques hasta encontrar uno v�lido
            for (int i = 0; i < 5; i++)
            {
                BossAttackStats candidate = attacks[Random.Range(0, attacks.Length)];
                if (candidate.IsInRange(transform, target))
                {
                    chosenAttack = candidate;
                    break;
                }
            }
        }

        if (chosenAttack == null)
        {
            currentState = BossState.Chasing; // no encontr� ataque, sigue persiguiendo
            return;
        }

        // Ejecutar el ataque
        chosenAttack.Execute(this, target, handleAnimations);
        attackCooldown = chosenAttack.cooldown; // usar el cooldown del ataque
        Debug.Log("Cooldown: " + attackCooldown);

    }

    private float GetCurrentAttackRange()
    {
        BossAttackStats[] attacks = GetAttacksForPhase();
        float maxRange = 0f;

        foreach (var attack in attacks)
        {
            if (attack.range > maxRange)
                maxRange = attack.range;
        }

        return maxRange;
    }

    private BossAttackStats[] GetAttacksForPhase()
    {
        if (currentPhase == 1) return phase1Attacks;
        if (currentPhase == 2) return phase2Attacks;
        if (currentPhase == 3) return phase3Attacks;
        return null;
    }

    protected override void OnDamage(float damage)
    {
        GetKnockback(stats.knockbackAmmount);
        Invoke(nameof(EndDamageState), 0.5f);
        base.OnDamage(damage);
      //  handleAnimations.ChangeAnimationState("TakeDamage_Boss");

        currentState = BossState.Damaged;
    }

    private void EndDamageState()
    {
        currentState = BossState.Chasing;
    }

    public void PrepareAttack(float damage, float knockbackHorizontal = 0f, float knockbackVertical = 0f)
    {
        if (meleeHitBox != null)
            meleeHitBox.SetDamage(damage, knockbackHorizontal, knockbackVertical);
    }

    public void DoMeleeAttack(float dmg, float delay, float duration, float knockbackHorizontal, float knockbackVertical)
    {
        StartCoroutine(MeleeRoutine(dmg, delay, duration, knockbackHorizontal, knockbackVertical));
    }

    private IEnumerator MeleeRoutine(float dmg, float delay, float duration, float knockbackHorizontal, float knockbackVertical)
    {
        meleeHitBox.SetDamage(dmg, knockbackHorizontal, knockbackVertical);

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        meleeHitBox.EnableHitbox();

        if (duration > 0f)
            yield return new WaitForSeconds(duration);

        meleeHitBox.DisableHitbox();
    }
}
