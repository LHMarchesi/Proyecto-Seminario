public class CatchingState : PlayerState
{
    public CatchingState(PlayerStateMachine stateMachine, HandleAnimations handleAnimations) : base(stateMachine, handleAnimations)
    {
    }

    public override void Enter()
    {
        handleAnimations.ChangeAnimationState("Catch");
    }

    public override void Update()
    {
        if (stateMachine.Inputs.IsCatching())
            stateMachine.ChangeState(stateMachine.catchingState);
    }
}
