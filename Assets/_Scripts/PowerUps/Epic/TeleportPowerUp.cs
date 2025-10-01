using UnityEngine;

public class TeleportPowerUp : BasePowerUp
{
    [SerializeField] private TeleportPowerUpStats stats; 
    private float lastStrikeTime = -Mathf.Infinity;

    protected override void ApplyEffect()
    {
        playerContext.Mjolnir.OnMjolnirRetract += TeleportToHammer;
        UIManager.Instance.RegisterHability("Teleport", stats.IconSprite);
    }

    private void TeleportToHammer()
    {
        if (Time.time - lastStrikeTime < stats.cooldown)
            return;

        lastStrikeTime = Time.time;

        //audio.PlayOneShot((AudioClip)Resources.Load("teleportVFX"));
        playerContext.PlayerController.transform.position = playerContext.Mjolnir.transform.position;
        playerContext.Mjolnir.Catch();

        UIManager.Instance.TriggerHabilityCooldown("Lightning", stats.cooldown);
    }

    protected override void Upgrade()
    {
        stats.cooldown = Mathf.Max(0.1f, stats.cooldown - 0.2f);
    }
}
