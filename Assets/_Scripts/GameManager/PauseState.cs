using UnityEngine;

public class PauseState : IGameState
{
    public void Enter()
    {
        UIManager.Instance.TogglePauseScreen(true); // Muestra la pantalla de pausa
        Time.timeScale = 0f; // Pausa el tiempo del juego
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.SetGameState(GameStates.Game);
        }
    }
    public void Exit()
    {
        Time.timeScale = 1f; // Despausa el tiempo del juego}
        UIManager.Instance.TogglePauseScreen(false); // Deja de mostrar la pantalla de pausa
    }
}


