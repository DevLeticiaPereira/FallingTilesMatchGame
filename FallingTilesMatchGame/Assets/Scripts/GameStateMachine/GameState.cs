using UnityEngine;

public class GameState : BaseState
{
    protected GameManager _gameManager;
    protected StateMachine<GameState> _gameStateMachine;
    protected float _stateTimer;

    public GameState(GameManager gameManager, StateMachine<GameState> gameStateMachine)
    {
        _gameManager = gameManager;
        _gameStateMachine = gameStateMachine;
    }

    public override void Enter()
    {
    }

    public override void Update()
    {
        _stateTimer -= Time.deltaTime;
    }

    public override void Exit()
    {
    }
}