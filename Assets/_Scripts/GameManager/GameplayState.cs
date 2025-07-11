using UnityEngine;

public class GameplayState : IGameState
{
    public void Enter() { Debug.Log("Enter gameplay");
     
    }
    public void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.SetGameState(GameStates.Pause);
        }
    }
    public void Exit() { }

}


