
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStates
{
    MainMenu, Pause, Transition, Game
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]private IGameState currentState;
    private Dictionary<GameStates, IGameState> states;

    private int pendingFromLevel = -1;
    private int pendingToLevel = -1;
    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(this); }
        else { Destroy(gameObject); return; }

        // Inicializa la fábrica de estados
        states = new Dictionary<GameStates, IGameState>()
        {
            { GameStates.MainMenu,     new MainMenuState() },
            { GameStates.Game,         new GameplayState() },
            { GameStates.Pause,        new PauseState() }
        };
    }

    public void SetGameState(GameStates newState, int levelParam = -1)
    {
        // Salida del estado actual
        if (currentState != null)
            currentState.Exit();

        // Estado siguiente
        if (newState == GameStates.Transition)
        {
            // GUARDA ORIGEN y DESTINO
            pendingFromLevel = SceneManager.GetActiveScene().buildIndex;
            pendingToLevel = levelParam;
            currentState = new TransitionState(pendingFromLevel, pendingToLevel);
        }
        else
        {
            currentState = states[newState];
        }

        currentState.Enter();
    }
    public IGameState GetCurrentState() => currentState;
    public (int from, int to) GetPendingTransition() => (pendingFromLevel, pendingToLevel);
}

public interface IGameState
{
    void Enter();
    void Exit();
}

public class MainMenuState : IGameState
{
    public void Enter() { }
    public void Exit() {  }
}

public class GameplayState : IGameState
{
    public void Enter() {  }
    public void Exit() { }
}

public class PauseState : IGameState
{
    public void Enter() {  }
    public void Exit() { }
}

public class TransitionState : IGameState
{
    public int FromLevel { get; }
    public int ToLevel { get; }

    public TransitionState(int from, int to)
    {
        FromLevel = from;
        ToLevel = to;
    }

    public void Enter()
    {
        // Carga la escena de transición
        SceneManager.LoadScene("TransitionScene");
    }

    public void Exit()
    {

    }

    // Este método lo llamará TransitionController al terminar la animación:
    public void OnTransitionComplete()
    {
        GameManager.Instance.SetGameState(GameStates.Game);
        // Carga la escena del nivel:
        SceneManager.LoadScene(ToLevel);
    }
}

