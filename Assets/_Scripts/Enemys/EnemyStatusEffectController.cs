using System.Collections;
using UnityEngine;

public class EnemyStatusEffectController : MonoBehaviour
{
    private BaseEnemy enemy;

    // =============================
    // POISON
    // =============================

    private Coroutine poisonRoutine;
    private int poisonStacks;
    private float poisonRemainingDuration;
    private float poisonDamagePerSecond;
    private float poisonTickInterval;
    private int poisonMaxStacks;
    private GameObject poisonVFXInstance;
    private PoisonApplicationData poisonData;

    // =============================
    // ELECTRICITY
    // =============================

    private Coroutine electricityRoutine;
    private float electricityEffectUntil;
    private float stunUntil;
    private GameObject electricityVFXInstance;

    private bool ownsEnemyDisable;
    private bool enemyWasEnabledBeforeStun;

    private Animator stunnedAnimator;
    private bool animatorFrozen;
    private float previousAnimatorSpeed;

    // Fire es independiente de Poison y Electricity.
    private Coroutine fireRoutine;
    private float fireRemainingDuration;
    private float fireDamagePerSecond;
    private float fireTickInterval;
    private int fireStacks;
    private int fireMaxStacks;
    private GameObject fireVFXInstance;
    private FireApplicationData fireData;

    // El BaseEnemy actual emite OnDeath despues de devolver el objeto al pool.
    // Guardamos la explosion ANTES de que OnDisable limpie los estados.
    private bool fireDeathExplosionPending;
    private FireDeathExplosionSnapshot pendingFireExplosion;
    private static int fireExplosionDepth;

    private struct FireDeathExplosionSnapshot
    {
        public FireApplicationData Data;
        public Vector3 GameplayCenter;
        public Vector3 VisualCenter;
        public int Stacks;
    }

    public bool IsBurning => fireRemainingDuration > 0f && enemy != null && !enemy.IsDead();
    public int FireStacks => IsBurning ? fireStacks : 0;

    private void Awake()
    {
        enemy = GetComponent<BaseEnemy>();
        if (enemy != null)
            enemy.OnDeath += HandleEnemyDeath;
    }

    // =====================================================
    // POISON
    // =====================================================

    public void ApplyPoison(PoisonApplicationData data)
    {
        if (enemy == null || enemy.IsDead())
            return;

        poisonData = data;
        poisonMaxStacks = Mathf.Max(1, data.maxStacks);
        poisonDamagePerSecond = Mathf.Max(0f, data.damagePerSecond);
        poisonTickInterval = Mathf.Max(0.05f, data.tickInterval);

        poisonStacks = Mathf.Clamp(
            poisonStacks + Mathf.Max(1, data.stacksToAdd),
            1,
            poisonMaxStacks);

        poisonRemainingDuration = Mathf.Max(
            poisonRemainingDuration,
            data.duration);

        EnsurePoisonVFX(data.vfxPrefab);

        if (poisonRoutine == null)
            poisonRoutine = StartCoroutine(PoisonRoutine());
    }

    private IEnumerator PoisonRoutine()
    {
        while (poisonRemainingDuration > 0f &&
               enemy != null &&
               !enemy.IsDead())
        {
            float step = Mathf.Min(
                poisonTickInterval,
                poisonRemainingDuration);

            yield return new WaitForSeconds(step);

            if (enemy == null || enemy.IsDead())
                break;

            float damage =
                poisonDamagePerSecond * step * poisonStacks;

            bool poisonWillKill =
                damage >= enemy.CurrentHealth;

            if (poisonWillKill && poisonData.spreadOnDeath)
                SpreadPoison();

            enemy.TakeEffectDamage(damage, DamageFeedbackType.Poison);
            poisonRemainingDuration -= step;
        }

        ClearPoison();
    }

    private void SpreadPoison()
    {
        var nearbyEnemies =
            CombatTargeting.FindClosestEnemies(
                transform.position,
                poisonData.spreadRadius,
                poisonData.enemyLayer,
                poisonData.maxSpreadTargets,
                enemy);

        foreach (BaseEnemy nearbyEnemy in nearbyEnemies)
        {
            if (nearbyEnemy == null || nearbyEnemy.IsDead())
                continue;

            EnemyStatusEffectController statusController =
                nearbyEnemy.GetComponent<EnemyStatusEffectController>();

            if (statusController == null)
            {
                statusController =
                    nearbyEnemy.gameObject.AddComponent<EnemyStatusEffectController>();
            }

            PoisonApplicationData spreadData = poisonData;
            spreadData.stacksToAdd = 1;

            statusController.ApplyPoison(spreadData);
        }
    }

    private void EnsurePoisonVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab == null || poisonVFXInstance != null)
            return;

        Transform anchor = GetVFXAnchor();

        poisonVFXInstance = Instantiate(
            vfxPrefab,
            anchor.position,
            Quaternion.identity,
            anchor);

        poisonVFXInstance.transform.localPosition = Vector3.zero;
    }

    private void ClearPoison()
    {
        poisonRoutine = null;
        poisonStacks = 0;
        poisonRemainingDuration = 0f;

        if (poisonVFXInstance != null)
        {
            Destroy(poisonVFXInstance);
            poisonVFXInstance = null;
        }
    }

    // =====================================================
    // FIRE
    // =====================================================

    public void ApplyFire(FireApplicationData data)
    {
        if (enemy == null || enemy.IsDead() || data.duration <= 0f)
            return;

        fireData = data;
        fireMaxStacks = Mathf.Max(1, data.maxStacks);
        fireDamagePerSecond = Mathf.Max(0f, data.damagePerSecond);
        fireTickInterval = Mathf.Max(0.05f, data.tickInterval);
        fireStacks = Mathf.Clamp(fireStacks + Mathf.Max(1, data.stacksToAdd), 1, fireMaxStacks);
        fireRemainingDuration = Mathf.Max(fireRemainingDuration, data.duration);

        EnsureFireVFX(data.vfxPrefab, data.vfxLocalOffset);
        if (fireRoutine == null)
            fireRoutine = StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        while (fireRemainingDuration > 0f && enemy != null && !enemy.IsDead())
        {
            float step = Mathf.Min(fireTickInterval, fireRemainingDuration);
            yield return new WaitForSeconds(step);
            if (enemy == null || enemy.IsDead())
                break;

            float damage = fireDamagePerSecond * step * fireStacks;
            // El tiempo se descuenta despues del daño para que una muerte
            // en el ultimo tick tambien pueda activar el remate de Lv5.
            if (damage > 0f)
                enemy.TakeEffectDamage(damage, DamageFeedbackType.Fire);
            if (enemy == null || enemy.IsDead())
                break;
            fireRemainingDuration -= step;
        }
        ClearFire();
    }

    private void EnsureFireVFX(GameObject prefab, Vector3 localOffset)
    {
        if (prefab == null) return;
        Transform anchor = GetVFXAnchor();
        if (fireVFXInstance == null)
            fireVFXInstance = Instantiate(prefab, anchor.position, Quaternion.identity, anchor);
        fireVFXInstance.transform.localPosition = localOffset;
        fireVFXInstance.transform.localRotation = Quaternion.identity;
    }

    private void ClearFire()
    {
        fireRoutine = null;
        fireStacks = 0;
        fireMaxStacks = 0;
        fireRemainingDuration = 0f;
        fireDamagePerSecond = 0f;
        fireTickInterval = 0f;
        fireData = default(FireApplicationData);
        if (fireVFXInstance != null)
        {
            Destroy(fireVFXInstance);
            fireVFXInstance = null;
        }
    }

    private void PrepareFireDeathExplosion()
    {
        if (fireDeathExplosionPending || fireExplosionDepth > 0 ||
            enemy == null || !enemy.IsDead() ||
            fireRemainingDuration <= 0f || fireStacks <= 0 ||
            !fireData.explodeOnDeath)
            return;

        pendingFireExplosion = new FireDeathExplosionSnapshot
        {
            Data = fireData,
            GameplayCenter = enemy.transform.position,
            VisualCenter = enemy.CombatVFXPosition,
            Stacks = fireStacks
        };
        fireDeathExplosionPending = true;
    }

    private void HandleEnemyDeath()
    {
        // Tambien funciona si en el futuro OnDeath se mueve antes del pooling.
        PrepareFireDeathExplosion();
        if (!fireDeathExplosionPending)
            return;

        FireDeathExplosionSnapshot snapshot = pendingFireExplosion;
        fireDeathExplosionPending = false;
        pendingFireExplosion = default(FireDeathExplosionSnapshot);
        ExplodeFromFire(snapshot);
    }

    private void ExplodeFromFire(FireDeathExplosionSnapshot snapshot)
    {
        FireApplicationData data = snapshot.Data;
        Vector3 visualPosition = snapshot.VisualCenter + data.explosionVFXOffset;

        if (data.explosionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(data.explosionVFXPrefab, visualPosition, Quaternion.identity);
            if (data.explosionVFXLifetime > 0f)
                Destroy(vfx, data.explosionVFXLifetime);
        }

        // La explosion no aplica Burn ni genera otra explosion en cadena.
        // Esto evita cascadas de decenas de detonaciones en una horda.
        float damage = Mathf.Max(0f, data.explosionDamagePerStack) * snapshot.Stacks;
        if (damage <= 0f || data.explosionRadius <= 0f)
            return;

        fireExplosionDepth++;
        try
        {
            CombatAreaDamage.DealDamage(snapshot.GameplayCenter, data.explosionRadius,
                data.enemyLayer, damage, DamageFeedbackType.Fire,
                0f, 0f, Mathf.Max(1, data.maxExplosionTargets), enemy);
        }
        finally
        {
            fireExplosionDepth--;
        }
    }

    // =====================================================
    // ELECTRICITY
    // =====================================================

    public void ApplyElectricity(ElectricityApplicationData data)
    {
        if (enemy == null || enemy.IsDead())
            return;

        float now = Time.time;

        stunUntil = Mathf.Max(
            stunUntil,
            now + Mathf.Max(0f, data.stunDuration));

        electricityEffectUntil = Mathf.Max(
            electricityEffectUntil,
            now + Mathf.Max(0f, data.effectDuration));

        EnsureElectricityVFX(
            data.vfxPrefab,
            data.vfxLocalOffset);

        if (data.stunDuration > 0f)
            BeginStun();

        if (electricityRoutine == null)
        {
            electricityRoutine =
                StartCoroutine(ElectricityRoutine());
        }
    }

    private IEnumerator ElectricityRoutine()
    {
        while (enemy != null &&
               !enemy.IsDead() &&
               (Time.time < electricityEffectUntil ||
                Time.time < stunUntil))
        {
            if (ownsEnemyDisable && Time.time >= stunUntil)
                EndStun();

            yield return null;
        }

        EndStun();
        ClearElectricityVFX();

        electricityEffectUntil = 0f;
        stunUntil = 0f;
        electricityRoutine = null;
    }

    private void BeginStun()
    {
        if (enemy == null || enemy.IsDead())
            return;

        if (!ownsEnemyDisable)
        {
            enemyWasEnabledBeforeStun = enemy.enabled;

            if (enemyWasEnabledBeforeStun)
            {
                enemy.enabled = false;
                ownsEnemyDisable = true;
            }
        }

        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();

        if (enemyRb != null && !enemyRb.isKinematic)
        {
            Vector3 velocity = enemyRb.velocity;
            velocity.x = 0f;
            velocity.z = 0f;
            enemyRb.velocity = velocity;

            enemyRb.angularVelocity = Vector3.zero;
        }

        if (!animatorFrozen)
        {
            stunnedAnimator = enemy.GetComponentInChildren<Animator>();

            if (stunnedAnimator != null)
            {
                previousAnimatorSpeed = stunnedAnimator.speed;
                stunnedAnimator.speed = 0f;
                animatorFrozen = true;
            }
        }
    }

    private void EndStun()
    {
        if (animatorFrozen && stunnedAnimator != null)
        {
            stunnedAnimator.speed = previousAnimatorSpeed;
        }

        animatorFrozen = false;
        stunnedAnimator = null;

        if (ownsEnemyDisable && enemy != null)
        {
            // Sólo reactivamos el comportamiento si nosotros lo desactivamos.
            enemy.enabled = enemyWasEnabledBeforeStun;
        }

        ownsEnemyDisable = false;
    }

    private void EnsureElectricityVFX(
        GameObject vfxPrefab,
        Vector3 localOffset)
    {
        if (vfxPrefab == null)
            return;

        Transform anchor = GetVFXAnchor();

        if (electricityVFXInstance == null)
        {
            electricityVFXInstance = Instantiate(
                vfxPrefab,
                anchor.position,
                Quaternion.identity,
                anchor);
        }

        electricityVFXInstance.transform.localPosition = localOffset;
        electricityVFXInstance.transform.localRotation = Quaternion.identity;
    }

    private void ClearElectricityVFX()
    {
        if (electricityVFXInstance != null)
        {
            Destroy(electricityVFXInstance);
            electricityVFXInstance = null;
        }
    }

    private Transform GetVFXAnchor()
    {
        if (enemy != null && enemy.CombatVFXAnchor != null)
            return enemy.CombatVFXAnchor;

        return transform;
    }

    private void OnDisable()
    {
        PrepareFireDeathExplosion();
        if (fireRoutine != null) StopCoroutine(fireRoutine);
        ClearFire();

        // Poison
        if (poisonRoutine != null)
        {
            StopCoroutine(poisonRoutine);
            poisonRoutine = null;
        }

        poisonStacks = 0;
        poisonRemainingDuration = 0f;

        if (poisonVFXInstance != null)
        {
            Destroy(poisonVFXInstance);
            poisonVFXInstance = null;
        }

        // Electricity
        if (electricityRoutine != null)
        {
            StopCoroutine(electricityRoutine);
            electricityRoutine = null;
        }

        EndStun();
        ClearElectricityVFX();

        electricityEffectUntil = 0f;
        stunUntil = 0f;
    }
    private void OnDestroy()
    {
        if (enemy != null)
            enemy.OnDeath -= HandleEnemyDeath;
        fireDeathExplosionPending = false;
    }

}

[System.Serializable]
public struct PoisonApplicationData
{
    public float damagePerSecond;
    public float duration;
    public float tickInterval;
    public int stacksToAdd;
    public int maxStacks;
    public GameObject vfxPrefab;

    public bool spreadOnDeath;
    public float spreadRadius;
    public int maxSpreadTargets;
    public LayerMask enemyLayer;
}

[System.Serializable]
public struct ElectricityApplicationData
{
    [Min(0f)] public float stunDuration;
    [Min(0f)] public float effectDuration;
    public GameObject vfxPrefab;
    public Vector3 vfxLocalOffset;
}
