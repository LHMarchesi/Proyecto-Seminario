using UnityEngine;

// Un solo item runtime por run. No modifica los assets de stats ni el daño de Throw.
public class StormRecallPowerUp : BasePowerUp
{
    [Header("Lv1 - Recall damage")]
    [SerializeField, Min(0f)] private float recallDamageMultiplier = 1.30f;

    [Header("Lv2 - Recall speed")]
    [SerializeField, Min(0.01f)] private float recallSpeedMultiplier = 1.20f;

    [Header("Lv1 - Electricity")]
    [SerializeField]
    private ElectricityApplicationData electricityData = new ElectricityApplicationData
    {
        stunDuration = 0.25f,
        effectDuration = 2f,
        damagePerSecond = 4f,
        tickInterval = 0.5f
    };

    [Header("Lv3 - Electricity bonuses")]
    [SerializeField, Min(0f)] private float level3StunMultiplier = 1.5f;
    [SerializeField, Min(0f)] private float level3EffectDamageMultiplier = 1.5f;

    [Header("Lv4 - Next recall hit")]
    [SerializeField, Min(0f)] private float bonusPerEnemy = 0.10f;
    [SerializeField, Min(0f)] private float maximumBonus = 0.50f;

    [Header("Lv5 - Catch discharge")]
    [SerializeField] private LayerMask enemyLayer = ~0;
    [SerializeField, Min(0f)] private float dischargeRadius = 3.5f;
    [SerializeField, Min(0f)] private float dischargeDamage = 15f;
    [SerializeField, Min(1)] private int maxDischargeTargets = 32;
    [SerializeField] private GameObject dischargeVFXPrefab;
    [SerializeField] private Vector3 dischargeVFXOffset = new Vector3(0f, 1f, 0f);
    [SerializeField, Min(0f)] private float vfxLifetime = 2f;
    [SerializeField] private bool applyElectricityOnCatch = true;
    [SerializeField] private bool logStormDebug;

    private Mjolnir hammer;
    private int level = 1;
    private bool subscribed;
    private bool recallActive;
    private float nextHitBonus;

    private void OnEnable()
    {
        if (IsAcquiredForRun && !subscribed)
            ApplyEffect();
    }

    protected override void ApplyEffect()
    {
        if (subscribed || playerContext == null || playerContext.Mjolnir == null) return;
        hammer = playerContext.Mjolnir;
        hammer.OnMjolnirThrow += HandleThrow;
        hammer.OnMjolnirRecallStarted += HandleRecallStarted;
        hammer.OnMjolnirRecallHit += HandleRecallHit;
        hammer.OnMjolnirRecallEnded += HandleRecallEnded;
        subscribed = true;
        recallActive = hammer.IsRetracting;
        nextHitBonus = 0f;
        ConfigureModifiers();
    }

    private void ConfigureModifiers()
    {
        if (hammer == null) return;
        float damageMultiplier = recallDamageMultiplier +
            (level >= 4 && recallActive ? nextHitBonus : 0f);
        float speedMultiplier = level >= 2 ? recallSpeedMultiplier : 1f;
        hammer.ConfigureRecallModifiers(damageMultiplier, speedMultiplier);
    }

    private void HandleThrow()
    {
        recallActive = false;
        nextHitBonus = 0f;
        ConfigureModifiers();
    }

    private void HandleRecallStarted()
    {
        recallActive = true;
        nextHitBonus = 0f;
        ConfigureModifiers();
        Log("Recall iniciado");
    }

    private void HandleRecallHit(BaseEnemy enemy, float hitDamage,
        Vector3 visualPosition, Vector3 gameplayPosition)
    {
        if (!recallActive || enemy == null) return;

        if (!enemy.IsDead())
        {
            EnemyStatusEffectController status = enemy.GetComponent<EnemyStatusEffectController>();
            if (status == null)
                status = enemy.gameObject.AddComponent<EnemyStatusEffectController>();

            // Lv1 aplica Electricity; Lv3 potencia stun y DPS sin modificar
            // el asset ni los valores base del prefab.
            ElectricityApplicationData data = electricityData;
            if (level >= 3)
            {
                data.stunDuration *= level3StunMultiplier;
                data.damagePerSecond *= level3EffectDamageMultiplier;
            }
            status.ApplyElectricity(data);
        }

        // Mjolnir ya capturó hitDamage antes de este evento. El aumento es
        // exclusivamente para el SIGUIENTE objetivo, no para el actual.
        if (level >= 4)
        {
            nextHitBonus = Mathf.Min(Mathf.Max(0f, maximumBonus),
                nextHitBonus + Mathf.Max(0f, bonusPerEnemy));
            ConfigureModifiers();
        }
        Log("Impacto: " + enemy.name + " | daño actual: " + hitDamage +
            " | bonus siguiente: " + nextHitBonus);
    }

    private void HandleRecallEnded(bool caught)
    {
        bool completedRecall = recallActive;
        recallActive = false;
        nextHitBonus = 0f;
        ConfigureModifiers();
        if (caught && completedRecall && level >= 5)
            CreateCatchDischarge();
    }

    private void CreateCatchDischarge()
    {
        if (playerContext == null) return;
        Vector3 center = playerContext.transform.position;
        Vector3 visualPosition = center + dischargeVFXOffset;

        if (dischargeVFXPrefab != null)
        {
            GameObject vfx = Instantiate(dischargeVFXPrefab, visualPosition, Quaternion.identity);
            if (vfxLifetime > 0f) Destroy(vfx, vfxLifetime);
        }

        // Capturamos la lista antes del daño para no depender de objetos que
        // puedan morir/despawnear. La consulta ignora triggers y deduplica enemigos.
        var targets = CombatAreaDamage.FindTargets(center, dischargeRadius,
            enemyLayer, Mathf.Max(1, maxDischargeTargets));
        foreach (BaseEnemy enemy in targets)
        {
            if (enemy == null || enemy.IsDead()) continue;
            if (dischargeDamage > 0f)
                enemy.TakeEffectDamage(dischargeDamage, DamageFeedbackType.Electricity);
            if (applyElectricityOnCatch && !enemy.IsDead())
            {
                EnemyStatusEffectController status = enemy.GetComponent<EnemyStatusEffectController>();
                if (status == null)
                    status = enemy.gameObject.AddComponent<EnemyStatusEffectController>();
                status.ApplyElectricity(electricityData);
            }
        }
        Log("Descarga al atrapar Mjolnir: " + targets.Count + " objetivos");
    }

    protected override void Upgrade()
    {
        level = Mathf.Min(5, level + 1);
        ConfigureModifiers();
    }

    private void Log(string message)
    {
        if (logStormDebug) Debug.Log("[Storm Recall] " + message, this);
    }

    private void OnDisable() { Cleanup(); }
    private void OnDestroy() { Cleanup(); }

    private void Cleanup()
    {
        if (subscribed && hammer != null)
        {
            hammer.OnMjolnirThrow -= HandleThrow;
            hammer.OnMjolnirRecallStarted -= HandleRecallStarted;
            hammer.OnMjolnirRecallHit -= HandleRecallHit;
            hammer.OnMjolnirRecallEnded -= HandleRecallEnded;
            hammer.ResetRecallModifiers();
        }
        subscribed = false;
        recallActive = false;
        nextHitBonus = 0f;
    }
}
