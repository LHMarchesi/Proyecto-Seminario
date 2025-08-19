using UnityEngine;

public class GameplayState : IGameState
{
    public void Enter() { Debug.Log("Enter gameplay");
     
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


