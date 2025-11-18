using UnityEngine;

public class WinState : IGameState
{
    PlayerContext playerContext;
    public void Enter()
    {
        playerContext = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerContext>();
        playerContext.HandleInputs.SetPaused(true);
        UIManager.Instance.ShowWinScreenn(true);
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
        UIManager.Instance.ShowWinScreenn(false);
    }
}


