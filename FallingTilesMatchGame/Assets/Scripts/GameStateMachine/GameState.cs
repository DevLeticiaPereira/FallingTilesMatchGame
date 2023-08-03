using UnityEngine;

public class GameState
{
    protected GameplayStateMachine _gameplayStateMachine;
    protected GameManager _gameManager;
    protected float _stateTimer;
    
    public GameState(GameManager gameManager, GameplayStateMachine gameplayStateMachine)
    {
        this._gameManager = gameManager;
        this._gameplayStateMachine = gameplayStateMachine;
    }

    public virtual void Enter()
    {
        
    }

    public virtual void Update()
    {
        _stateTimer -= Time.deltaTime;
    }

    public virtual void Exit()
    {
        
    }
}
