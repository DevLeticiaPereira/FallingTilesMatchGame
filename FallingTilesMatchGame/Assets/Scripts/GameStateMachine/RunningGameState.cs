public class RunningGameState : GameState
{
    public RunningGameState(GameManager gameManager, GameplayStateMachine gameplayStateMachine) : base(gameManager, gameplayStateMachine) { }

    public override void Enter()
    {
        base.Enter();
        InputManager.Instance.EnablePlayerInput(true);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
        InputManager.Instance.EnablePlayerInput(false);
    }
}
