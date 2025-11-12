using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DragonBoss : BaseEnemy
{
    [Header("-----------Basic Settings----------------")]
    [Header("Entry scene range")]
    [SerializeField] private float entryScene_Range;

    [Header("Defense Settings")]
    [SerializeField] private float baseArmor;           // Armadura inicial
    [SerializeField] private float armorPhase2Bonus;   // Bonus de fase 2
    [SerializeField] private float armorPhase3Bonus;   // Bonus de fase 3
    [Header("")]
    [Header("-----------Phase 1----------------")]
    [Header("------Range Attack Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float rangeAttack_Range;
    [SerializeField] private float rangeAttack_Cooldown;
    [SerializeField] private float rangeAttack_Damage;

    [Header("")]
    [Header("------Melee Attack Settings")]
    [SerializeField] private float meleeAreaAttack_Range;
    [SerializeField] private BossHitbox meleeAreaAttack_HitBox;
    [SerializeField] private float meleeAreaAttack_Damage;
    [SerializeField] private float meleeAreaAttack_Cooldown;
    [SerializeField] private float meleeAreaAttack_Duration;
    [SerializeField] private float meleeAreaAttack_Delay;
    [SerializeField] private float meleeAreaAttack_HorizontalKnockback;
    [SerializeField] private float meleeAreaAttack_VerticalKnockback;

    [Header("----------------------------------")]
    [Header("")]
    [Header("-----------Phase 3----------------")]
    [Header("------Slam Attack")]
    [SerializeField] private float slamAttack_Radius;
    [SerializeField] private float slamAttack_Damage;

    [Header("")]
    [Header("------Melee Attack Settings")]
    [SerializeField] private float meleeAttack_Range;
    [SerializeField] private BossHitbox meleeAttack_HitBox;
    [SerializeField] private float meleeAttack_Damage;
    [SerializeField] private float meleeAttack_Cooldown;
    [SerializeField] private float meleeAttack_Duration;
    [SerializeField] private float meleeAttack_Delay;
    [SerializeField] private float meleeAttack_HorizontalKnockback;
    [SerializeField] private float meleeAttack_VerticalKnockback;

    private BossState currentState;

    private int currentPhase = 1;
    private float rangeAttack_CurrentCooldown;
    private float meleeAreaAttack_CurrentCooldown;
    private float meleeAttack_CurrentCooldown;
    private SkinnedMeshRenderer bossRenderer;
    bool fase3startedEntry = false;
    private bool fase3Active = false;
    private float currentArmor;


    private enum BossState
    {
        Entry,
        Idle,
        Attacking,
        Damaged
    }
    protected override void OnDamage(float damage)
    {
        float effectiveDamage = Mathf.Max(0, damage - currentArmor);
        currentHealth -= effectiveDamage;

        if (currentHealth < 0)
        {
            Die(stats.expDrop);
        }
        else
        {
            UIManager.Instance.SetBossHealth(currentHealth);
        }
    }

    protected override void Die(float experience)
    {

        UIManager.Instance.DisableBossName();
    }

    public void Start()
    {
        handleAnimations = GetComponent<HandleAnimations>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        bossRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        bossRenderer.gameObject.SetActive(false);

        currentState = BossState.Entry;
        rangeAttack_CurrentCooldown = rangeAttack_Cooldown;

        currentHealth = stats.maxHealth;
        projectilePoolManager = new PoolManager<Projectile>(projectilePrefab.GetComponent<Projectile>(), poolSize, transform);
    }

    protected override void Update()
    {
        if (rangeAttack_CurrentCooldown > 0f)
            rangeAttack_CurrentCooldown -= Time.deltaTime;

        if (meleeAreaAttack_CurrentCooldown > 0f)
            meleeAreaAttack_CurrentCooldown -= Time.deltaTime;

        if (meleeAttack_CurrentCooldown > 0f)
            meleeAttack_CurrentCooldown -= Time.deltaTime;

        if (target == null) return;

        HandlePhases();

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case BossState.Entry:
                if (distance < entryScene_Range)
                {
                    handleAnimations.ChangeAnimationState("Entry_Boss");
                    bossRenderer.gameObject.SetActive(true);
                    UIManager.Instance.SetBossHealth(currentHealth);
                    UIManager.Instance.SetBossName("Dragon Boss");

                    StartCoroutine(WaitForEntryAnimation());
                }
                break;


            case BossState.Idle:
                handleAnimations.ChangeAnimationState("Idle_Boss");
                if (distance < rangeAttack_Range)
                    currentState = BossState.Attacking;
                break;

            case BossState.Attacking:
                float dist = Vector3.Distance(transform.position, target.position);

                if (distance > rangeAttack_Range)
                {
                    currentState = BossState.Idle;
                }
                else
                {
                    HandleAttackByPhase(distance);
                }
                break;

            case BossState.Damaged:

                break;
        }
    }

    private IEnumerator WaitForEntryAnimation()
    {
        yield return null; // Espera un frame para asegurar que la animación empezó

        float animLength = handleAnimations.GetCurrentAnimationLength();
        yield return new WaitForSecondsRealtime(animLength);
        currentState = BossState.Idle;
    }


    PoolManager<Projectile> projectilePoolManager;
    private int poolSize = 7;
    public void PerformRangeAttack() // Usado en animation event instancia un proyectil
    {
        Vector3 directionToTarget = (target.position - firePoint.position).normalized;

        GameObject projectile = projectilePoolManager.Get().gameObject;
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.Initialize(directionToTarget * projectileSpeed, rangeAttack_Damage, projectilePoolManager, firePoint.transform);
    }



    private void HandlePhases()
    {
        float healthPercent = currentHealth / stats.maxHealth;

        if (healthPercent <= 0.5f && currentPhase < 3)
            currentPhase = 3;
        else if (healthPercent <= 0.8f && currentPhase < 2)
            currentPhase = 2;
    }

    private void HandleAttackByPhase(float distance)
    {
        switch (currentPhase)
        {
            case 1:
                Phase1Attack(distance);
                break;

            case 2:
                Phase2Attack(distance);
                break;

            case 3:
                Phase3Attack(distance);
                break;
        }
    }

    // ------------------ FASE 1 ------------------
    private void Phase1Attack(float distance)
    {
        if (distance < meleeAttack_Range && meleeAreaAttack_CurrentCooldown <= 0f)
        {
            handleAnimations.ChangeAnimationState("MeleeAttack_Boss", true);
            DoMeleeAttack(meleeAreaAttack_HitBox, meleeAreaAttack_Damage, meleeAreaAttack_Delay, meleeAreaAttack_Duration, meleeAreaAttack_HorizontalKnockback, meleeAreaAttack_VerticalKnockback);
            meleeAreaAttack_CurrentCooldown = meleeAttack_Cooldown;
            rangeAttack_CurrentCooldown = rangeAttack_Cooldown;
        }
        else if (rangeAttack_CurrentCooldown <= 0f)
        {
            handleAnimations.ChangeAnimationState("RangeAttack_Boss", true);
            rangeAttack_CurrentCooldown = rangeAttack_Cooldown;
        }
    }

    // ------------------ FASE 2 ------------------
    private void Phase2Attack(float distance)
    {
        if (distance < meleeAreaAttack_Range && meleeAreaAttack_CurrentCooldown <= 0f)
        {
            handleAnimations.ChangeAnimationState("MeleeAttack_Boss");
            DoMeleeAttack(meleeAreaAttack_HitBox, meleeAreaAttack_Damage, meleeAreaAttack_Delay, meleeAreaAttack_Duration, meleeAreaAttack_HorizontalKnockback, meleeAreaAttack_VerticalKnockback);
            meleeAreaAttack_CurrentCooldown = meleeAreaAttack_Cooldown;
            rangeAttack_CurrentCooldown = rangeAttack_Cooldown;
        }
        else if (rangeAttack_CurrentCooldown <= 0f)
        {
            handleAnimations.ChangeAnimationState("RangeAttack_Boss", true);
            StartCoroutine(MultipleShot());
            rangeAttack_CurrentCooldown = rangeAttack_Cooldown;
        }
    }

    private IEnumerator MultipleShot()
    {
        PerformRangeAttack();
        yield return new WaitForSeconds(0.3f);
        PerformRangeAttack();
        yield return new WaitForSeconds(0.3f);
        PerformRangeAttack();
        yield return new WaitForSeconds(0.3f);
        PerformRangeAttack();
        yield return new WaitForSeconds(0.3f);
        PerformRangeAttack();
    }

    // ------------------ FASE 3 ------------------



    private void Phase3Attack(float distance)
    {
        if (!fase3startedEntry)
        {
            fase3startedEntry = true;
            StartCoroutine(Phase3EntryRoutine());
            return; // evitar seguir ejecutando ataques
        }

        if (fase3Active)
        {
            if (distance < meleeAttack_Range && meleeAttack_CurrentCooldown <= 0f)
            {
                FaceTarget();
                handleAnimations.ChangeAnimationState("MeleeAttack2");
                DoMeleeAttack(meleeAttack_HitBox, meleeAttack_Damage, meleeAttack_Delay, meleeAttack_Duration, meleeAttack_HorizontalKnockback, meleeAttack_VerticalKnockback);
                meleeAttack_CurrentCooldown = meleeAttack_Cooldown;
                rangeAttack_CurrentCooldown = rangeAttack_Cooldown;
            }
            else if (rangeAttack_CurrentCooldown <= 0f)
            {

                FaceTarget();
                handleAnimations.ChangeAnimationState("RangeAttack_Boss", true);
                rangeAttack_CurrentCooldown = rangeAttack_Cooldown;

            }
        }
    }
    [SerializeField] Transform thirdFaseSpawnPoint;
    private IEnumerator Phase3EntryRoutine()
    {
        handleAnimations.ChangeAnimationState("EntryFase3_Boss", true);

        float animLength = handleAnimations.GetCurrentAnimationLength();
        yield return new WaitForSecondsRealtime(animLength);

        transform.position = thirdFaseSpawnPoint.position;
        FaceTarget();
        handleAnimations.ChangeAnimationState("Entry_Boss", true);
        rangeAttack_Cooldown = 2f;
        float animmLength = handleAnimations.GetCurrentAnimationLength();
        yield return new WaitForSecondsRealtime(animmLength);

        fase3Active = true; // ✅ Ahora sí puede atacar
        currentState = BossState.Idle;
    }


    public void PerformSlamAttack()
    {
        // Detectar al jugador dentro del radio
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, slamAttack_Radius);
        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.TakeDamage(slamAttack_Damage);
            }
        }
        // Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangeAttack_Range);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, meleeAreaAttack_Range);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, meleeAttack_Range);


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, entryScene_Range);
    }


    public void DoMeleeAttack(BossHitbox hitbox, float dmg, float delay, float duration, float knockbackHorizontal, float knockbackVertical)
    {
        StartCoroutine(meleeHitboxRoutine(hitbox, dmg, delay, duration, knockbackHorizontal, knockbackVertical));
    }

    private IEnumerator meleeHitboxRoutine(BossHitbox hitbox, float dmg, float delay, float duration, float knockbackHorizontal, float knockbackVertical)
    {

        hitbox.SetDamage(dmg, knockbackHorizontal, knockbackVertical);  // Setea damage y knocback

        if (delay > 0f)
            yield return new WaitForSeconds(delay); // Espera por el delay, en base a la animacion

        hitbox.EnableHitbox();

        if (duration > 0f)
            yield return new WaitForSeconds(duration); // Espera por la duracion del ataque

        hitbox.DisableHitbox();
    }
}
