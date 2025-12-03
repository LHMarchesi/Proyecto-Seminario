using UnityEngine;

public class LoseState : IGameState
{
    PlayerContext playerContext;

    public void Enter()
    {
        // Register a new death
        PlayerDeathTracker.RegisterDeath();

        playerContext = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerContext>();
        playerContext.HandleInputs.SetPaused(true);
        UIManager.Instance.ShowLoseScreenn(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameManager.Instance.currentState = GameStates.Lose;
    }

    public void Update()
    {

    }

    public void Exit()
    {
        playerContext.HandleInputs.SetPaused(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UIManager.Instance.ShowLoseScreenn(false);
    }
}
