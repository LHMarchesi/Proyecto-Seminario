using UnityEngine;

public class MaxJumpForcePowerUp : BasePowerUp
{
    [SerializeField] MaxJumpForceStats stats;

    protected override void ApplyEffect()
    {
        AddJumpForce();
        UIManager.Instance.OnPlayerAddHealth();
    }

    protected override void Upgrade()
    {
    }

    private void AddJumpForce()
    {
        playerContext.PlayerController.AddMaxJumpForce(stats.newMaxJumpForce);
    }
}