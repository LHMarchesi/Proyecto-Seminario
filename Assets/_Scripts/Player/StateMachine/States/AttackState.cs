using UnityEngine;

public class AttackState : PlayerState
{
    private float attackDuration = 0.6f;
    private float timer = 0f;
    private bool queuedNextAttack;

    public AttackState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.handleAnimations.ChangeAnimationState("Attack1");
        queuedNextAttack = false;
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        // Allow combo if you press attack again
        if (playerContext.handleInputs.IsAttacking() && timer > 0.3f && !queuedNextAttack)
        {
            queuedNextAttack = true;
        }

        if (timer >= attackDuration)
        {
            if (queuedNextAttack) // Jump to Second attack state
                stateMachine.ChangeState(stateMachine.secondAttackState);
            else
                stateMachine.ResetAnimations();
        }
    }
}
