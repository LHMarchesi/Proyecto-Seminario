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
        if (playerContext.Inputs.GetMoveVector2() != Vector2.zero)
            stateMachine.ChangeState(stateMachine.walkState);

        if (playerContext.Inputs.IsAttacking())
            stateMachine.ChangeState(stateMachine.attackState);

        if (playerContext.Inputs.IsThrowing())
            stateMachine.ChangeState(stateMachine.startThrowingState);
    }
}
