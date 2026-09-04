using UnityEngine;

public class VenomousBlowsPowerUp : BasePowerUp
{
    [SerializeField] private PoisonStrikeStats stats;

    private int level = 1;
    private int maxStacks = 1;
    private int attack1Stacks = 1;
    private int attack2Stacks = 1;
    private bool spreadOnDeath;

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
        if (hitInfo.AttackType != MeleeAttackType.Attack1 &&
            hitInfo.AttackType != MeleeAttackType.Attack2)
            return;

        if (stats == null || hitInfo.Enemy == null)
            return;

        EnemyStatusEffectController statusController =
            hitInfo.Enemy.GetComponent<EnemyStatusEffectController>();

        if (statusController == null)
        {
            statusController =
                hitInfo.Enemy.gameObject.AddComponent<EnemyStatusEffectController>();
        }

        int stacksToAdd = hitInfo.AttackType == MeleeAttackType.Attack2
            ? attack2Stacks
            : attack1Stacks;

        PoisonApplicationData poisonData = new PoisonApplicationData
        {
            damagePerSecond = stats.poisonDamagePerSecond,
            duration = stats.poisonDuration,
            tickInterval = stats.tickInterval,
            stacksToAdd = stacksToAdd,
            maxStacks = maxStacks,
            vfxPrefab = stats.poisonVFXPrefab,
            spreadOnDeath = spreadOnDeath,
            spreadRadius = stats.spreadRadius,
            maxSpreadTargets = stats.maxSpreadTargets,
            enemyLayer = stats.enemyLayer
        };

        statusController.ApplyPoison(poisonData);
    }

    protected override void Upgrade()
    {
        level++;

        switch (level)
        {
            case 2:
                maxStacks = 2;
                break;

            case 3:
                attack2Stacks = 2;
                break;

            case 4:
                maxStacks = 4;
                break;

            case 5:
                spreadOnDeath = true;
                break;
        }
    }

    private void OnDestroy()
    {
        if (playerContext != null && playerContext.HandleAttack != null)
            playerContext.HandleAttack.OnMeleeHit -= HandleMeleeHit;
    }
}
