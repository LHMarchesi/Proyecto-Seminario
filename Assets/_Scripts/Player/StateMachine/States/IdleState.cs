public class IdleState : PlayerState
{
    public IdleState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        if (playerContext.Mjolnir.IsHeld())
        {
            playerContext.HandleAnimations.ChangeAnimationState("Idle");
        }
        else
        {
            playerContext.HandleAnimations.ChangeAnimationState("IdleWithOutHammer");
        }
    }

    public override void Update()
    {
        stateMachine.ResetAnimations();

        if (playerContext.HandleInputs.IsAttackHeld())
            stateMachine.ChangeState(stateMachine.chargingAttackState);

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
