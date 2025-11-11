using UnityEngine;

public class LoseState : IGameState
{
    PlayerContext playerContext;
    public void Enter()
    {
        playerContext = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerContext>();
        playerContext.HandleInputs.SetPaused(true);
        UIManager.Instance.ShowLoseScreenn(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
       
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


