using System;

public class RunningGameState : GameState
{
    public RunningGameState(GameManager gameManager, StateMachine<GameState> gameStateMachine) : base(gameManager, gameStateMachine) { }
    public static event Action OnGameStartRunning;
    public static event Action OnGameStopRunning;
        
    public override void Enter()
    {
        base.Enter();
        OnGameStartRunning?.Invoke();
        EventManager.EventGridGameOver += OnGridGameOver;
    }

    private void OnGridGameOver(Guid gridID)
    {
        if (_gameManager.NumberOfPlayers == 1)
        {
            _gameManager.StateMachine.ChangeState(_gameManager.EndState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        OnGameStopRunning?.Invoke();
        EventManager.EventGridGameOver -= OnGridGameOver;
    }
}
