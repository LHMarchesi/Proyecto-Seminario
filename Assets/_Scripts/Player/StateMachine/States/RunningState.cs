public class RunningState : PlayerState
{
    public RunningState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("Running");
        playerContext.PlayerController.ChangeSpeed(playerContext.PlayerController.RunningSpeed);
    }

    public override void Update()
    {

        if (playerContext.HandleInputs.IsAttackHeld())
            stateMachine.ChangeState(stateMachine.chargingAttackState);

        if (playerContext.HandleInputs.IsThrowing())
            stateMachine.ChangeState(stateMachine.startThrowingState);

        if (!playerContext.HandleInputs.IsRunning())
            stateMachine.ResetAnimations();

        if (playerContext.HandleInputs.IsDashing() && playerContext.PlayerController.CanDash())
            stateMachine.ChangeState(stateMachine.dashState);

        if (playerContext.HandleInputs.IsChargingJump())
            stateMachine.ChangeState(stateMachine.chargingJumpState);

    }
}

