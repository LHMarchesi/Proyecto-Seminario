using UnityEngine;

public class MaxJumpForcePowerUp : BasePowerUp
{
    [SerializeField] MaxJumpForceStats stats;

    protected override void ApplyEffect()
    {
        AddJumpForce();
        UIManager.Instance.OnPlayerAddHealth();
    }

    private void AddJumpForce()
    {
        playerContext.PlayerController.AddMaxJumpForce(stats.newMaxJumpForce);
    }
}