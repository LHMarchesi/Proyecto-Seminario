using Unity.VisualScripting;
using UnityEngine;

public class ReSizePowerUp : BasePowerUp
{
    [SerializeField] private float maxSize;
    [SerializeField] private float cooldown;

    private Sprite IconSprite;
    private float sizeForce = 1;
    private float lastStrikeTime = -Mathf.Infinity;

    protected override void ApplyEffect()
    {
        // Suscripción al evento solo cuando ya se recogió el power-up y se tiene el contexto
        playerContext.Mjolnir.OnChrgingThrow += ResizeMjolnir;

        UIManager.Instance.RegisterHability("Resize", IconSprite);
    }

    private void ResizeMjolnir()
    {
        if (Time.time - lastStrikeTime < cooldown)
            return;

        UIManager.Instance.TriggerHabilityCooldown("Resize", cooldown);

        float chargeTime = playerContext.Mjolnir.GetChargeTime();
        float scaleMultiplier = 1f + (chargeTime * sizeForce);
        scaleMultiplier = Mathf.Clamp(scaleMultiplier, 1f, maxSize);

        Vector3 newScale = playerContext.Mjolnir.GetOriginalSize() * scaleMultiplier;
        playerContext.Mjolnir.transform.localScale = newScale;
    }
}
