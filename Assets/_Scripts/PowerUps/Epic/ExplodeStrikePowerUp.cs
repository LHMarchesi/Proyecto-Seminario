using System.Collections.Generic;
using UnityEngine;

public class ExplodeStrikePowerUp : BasePowerUp
{
    [SerializeField] private ExplodeStrikeStats stats;

    [Header("Upgrade por nivel")]
    [SerializeField] private float damageIncrease = 5f;
    [SerializeField] private float rangeIncrease = 0.15f;
    [SerializeField] private float cooldownReduction = 0.1f;
    [SerializeField] private float minimumCooldown = 0.1f;

    private float lastStrikeTime = -Mathf.Infinity;

    private void Awake()
    {
        stats = CreateRuntimeStatsCopy(stats);
    }

    protected override void ApplyEffect()
    {
        if (playerContext == null || playerContext.Mjolnir == null)
            return;

        playerContext.Mjolnir.OnHitEnemy += Explode;
    }

    protected override void Upgrade()
    {
        if (stats == null)
            return;

        stats.explosionDamage += damageIncrease;
        stats.explosionRange += rangeIncrease;
        stats.cooldown = Mathf.Max(minimumCooldown, stats.cooldown - cooldownReduction);
    }

    private void Explode(Collider enemyCollider)
    {
        if (stats == null || enemyCollider == null)
            return;

        if (Time.time - lastStrikeTime < stats.cooldown)
            return;

        lastStrikeTime = Time.time;

        Vector3 effectPosition = enemyCollider.bounds.center;

        if (stats.EffectPrefab != null)
            Instantiate(stats.EffectPrefab, effectPosition, Quaternion.identity);

        Collider[] enemies = Physics.OverlapSphere(effectPosition, stats.explosionRange, stats.enemyLayer);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider enemyColliderHit in enemies)
        {
            IDamageable damageable = enemyColliderHit.GetComponentInParent<IDamageable>();
            if (damageable == null || !damagedTargets.Add(damageable))
                continue;

            Rigidbody body = enemyColliderHit.GetComponentInParent<Rigidbody>();
            if (body != null)
                body.AddExplosionForce(stats.explosionForce, effectPosition, stats.explosionRange);

            damageable.TakeDamage(stats.explosionDamage);
        }

        if (UIManager.Instance != null)
            UIManager.Instance.TriggerHabilityCooldown("Explode", stats.cooldown);
    }

    private void OnDestroy()
    {
        if (playerContext != null && playerContext.Mjolnir != null)
            playerContext.Mjolnir.OnHitEnemy -= Explode;
    }
}
