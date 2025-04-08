using System.Collections;
using System.Collections.Generic;

public abstract class PlayerState  // Base Abstract Player State
{
    protected PlayerStateMachine stateMachine;
    protected PlayerContext playerContext;

    public PlayerState(PlayerStateMachine stateMachine, PlayerContext playerContext)
    {
        this.stateMachine = stateMachine;
        this.playerContext = playerContext;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
