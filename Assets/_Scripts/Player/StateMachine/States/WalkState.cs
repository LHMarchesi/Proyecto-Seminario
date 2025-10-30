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

        if (playerContext.HandleInputs.IsAttackHeld())
            stateMachine.ChangeState(stateMachine.chargingAttackState);

        if (playerContext.HandleInputs.IsRunning())
            stateMachine.ChangeState(stateMachine.runningState);

        if (playerContext.HandleInputs.IsThrowing())
            stateMachine.ChangeState(stateMachine.startThrowingState);

        if (playerContext.HandleInputs.IsDashing() && playerContext.PlayerController.CanDash())
            stateMachine.ChangeState(stateMachine.dashState);

        if (playerContext.HandleInputs.IsJumping())
        {
            playerContext.PlayerController.DoJump(playerContext.PlayerController.playerStats.minJumpForce);
            stateMachine.ChangeState(stateMachine.jumpState);
        }
    }
}
