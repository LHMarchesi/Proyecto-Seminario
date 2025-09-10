public class ChargedAttack : PlayerState
{
    public ChargedAttack(PlayerStateMachine stateMachine, PlayerContext playerContext)
        : base(stateMachine, playerContext) { }


    public override void Enter()
    {
            playerContext.HandleAnimations.ChangeAnimationState("2ndAttackWithHammer");
    }

    public override void Update()
    {
    }
}