using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonPowerUp : BasePowerUp
{
    [SerializeField] private PoisonStrikeStats stats;

    private float lastStrikeTime = -Mathf.Infinity;

    private void Awake()
    {
        stats = CreateRuntimeStatsCopy(stats);
    }

    protected override void ApplyEffect()
    {
        if (playerContext == null || playerContext.Mjolnir == null)
            return;

        playerContext.Mjolnir.OnHitEnemy += ApplyPoison;
    }

    protected override void Upgrade()
    {
        if (stats == null)
            return;

        stats.poisonDamagePerSecond += stats.upgradeDamageIncrease;
        stats.poisonDuration += stats.upgradeDurationIncrease;
    }

    private void ApplyPoison(Collider enemyCollider)
    {
        if (stats == null || enemyCollider == null)
            return;

        if (Time.time - lastStrikeTime < stats.cooldown)
            return;

        lastStrikeTime = Time.time;

        BaseEnemy enemy = enemyCollider.GetComponentInParent<BaseEnemy>();
        if (enemy == null)
            return;

        enemy.StartCoroutine(ApplyPoisonEffect(enemy));

        if (UIManager.Instance != null)
            UIManager.Instance.TriggerHabilityCooldown("Poison", stats.cooldown);
    }

    private IEnumerator ApplyPoisonEffect(BaseEnemy enemy)
    {
        float elapsed = 0f;

        while (elapsed < stats.poisonDuration)
        {
            if (enemy == null)
                yield break;

            enemy.TakeDamage(stats.poisonDamagePerSecond * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (playerContext != null && playerContext.Mjolnir != null)
            playerContext.Mjolnir.OnHitEnemy -= ApplyPoison;
    }
}

