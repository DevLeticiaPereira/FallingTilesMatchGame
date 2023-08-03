using System;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : Singleton<GameManager>
{
	[SerializeField] private string _mainMenuSceneName;
	[SerializeField] private string _gameSceneSuffix;
	[SerializeField] private string _exitGameConfirmMessage = "Do you really want to exit the game?";
	private int _numberOfPlayer;
	
	#region States
	public GameplayStateMachine StateMachine { get; private set; }
	public MainMenuGameState MenuState { get; private set; }
	public StartGameState StartState { get; private set; }
	public RunningGameState RunningState { get; private set; }
	public EndGameState EndState { get; private set; }
	public PauseGameState PauseState { get; private set; }
	
	#endregion
	
	protected override void Awake()
	{
		base.Awake();

		StateMachine = new GameplayStateMachine();

		MenuState = new MainMenuGameState(this, StateMachine);
		StartState = new StartGameState(this, StateMachine);
		RunningState = new RunningGameState(this, StateMachine);
		EndState = new EndGameState(this, StateMachine);
		PauseState = new PauseGameState(this, StateMachine);
		
		StateMachine.Initialize(MenuState);
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Update()
	{
		StateMachine.CurrenState.Update();
	}
	
	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (scene.name.Contains(_gameSceneSuffix))
		{
			StateMachine.ChangeState(StartState);
		}
		else if(scene.name.Contains(_mainMenuSceneName))
		{
			StateMachine.ChangeState(MenuState);
		}
	}
	
	public void ExitGame()
	{
		bool success = UIManager.Instance.ShowConfirmPanel(_exitGameConfirmMessage, 
			() => GameManager.Instance.LoadMainMenu(), 
			() =>GameManager.Instance.PauseGame(false));
		if (success)
		{
			StateMachine.ChangeState(PauseState);
		}
	}

	public void PauseGame(bool pause)
	{
		// only toggle pause panel if change state was a success
		if (pause && StateMachine.ChangeState(PauseState))
		{
			UIManager.Instance.LoadPanel(UIManager.PanelType.Pause);
		}
		else if(!pause && StateMachine.ChangeState(RunningState))
		{
			UIManager.Instance.UnloadPanel(UIManager.PanelType.Pause);
		}
	}
	
	public void LoadMainMenu()
	{
		if (!Application.CanStreamedLevelBeLoaded(_mainMenuSceneName))
		{
			Debug.LogError($"Scene {_mainMenuSceneName} cannot be loaded.");
			return;
		}
		UIManager.Instance.UnloadAll();
		SceneManager.LoadScene(_mainMenuSceneName);
	}
	
	public void LoadGameScene(int numberOfPlayers)
	{
		string sceneName = numberOfPlayers.ToString() + _gameSceneSuffix;
		if (!Application.CanStreamedLevelBeLoaded(sceneName))
		{
			Debug.LogError($"Scene {sceneName} cannot be loaded.");
			return;
		};
		
		UIManager.Instance.UnloadAll();
		_numberOfPlayer = numberOfPlayers;
		SceneManager.LoadScene(sceneName);
	}
}
