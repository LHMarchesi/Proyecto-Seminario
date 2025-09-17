using UnityEngine;

public class ChargingAttack : PlayerState
{
    private float chargeTimer;
    private float minChargeTime = 0.5f;
    private bool queuedNextAttack;

    public ChargingAttack(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        chargeTimer = 0f;
        queuedNextAttack = false;
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

        if (!queuedNextAttack && playerContext.HandleInputs.TryConsumeTap() && chargeTimer > 0.2f)
            queuedNextAttack = true;

        if (queuedNextAttack)
            stateMachine.ChangeState(stateMachine.secondAttackState);

        if (playerContext.HandleInputs.TryConsumeHoldReleased() && chargeTimer >= minChargeTime)
            stateMachine.ChangeState(stateMachine.chargedAttackState);
    }
}
