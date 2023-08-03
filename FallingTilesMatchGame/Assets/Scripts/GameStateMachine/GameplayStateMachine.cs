using System;
using UnityEngine;

public class GameplayStateMachine 
{
    public GameState CurrenState { get; private set; }
    public static event Action<GameState> OnGameStateChanged;
    
    public void Initialize(GameState startState)
    {
        CurrenState = startState;
        CurrenState.Enter();
    }

    public bool ChangeState(GameState newState)
    {
        if(newState == CurrenState)
            return false;
       
        CurrenState.Exit();
        CurrenState = newState;
        CurrenState.Enter();
        Debug.Log("New State "+ newState);
        OnGameStateChanged?.Invoke(CurrenState);
        return true;
    }
}
