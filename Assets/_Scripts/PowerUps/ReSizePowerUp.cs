using UnityEngine;

public class ReSizePowerUp : BasePowerUp
{
    private float sizeForce = 1;
    [SerializeField] private float maxSize;

    protected override void ApplyEffect()
    {
        // Suscripción al evento solo cuando ya se recogió el power-up y se tiene el contexto
        playerContext.Mjolnir.OnChrgingThrow += ResizeMjolnir;
    }

    private void ResizeMjolnir()
    {
        float chargeTime = playerContext.Mjolnir.GetChargeTime();
        float scaleMultiplier = 1f + (chargeTime * sizeForce);
        scaleMultiplier = Mathf.Clamp(scaleMultiplier, 1f, maxSize);

        Vector3 newScale = playerContext.Mjolnir.GetOriginalSize() * scaleMultiplier;
        playerContext.Mjolnir.transform.localScale = newScale;
    }
}
