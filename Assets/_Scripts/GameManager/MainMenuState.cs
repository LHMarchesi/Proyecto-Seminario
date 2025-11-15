using UnityEngine;

public class MainMenuState : IGameState
{
    public void Enter()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        GameManager.Instance.currentState = GameStates.MainMenu;
    }
    public void Exit()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Update()
    {
        Cursor.visible = true;
    }
}


