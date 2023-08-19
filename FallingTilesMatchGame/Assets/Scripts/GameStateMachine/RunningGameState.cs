using System;
using System.Collections.Generic;

public class RunningGameState : GameState
{
    private Dictionary<Guid, int> GridScoreMap = new();
    public RunningGameState(GameManager gameManager, StateMachine<GameState> gameStateMachine) : base(gameManager,
        gameStateMachine)
    {
    }

    public static event Action OnGameStartRunning;
    public static event Action OnGameStopRunning;

    public override void Enter()
    {
        base.Enter();
        OnGameStartRunning?.Invoke();
        EventManager.EventScore += OnScore;
        EventManager.EventGridGameOver += OnGridGameOver;
    }

    private void OnScore(Guid gridID, int newScore)
    {
        _gameManager.GridScoreMap[gridID] = newScore;
        _gameManager.GameMode.HandleScore(newScore);
    }

    private void OnGridGameOver(Guid gridID)
    {
        _gameManager.GameMode.HandleGridGameOver(gridID);
        if (_gameManager.GameMode.IsGameOver) 
        {
            _gameManager.StateMachine.ChangeState(_gameManager.EndState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        OnGameStopRunning?.Invoke();
        EventManager.EventScore -= OnScore;
        EventManager.EventGridGameOver -= OnGridGameOver;
    }
}