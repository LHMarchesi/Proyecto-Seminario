public class FallingState : PlayerState
{
    public FallingState(
        PlayerStateMachine stateMachine,
        PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
    }

    public override void Update()
    {
        // El aterrizaje tiene prioridad absoluta.
        // Después de cambiar de estado no seguimos ejecutando FallingState.
        if (playerContext.PlayerController.IsGrounded())
        {
            stateMachine.ResetAnimations();
            return;
        }

        if (playerContext.HandleInputs.IsThrowing())
        {
            stateMachine.ChangeState(stateMachine.startThrowingState);
            return;
        }

        if (playerContext.HandleInputs.IsDashing() &&
            playerContext.PlayerController.CanDash())
        {
            stateMachine.ChangeState(stateMachine.dashState);
            return;
        }

        if (playerContext.HandleInputs.IsCatching() &&
            !playerContext.Mjolnir.IsHeld())
        {
            stateMachine.ChangeState(stateMachine.catchingState);
            return;
        }

        if (playerContext.HandleInputs.TryConsumeTap() &&
            playerContext.PlayerController.HasMinimumAirHeight(
                playerContext.PlayerController.playerStats.minDistWGround))
        {
            stateMachine.ChangeState(stateMachine.fallingWithHammer);
            return;
        }
    }
}

