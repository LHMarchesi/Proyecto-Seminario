using System.Collections.Generic;
using UnityEngine;

public class ThunderstruckPowerUp : BasePowerUp
{
    [Header("Base")]
    [SerializeField] private LightingStrikeStats stats;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float searchRadius = 8f;
    [SerializeField] private float verticalVFXOffset = 0.5f;
    [SerializeField] private float vfxLifetime = 2f;

    [Header("Level 2 - Charged bonus damage")]
    [SerializeField, Range(0f, 1f)] private float chargedBonusDamagePercent = 0.20f;

    [Header("Level 4 - Extra knockback")]
    [SerializeField, Range(0f, 1f)] private float extraKnockbackPercent = 0.30f;

    [Header("Level 5 - Discharge")]
    [SerializeField] private float dischargeRadius = 3f;
    [SerializeField, Range(0f, 2f)] private float dischargeDamageMultiplier = 0.5f;
    [SerializeField] private GameObject dischargeVFXPrefab;

    private int level = 1;
    private int secondaryTargetCount = 1;

    private void Awake()
    {
        stats = CreateRuntimeStatsCopy(stats);
    }

    protected override void ApplyEffect()
    {
        if (playerContext == null || playerContext.HandleAttack == null)
            return;

        playerContext.HandleAttack.OnMeleeHit += HandleMeleeHit;
    }

    private void HandleMeleeHit(MeleeHitInfo hitInfo)
    {
        if (hitInfo.AttackType != MeleeAttackType.Charged)
            return;

        BaseEnemy primaryEnemy = hitInfo.Enemy;
        if (primaryEnemy == null)
            return;

        // Lv2: el Charged obtiene daño adicional sin volver a disparar la
        // reacción normal de OnDamage/knockback del enemigo.
        if (level >= 2)
            primaryEnemy.TakeEffectDamage(hitInfo.Damage * chargedBonusDamagePercent);

        // Lv4: añade un impulso equivalente a un porcentaje del knockback
        // que ya tenga configurado el Charged Attack.
        if (level >= 4 && hitInfo.KnockbackForce > 0f)
        {
            primaryEnemy.ApplyExternalKnockback(
                hitInfo.HitDirection,
                hitInfo.KnockbackForce * extraKnockbackPercent);
        }

        List<BaseEnemy> lightningTargets = CombatTargeting.FindClosestEnemies(
            primaryEnemy.transform.position,
            searchRadius,
            enemyLayer,
            secondaryTargetCount,
            primaryEnemy);

        // Si no existe otro enemigo cerca, el rayo cae sobre el objetivo
        // primario para que el item nunca quede sin efecto.
        if (lightningTargets.Count == 0)
        {
            StrikeEnemy(primaryEnemy, stats != null ? stats.additionalDamage : 0f);
        }
        else
        {
            foreach (BaseEnemy target in lightningTargets)
                StrikeEnemy(target, stats != null ? stats.additionalDamage : 0f);
        }

        // Lv5: descarga en área alrededor del impacto principal.
        if (level >= 5)
            CreateDischarge(primaryEnemy);
    }

    private void StrikeEnemy(BaseEnemy enemy, float damage)
    {
        if (enemy == null || enemy.IsDead())
            return;

        enemy.TakeEffectDamage(damage);

        if (stats != null && stats.lightningEffectPrefab != null)
        {
            Vector3 spawnPosition =
                enemy.transform.position + Vector3.up * verticalVFXOffset;

            GameObject vfx = Instantiate(
                stats.lightningEffectPrefab,
                spawnPosition,
                Quaternion.identity);

            if (vfxLifetime > 0f)
                Destroy(vfx, vfxLifetime);
        }

        if (SoundManagerOcta.Instance != null)
            SoundManagerOcta.Instance.PlaySound("LightningStrike");
    }

    private void CreateDischarge(BaseEnemy primaryEnemy)
    {
        Vector3 center = primaryEnemy.transform.position;

        if (dischargeVFXPrefab != null)
        {
            GameObject vfx = Instantiate(
                dischargeVFXPrefab,
                center,
                Quaternion.identity);

            if (vfxLifetime > 0f)
                Destroy(vfx, vfxLifetime);
        }

        List<BaseEnemy> targets = CombatTargeting.FindClosestEnemies(
            center,
            dischargeRadius,
            enemyLayer,
            32,
            null);

        float dischargeDamage =
            (stats != null ? stats.additionalDamage : 0f) * dischargeDamageMultiplier;

        foreach (BaseEnemy target in targets)
            target.TakeEffectDamage(dischargeDamage);
    }

    protected override void Upgrade()
    {
        level++;

        // Lv3: un objetivo adicional.
        if (level == 3)
            secondaryTargetCount++;
    }

    private void OnDestroy()
    {
        if (playerContext != null && playerContext.HandleAttack != null)
            playerContext.HandleAttack.OnMeleeHit -= HandleMeleeHit;
    }
}
