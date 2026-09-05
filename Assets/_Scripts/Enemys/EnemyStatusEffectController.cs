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

    private void Awake()
    {
        enemy = GetComponent<BaseEnemy>();
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
