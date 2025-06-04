using System;
using UnityEngine;

public class LightningStrikePowerUp : BasePowerUp
{
    [SerializeField] private GameObject lightningEffectPrefab; // Prefab (Efecto)
    [SerializeField] private float cooldown;
    [SerializeField] private float verticalOffset;

    private float lastStrikeTime = -Mathf.Infinity;
    protected override void ApplyEffect()
    {
        // Se subscribe al evento en el Mjolnir que detecta cuando colisiona con un enemigo
        playerContext.Mjolnir.OnHitEnemy += SpawnLightningEffect;
    }

    void SpawnLightningEffect(Collider enemyCollider)
    {
        if (Time.time - lastStrikeTime < cooldown) return;  // cooldown check

        lastStrikeTime = Time.time;

        if (enemyCollider == null) return;

        Vector3 effectPosition = enemyCollider.bounds.center + Vector3.up * verticalOffset; // Lo posiciona en el centro
        Quaternion effectRotation = Quaternion.identity;

        Instantiate(lightningEffectPrefab, effectPosition, effectRotation); // Lo instancia
        Debug.Log(enemyCollider.name);
    }
}
