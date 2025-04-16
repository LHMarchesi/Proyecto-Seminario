using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState currentState;
    private PlayerContext playerContext;

    // PLAYER STATES
    public IdleState idleState;
    public WalkState walkState;
    public ThrowState throwState;
    public AttackState attackState;
    public SecondAttackState secondAttackState;
    public StartThrowingState startThrowingState;
    public CatchingState catchingState;
    public RunningState runningState;


    void Awake()
    {
        playerContext = GetComponent<PlayerContext>();

        // Initialize states
        idleState = new IdleState(this, playerContext);
        walkState = new WalkState(this, playerContext);
        runningState = new RunningState(this, playerContext);
        throwState = new ThrowState(this, playerContext);
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
        UIManager.Instance.stateText.text = "PlayerState : " + newState.ToString();
        currentState.Enter();
    }

    public void ResetAnimations()
    {
        if (playerContext.HandleInputs.IsCatching() && !playerContext.Mjolnir.IsHeld()) // Check for tryng Catch
        {
            ChangeState(catchingState);
        }
        else if (playerContext.HandleInputs.GetMoveVector2() != Vector2.zero) // Check for player movement
        {
            ChangeState(walkState);
        }else
            ChangeState(idleState);
    }
}
