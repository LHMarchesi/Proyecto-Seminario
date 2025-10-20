using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// A flexible UI button that can be configured to perform various actions
/// like changing scenes, quitting the game, or resuming gameplay.
/// </summary>

/// <summary>
/// Enum representing the different actions a UI button can perform.
/// </summary>
public enum ButtonAction
{
    ChangeState, // Load a scene and change game state
    RestartGame, // Load a scene and change game state
    QuitGame,    // Exit the application
    Resume       // Resume gameplay from pause
}


public class FlexibleUIButton : MonoBehaviour
{
    [SerializeField] private ButtonAction actionType;
    [SerializeField] private string parameter;  // Can be a scene name, state name, etc.
    [SerializeField] private AudioClip OnClickSfx;  // Can be a scene name, state name, etc.

    private Button button;
    private void Start()
    {
        button = GetComponent<Button>();

        // Add PerformAction as a listener to the button click event
        button.onClick.AddListener(PerformAction);
        button.interactable = true;     // Ensure the button is clickable at start
    }

    /// <summary>
    /// Performs an action based on the selected ButtonAction and parameter.
    /// </summary>
    private void PerformAction()
    {
        if (OnClickSfx != null)
            SoundManager.Instance.PlaySFX(OnClickSfx);

        StartCoroutine(DelayedAction());
    }

    private IEnumerator DelayedAction()
    {
        yield return new WaitForSecondsRealtime(0.1f); // Espera para que suene el SFX (ajustable)

        switch (actionType)
        {
            case ButtonAction.ChangeState:
                switch (parameter)
                {
                    case "MainMenu":
                        TransitionManager.Instance.PlayTransitionAndLoadScene(TransitionType.FadeIn, 0);
                        GameManager.Instance.ChangeState(new MainMenuState());
                        break;

                    case "Game":
                        TransitionManager.Instance.PlayTransitionAndLoadScene(TransitionType.FadeIn, 1);
                        GameManager.Instance.ChangeState(new GameplayState());
                        break;

                    case "Credits":
                        TransitionManager.Instance.PlayTransitionAndLoadScene(TransitionType.FadeIn, 2);
                        break;
                }
                break;

            case ButtonAction.QuitGame:
                Application.Quit();
                break;

            case ButtonAction.Resume:
                Time.timeScale = 1;
                GameManager.Instance.ChangeState(new GameplayState());
                break;

            case ButtonAction.RestartGame:
                TransitionManager.Instance.PlayTransitionAndLoadScene(TransitionType.FadeIn, SceneManager.GetActiveScene().buildIndex);
                Time.timeScale = 1;
                GameManager.Instance.ChangeState(new GameplayState());
                break;
        }

        button.interactable = false;
    }
}
