public class PauseGameState : GameState
{
    public PauseGameState(GameManager gameManager, StateMachine<GameState> gameStateMachine) : base(gameManager,
        gameStateMachine)
    {
    }
}