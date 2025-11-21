using UnityEngine;
using System;

public enum GameState
{
    Battle,
    Exploration,
    Victory,
    Failure
}

public class GameStateController
{
    public GameState CurrentState { get; private set; }

    public event Action<GameState, GameState> OnStateEntered;
    public event Action<GameState, GameState> OnStateExited;

    public void ChangeState(GameState newState)
    {
        GameState previousState = CurrentState;
        if (CurrentState == newState) return;
        OnStateExited?.Invoke(previousState, newState);
        CurrentState = newState;
        OnStateEntered?.Invoke(previousState, newState);
    }

}
