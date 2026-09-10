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

        if (playerContext.HandleInputs.IsAttackHeld()) { 
            stateMachine.ChangeState(stateMachine.chargingAttackState);
            return;
        }

        if (playerContext.Mjolnir.IsChargingThrow) { 
            stateMachine.ChangeState(stateMachine.startThrowingState);
            return  ;
        }

        if (!playerContext.HandleInputs.IsRunning()) { 
            stateMachine.ResetAnimations();
            return;
        }

        if (playerContext.HandleInputs.IsDashing() && playerContext.PlayerController.CanDash())
        {
            stateMachine.ChangeState(stateMachine.dashState);
            return;
        }
        if (playerContext.HandleInputs.IsChargingJump()) { 
            stateMachine.ChangeState(stateMachine.chargingJumpState);
            return;
        }

    }
}

