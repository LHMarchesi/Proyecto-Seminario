using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuraStrikePowerUp : BasePowerUp
{
    [SerializeField] private AuraStrikeStats stats;
    private Coroutine auraCoroutine;

    protected override void ApplyEffect()
    {
        // Activamos la rutina que hace daño por tic a los enemigos cercanos
        auraCoroutine = playerContext.StartCoroutine(AuraDamageRoutine());
        UIManager.Instance.RegisterHability("Aura", stats.IconSprite);
    }

    protected override void Upgrade()
    {
        // Mejora el daño y/o el radio con cada nivel
        stats.damagePerTick += stats.upgradeDamageIncrease;
        stats.range += stats.upgradeRangeIncrease;
    }

    private IEnumerator AuraDamageRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(stats.tickInterval);

        while (true)
        {
            Collider[] enemies = Physics.OverlapSphere(playerContext.transform.position, stats.range, stats.enemyLayer);
            for (int i = 0; i < enemies.Length; i++)
            {
                BaseEnemy enemy = enemies[i].GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(stats.damagePerTick);
                }
            }

            yield return wait; // Espera entre tics
        }
    }

    private void OnDisable()
    {
        if (auraCoroutine != null)
        {
            playerContext.StopCoroutine(auraCoroutine);
        }
    }
}
