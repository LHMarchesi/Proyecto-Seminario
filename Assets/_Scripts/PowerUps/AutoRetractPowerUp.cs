using UnityEngine;

public class AutoRetractPowerUp : BasePowerUp
{
    [SerializeField] private float autoRetractForce;
    protected override void ApplyEffect()
    {
        playerContext.Mjolnir.OnHitEnemy += AutoRetractMjolnir;
    }

    void AutoRetractMjolnir(Collider enemyCollider)
    {
        if (enemyCollider == null) return;

        playerContext.Mjolnir.ForceRetract(autoRetractForce);
    }
}
