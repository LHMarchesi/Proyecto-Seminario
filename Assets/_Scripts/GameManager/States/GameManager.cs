
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStates
{
    MainMenu, Pause, SkillChoose, Game
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private IGameState currentState;
    private Dictionary<GameStates, IGameState> states;

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

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0: // Main Menu Scene
                currentState = states[GameStates.Game];
                break;
            case 1: // Gameplay Scene
                //currentState = states[GameStates.Game];
                break;
        }
    }
    private void Update()
    {
        currentState?.Update();
    }

    public void SetGameState(GameStates newState)
    {
        // Salida del estado actual
        if (currentState != null)
            currentState.Exit();

        currentState = states[newState];
        currentState.Enter();
    }
    public IGameState GetCurrentState() => currentState;
}

public interface IGameState
{
    void Enter();
    void Update(); 
    void Exit();
}


