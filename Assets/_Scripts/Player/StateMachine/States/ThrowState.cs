public class ThrowState : PlayerState
{
    public ThrowState(PlayerStateMachine stateMachine, PlayerContext playerContext) : base(stateMachine, playerContext)
    {
    }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("Throw", false, 0.05f);
    }

    public override void Update()
    {
        if (playerContext.HandleInputs.IsCatching()) { 
            stateMachine.ChangeState(stateMachine.catchingState);
            return;
        }

        if (playerContext.HandleInputs.TryConsumeTap())
        {
            stateMachine.ChangeState(stateMachine.attackState);
            return;
        }
    }
}
