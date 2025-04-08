using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.handleAnimations.ChangeAnimationState("Idle");
    }

    public override void Update()
    {
        stateMachine.ResetAnimations();

        if (playerContext.handleInputs.IsAttacking())
            stateMachine.ChangeState(stateMachine.attackState);

        if (playerContext.handleInputs.IsThrowing())
            stateMachine.ChangeState(stateMachine.startThrowingState);
    }
}
