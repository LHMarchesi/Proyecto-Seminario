using UnityEngine;

// El aim assist normal no cambia: Helvegr solamente controla saltos posteriores al impacto.
public class HelvegrPowerUp : BasePowerUp
{
    [Header("Chain movement")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float chainRange = 8f;
    [SerializeField] private float level4RangeMultiplier = 1.25f;
    [SerializeField] private float chainSpeed = 40f;
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float arrivalDistance = 0.18f;

    [Header("Level 3 - Discharge")]
    [SerializeField] private float dischargeRadius = 2f;
    [SerializeField, Range(0f, 2f)] private float dischargeDamageMultiplier = 0.30f;
    [SerializeField] private GameObject dischargeVFXPrefab;
    [SerializeField] private Vector3 dischargeVFXOffset;
    [SerializeField] private float vfxLifetime = 2f;

    [Header("Level 5 - Electricity")]
    [SerializeField] private ElectricityApplicationData electricityData;

    private MjolnirChainController controller;
    private int level = 1;

    protected override void ApplyEffect()
    {
        if (playerContext == null || playerContext.Mjolnir == null) return;
        controller = playerContext.Mjolnir.GetComponent<MjolnirChainController>();
        if (controller == null)
            controller = playerContext.Mjolnir.gameObject.AddComponent<MjolnirChainController>();
        ConfigureController();
    }

    private void ConfigureController()
    {
        if (controller == null) return;
        int jumps = level >= 4 ? 3 : level >= 2 ? 2 : 1;
        float range = chainRange * (level >= 4 ? level4RangeMultiplier : 1f);
        controller.Configure(this, enemyLayer, obstacleLayer, jumps, range,
            chainSpeed, collisionRadius, arrivalDistance);
    }

    // Llamado sólo por el controlador al alcanzar B, C, etc. No reinicia el lanzamiento.
    public void OnChainImpact(BaseEnemy enemy, Vector3 visualPosition, Vector3 gameplayPosition)
    {
        if (level >= 3)
        {
            if (dischargeVFXPrefab != null)
            {
                GameObject vfx = Instantiate(dischargeVFXPrefab,
                    visualPosition + dischargeVFXOffset, Quaternion.identity);
                if (vfxLifetime > 0f) Destroy(vfx, vfxLifetime);
            }
            Vector3 center = gameplayPosition;
            float damage = playerContext.Mjolnir.damage * dischargeDamageMultiplier;
            CombatAreaDamage.DealDamage(center, dischargeRadius, enemyLayer,
                damage, DamageFeedbackType.Electricity);
        }
        if (level >= 5 && enemy != null && !enemy.IsDead())
        {
            EnemyStatusEffectController status = enemy.GetComponent<EnemyStatusEffectController>();
            if (status != null) status.ApplyElectricity(electricityData);
        }
    }

    protected override void Upgrade()
    {
        level = Mathf.Min(5, level + 1);
        ConfigureController();
    }

    private void OnDestroy()
    {
        if (controller != null) controller.Release();
    }
}
