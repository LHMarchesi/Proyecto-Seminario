using UnityEngine;

public class ChargingAttack : PlayerState
{
    private float chargeTimer;
    private float minChargeTime = 0.5f;

    public ChargingAttack(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        chargeTimer = 0f;
        playerContext.HandleAnimations.ChangeAnimationState("Charging");
    }

    public override void Update()
    {

        // Mientras el botón siga presionado, acumula tiempo
        if (playerContext.HandleInputs.IsAttackHeld())
        {
            chargeTimer += Time.deltaTime;
            // opcional: feedback visual
            return;
        }

        if (playerContext.HandleInputs.TryConsumeTap())
            stateMachine.ChangeState(stateMachine.attackState);


        if (playerContext.HandleInputs.TryConsumeHoldReleased() && chargeTimer >= minChargeTime)
            stateMachine.ChangeState(stateMachine.chargedAttackState);
    }
}
