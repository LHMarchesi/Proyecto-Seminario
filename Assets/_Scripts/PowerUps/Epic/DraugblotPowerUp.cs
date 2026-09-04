using System.Collections.Generic;
using UnityEngine;

public class DraugblotPowerUp : BasePowerUp
{
    [SerializeField]
    private ExplodeStrikeStats stats;

    [Header("VFX")]
    [SerializeField]
    private float vfxLifetime = 2f;

    [SerializeField]
    private float vfxHeightOffset = 1.5f;

    [Header("Level Upgrades")]
    [SerializeField]
    private float level2RadiusMultiplier = 1.20f;

    [SerializeField]
    private float level3DamageMultiplier = 1.20f;

    [SerializeField]
    private float level4ForceMultiplier = 1.40f;

    private int level = 1;

    private bool outboundExplosionUsed;
    private bool recallExplosionUsed;

    private void Awake()
    {
        stats = CreateRuntimeStatsCopy(stats);
    }

    protected override void ApplyEffect()
    {
        if (playerContext == null ||
            playerContext.Mjolnir == null)
            return;

        playerContext.Mjolnir.OnMjolnirThrow +=
            ResetThrowState;

        playerContext.Mjolnir.OnMjolnirImpact +=
            HandleMjolnirHit;
    }

    private void ResetThrowState()
    {
        outboundExplosionUsed = false;
        recallExplosionUsed = false;
    }

    private void HandleMjolnirHit(
    Collider enemyCollider,
    Vector3 hitPoint,
    Vector3 hitNormal,
    bool isRecallHit)
    {
        Debug.Log(
      $"DRAUGBLOT EVENT - {enemyCollider?.name}"
  );
        if (stats == null ||
            enemyCollider == null)
            return;

        BaseEnemy enemy =
            enemyCollider.GetComponentInParent<BaseEnemy>();

        if (enemy == null)
            return;

        if (isRecallHit)
        {
            if (level < 5)
                return;

            if (recallExplosionUsed)
                return;

            recallExplosionUsed = true;
        }
        else
        {
            if (outboundExplosionUsed)
                return;

            outboundExplosionUsed = true;
        }

        Explode(enemy);
    }

    private void Explode(BaseEnemy mainEnemy)
    {
        Debug.Log(
    $"DRAUGBLOT EXPLOSION - {mainEnemy.name}"
);
        if (mainEnemy == null)
            return;

        Vector3 explosionCenter =
            mainEnemy.CombatVFXPosition;

        // ==========================================
        // VFX
        // ==========================================

        Vector3 visualPosition =
            explosionCenter +
            Vector3.up * vfxHeightOffset;

        if (stats.EffectPrefab != null)
        {
            GameObject vfx =
                Instantiate(
                    stats.EffectPrefab,
                    visualPosition,
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
        // AOE
        // ==========================================

        Collider[] hits =
            Physics.OverlapSphere(
                explosionCenter,
                stats.explosionRange,
                stats.enemyLayer,
                QueryTriggerInteraction.Ignore
            );

        HashSet<BaseEnemy> damagedEnemies =
            new HashSet<BaseEnemy>();

        foreach (Collider hit in hits)
        {
            BaseEnemy enemy =
                hit.GetComponentInParent<BaseEnemy>();

            if (enemy == null)
                continue;

            if (!damagedEnemies.Add(enemy))
                continue;

            enemy.TakeEffectDamage(
                stats.explosionDamage
            );

            Rigidbody enemyRb =
                enemy.GetComponent<Rigidbody>();

            if (enemyRb != null)
            {
                enemyRb.AddExplosionForce(
                    stats.explosionForce,
                    explosionCenter,
                    stats.explosionRange,
                    0.25f,
                    ForceMode.Impulse
                );
            }
        }
    }


    protected override void Upgrade()
    {
        level++;

        switch (level)
        {
            case 2:
                stats.explosionRange *=
                    level2RadiusMultiplier;
                break;

            case 3:
                stats.explosionDamage *=
                    level3DamageMultiplier;
                break;

            case 4:
                stats.explosionForce *=
                    level4ForceMultiplier;
                break;

            case 5:
                // Habilita explosión durante Recall.
                break;
        }
    }

    private void OnDestroy()
    {
        if (playerContext == null ||
            playerContext.Mjolnir == null)
            return;

        playerContext.Mjolnir.OnMjolnirThrow -=
            ResetThrowState;

        playerContext.Mjolnir.OnMjolnirImpact -=
            HandleMjolnirHit;
    }
}