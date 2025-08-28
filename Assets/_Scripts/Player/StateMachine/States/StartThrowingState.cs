using UnityEngine;

public class StartThrowingState : PlayerState
{
    private float chargeTime = 0f;
    private float maxChargeTime = 1.5f;
    private bool playerSpeedReduced;
    private float originalPlayerSpeed;

    public StartThrowingState(PlayerStateMachine stateMachine, PlayerContext playerContext) : base(stateMachine, playerContext)
    {
    }

    public override void Enter()
    {
        chargeTime = 0f;
        playerContext.HandleAnimations.ChangeAnimationState("ChargeThrow");
        ReducePlayerSpeed();
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
            if (playerSpeedReduced)
            {
                playerContext.PlayerController.ChangeSpeed(originalPlayerSpeed); // Restaurar la original
                playerSpeedReduced = false;
            }

            stateMachine.ChangeState(stateMachine.throwState);
            UIManager.Instance.PowerSlider.Disable(); // Disable UI 
        }
    }

    private void ReducePlayerSpeed()
    {
        if (!playerSpeedReduced)
        {
            originalPlayerSpeed = playerContext.PlayerController.currentSpeed; // Guardamos la velocidad real
            playerContext.PlayerController.ChangeSpeed(originalPlayerSpeed / 2); // Reducimos
            playerSpeedReduced = true;
        }
    }
}
