using System.Collections.Generic;
using UnityEngine;

public class ThunderstruckPowerUp : BasePowerUp
{
    [Header("Base")]
    [SerializeField] private LightingStrikeStats stats;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float searchRadius = 8f;
    [SerializeField] private float vfxLifetime = 2f;

    [Header("Level 2 - Charged bonus damage")]
    [SerializeField, Range(0f, 1f)]
    private float chargedBonusDamagePercent = 0.20f;

    [Header("Level 4 - Extra knockback")]
    [SerializeField, Range(0f, 1f)]
    private float extraKnockbackPercent = 0.30f;

    [Header("Level 5 - Discharge")]
    [SerializeField] private float dischargeRadius = 3f;

    [SerializeField, Range(0f, 2f)]
    private float dischargeDamageMultiplier = 0.5f;

    [SerializeField] private GameObject dischargeVFXPrefab;

    private int level = 1;
    private int secondaryTargetCount = 1;

    private void Awake()
    {
        stats = CreateRuntimeStatsCopy(stats);
    }

    protected override void ApplyEffect()
    {
        if (playerContext == null ||
            playerContext.HandleAttack == null)
            return;

        playerContext.HandleAttack.OnMeleeHit +=
            HandleMeleeHit;
    }

    private void HandleMeleeHit(
        MeleeHitInfo hitInfo)
    {
        if (hitInfo.AttackType !=
            MeleeAttackType.Charged)
            return;

        BaseEnemy primaryEnemy =
            hitInfo.Enemy;

        if (primaryEnemy == null)
            return;

        // ==========================================
        // LV2 - EXTRA CHARGED DAMAGE
        // ==========================================

        if (level >= 2 &&
            !primaryEnemy.IsDead())
        {
            primaryEnemy.TakeEffectDamage(
                hitInfo.Damage *
                chargedBonusDamagePercent
            );
        }

        // ==========================================
        // LV4 - EXTRA KNOCKBACK
        // ==========================================

        if (level >= 4 &&
            !primaryEnemy.IsDead() &&
            hitInfo.KnockbackForce > 0f)
        {
            primaryEnemy.ApplyExternalKnockback(
                hitInfo.HitDirection,
                hitInfo.KnockbackForce *
                extraKnockbackPercent
            );
        }

        // ==========================================
        // LIGHTNING TARGETS
        // ==========================================

        List<BaseEnemy> lightningTargets =
            CombatTargeting.FindClosestEnemies(
                primaryEnemy.transform.position,
                searchRadius,
                enemyLayer,
                secondaryTargetCount,
                primaryEnemy
            );

        // Si no encontramos otro enemigo,
        // el rayo cae sobre el enemigo golpeado.
        if (lightningTargets.Count == 0)
        {
            StrikeEnemy(
                primaryEnemy,
                stats != null
                    ? stats.additionalDamage
                    : 0f
            );
        }
        else
        {
            foreach (BaseEnemy target
                     in lightningTargets)
            {
                StrikeEnemy(
                    target,
                    stats != null
                        ? stats.additionalDamage
                        : 0f
                );
            }
        }

        // ==========================================
        // LV5 - DISCHARGE
        // ==========================================

        if (level >= 5)
        {
            CreateDischarge(
                primaryEnemy
            );
        }
    }

    private void StrikeEnemy(
        BaseEnemy enemy,
        float damage)
    {
        if (enemy == null)
            return;

        // Guardamos la posición ANTES de hacer
        // cualquier daño adicional.
        Vector3 vfxPosition =
            enemy.CombatVFXPosition;

        // ==========================================
        // DAMAGE
        // ==========================================

        // Si el Charged ya lo mató,
        // no intentamos dañarlo otra vez.
        if (!enemy.IsDead())
        {
            enemy.TakeEffectDamage(
                damage
            );
        }

        // ==========================================
        // LIGHTNING VFX
        // ==========================================

        if (stats != null &&
            stats.lightningEffectPrefab != null)
        {
            GameObject vfx =
                Instantiate(
                    stats.lightningEffectPrefab,
                    vfxPosition,
                    Quaternion.identity
                );

            if (vfxLifetime > 0f)
            {
                Destroy(
                    vfx,
                    vfxLifetime
                );
            }
        }

        // ==========================================
        // SOUND
        // ==========================================

        if (SoundManagerOcta.Instance != null)
        {
            SoundManagerOcta.Instance.PlaySound(
                "LightningStrike"
            );
        }
    }

    private void CreateDischarge(
        BaseEnemy primaryEnemy)
    {
        if (primaryEnemy == null)
            return;

        // Para GAMEPLAY dejamos transform.position.
        // Es más fiable como centro de OverlapSphere.
        Vector3 gameplayCenter =
            primaryEnemy.transform.position;

        // Para VISUALES usamos el anchor.
        Vector3 visualCenter =
            primaryEnemy.CombatVFXPosition;

        // ==========================================
        // DISCHARGE VFX
        // ==========================================

        if (dischargeVFXPrefab != null)
        {
            GameObject vfx =
                Instantiate(
                    dischargeVFXPrefab,
                    visualCenter,
                    Quaternion.identity
                );

            if (vfxLifetime > 0f)
            {
                Destroy(
                    vfx,
                    vfxLifetime
                );
            }
        }

        // ==========================================
        // DISCHARGE DAMAGE
        // ==========================================

        List<BaseEnemy> targets =
            CombatTargeting.FindClosestEnemies(
                gameplayCenter,
                dischargeRadius,
                enemyLayer,
                32,
                null
            );

        float dischargeDamage =
            (stats != null
                ? stats.additionalDamage
                : 0f)
            * dischargeDamageMultiplier;

        foreach (BaseEnemy target
                 in targets)
        {
            if (target == null ||
                target.IsDead())
                continue;

            target.TakeEffectDamage(
                dischargeDamage
            );
        }
    }

    protected override void Upgrade()
    {
        level++;

        // Lv3:
        // un objetivo adicional.
        if (level == 3)
        {
            secondaryTargetCount++;
        }
    }

    private void OnDestroy()
    {
        if (playerContext != null &&
            playerContext.HandleAttack != null)
        {
            playerContext.HandleAttack.OnMeleeHit -=
                HandleMeleeHit;
        }
    }
}