using System.Collections;
using System.Collections.Generic;

public abstract class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected HandleAnimations handleAnimations;

    public PlayerState(PlayerStateMachine stateMachine, HandleAnimations handleAnimations)
    {
        this.stateMachine = stateMachine;
        this.handleAnimations = handleAnimations;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
