using UnityEngine;

public class PauseState : IGameState
{
    PlayerContext playerContext;
    public void Enter()
    {
        UIManager.Instance.TogglePauseScreen(true); // Muestra la pantalla de pausa
        playerContext = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerContext>();
        playerContext.HandleInputs.SetPaused(true);
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
        playerContext.HandleInputs.SetPaused(false);
        UIManager.Instance.TogglePauseScreen(false); // Deja de mostrar la pantalla de pausa
    }
}


