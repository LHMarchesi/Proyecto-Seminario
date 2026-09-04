using System.Collections;
using UnityEngine;

public class EnemyStatusEffectController : MonoBehaviour
{
    private BaseEnemy enemy;

    private Coroutine poisonRoutine;
    private int poisonStacks;
    private float poisonRemainingDuration;
    private float poisonDamagePerSecond;
    private float poisonTickInterval;
    private int poisonMaxStacks;
    private GameObject poisonVFXInstance;
    private PoisonApplicationData poisonData;

    private void Awake()
    {
        enemy = GetComponent<BaseEnemy>();
    }

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

        // Cada nueva aplicación refresca la duración.
        poisonRemainingDuration = Mathf.Max(poisonRemainingDuration, data.duration);

        EnsurePoisonVFX(data.vfxPrefab);

        if (poisonRoutine == null)
            poisonRoutine = StartCoroutine(PoisonRoutine());
    }

    private IEnumerator PoisonRoutine()
    {
        while (poisonRemainingDuration > 0f && enemy != null && !enemy.IsDead())
        {
            float step = Mathf.Min(poisonTickInterval, poisonRemainingDuration);
            yield return new WaitForSeconds(step);

            if (enemy == null || enemy.IsDead())
                break;

            float damage = poisonDamagePerSecond * step * poisonStacks;

            // Si este tick va a matar al objetivo, propagamos ANTES de que el
            // enemigo se desactive por pooling/muerte.
            bool poisonWillKill = damage >= enemy.CurrentHealth;

            if (poisonWillKill && poisonData.spreadOnDeath)
                SpreadPoison();

            enemy.TakeEffectDamage(damage);
            poisonRemainingDuration -= step;
        }

        ClearPoison();
    }

    private void SpreadPoison()
    {
        var nearbyEnemies = CombatTargeting.FindClosestEnemies(
            transform.position,
            poisonData.spreadRadius,
            poisonData.enemyLayer,
            poisonData.maxSpreadTargets,
            enemy);

        foreach (BaseEnemy nearbyEnemy in nearbyEnemies)
        {
            EnemyStatusEffectController statusController =
                nearbyEnemy.GetComponent<EnemyStatusEffectController>();

            if (statusController == null)
                statusController = nearbyEnemy.gameObject.AddComponent<EnemyStatusEffectController>();

            PoisonApplicationData spreadData = poisonData;
            spreadData.stacksToAdd = 1;

            statusController.ApplyPoison(spreadData);
        }
    }

    private void EnsurePoisonVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab == null || poisonVFXInstance != null)
            return;

        poisonVFXInstance = Instantiate(vfxPrefab, transform);
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

    private void OnDisable()
    {
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
