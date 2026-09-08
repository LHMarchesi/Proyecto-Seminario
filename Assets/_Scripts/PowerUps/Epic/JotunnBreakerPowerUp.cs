using System.Collections.Generic;
using UnityEngine;

// Alternativa física a Thunderstruck. No modifica PlayerStats ni la animación.
public class JotunnBreakerPowerUp : BasePowerUp
{
    [Header("Lv1 - Charged knockback")]
    [SerializeField, Min(0f)] private float baseExternalKnockbackForce = 8f;
    [SerializeField, Min(0f)] private float level1ForceMultiplier = 1.40f;

    [Header("Lv2 - Charged damage")]
    [SerializeField, Min(0f)] private float level2DamageMultiplier = 1.20f;

    [Header("Lv3 - Launched enemy impact")]
    [SerializeField] private LayerMask enemyLayer = ~0;
    [SerializeField, Min(0f)] private float collisionDamageMultiplier = 0.35f;
    [SerializeField, Min(0f)] private float collisionExplosionRadius = 1.75f;
    [SerializeField, Min(1)] private int maxCollisionTargets = 8;
    [SerializeField, Min(0.05f)] private float impactWindow = 0.8f;
    [SerializeField, Min(0f)] private float impactProbeRadius = 0.65f;
    [SerializeField, Min(0f)] private float minimumTravelDistance = 0.65f;
    [SerializeField, Min(0f)] private float minimumImpactSpeed = 1.5f;
    [SerializeField] private GameObject collisionVFXPrefab;
    [SerializeField] private Vector3 collisionVFXOffset;

    [Header("Lv4 - Charged radius")]
    [SerializeField, Min(0f)] private float level4RadiusMultiplier = 1.25f;

    [Header("Lv5 - Frontal shockwave")]
    [SerializeField, Min(0f)] private float shockwaveRange = 5f;
    [SerializeField, Range(1f, 180f)] private float shockwaveHalfAngle = 55f;
    [SerializeField, Min(0f)] private float shockwaveDamageMultiplier = 0.4f;
    [SerializeField, Min(1)] private int maxShockwaveTargets = 24;
    [SerializeField] private GameObject shockwaveVFXPrefab;
    [SerializeField, Min(0f)] private float shockwaveVFXForwardOffset = 1f;
    [SerializeField] private Vector3 shockwaveVFXRotation;
    [SerializeField, Min(0f)] private float vfxLifetime = 2f;

    private HandleAttack handleAttack;
    private int level = 1;
    private bool subscribed;

    private void OnEnable()
    {
        if (IsAcquiredForRun && !subscribed)
            ApplyEffect();
    }

    protected override void ApplyEffect()
    {
        if (subscribed || playerContext == null || playerContext.HandleAttack == null)
            return;

        handleAttack = playerContext.HandleAttack;
        handleAttack.OnMeleeAttackPreparing += PrepareAttack;
        handleAttack.OnMeleeAttackResolved += HandleResolvedAttack;
        handleAttack.OnMeleeAttackExecuted += HandleExecutedAttack;
        subscribed = true;
    }

    private void PrepareAttack(MeleeAttackContext attack)
    {
        if (attack.AttackType != MeleeAttackType.Charged) return;

        // El Charged actual pasa 0. En ese caso usamos una fuerza externa
        // configurable, sin intentar modificar la reacción propia del enemigo.
        float baseForce = attack.KnockbackForce > 0f
            ? attack.KnockbackForce : baseExternalKnockbackForce;
        attack.KnockbackForce = baseForce * level1ForceMultiplier;

        if (level >= 2)
            attack.Damage *= level2DamageMultiplier;
        if (level >= 4)
            attack.Radius *= level4RadiusMultiplier;
    }

    private void HandleResolvedAttack(IReadOnlyList<MeleeHitInfo> hits)
    {
        if (hits == null || hits.Count == 0 ||
            hits[0].AttackType != MeleeAttackType.Charged) return;

        HashSet<BaseEnemy> unique = new HashSet<BaseEnemy>();
        foreach (MeleeHitInfo hit in hits)
        {
            BaseEnemy enemy = hit.Enemy;
            if (enemy == null || !unique.Add(enemy) || enemy.IsDead() ||
                !enemy.gameObject.activeInHierarchy || hit.KnockbackForce <= 0f)
                continue;

            Rigidbody body = enemy.GetComponent<Rigidbody>();
            if (body == null || body.isKinematic) continue;

            Vector3 direction = hit.HitDirection;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = playerContext.transform.forward;
                direction.y = 0f;
            }
            direction = direction.sqrMagnitude > 0.0001f
                ? direction.normalized : Vector3.forward;

            // Armamos el detector antes del impulso para registrar el movimiento.
            if (level >= 3 && collisionDamageMultiplier > 0f)
            {
                JotunnImpactMonitor monitor = enemy.GetComponent<JotunnImpactMonitor>();
                if (monitor == null)
                    monitor = enemy.gameObject.AddComponent<JotunnImpactMonitor>();
                monitor.Arm(this, enemy, body, direction,
                    hit.Damage * collisionDamageMultiplier, enemyLayer,
                    impactWindow, impactProbeRadius, minimumTravelDistance,
                    minimumImpactSpeed);
            }

            enemy.ApplyExternalKnockback(direction, hit.KnockbackForce);
        }
    }

    // Se llama a lo sumo una vez por enemigo lanzado; nunca genera otro melee hit.
    public void OnLaunchedEnemyImpact(BaseEnemy source, Vector3 position, float damage)
    {
        if (level < 3 || !IsAcquiredForRun || !isActiveAndEnabled || damage <= 0f) return;
        SpawnVFX(collisionVFXPrefab, position + collisionVFXOffset, Quaternion.identity);
        CombatAreaDamage.DealDamage(position, collisionExplosionRadius, enemyLayer,
            damage, DamageFeedbackType.Normal, 0f, 0f,
            Mathf.Max(1, maxCollisionTargets), source);
    }

    private void HandleExecutedAttack(MeleeAttackContext attack)
    {
        if (level < 5 || attack.AttackType != MeleeAttackType.Charged) return;
        Vector3 origin = attack.Origin;
        Vector3 forward = attack.Forward;
        SpawnVFX(shockwaveVFXPrefab,
            origin + forward * shockwaveVFXForwardOffset,
            Quaternion.LookRotation(forward) * Quaternion.Euler(shockwaveVFXRotation));

        float damage = attack.Damage * shockwaveDamageMultiplier;
        if (damage <= 0f || shockwaveRange <= 0f) return;

        Collider[] colliders = Physics.OverlapSphere(origin, shockwaveRange,
            enemyLayer, QueryTriggerInteraction.Ignore);
        HashSet<BaseEnemy> unique = new HashSet<BaseEnemy>();
        float minDot = Mathf.Cos(shockwaveHalfAngle * Mathf.Deg2Rad);
        int count = 0;
        foreach (Collider collider in colliders)
        {
            BaseEnemy enemy = collider.GetComponentInParent<BaseEnemy>();
            if (enemy == null || !unique.Add(enemy) || enemy.IsDead() ||
                !enemy.gameObject.activeInHierarchy) continue;

            Vector3 toEnemy = enemy.CombatVFXPosition - origin;
            if (toEnemy.sqrMagnitude > 0.0001f &&
                Vector3.Dot(forward, toEnemy.normalized) < minDot) continue;

            enemy.TakeEffectDamage(damage, DamageFeedbackType.Normal);
            if (++count >= Mathf.Max(1, maxShockwaveTargets)) break;
        }
    }

    private void SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;
        GameObject vfx = Instantiate(prefab, position, rotation);
        if (vfxLifetime > 0f) Destroy(vfx, vfxLifetime);
    }

    protected override void Upgrade()
    {
        level = Mathf.Min(5, level + 1);
    }

    private void OnDisable() { Cleanup(); }
    private void OnDestroy() { Cleanup(); }

    private void Cleanup()
    {
        if (subscribed && handleAttack != null)
        {
            handleAttack.OnMeleeAttackPreparing -= PrepareAttack;
            handleAttack.OnMeleeAttackResolved -= HandleResolvedAttack;
            handleAttack.OnMeleeAttackExecuted -= HandleExecutedAttack;
        }
        subscribed = false;
    }
}
