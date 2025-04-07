using UnityEngine;

public class WalkState : PlayerState
{
    public WalkState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.handleAnimations.ChangeAnimationState("Walking");
    }

    public override void Update()
    {
        if (playerContext.Inputs.GetMoveVector2() == Vector2.zero)
            stateMachine.ChangeState(stateMachine.idleState);

        if (playerContext.Inputs.IsAttacking())
            stateMachine.ChangeState(stateMachine.attackState);
    }
}
