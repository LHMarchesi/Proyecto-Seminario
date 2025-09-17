using UnityEngine;

public class ChargedAttack : PlayerState
{
    private float timer;
    private float attackDuration = 1f;
   

    public ChargedAttack(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }


    public override void Enter()
    {
        // Al entrar en ataque cargado, aseguramos que no siga marcado como "charging"
        playerContext.HandleInputs.ResetAttackFlags();

        playerContext.HandleAnimations.ChangeAnimationState("CharguedMelee");
        timer = 0f;

        playerContext.HandleAttack.Attack(
            playerContext.PlayerController.playerStats.chargedMaxDamage,
            playerContext.PlayerController.playerStats.chargedAttackRadius,
            playerContext.PlayerController.playerStats.chargedAttackShakeDuration,
            playerContext.PlayerController.playerStats.chargedAttackShakeMagnitude
        );
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackDuration)
        {
            stateMachine.ResetAnimations();
            stateMachine.ChangeState(stateMachine.idleState);
        }

    }
}