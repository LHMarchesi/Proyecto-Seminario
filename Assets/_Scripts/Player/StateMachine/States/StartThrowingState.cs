using UnityEngine;
using UnityEngine.Rendering;

public class StartThrowingState : PlayerState
{
    private float chargeTime = 0f;
    private float maxChargeTime = 1.5f;
    

    public StartThrowingState(PlayerStateMachine stateMachine, PlayerContext playerContext ) : base(stateMachine, playerContext)
    {
    }

    public override void Enter()
    {
        chargeTime = 0f;
        playerContext.handleAnimations.ChangeAnimationState("ChargeThrow");
    }

    public override void Update()
    {
        // Cargando mientras el jugador mantiene el botón
        if (playerContext.Inputs.IsThrowing()  && playerContext.mjolnir.IsHeld())
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
            UIManager.Instance.powerSlider.ChangeValue(chargeTime / maxChargeTime); // Update slider from charge
        }
        // Al soltar el botón, lanzar el martillo
        else
        {
            UIManager.Instance.powerSlider.Disable(); // Disable slider
            stateMachine.GoToIdleOrWalk();
        }




        if (playerContext.Inputs.IsCatching())
            stateMachine.ChangeState(stateMachine.catchingState);
    }
}
