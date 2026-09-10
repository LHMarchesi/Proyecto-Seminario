using UnityEngine;

public class SecondAttackState : PlayerState
{
    private float attackDuration = 0.6f;
    private float timer = 0f;

    public SecondAttackState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }


    public override void Enter()
    {
        timer = 0f;

        playerContext.HandleAttack.Attack(playerContext.PlayerController.playerStats.secondMaxDamage,
              playerContext.PlayerController.playerStats.secondAttackRadius,
              playerContext.PlayerController.playerStats.secondAttackShakeDuration,
              playerContext.PlayerController.playerStats.secondAttackShakeMagnitude,
              playerContext.PlayerController.playerStats.secondAttackKickPitch,
              playerContext.PlayerController.playerStats.secondAttackKickYaw,
              playerContext.PlayerController.playerStats.secondHitStopDuration, MeleeAttackType.Attack2);


        if (playerContext.Mjolnir.IsHeld())
        {
            playerContext.HandleAnimations.ChangeAnimationState("2ndAttackWithHammer");
            return;
        }
        else
        {
            playerContext.HandleAnimations.ChangeAnimationState("2ndAttackWithOutHammer");
            return;
        }

    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackDuration)
        {
            if (playerContext.HandleInputs.GetMoveVector2() != Vector2.zero)
            {
                stateMachine.ChangeState(stateMachine.walkState);
                return;
            }
            else
            {
                stateMachine.ChangeState(stateMachine.idleState);
                return;
            }
        }
    }
}
