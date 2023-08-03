using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;

public class MainMenuGameState : GameState
{
    public MainMenuGameState(GameManager gameManager, GameplayStateMachine gameplayStateMachine) : base(gameManager, gameplayStateMachine) { }

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
