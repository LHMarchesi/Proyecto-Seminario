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

        playerContext.HandleAttack.Attack(playerContext.PlayerController.playerStats.basicMaxDamage,
              playerContext.PlayerController.playerStats.basicAttackRadius,
              playerContext.PlayerController.playerStats.basicAttackShakeDuration,
              playerContext.PlayerController.playerStats.basicAttackShakeMagnitude);


        if (playerContext.Mjolnir.IsHeld())
        {
            playerContext.HandleAnimations.ChangeAnimationState("2ndAttackWithHammer");
        }
        else
        {
            playerContext.HandleAnimations.ChangeAnimationState("2ndAttackWithOutHammer");
  
        }
        
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackDuration)
        {
            if (playerContext.HandleInputs.GetMoveVector2() != Vector2.zero)
                stateMachine.ChangeState(stateMachine.walkState);
            else
                stateMachine.ChangeState(stateMachine.idleState);
        }
    }
}
