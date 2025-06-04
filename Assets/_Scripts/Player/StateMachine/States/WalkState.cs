using UnityEngine;

public class WalkState : PlayerState
{
    public WalkState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("Walking");
        playerContext.PlayerController.ChangeSpeed(playerContext.PlayerController.WalkingSpeed);
    }

    public override void Update()
    {
        stateMachine.ResetAnimations();

        if (playerContext.HandleInputs.IsAttacking())
            stateMachine.ChangeState(stateMachine.attackState);

        if (playerContext.HandleInputs.IsRunning())
            stateMachine.ChangeState(stateMachine.runningState);

        if (playerContext.HandleInputs.IsThrowing())
            stateMachine.ChangeState(stateMachine.startThrowingState);
    }
}
