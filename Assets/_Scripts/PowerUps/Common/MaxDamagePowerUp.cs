using UnityEngine;

public class MaxDamagePowerUp : BasePowerUp
{
    [SerializeField] MaxDamageStats stats;

    protected override void ApplyEffect()
    {
        AddDaamge();
        UIManager.Instance.OnPlayerAddHealth();
    }

    private void AddDaamge()
    {
        playerContext.PlayerController.AddMaxDamage(stats.newMaxDamage);
    }
}