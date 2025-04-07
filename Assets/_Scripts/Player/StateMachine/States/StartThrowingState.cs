using UnityEngine;
using UnityEngine.Rendering;

public class StartThrowingState : PlayerState
{
    private float chargeTime = 0f;
    private float maxChargeTime = 3f;
    private SliderPassValue slider;

    public StartThrowingState(PlayerStateMachine stateMachine, PlayerContext playerContext, SliderPassValue slider ) : base(stateMachine, playerContext)
    {
        this.slider = slider;
    }

    public override void Enter()
    {
        chargeTime = 0f;
        slider.ChangeValue(0f); // Show Slider
        playerContext.handleAnimations.ChangeAnimationState("ChargeThrow");
    }

    public override void Update()
    {
        // Cargando mientras el jugador mantiene el botón
        if (playerContext.Inputs.IsThrowing())
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
            slider.ChangeValue(chargeTime / maxChargeTime); // Update slider from charge
        }
        // Al soltar el botón, lanzar el martillo
        else
        {
            slider.ChangeValue(0f);
            slider.Disable(); // Disable slider
            stateMachine.ChangeState(stateMachine.idleState);
        }




        if (playerContext.Inputs.IsCatching())
            stateMachine.ChangeState(stateMachine.catchingState);
    }
}
