using System;
using UnityEngine;

public class ParryPowerUp : BasePowerUp, IMjolnirRetractBehavior
{
    private float parryWindow = 5f;
    PlayerContext context;
    protected override void ApplyEffect()
    {

    }
    void Start()
    {
        context = FindObjectOfType<PlayerContext>();
        Console.WriteLine(context);
        context.Mjolnir.RegisterRetractBehavior(this);
    }
    public void OnRetract(Mjolnir mjolnir)
    {
        if (!context.Mjolnir.IsHeld() && context.HandleInputs.TryConsumeTap())
        {
            float distanceFromHand = mjolnir.DistanceFromHand();
            if (distanceFromHand < parryWindow)
            {
                ExecuteParry(mjolnir);
            }
        }
    }

    private void ExecuteParry(Mjolnir mjolnir)
    {
        mjolnir.ThrowWithPower(100f);
        context.Mjolnir.ShowMessage("Parry execute");

        // VFX, sonido, etc.
        // AudioSource.PlayClipAtPoint(parrySFX, mjolnir.transform.position);
        // Instantiate(parryVFX, mjolnir.transform.position, Quaternion.identity);
    }

    protected override void Upgrade()
    {
    }
}