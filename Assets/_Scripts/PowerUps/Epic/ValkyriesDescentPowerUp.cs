using UnityEngine;

public class ValkyriesDescentPowerUp : BasePowerUp
{
    [Header("Shockwave")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float radius = 4f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float level2RadiusMultiplier = 1.25f;
    [SerializeField] private float level3DamageMultiplier = 1.25f;
    [SerializeField] private float level4UpwardForce = 6f;
    [SerializeField] private GameObject shockwaveVFXPrefab;
    [SerializeField] private float vfxLifetime = 2f;

    [Header("Level 5 - Lightning")]
    [SerializeField] private GameObject lightningVFXPrefab;
    [SerializeField] private Vector3 lightningVFXOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private float lightningDamage = 12f;
    [SerializeField] private bool applyElectricityAtLevel5 = true;
    [SerializeField] private ElectricityApplicationData electricityData;

    private int level = 1;
    private bool subscribed;

    protected override void ApplyEffect()
    {
        if (subscribed || playerContext == null || playerContext.HandleAttack == null) return;
        playerContext.HandleAttack.OnFallingHammerLanded += HandleLanding;
        subscribed = true;
    }

    private void HandleLanding(Vector3 groundPoint)
    {
        float finalRadius = radius * (level >= 2 ? level2RadiusMultiplier : 1f);
        float finalDamage = damage * (level >= 3 ? level3DamageMultiplier : 1f);
        SpawnVFX(shockwaveVFXPrefab, groundPoint);
        CombatAreaDamage.DealDamage(groundPoint, finalRadius, enemyLayer, finalDamage,
            DamageFeedbackType.Normal);

        // La onda de nivel 4 impulsa hacia arriba, incluso sin fuerza horizontal.
        if (level >= 4 && level4UpwardForce > 0f)
        {
            foreach (BaseEnemy enemy in CombatAreaDamage.FindTargets(groundPoint, finalRadius, enemyLayer))
                if (enemy != null && !enemy.IsDead())
                {
                    Rigidbody body = enemy.GetComponent<Rigidbody>();
                    if (body != null && !body.isKinematic)
                        body.AddForce(Vector3.up * level4UpwardForce, ForceMode.Impulse);
                }
        }

        if (level < 5) return;
        SpawnVFX(lightningVFXPrefab, groundPoint + lightningVFXOffset);
        foreach (BaseEnemy enemy in CombatAreaDamage.FindTargets(groundPoint, finalRadius, enemyLayer))
        {
            if (enemy == null || enemy.IsDead()) continue;
            if (lightningDamage > 0f)
                enemy.TakeEffectDamage(lightningDamage, DamageFeedbackType.Electricity);
            if (applyElectricityAtLevel5 && !enemy.IsDead())
            {
                EnemyStatusEffectController status = enemy.GetComponent<EnemyStatusEffectController>();
                if (status != null) status.ApplyElectricity(electricityData);
            }
        }
    }

    private void SpawnVFX(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        GameObject vfx = Instantiate(prefab, position, Quaternion.identity);
        if (vfxLifetime > 0f) Destroy(vfx, vfxLifetime);
    }

    protected override void Upgrade() { level = Mathf.Min(5, level + 1); }

    private void OnDestroy()
    {
        if (subscribed && playerContext != null && playerContext.HandleAttack != null)
            playerContext.HandleAttack.OnFallingHammerLanded -= HandleLanding;
        subscribed = false;
    }
}
