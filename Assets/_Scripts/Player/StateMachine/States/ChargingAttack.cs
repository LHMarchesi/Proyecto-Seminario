using UnityEngine;

public class ChargingAttack : PlayerState
{
    private float chargeTimer;
    private float minChargeTime = .5f;
    public ChargingAttack(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }


    public override void Enter()
    {
        chargeTimer = 0f;
        //playerContext.HandleAnimations.ChangeAnimationState("Charging");
    }

    public override void Update()
    {
        //playerContext.PlayerController.ChargingHoldAttack();

        chargeTimer += Time.deltaTime;

        if (playerContext.HandleInputs.AttackRealesed())
        {
            playerContext.HandleInputs.ConsumeAttackRealesed();

            if (chargeTimer >= minChargeTime)
            {
                // Ataque cargado
                stateMachine.ChangeState(new ChargedAttack(stateMachine, playerContext));
            }
            else
            {
                stateMachine.ChangeState(stateMachine.idleState);
            }
        }
    }
}
