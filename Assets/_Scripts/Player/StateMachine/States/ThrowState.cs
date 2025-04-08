public class ThrowState : PlayerState
{
    public ThrowState(PlayerStateMachine stateMachine, PlayerContext playerContext) : base(stateMachine, playerContext)
    {
    }

    public override void Enter()
    {
        playerContext.handleAnimations.ChangeAnimationState("Throw");
    }

    public override void Update()
    {
        if (playerContext.handleInputs.IsCatching())
            stateMachine.ChangeState(stateMachine.catchingState);
        else
            stateMachine.ResetAnimations();
    }
}
