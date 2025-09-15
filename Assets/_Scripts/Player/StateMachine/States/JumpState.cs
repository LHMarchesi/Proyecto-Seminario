public class JumpState : PlayerState
{
    public JumpState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("Jump");
        playerContext.PlayerController.DoJump();
    }

    public override void Update()
    {
        if (playerContext.HandleInputs.IsThrowing())
            stateMachine.ChangeState(stateMachine.startThrowingState);


        if (playerContext.HandleInputs.IsCatching() && !playerContext.Mjolnir.IsHeld()) // Check for tryng Catch
            stateMachine.ChangeState(stateMachine.catchingState);
        else if(playerContext.PlayerController.IsFalling())
            stateMachine.ChangeState(stateMachine.fallingState);

        // Detectar si se puede hacer ataque en el aire
        if (playerContext.HandleInputs.IsAttacking()){

            if (playerContext.PlayerController.HasMinimumAirHeight(playerContext.PlayerController.playerStats.minDistWGround))
            {
                stateMachine.ChangeState(stateMachine.fallingWithHammer);
            }
            else // si no tiene la altura m�nima, hacer el ataque normal en el aire
            {
                stateMachine.ChangeState(stateMachine.attackState);
            }
        }
    }
}
