using System.Collections.Generic;
using UnityEngine;

public class ThunderstruckPowerUp : BasePowerUp
{
    [Header("Base")]
    [SerializeField] private LightingStrikeStats stats;
    [SerializeField] private float vfxLifetime = 2f;
    [SerializeField] private float lightningVFXHeightOffset = 0f;

    [Header("Lightning targets")]
    [SerializeField] private int level1TargetCount = 2;
    [SerializeField] private int level3TargetCount = 3;

    [Header("Electricity")]
    [SerializeField] private ElectricityApplicationData electricityData;

    [Header("Level 2 - Charged bonus damage")]
    [SerializeField, Range(0f, 1f)]
    private float chargedBonusDamagePercent = 0.20f;

    [Header("Level 4 - Extra knockback")]
    [SerializeField, Range(0f, 1f)]
    private float extraKnockbackPercent = 0.30f;

    [Header("Level 5 - Discharge")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float dischargeRadius = 3f;
    [SerializeField, Range(0f, 2f)]
    private float dischargeDamageMultiplier = 0.5f;
    [SerializeField] private GameObject dischargeVFXPrefab;

    private int level = 1;

    private void Awake()
    {
        stats = CreateRuntimeStatsCopy(stats);
    }

    protected override void ApplyEffect()
    {
        if (playerContext == null ||
            playerContext.HandleAttack == null)
            return;

        playerContext.HandleAttack.OnMeleeAttackResolved +=
            HandleMeleeAttackResolved;
    }

    private void HandleMeleeAttackResolved(
        IReadOnlyList<MeleeHitInfo> hits)
    {
        if (hits == null || hits.Count == 0)
            return;

        if (hits[0].AttackType != MeleeAttackType.Charged)
            return;

        List<BaseEnemy> chargedEnemies =
            GetUniqueChargedEnemies(hits);

        if (chargedEnemies.Count == 0)
            return;

        // -------------------------------------------------
        // Lv2: +20% al Charged sobre TODOS los golpeados.
        // -------------------------------------------------
        if (level >= 2)
        {
            foreach (MeleeHitInfo hit in hits)
            {
                BaseEnemy enemy = hit.Enemy;

                if (enemy == null || enemy.IsDead())
                    continue;

                enemy.TakeEffectDamage(
                    hit.Damage * chargedBonusDamagePercent,
                    DamageFeedbackType.Normal);
            }
        }

        // -------------------------------------------------
        // Lv4: knockback adicional sobre TODOS los golpeados.
        // -------------------------------------------------
        if (level >= 4)
        {
            foreach (MeleeHitInfo hit in hits)
            {
                BaseEnemy enemy = hit.Enemy;

                if (enemy == null ||
                    enemy.IsDead() ||
                    hit.KnockbackForce <= 0f)
                    continue;

                enemy.ApplyExternalKnockback(
                    hit.HitDirection,
                    hit.KnockbackForce * extraKnockbackPercent);
            }
        }

        // Elegimos los más cercanos al jugador para que sea determinista.
        chargedEnemies.Sort((a, b) =>
        {
            float distanceA =
                (a.transform.position - playerContext.transform.position)
                .sqrMagnitude;

            float distanceB =
                (b.transform.position - playerContext.transform.position)
                .sqrMagnitude;

            return distanceA.CompareTo(distanceB);
        });

        int targetCount =
            level >= 3
                ? level3TargetCount
                : level1TargetCount;

        int amountToStrike = Mathf.Min(
            targetCount,
            chargedEnemies.Count);

        // -------------------------------------------------
        // Lightning + daño extra + Electricity.
        // -------------------------------------------------
        for (int i = 0; i < amountToStrike; i++)
        {
            StrikeEnemy(chargedEnemies[i]);
        }

        // -------------------------------------------------
        // Lv5: descarga alrededor del primer objetivo.
        // -------------------------------------------------
        if (level >= 5)
        {
            CreateDischarge(chargedEnemies[0]);
        }
    }

    private List<BaseEnemy> GetUniqueChargedEnemies(
        IReadOnlyList<MeleeHitInfo> hits)
    {
        List<BaseEnemy> enemies = new List<BaseEnemy>();
        HashSet<BaseEnemy> unique = new HashSet<BaseEnemy>();

        foreach (MeleeHitInfo hit in hits)
        {
            if (hit.AttackType != MeleeAttackType.Charged)
                continue;

            BaseEnemy enemy = hit.Enemy;

            if (enemy == null)
                continue;

            if (!unique.Add(enemy))
                continue;

            enemies.Add(enemy);
        }

        return enemies;
    }

    private void StrikeEnemy(BaseEnemy enemy)
    {
        if (enemy == null)
            return;

        // Guardamos la posición ANTES de hacer daño por si el rayo mata.
        Vector3 vfxPosition =
            enemy.CombatVFXPosition +
            Vector3.up * lightningVFXHeightOffset;

        // -------------------------------------------------
        // Lightning VFX
        // -------------------------------------------------
        if (stats != null &&
            stats.lightningEffectPrefab != null)
        {
            GameObject vfx = Instantiate(
                stats.lightningEffectPrefab,
                vfxPosition,
                Quaternion.identity);

            if (vfxLifetime > 0f)
                Destroy(vfx, vfxLifetime);
        }

        if (SoundManagerOcta.Instance != null)
        {
            SoundManagerOcta.Instance.PlaySound(
                "LightningStrike");
        }

        if (enemy.IsDead())
            return;

        // -------------------------------------------------
        // Daño adicional del rayo
        // -------------------------------------------------
        float lightningDamage =
            stats != null
                ? stats.additionalDamage
                : 0f;

        if (lightningDamage > 0f)
        {
            enemy.TakeEffectDamage(lightningDamage, DamageFeedbackType.Electricity);
        }

        // Si el rayo lo mató, ya no aplicamos status.
        if (enemy.IsDead())
            return;

        // -------------------------------------------------
        // Electricity: VFX persistente + stun corto.
        // -------------------------------------------------
        EnemyStatusEffectController status =
            enemy.GetComponent<EnemyStatusEffectController>();

        if (status == null)
        {
            status = enemy.gameObject.AddComponent<
                EnemyStatusEffectController>();
        }

        status.ApplyElectricity(electricityData);
    }

    private void CreateDischarge(BaseEnemy primaryEnemy)
    {
        if (primaryEnemy == null)
            return;

        Vector3 gameplayCenter =
            primaryEnemy.transform.position;

        Vector3 visualCenter =
            primaryEnemy.CombatVFXPosition;

        if (dischargeVFXPrefab != null)
        {
            GameObject vfx = Instantiate(
                dischargeVFXPrefab,
                visualCenter,
                Quaternion.identity);

            if (vfxLifetime > 0f)
                Destroy(vfx, vfxLifetime);
        }

        List<BaseEnemy> targets =
            CombatTargeting.FindClosestEnemies(
                gameplayCenter,
                dischargeRadius,
                enemyLayer,
                32,
                null);

        float dischargeDamage =
            (stats != null ? stats.additionalDamage : 0f) *
            dischargeDamageMultiplier;

        foreach (BaseEnemy target in targets)
        {
            if (target == null || target.IsDead())
                continue;

            target.TakeEffectDamage(dischargeDamage, DamageFeedbackType.Electricity);
        }
    }

    protected override void Upgrade()
    {
        level++;
    }

    private void OnDestroy()
    {
        if (playerContext != null &&
            playerContext.HandleAttack != null)
        {
            playerContext.HandleAttack.OnMeleeAttackResolved -=
                HandleMeleeAttackResolved;
        }
    }
}
