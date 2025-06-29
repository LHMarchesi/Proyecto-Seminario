using UnityEngine;

public class MaxSpeedPowerUp : BasePowerUp
{
    [SerializeField] MaxSpeedStats stats;

    protected override void ApplyEffect()
    {
        AddSpeed();
        UIManager.Instance.OnPlayerAddHealth();
    }

    private void AddSpeed()
    {
        playerContext.PlayerController.AddSpeed(stats.newMaxSpeed);
    }
}
