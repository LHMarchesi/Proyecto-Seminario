using UnityEngine;
public class CatchingState : PlayerState
{
    public CatchingState(PlayerStateMachine stateMachine, PlayerContext playerContext) : base(stateMachine, playerContext)
    {
    }

    public override void Enter()
    {
        playerContext.handleAnimations.ChangeAnimationState("Catch");
    }

    public override void Update()
    {
        if (!playerContext.handleInputs.IsCatching())
        {
           stateMachine.GoToIdleOrWalk();
        }
    }
}
