using UnityEngine;
public class CatchingState : PlayerState
{
    public CatchingState(PlayerStateMachine stateMachine, PlayerContext playerContext) : base(stateMachine, playerContext)
    {
    }

    float timer = 0f;
    float delay = .5f;

    public override void Enter()
    {
        playerContext.handleAnimations.ChangeAnimationState("Catching");
        timer = 0f;
    }

    public override void Update()
    {
        if (playerContext.mjolnir.IsHeld())
        {
            playerContext.handleAnimations.ChangeAnimationState("Catch");

            timer += Time.deltaTime;
            if (timer > delay)
                stateMachine.ChangeState(stateMachine.idleState);
        }
    }
}
