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
            stateMachine.ResetAnimations();

        if (playerContext.HandleInputs.IsThrowing())
            stateMachine.ChangeState(stateMachine.startThrowingState);

        if (playerContext.HandleInputs.IsCatching() && !playerContext.Mjolnir.IsHeld()) // Check for tryng Catch
            stateMachine.ChangeState(stateMachine.catchingState);

        if (playerContext.HandleInputs.IsAttacking() &&
           playerContext.PlayerController.HasMinimumAirHeight(playerContext.PlayerController.playerStats.minDistWGround)) // altura mínima de 1.5 unidades
        {
            stateMachine.ChangeState(stateMachine.fallingWithHammer);
        }
    }
}
