using UnityEngine;

public class GameplayState : IGameState
{
    public void Enter() {
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.ChangeState(new PauseState());
        }
    }
    public void Exit() { }
}


