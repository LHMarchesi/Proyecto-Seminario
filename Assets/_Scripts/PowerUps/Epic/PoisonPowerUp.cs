using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonPowerUp : BasePowerUp
{
    [SerializeField] private PoisonStrikeStats stats;
    private float lastStrikeTime = -Mathf.Infinity;

    protected override void ApplyEffect()
    {
        playerContext.Mjolnir.OnHitEnemy += ApplyPoison;
        UIManager.Instance.RegisterHability("Poison", stats.IconSprite);
    }

    protected override void Upgrade()
    {
        stats.poisonDamagePerSecond += stats.upgradeDamageIncrease;
        stats.poisonDuration += stats.upgradeDurationIncrease;
    }

    private void ApplyPoison(Collider enemyCollider)
    {
        if (Time.time - lastStrikeTime < stats.cooldown) return; // cooldown check
        lastStrikeTime = Time.time;

        if (enemyCollider == null) return;

        BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
        if (enemy == null) return;

        // Inicia la corrutina del efecto de veneno en el enemigo
        enemy.StartCoroutine(ApplyPoisonEffect(enemy));

        UIManager.Instance.TriggerHabilityCooldown("Poison", stats.cooldown);
    }

    private IEnumerator ApplyPoisonEffect(BaseEnemy enemy)
    {
        float elapsed = 0f;

        while (elapsed < stats.poisonDuration)
        {
            if (enemy == null) yield break;

            enemy.TakeDamage(stats.poisonDamagePerSecond * Time.deltaTime);
            elapsed += Time.deltaTime;

            yield return null;
        }
    }
}
