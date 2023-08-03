public class StartGameState : GameState
{
    public StartGameState(GameManager gameManager, GameplayStateMachine gameplayStateMachine) : base(gameManager, gameplayStateMachine) { }

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
        
        //Todo: remove temporary code - change from start to running
        if (_stateTimer <= 0)
        {
            _gameplayStateMachine.ChangeState(_gameManager.RunningState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
