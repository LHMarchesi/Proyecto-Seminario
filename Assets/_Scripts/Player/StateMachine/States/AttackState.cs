using UnityEngine;

public class AttackState : PlayerState
{
    private float attackDuration = 0.8f;
    private float timer = 0f;
    private bool queuedNextAttack;

    public AttackState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        timer = 0f;
        queuedNextAttack = false;

        playerContext.HandleAttack.Attack(playerContext.PlayerController.playerStats.basicMaxDamage,
               playerContext.PlayerController.playerStats.basicAttackRadius,
               playerContext.PlayerController.playerStats.basicAttackShakeDuration,
               playerContext.PlayerController.playerStats.basicAttackShakeMagnitude,
               playerContext.PlayerController.playerStats.basicAttackKickPitch,
               playerContext.PlayerController.playerStats.basicAttackKickYaw,
               playerContext.PlayerController.playerStats.basicHitStopDuration);

        if (playerContext.Mjolnir.IsHeld())
        {
            playerContext.HandleAnimations.ChangeAnimationState("AttackWithHammer");
        }
        else
        {
            playerContext.HandleAnimations.ChangeAnimationState("AttackWithOutHammer");
        }

    }

    public override void Update()
    {
        timer += Time.deltaTime;


        if (timer >= attackDuration)
        {
            stateMachine.ChangeState(stateMachine.idleState);
            return;
        }
        else if (!queuedNextAttack && playerContext.HandleInputs.TryConsumeTap() && timer > 0.4f)
        {
            queuedNextAttack = true;
        }

        if (queuedNextAttack) { 
            stateMachine.ChangeState(stateMachine.secondAttackState);
            return;
        }
    }
}
