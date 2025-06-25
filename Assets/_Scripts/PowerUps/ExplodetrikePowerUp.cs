using UnityEngine;

public class ExplodetrikePowerUp : BasePowerUp
{
    [SerializeField] private Sprite IconSprite;
    [SerializeField] private GameObject EffectPrefab; // Prefab (Efecto)
    [SerializeField] private float explosionRange;
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionDamage;
    [SerializeField] private float cooldown;
    [SerializeField] private LayerMask EnemyLayer;

    private float lastStrikeTime = -Mathf.Infinity;
    protected override void ApplyEffect()
    {
        playerContext.Mjolnir.OnHitEnemy += Explode;
        UIManager.Instance.RegisterHability("Explode", IconSprite);
    }

    private void Explode(Collider enemyCollider)
    {
        if (Time.time - lastStrikeTime < cooldown) return;  // cooldown check

        lastStrikeTime = Time.time;

        if (enemyCollider == null) return;

        Vector3 effectPosition = enemyCollider.bounds.center; // Lo posiciona en el centro
        Quaternion effectRotation = Quaternion.identity;

        // if (EffectPrefab != null) Instantiate(EffectPrefab, effectPosition, effectRotation);

        Collider[] enemies = Physics.OverlapSphere(effectPosition, explosionRange, EnemyLayer);
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, effectPosition, explosionRange);
            enemies[i].GetComponent<IDamageable>().TakeDamage(explosionDamage);
        }

        UIManager.Instance.TriggerHabilityCooldown("Explode", cooldown);
    }
}