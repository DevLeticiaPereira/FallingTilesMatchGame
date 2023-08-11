using System;

public class StartGameState : GameState
{
    public StartGameState(GameManager gameManager, StateMachine<GameState> gameStateMachine) : base(gameManager, gameStateMachine) { }
    public static event Action OnGameStart;
    public override void Enter()
    {
        base.Enter();
        //Todo: remove temporary code - change from start to running
        _stateTimer = 3;
        Managers.UIManager.Instance.LoadPanel(Managers.UIManager.PanelType.Hud);
    }

    public override void Update()
    {
        base.Update();
        
        if (_stateTimer <= 0)
        {
            _gameStateMachine.ChangeState(_gameManager.RunningState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        OnGameStart?.Invoke();
    }
}
