using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayState : IGameState
{
    public void Enter()
    {
        Time.timeScale = 1f;

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentState =
                GameStates.Game;
        }
    }

    public void Update()
    {
        // El panel de Level Up pausa con Time.timeScale = 0.
        // No permitimos abrir Pause encima del selector de skills.
        if (Time.timeScale <= 0f)
            return;

        Keyboard keyboard =
            Keyboard.current;

        if (keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(
                    new PauseState());
            }
        }
    }

    public void Exit()
    {
    }
}
