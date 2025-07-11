using UnityEngine;

public class ExplodeStrikePowerUp : BasePowerUp
{
    [SerializeField] private ExplodeStrikeStats stats;
    private float lastStrikeTime = -Mathf.Infinity;
    protected override void ApplyEffect()
    {
        playerContext.Mjolnir.OnHitEnemy += Explode;
        UIManager.Instance.RegisterHability("Explode", stats.IconSprite);
    }

    protected override void Upgrade()
    {
    }

    private void Explode(Collider enemyCollider)
    {
        if (Time.time - lastStrikeTime < stats.cooldown) return;  // cooldown check

        lastStrikeTime = Time.time;

        if (enemyCollider == null) return;

        Vector3 effectPosition = enemyCollider.bounds.center; // Lo posiciona en el centro
        Quaternion effectRotation = Quaternion.identity;

        // if (EffectPrefab != null) Instantiate(EffectPrefab, effectPosition, effectRotation);

        Collider[] enemies = Physics.OverlapSphere(effectPosition, stats.explosionRange, stats.enemyLayer);
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<Rigidbody>().AddExplosionForce(stats.explosionForce, effectPosition, stats.explosionRange);
            enemies[i].GetComponent<IDamageable>().TakeDamage(stats.explosionDamage);
        }

        UIManager.Instance.TriggerHabilityCooldown("Explode", stats.cooldown);
    }
}