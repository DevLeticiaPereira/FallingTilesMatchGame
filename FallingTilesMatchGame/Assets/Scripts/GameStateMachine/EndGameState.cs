using Managers;

public class EndGameState : GameState
{
    public EndGameState(GameManager gameManager, StateMachine<GameState> gameStateMachine) : base(gameManager,
        gameStateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        string finalGameMessage = _gameManager.GameMode.GetGameEndMessage();
        UIManager.Instance.ShowConfirmPanel(finalGameMessage, () => GameManager.Instance.LoadMainMenu(),null);
    }
}