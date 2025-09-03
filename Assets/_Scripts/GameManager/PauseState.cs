using UnityEngine;

public class PauseState : IGameState
{
    public void Enter()
    {
        UIManager.Instance.TogglePauseScreen(true); // Muestra la pantalla de pausa
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.ChangeState(new GameplayState());
        }
    }
    public void Exit()
    {
        UIManager.Instance.TogglePauseScreen(false); // Deja de mostrar la pantalla de pausa
    }
}


