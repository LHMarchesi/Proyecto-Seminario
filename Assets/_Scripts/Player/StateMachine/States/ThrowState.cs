public class ThrowState : PlayerState
{
    public ThrowState(PlayerStateMachine stateMachine, PlayerContext playerContext) : base(stateMachine, playerContext)
    {
    }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("Throw");
    }

    public override void Update()
    {
        if (playerContext.HandleInputs.IsCatching())
            stateMachine.ChangeState(stateMachine.catchingState);

        if (playerContext.HandleInputs.IsAttacking())
        {
            stateMachine.ChangeState(stateMachine.attackState);
        }
    }
}
