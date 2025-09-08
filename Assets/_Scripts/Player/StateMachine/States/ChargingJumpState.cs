public class ChargingJumpState : PlayerState
{
    public ChargingJumpState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        //    playerContext.HandleAnimations.ChangeAnimationState("");
        //  playerContext.PlayerController.ChangeSpeed(playerContext.PlayerController.WalkingSpeed);
    }

    public override void Update()
    {
        playerContext.PlayerController.ChargingJump();

        // Cuando suelta, cambiamos al estado de Jump
        if (playerContext.HandleInputs.JumpReleased())
        {
            playerContext.HandleInputs.ConsumeJumpReleased();
            stateMachine.ChangeState(stateMachine.jumpState);
        }
    }
}
