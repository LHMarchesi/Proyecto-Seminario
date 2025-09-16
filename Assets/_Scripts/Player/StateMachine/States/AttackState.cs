using UnityEngine;

public class AttackState : PlayerState
{
    private float attackDuration = 0.8f;
    private float comboWindow = 0.5f;
    private float timer = 0f;
    private bool queuedNextTapAttack;
    private bool queuedHoldAttack;

    public AttackState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        queuedNextTapAttack = false;
        queuedHoldAttack = false;
        timer = 0f;

        if (playerContext.Mjolnir.IsHeld())
            playerContext.HandleAnimations.ChangeAnimationState("AttackWithHammer");
        else
            playerContext.HandleAnimations.ChangeAnimationState("AttackWithOutHammer");

    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (playerContext.HandleInputs.IsChragingAttack() && timer > comboWindow && !queuedHoldAttack)
        { queuedHoldAttack = true; }

        else if (playerContext.HandleInputs.IsAttacking() && timer > comboWindow && !queuedNextTapAttack)
        {
            queuedNextTapAttack = true;
        }


        if (timer >= attackDuration)
        {
            if (queuedHoldAttack) // Jump to Charged Attack state
                stateMachine.ChangeState(stateMachine.chargingAttackState);
            else if (queuedNextTapAttack) // Jump to Second attack state
                stateMachine.ChangeState(stateMachine.secondAttackState);
            else
                stateMachine.ResetAnimations();
        }
    }
}
