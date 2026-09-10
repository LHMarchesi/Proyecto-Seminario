using UnityEngine;

// Fire aplica stacks como Venomous Blows. Solo Lv5 agrega explosion al morir.
public class MuspelComboPowerUp : BasePowerUp
{
    [Header("Fire")]
    [SerializeField]
    private FireApplicationData fireData = new FireApplicationData
    {
        damagePerSecond = 5f,
        duration = 3f,
        tickInterval = 0.5f,
        stacksToAdd = 1,
        maxStacks = 1
    };

    [Header("Level upgrades")]
    [SerializeField, Min(1)] private int level2MaxStacks = 2;
    [SerializeField, Min(1)] private int level3Attack2Stacks = 2;
    [SerializeField, Min(1)] private int level4MaxStacks = 4;

    [Header("Level 5 - Death explosion")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float explosionRadius = 3f;
    [SerializeField, Min(0f)] private float explosionDamagePerStack = 5f;
    [SerializeField, Min(1)] private int maxExplosionTargets = 16;
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private Vector3 explosionVFXOffset;
    [SerializeField, Min(0f)] private float vfxLifetime = 2f;

    private int level = 1;
    private bool subscribed;
    [Header("Diagnostics")]
    [SerializeField] private bool logFireDebug;

    protected override void ApplyEffect()
    {
        if (subscribed || playerContext == null || playerContext.HandleAttack == null)
            return;

        playerContext.HandleAttack.OnMeleeHitBeforeDamage += HandleMeleeHit;
        subscribed = true;
    }

    private void HandleMeleeHit(MeleeHitInfo hit)
    {
        if (hit.AttackType != MeleeAttackType.Attack1 &&
            hit.AttackType != MeleeAttackType.Attack2)
            return;

        BaseEnemy enemy = hit.Enemy;
        if (enemy == null || enemy.IsDead())
            return;

        // El status pertenece al root, junto a BaseEnemy.
        // No fallamos silenciosamente si faltó configurarlo en un prefab.
        EnemyStatusEffectController status = enemy.GetComponent<EnemyStatusEffectController>();
        if (status == null)
        {
            status = enemy.gameObject.AddComponent<EnemyStatusEffectController>();
            Debug.LogWarning($"Muspel: {enemy.name} no tenía EnemyStatusEffectController; se agregó al root.", enemy);
        }
        if (!status.isActiveAndEnabled)
        {
            Debug.LogWarning($"Muspel: el controlador de status de {enemy.name} está deshabilitado.", enemy);
            return;
        }

        FireApplicationData data = fireData;
        data.stacksToAdd = level >= 3 && hit.AttackType == MeleeAttackType.Attack2
            ? Mathf.Max(1, level3Attack2Stacks) : 1;
        data.maxStacks = level >= 4 ? Mathf.Max(1, level4MaxStacks)
            : level >= 2 ? Mathf.Max(1, level2MaxStacks) : 1;

        // El remate es exclusivo de Lv5. Las explosiones no aplican Burn.
        data.explodeOnDeath = level >= 5;
        data.enemyLayer = enemyLayer;
        data.explosionRadius = explosionRadius;
        data.explosionDamagePerStack = explosionDamagePerStack;
        data.maxExplosionTargets = maxExplosionTargets;
        data.explosionVFXPrefab = explosionVFXPrefab;
        data.explosionVFXOffset = explosionVFXOffset;
        data.explosionVFXLifetime = vfxLifetime;

        status.ApplyFire(data);

        if (logFireDebug)
        {
            Debug.Log($"Muspel: Burn aplicado a {enemy.name} | Lv{level} | " +
                $"Stacks añadidos: {data.stacksToAdd} | Máximo: {data.maxStacks} | " +
                $"DPS: {data.damagePerSecond} | Tick: {data.tickInterval}", enemy);
        }
    }

    protected override void Upgrade()
    {
        level = Mathf.Min(5, level + 1);
    }

    private void OnDestroy()
    {
        if (subscribed && playerContext != null && playerContext.HandleAttack != null)
            playerContext.HandleAttack.OnMeleeHitBeforeDamage -= HandleMeleeHit;
        subscribed = false;
    }
}
