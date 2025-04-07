using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState currentState;

    public IdleState idleState;
    public WalkState walkState;
    public AttackState attackState;
    public SecondAttackState secondAttackState;
    public StartThrowingState startThrowingState;
    public CatchingState catchingState;

    public HandleInputs Inputs { get; private set; }

    private HandleAnimations handleAnimations;

    void Awake()
    {
        Inputs = GetComponent<HandleInputs>();
        handleAnimations = GetComponent<HandleAnimations>();

        idleState = new IdleState(this, handleAnimations);
        walkState = new WalkState(this, handleAnimations);
        attackState = new AttackState(this, handleAnimations);
        secondAttackState = new SecondAttackState(this, handleAnimations);
        startThrowingState = new StartThrowingState(this, handleAnimations);
        catchingState = new CatchingState(this, handleAnimations);
    }

    void Start()
    {
        ChangeState(idleState);
    }

    void Update()
    {
        currentState.Update();
    }

    public void ChangeState(PlayerState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }
}
