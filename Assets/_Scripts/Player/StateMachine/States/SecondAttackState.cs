using UnityEngine;

public class SecondAttackState : PlayerState
{
    private float attackDuration = 0.6f;
    private float timer = 0f;
    private bool queuedNextAttack;

    public SecondAttackState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }


    public override void Enter()
    {
        playerContext.handleAnimations.ChangeAnimationState("Attack2");
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackDuration)
        {
            if (playerContext.Inputs.GetMoveVector2() != Vector2.zero)
                stateMachine.ChangeState(stateMachine.walkState);
            else
                stateMachine.ChangeState(stateMachine.idleState);
        }
    }
}