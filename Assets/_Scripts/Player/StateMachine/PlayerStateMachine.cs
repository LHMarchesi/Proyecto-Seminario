using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState currentState;
    private PlayerContext playerContext;

    // PLAYER STATES
    public IdleState idleState;
    public WalkState walkState;
    public AttackState attackState;
    public SecondAttackState secondAttackState;
    public StartThrowingState startThrowingState;
    public CatchingState catchingState;

   
    void Awake()
    {
        playerContext = GetComponent<PlayerContext>();

        // Initialize states
        idleState = new IdleState(this, playerContext);
        walkState = new WalkState(this, playerContext);
        attackState = new AttackState(this, playerContext);
        secondAttackState = new SecondAttackState(this, playerContext);
        startThrowingState = new StartThrowingState(this, playerContext);
        catchingState = new CatchingState(this, playerContext);
    }

    void Start()
    {
        ChangeState(idleState); // Starting State
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

    public void GoToIdleOrWalk()
    {
        if (playerContext.handleInputs.GetMoveVector2() != Vector2.zero)
            ChangeState(walkState);
        else
            ChangeState(idleState);
    }
}
