using UnityEngine;

public class MainMenuState : IGameState
{
    public void Enter() {
        SoundManager.Instance.PlayMusic(SoundManager.Instance.mainMenuMusic, true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void Exit()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Update() { }
}


