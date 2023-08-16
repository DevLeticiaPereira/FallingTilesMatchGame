using Managers;

public class MainMenuGameState : GameState
{
    public MainMenuGameState(GameManager gameManager, StateMachine<GameState> gameStateMachine) : base(gameManager,
        gameStateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        UIManager.Instance.LoadPanel(UIManager.PanelType.MainMenu);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
        UIManager.Instance.UnloadPanel(UIManager.PanelType.MainMenu);
    }
}