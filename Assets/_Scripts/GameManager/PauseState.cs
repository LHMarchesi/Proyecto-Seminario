using UnityEngine;
using UnityEngine.InputSystem;

public class PauseState : IGameState
{
    private PlayerContext playerContext;

    public void Enter()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag(
                "Player");

        if (player != null)
        {
            playerContext =
                player.GetComponent<PlayerContext>();
        }

        if (playerContext != null &&
            playerContext.HandleInputs != null)
        {
            playerContext.HandleInputs.SetPaused(
                true);
        }

        if (UIManager.Instance != null)
            UIManager.Instance.TogglePauseScreen(true);
        else
            Time.timeScale = 0f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentState =
                GameStates.Pause;
        }
    }

    public void Update()
    {
        // Funciona aunque PlayerInput esté desactivado
        // y aunque Time.timeScale sea 0.
        Keyboard keyboard =
            Keyboard.current;

        if (keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(
                    new GameplayState());
            }
        }
    }

    public void Exit()
    {
        if (playerContext != null &&
            playerContext.HandleInputs != null)
        {
            playerContext.HandleInputs.SetPaused(
                false);
        }

        if (UIManager.Instance != null)
            UIManager.Instance.TogglePauseScreen(false);
        else
            Time.timeScale = 1f;
    }
}
