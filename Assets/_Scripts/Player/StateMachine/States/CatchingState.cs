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
        playerContext.HandleAnimations.ChangeAnimationState("Catching");
        timer = 0f;
    }

    public override void Update()
    {
        if (playerContext.Mjolnir.IsHeld())
        {
            playerContext.HandleAnimations.ChangeAnimationState("Catch");

            timer += Time.deltaTime;
            if (timer > delay)
                stateMachine.ChangeState(stateMachine.idleState);
        }

        if (playerContext.HandleInputs.IsAttacking())
            stateMachine.ChangeState(stateMachine.attackState);
    }
}
