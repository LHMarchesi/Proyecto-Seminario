using UnityEngine;

public class StartThrowingState : PlayerState
{
    private float chargeTime = 0f;
    private float maxChargeTime = 1.5f;


    public StartThrowingState(PlayerStateMachine stateMachine, PlayerContext playerContext) : base(stateMachine, playerContext)
    {
    }

    public override void Enter()
    {
        chargeTime = 0f;
        playerContext.HandleAnimations.ChangeAnimationState("ChargeThrow");
    }

    public override void Update()
    {
        // Cargando mientras el jugador mantiene el botón
        if (playerContext.HandleInputs.IsThrowing() && playerContext.Mjolnir.IsHeld())
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
            UIManager.Instance.PowerSlider.ChangeValue(chargeTime / maxChargeTime); // Update UI slider from charge
        }
        // Al soltar el botón, lanzar el martillo
        else
        {
           
            stateMachine.ChangeState(stateMachine.throwState);
            UIManager.Instance.PowerSlider.Disable(); // Disable UI 
        }
    }
  
}
