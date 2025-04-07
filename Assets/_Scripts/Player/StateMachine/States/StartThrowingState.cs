using UnityEngine.Rendering;

public class StartThrowingState : PlayerState
{
    public StartThrowingState(PlayerStateMachine stateMachine, HandleAnimations handleAnimations) : base(stateMachine, handleAnimations)
    {
    }

    public override void Enter()
    {
        handleAnimations.ChangeAnimationState("ChargeThrow");
    }

    public override void Update()
    {
        if (stateMachine.Inputs.IsCatching())
            stateMachine.ChangeState(stateMachine.catchingState);
    }
}
