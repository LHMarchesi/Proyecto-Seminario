using UnityEngine;

public class DashState : PlayerState
{
    private float dashDuration = 0.2f;
    private float dashSpeed = 80f;
    private float timer;

    public DashState(PlayerStateMachine stateMachine, PlayerContext playerContext) : base(stateMachine, playerContext) { }

    public override void Enter()
    {
        timer = 0f;
        Vector2 input = playerContext.HandleInputs.GetMoveVector2();
        Vector3 dir = playerContext.PlayerController.transform.TransformDirection(new Vector3(input.x, 0, input.y));
        if (dir == Vector3.zero) dir = playerContext.PlayerController.transform.forward;
        playerContext.PlayerController.Dash(dir.normalized, dashSpeed);
        //   playerContex.HandleAnimations.ChangeAnimationState("Dash");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= dashDuration)
            stateMachine.ResetAnimations();
    }

    public override void Exit()
    {
        playerContext.PlayerController.EndDash();
    }
}
