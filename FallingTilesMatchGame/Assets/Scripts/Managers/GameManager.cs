using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameSettings _gameSettings;
    [SerializeField] private string _mainMenuSceneName;
    [SerializeField] private string _gameSceneSuffix;
    [SerializeField] private string _exitGameConfirmMessage = "Do you really want to exit the game?";

    //todo: make a game rules scriptable and take this out of here
    //[SerializeField] private int _minNumberOfTilesToMatch = 4;

    private readonly Dictionary<Guid, int> _gridScoreMap = new();
    public Guid PlayerGrid { get; private set; }

    public int NumberOfPlayers { get; private set; }

    //public int MinNumberOfTilesToMatch => _minNumberOfTilesToMatch;
    public GameSettings GameSettings => _gameSettings;

    #region Event Subscription

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (StateMachine.CurrentState == null) return;

        if (scene.name.Contains(_gameSceneSuffix))
            StateMachine.ChangeState(StartState);
        else if (scene.name.Contains(_mainMenuSceneName)) StateMachine.ChangeState(MenuState);
    }

    #endregion

    #region Private Functions

    public void LoadMainMenu()
    {
        if (!Application.CanStreamedLevelBeLoaded(_mainMenuSceneName))
        {
            Debug.LogError($"Scene {_mainMenuSceneName} cannot be loaded.");
            return;
        }

        EventManager.InvokeExitedGameplayScene();
        UIManager.Instance.UnloadAll();
        _gridScoreMap.Clear();
        PlayerGrid = Guid.NewGuid();
        SceneManager.LoadScene(_mainMenuSceneName);
    }

    #endregion

    #region States

    public StateMachine<GameState> StateMachine { get; private set; }
    public MainMenuGameState MenuState { get; private set; }
    public StartGameState StartState { get; private set; }
    public RunningGameState RunningState { get; private set; }
    public EndGameState EndState { get; private set; }
    public PauseGameState PauseState { get; private set; }

    #endregion

    #region Monobehavior Funtions

    protected override void Awake()
    {
        base.Awake();

        StateMachine = new StateMachine<GameState>();

        MenuState = new MainMenuGameState(this, StateMachine);
        StartState = new StartGameState(this, StateMachine);
        RunningState = new RunningGameState(this, StateMachine);
        EndState = new EndGameState(this, StateMachine);
        PauseState = new PauseGameState(this, StateMachine);
    }

    private void Start()
    {
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
        StateMachine.CurrentState.Update();
    }

    #endregion

    #region Public Functions

    public void ExitGame()
    {
        var success = UIManager.Instance.ShowConfirmPanel(_exitGameConfirmMessage,
            () => Instance.LoadMainMenu(),
            () => Instance.PauseGame(false));
        if (success) StateMachine.ChangeState(PauseState);
    }

    public void PauseGame(bool pause)
    {
        // only toggle pause panel if change state was a success
        if (pause && StateMachine.ChangeState(PauseState))
            UIManager.Instance.LoadPanel(UIManager.PanelType.Pause);
        else if (!pause && StateMachine.ChangeState(RunningState))
            UIManager.Instance.UnloadPanel(UIManager.PanelType.Pause);
    }

    public void LoadGameScene(int numberOfPlayers)
    {
        var sceneName = numberOfPlayers + _gameSceneSuffix;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene {sceneName} cannot be loaded.");
            return;
        }

        ;

        UIManager.Instance.UnloadAll();
        NumberOfPlayers = numberOfPlayers;
        SceneManager.LoadScene(sceneName);
    }

    public void SignUpGridToGame(Guid gridID, bool isAiControlled)
    {
        if (!isAiControlled) PlayerGrid = gridID;
        _gridScoreMap[gridID] = 0;
    }

    public void UpdateGridScore(Guid gridId, int score)
    {
        _gridScoreMap[gridId] = score;
    }

    public void GameEnd()
    {
        StateMachine.ChangeState(EndState);
    }

    #endregion
}