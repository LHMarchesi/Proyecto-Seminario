using UnityEngine;

public class GameplayState : IGameState
{
    public void Enter() { 
                SoundManager.Instance.PlayMusic(SoundManager.Instance.levelMusic, true);
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


