using UnityEngine;

public class LightningStrikePowerUp : BasePowerUp
{
    [SerializeField] private LightingStrikeStats stats;
    [SerializeField] private float verticalOffset;
    [SerializeField] private float upgradeDamageIncrease = 5f;
    [SerializeField] private float cooldownReductionPerLevel = 0.2f;
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

        playerContext.Mjolnir.OnHitEnemy += HandleLightningHit;
    }

    private void HandleLightningHit(Collider enemyCollider)
    {
        if (stats == null || enemyCollider == null)
            return;

        if (Time.time - lastStrikeTime < stats.cooldown)
            return;

        lastStrikeTime = Time.time;

        IDamageable damageable = enemyCollider.GetComponentInParent<IDamageable>();
        damageable?.TakeDamage(stats.additionalDamage);

        Vector3 effectPosition = enemyCollider.bounds.center + Vector3.up * verticalOffset;

        if (stats.lightningEffectPrefab != null)
            Instantiate(stats.lightningEffectPrefab, effectPosition, Quaternion.identity);

        SoundManagerOcta.Instance.PlaySound("LightningStrike");

        if (UIManager.Instance != null)
            UIManager.Instance.TriggerHabilityCooldown("Lightning", stats.cooldown);
    }

    protected override void Upgrade()
    {
        if (stats == null)
            return;

        stats.additionalDamage += upgradeDamageIncrease;
        stats.cooldown = Mathf.Max(minimumCooldown, stats.cooldown - cooldownReductionPerLevel);
    }

    private void OnDestroy()
    {
        if (playerContext != null && playerContext.Mjolnir != null)
            playerContext.Mjolnir.OnHitEnemy -= HandleLightningHit;
    }
}
