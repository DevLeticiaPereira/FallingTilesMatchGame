using System;
using Managers;
using UnityEngine;

public class StartGameState : GameState
{
    public StartGameState(GameManager gameManager, StateMachine<GameState> gameStateMachine) : base(gameManager,
        gameStateMachine)
    {
    }

    public static event Action OnGameStart;

    public override void Enter()
    {
        base.Enter();
        _stateTimer = 3;
        UIManager.Instance.LoadPanel(UIManager.PanelType.Hud);
        _gameManager.GameMode.GameStart();
    }

    public override void Update()
    {
        base.Update();

        if (_stateTimer <= 0) _gameStateMachine.ChangeState(_gameManager.RunningState);
    }

    public override void Exit()
    {
        base.Exit();
        OnGameStart?.Invoke();
    }
}