using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerStateMachine stateMachine, HandleAnimations handleAnimations)
        : base(stateMachine, handleAnimations) { }

    public override void Enter()
    {
        handleAnimations.ChangeAnimationState("Idle");
    }

    public override void Update()
    {
        if (stateMachine.Inputs.GetMoveVector2() != Vector2.zero)
            stateMachine.ChangeState(stateMachine.walkState);

        if (stateMachine.Inputs.IsAttacking())
            stateMachine.ChangeState(stateMachine.attackState);

        if (stateMachine.Inputs.IsThrowing())
            stateMachine.ChangeState(stateMachine.startThrowingState);
    }
}
