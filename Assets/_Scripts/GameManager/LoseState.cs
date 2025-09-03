using UnityEngine;

public class LoseState : IGameState
{
    public void Enter()
    {
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
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UIManager.Instance.ShowLoseScreenn(false);
    }
}


