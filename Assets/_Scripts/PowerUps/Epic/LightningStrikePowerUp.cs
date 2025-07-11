using UnityEngine;

public class LightningStrikePowerUp : BasePowerUp
{
    // Prefab (Efecto)
    [SerializeField] private LightingStrikeStats stats;
    [SerializeField] private float verticalOffset;

    private float lastStrikeTime = -Mathf.Infinity;

    protected override void ApplyEffect()
    {
        // Se subscribe al evento en el Mjolnir que detecta cuando colisiona con un enemigo
        playerContext.Mjolnir.OnHitEnemy += SpawnLightningEffect;
        playerContext.Mjolnir.OnHitEnemy += AddAditionalDamage;

        UIManager.Instance.RegisterHability("Lightning", stats.lightningIconSprite);
    }

    void SpawnLightningEffect(Collider enemyCollider)
    {
        if (Time.time - lastStrikeTime < stats.cooldown)
            return;

        lastStrikeTime = Time.time;

        if (enemyCollider == null) return;

        Vector3 effectPosition = enemyCollider.bounds.center + Vector3.up * verticalOffset; // Lo posiciona en el centro
        Quaternion effectRotation = Quaternion.identity;
        Instantiate(stats.lightningEffectPrefab, effectPosition, effectRotation); // Lo instancia

        //Play Sound

        UIManager.Instance.TriggerHabilityCooldown("Lightning", stats.cooldown);    //Triiger Cooldown
    }

    void AddAditionalDamage(Collider enemyCollider)
    {
        if (Time.time - lastStrikeTime < stats.cooldown)
            return;

        if (enemyCollider == null) return;

        IDamageable damageable = enemyCollider.GetComponent<IDamageable>();
        damageable?.TakeDamage(stats.additionalDamage);
    }

    protected override void Upgrade()
    {
        stats.additionalDamage += 5f;
        stats.cooldown = Mathf.Max(0.1f, stats.cooldown - 0.2f);
    }
}
