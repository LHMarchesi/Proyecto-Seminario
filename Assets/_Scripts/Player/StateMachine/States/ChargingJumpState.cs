public class ChargingJumpState : PlayerState
{
    public ChargingJumpState(
        PlayerStateMachine stateMachine,
        PlayerContext playerContext)
        : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        playerContext.HandleAnimations.ChangeAnimationState("ChargueJump");

        playerContext.PlayerController.currentJumpCharge =
            playerContext.PlayerController.playerStats.minJumpForce;

        // Nunca heredar un release anterior.
        playerContext.HandleInputs.ConsumeJumpReleased();
    }

    public override void Update()
    {
        playerContext.PlayerController.ChargingJump();

        bool releasedJump =
            playerContext.HandleInputs.JumpReleased();

        bool releasedRun =
            !playerContext.HandleInputs.IsRunning();

        if (!releasedJump && !releasedRun)
            return;

        float force =
            playerContext.PlayerController.currentJumpCharge;

        // Limpiamos el input ANTES de cambiar de estado.
        // Esto también cubre soltar Shift antes que Space.
        playerContext.HandleInputs.FinishChargedJumpInput();

        playerContext.PlayerController.DoJump(force);

        stateMachine.ChangeState(stateMachine.jumpState);
    }

    public override void Exit()
    {
        // Si otro sistema interrumpe la carga tampoco queda ningún flag vivo.
        playerContext.HandleInputs.FinishChargedJumpInput();
        playerContext.PlayerController.StopChargingJump();
    }
}
