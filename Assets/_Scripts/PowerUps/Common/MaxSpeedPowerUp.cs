using UnityEngine;

public class MaxSpeedPowerUp : BasePowerUp
{
    [SerializeField] MaxSpeedStats stats;

    protected override void ApplyEffect()
    {
        AddSpeed();
        UIManager.Instance.OnPlayerAddHealth();
    }

    protected override void Upgrade()
    {
    }

    private void AddSpeed()
    {
        playerContext.PlayerController.ChangeSpeed( playerContext.PlayerController.RunningSpeed + stats.newMaxSpeed);
        playerContext.PlayerController.ChangeSpeed( playerContext.PlayerController.WalkingSpeed + stats.newMaxSpeed);
    }
}
