using System.Diagnostics;

public class ChargingJumpState : PlayerState
{
    public ChargingJumpState(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("ChargueJump");
    }

    public override void Update()
    {
        playerContext.PlayerController.ChargingJump();

        // --- Si deja de correr, cancelar carga o forzar salto ---
        if (!playerContext.HandleInputs.IsRunning())
        {
            // Si ya tiene una buena carga, salta
            if (playerContext.PlayerController.currentJumpCharge >= 30f)
            {
                playerContext.PlayerController.DoJump(playerContext.PlayerController.currentJumpCharge);
                playerContext.PlayerController.currentJumpCharge = 0;
                stateMachine.ChangeState(stateMachine.jumpState);
            }
            else
            {
                // Si no llegó al mínimo, cancelar la carga
                playerContext.PlayerController.currentJumpCharge = 0;
                stateMachine.ChangeState(stateMachine.idleState);
            }

            // Consumimos la señal de jumpReleased si existía
            playerContext.HandleInputs.ConsumeJumpReleased();
            return; // importante: salimos del Update
        }

        // --- Normal: mientras carga ---
        if (playerContext.HandleInputs.JumpReleased())
        {
            if (playerContext.PlayerController.currentJumpCharge >= 30f)
            {
                playerContext.PlayerController.DoJump(playerContext.PlayerController.currentJumpCharge);
                playerContext.PlayerController.currentJumpCharge = 0;
                stateMachine.ChangeState(stateMachine.jumpState);
            }
            else
            {
                playerContext.PlayerController.currentJumpCharge = 0;
                stateMachine.ChangeState(stateMachine.idleState);
            }

            playerContext.HandleInputs.ConsumeJumpReleased();
        }
    }
}
