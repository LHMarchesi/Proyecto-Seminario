using UnityEngine;

public class LightningStrikePowerUp : BasePowerUp
{
    [SerializeField] private GameObject lightningEffectPrefab;
    [SerializeField] private float cooldown;
    private float lastStrikeTime = -Mathf.Infinity;
    protected override void ApplyEffect()
    {
        playerContext.Mjolnir.OnHitEnemy += SpawnLightningEffect;
    }

    void SpawnLightningEffect(Collider enemyCollider)
    {
        if (Time.time - lastStrikeTime < cooldown) return;

        lastStrikeTime = Time.time;

        if (enemyCollider == null) return;

        Vector3 effectPosition = enemyCollider.bounds.center + Vector3.up * 0.5f;
        Quaternion effectRotation = Quaternion.identity;

        Instantiate(lightningEffectPrefab, effectPosition, effectRotation);
        Debug.Log(enemyCollider.name);
    }
}