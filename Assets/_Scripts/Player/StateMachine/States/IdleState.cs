using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("Idle");
    }

    public override void Update()
    {
        stateMachine.ResetAnimations();

        if (playerContext.HandleInputs.IsAttacking())
            stateMachine.ChangeState(stateMachine.attackState);

        if (playerContext.HandleInputs.IsThrowing())
            stateMachine.ChangeState(stateMachine.startThrowingState);

        if (playerContext.HandleInputs.IsDashing() && playerContext.PlayerController.CanDash())
            stateMachine.ChangeState(stateMachine.dashState);
    }
}

