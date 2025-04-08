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
        playerContext.handleAnimations.ChangeAnimationState("ChargeThrow");
    }

    public override void Update()
    {
        // Cargando mientras el jugador mantiene el bot�n
        if (playerContext.handleInputs.IsThrowing() && playerContext.mjolnir.IsHeld())
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
            UIManager.Instance.powerSlider.ChangeValue(chargeTime / maxChargeTime); // Update UI slider from charge
        }
        // Al soltar el bot�n, lanzar el martillo
        else
        {
            playerContext.handleAnimations.ChangeAnimationState("Throw");
            stateMachine.ChangeState(stateMachine.throwState);
            UIManager.Instance.powerSlider.Disable(); // Disable UI 
        }
    }
  
}
