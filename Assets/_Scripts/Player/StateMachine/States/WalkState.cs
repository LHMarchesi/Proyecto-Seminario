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
        stateMachine.ResetAnimations();

        if (playerContext.handleInputs.IsAttacking())
            stateMachine.ChangeState(stateMachine.attackState);
    }
}
