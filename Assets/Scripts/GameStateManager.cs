using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    GameOver,
    Victory,
    Paused
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Playing;
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Time.timeScale = 1f;
    }

    public void TriggerGameOver()
    {
        if (State != GameState.Playing) return;
        SetState(GameState.GameOver);
    }

    public void TriggerVictory()
    {
        if (State != GameState.Playing) return;
        SetState(GameState.Victory);
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;
        SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (State == GameState.Playing) return;
        SetState(GameState.Playing);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
        State = GameState.Playing;
        OnStateChanged?.Invoke(State);
    }

    private void SetState(GameState newState)
    {
        if (State == newState) return;
        State = newState;

        // Freeze time for non-playing states
        if (newState == GameState.Playing)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;

        OnStateChanged?.Invoke(State);
    }
}

