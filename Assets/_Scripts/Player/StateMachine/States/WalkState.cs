using UnityEngine;

public class WalkState : PlayerState
{
    public WalkState(PlayerStateMachine stateMachine, HandleAnimations handleAnimations)
        : base(stateMachine, handleAnimations) { }

    public override void Enter()
    {
        handleAnimations.ChangeAnimationState("Walking");
    }

    public override void Update()
    {
        if (stateMachine.Inputs.GetMoveVector2() == Vector2.zero)
            stateMachine.ChangeState(stateMachine.idleState);

        if (stateMachine.Inputs.IsAttacking())
            stateMachine.ChangeState(stateMachine.attackState);
    }
}
