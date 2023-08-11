using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseGameState : GameState
{
    public PauseGameState(GameManager gameManager, StateMachine<GameState> gameStateMachine ) : base(gameManager, gameStateMachine) { }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
