using System.Collections.Generic;
using UnityEngine;

public class DraugblotPowerUp : BasePowerUp
{
    [SerializeField] private ExplodeStrikeStats stats;
    [SerializeField] private float vfxLifetime = 2f;

    [Header("Level upgrades")]
    [SerializeField] private float level2RadiusMultiplier = 1.20f;
    [SerializeField] private float level3DamageMultiplier = 1.20f;
    [SerializeField] private float level4ForceMultiplier = 1.40f;

    private int level = 1;
    private bool outboundExplosionUsed;
    private bool recallExplosionUsed;

    private void Awake()
    {
        stats = CreateRuntimeStatsCopy(stats);
    }

    protected override void ApplyEffect()
    {
        if (playerContext == null || playerContext.Mjolnir == null)
            return;

        playerContext.Mjolnir.OnMjolnirThrow += ResetThrowState;
        playerContext.Mjolnir.OnHitEnemy += HandleMjolnirHit;
    }

    private void ResetThrowState()
    {
        outboundExplosionUsed = false;
        recallExplosionUsed = false;
    }

    private void HandleMjolnirHit(Collider enemyCollider)
    {
        if (stats == null || enemyCollider == null || playerContext.Mjolnir == null)
            return;

        bool isRecallHit = playerContext.Mjolnir.IsRetracting;

        if (isRecallHit)
        {
            // Lv5 habilita una segunda explosión durante el recall.
            if (level < 5 || recallExplosionUsed)
                return;

            recallExplosionUsed = true;
        }
        else
        {
            if (outboundExplosionUsed)
                return;

            outboundExplosionUsed = true;
        }

        Explode(enemyCollider.bounds.center);
    }

    private void Explode(Vector3 center)
    {
        if (stats.EffectPrefab != null)
        {
            GameObject vfx = Instantiate(
                stats.EffectPrefab,
                center,
                Quaternion.identity);

            if (vfxLifetime > 0f)
                Destroy(vfx, vfxLifetime);
        }

        Collider[] hits = Physics.OverlapSphere(
            center,
            stats.explosionRange,
            stats.enemyLayer,
            QueryTriggerInteraction.Collide);

        HashSet<BaseEnemy> damagedEnemies = new HashSet<BaseEnemy>();

        foreach (Collider hit in hits)
        {
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !damagedEnemies.Add(enemy))
                continue;

            enemy.TakeEffectDamage(stats.explosionDamage);

            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.AddExplosionForce(
                    stats.explosionForce,
                    center,
                    stats.explosionRange,
                    0.25f,
                    ForceMode.Impulse);
            }
        }
    }

    protected override void Upgrade()
    {
        level++;

        switch (level)
        {
            case 2:
                stats.explosionRange *= level2RadiusMultiplier;
                break;

            case 3:
                stats.explosionDamage *= level3DamageMultiplier;
                break;

            case 4:
                stats.explosionForce *= level4ForceMultiplier;
                break;

            // Lv5 se resuelve en HandleMjolnirHit: habilita explosión en Recall.
            case 5:
                break;
        }
    }

    private void OnDestroy()
    {
        if (playerContext == null || playerContext.Mjolnir == null)
            return;

        playerContext.Mjolnir.OnMjolnirThrow -= ResetThrowState;
        playerContext.Mjolnir.OnHitEnemy -= HandleMjolnirHit;
    }
}
