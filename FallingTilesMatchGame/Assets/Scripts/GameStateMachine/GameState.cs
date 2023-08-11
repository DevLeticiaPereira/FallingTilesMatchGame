using UnityEngine;

public class GameState : BaseState
{
    protected StateMachine<GameState> _gameStateMachine;
    protected GameManager _gameManager;
    protected float _stateTimer;
    
    public GameState(GameManager gameManager, StateMachine<GameState> gameStateMachine)
    {
        this._gameManager = gameManager;
        this._gameStateMachine = gameStateMachine;
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
