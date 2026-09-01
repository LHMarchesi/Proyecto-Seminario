public class FallingState : PlayerState
{
    public FallingState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
    }

    public override void Update()
    {
        if (playerContext.PlayerController.IsGrounded())
        {
            stateMachine.ResetAnimations();
            return;
        }

        if (playerContext.Mjolnir.IsChargingThrow)
        {
            stateMachine.ChangeState(stateMachine.startThrowingState);
            return;
        }

        if (playerContext.HandleInputs.IsDashing() && playerContext.PlayerController.CanDash())
        {
            stateMachine.ChangeState(stateMachine.dashState);
            return;
        }

        if (playerContext.HandleInputs.IsCatching() && !playerContext.Mjolnir.IsHeld())
        { // Check for tryng Catch
            stateMachine.ChangeState(stateMachine.catchingState);
            return;
        }

        if (playerContext.HandleInputs.TryConsumeTap() &&
           playerContext.PlayerController.HasMinimumAirHeight(playerContext.PlayerController.playerStats.minDistWGround)) // altura mínima de 1.5 unidades
        {
            stateMachine.ChangeState(stateMachine.fallingWithHammer);
            return;
        }
    }
}
