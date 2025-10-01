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
        Time.timeScale = 0f;
       
    }
    public void Update()
    {

    }
    public void Exit()
    {
        playerContext.HandleInputs.SetPaused(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UIManager.Instance.ShowLoseScreenn(false);
    }
}


