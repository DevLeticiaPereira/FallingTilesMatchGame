public class EndGameState : GameState
{
    private readonly float _endGameScreenTime = 3.0f;
    public EndGameState(GameManager gameManager, StateMachine<GameState> gameStateMachine) : base(gameManager, gameStateMachine) { }

    public override void Enter()
    {
        base.Enter();
        _stateTimer = _endGameScreenTime;
    }

    public override void Update()
    {
        base.Update();
        if (_stateTimer <= 0)
        {
            _gameManager.LoadMainMenu();
        }
    }

    public override void Exit()
    {
        
    }
}
