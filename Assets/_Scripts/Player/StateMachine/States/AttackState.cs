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
        playerContext.HandleAttack.Attack(playerContext.PlayerController.playerStats.basicMaxDamage,
               playerContext.PlayerController.playerStats.basicAttackRadius,
               playerContext.PlayerController.playerStats.basicAttackShakeDuration,
               playerContext.PlayerController.playerStats.basicAttackShakeMagnitude);

        timer = 0f;

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
        }
    }
}
