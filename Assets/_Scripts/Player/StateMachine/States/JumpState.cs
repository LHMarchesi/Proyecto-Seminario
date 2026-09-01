public class JumpState : PlayerState
{
    public JumpState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("Jump");
    }

    public override void Update()
    {
        if (playerContext.HandleInputs.IsThrowing())
        {
            stateMachine.ChangeState(stateMachine.startThrowingState);
            return;
        }

        if (playerContext.HandleInputs.IsDashing() && playerContext.PlayerController.CanDash())
        {
            stateMachine.ChangeState(stateMachine.dashState);
            return;
        }

        if (playerContext.HandleInputs.IsCatching() && !playerContext.Mjolnir.IsHeld())// Check for tryng Catch
        {
            stateMachine.ChangeState(stateMachine.catchingState);
            return;
        }
        else if (playerContext.PlayerController.IsFalling())
        {
            stateMachine.ChangeState(stateMachine.fallingState);
            return;
        }

        // Detectar si se puede hacer ataque en el aire
        if (playerContext.HandleInputs.TryConsumeTap())
        {

            if (playerContext.PlayerController.HasMinimumAirHeight(playerContext.PlayerController.playerStats.minDistWGround))
            {
                stateMachine.ChangeState(stateMachine.fallingWithHammer);
                return;
            }
            else // si no tiene la altura mínima, hacer el ataque normal en el aire
            {
                stateMachine.ChangeState(stateMachine.attackState);
                return;
            }
        }
    }
}
