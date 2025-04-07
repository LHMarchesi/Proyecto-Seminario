using UnityEngine;

public class AttackState : PlayerState
{
    private float attackDuration = 0.6f;
    private float timer = 0f;
    private bool queuedNextAttack;

    public AttackState(PlayerStateMachine stateMachine, HandleAnimations handleAnimations)
        : base(stateMachine, handleAnimations) { }

    public override void Enter()
    {
        handleAnimations.ChangeAnimationState("Attack1");
        queuedNextAttack = false;
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        // Permitir encadenar combo si presiona ataque nuevamente
        if (stateMachine.Inputs.IsAttacking() && timer > 0.3f && !queuedNextAttack)
        {
            queuedNextAttack = true;
        }

        if (timer >= attackDuration)
        {
            if (queuedNextAttack)
                stateMachine.ChangeState(stateMachine.secondAttackState);
            else
                GoToIdleOrWalk();
        }
    }

    private void GoToIdleOrWalk()
    {
        if (stateMachine.Inputs.GetMoveVector2() != Vector2.zero)
            stateMachine.ChangeState(stateMachine.walkState);
        else
            stateMachine.ChangeState(stateMachine.idleState);
    }
}
