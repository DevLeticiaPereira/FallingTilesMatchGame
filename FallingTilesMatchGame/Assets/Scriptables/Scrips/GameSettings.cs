using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms.Impl;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    [Header("general Settings")]
    [SerializeField] private float _endGameScreenTime = 3.0f;

    [Header("Grid Setup")] 
    [SerializeField] private string _endGameWinMessage; 
    [SerializeField] private string _endGameLostMessage;
    [SerializeField] private string _exitGameConfirmMessage;
    [SerializeField] private string _singlePlayerHighScoreMessage;
    [SerializeField] private string _singlePlayerStandardMessage;
    
    [Header("Grid Setup")]
    [SerializeField] private GridSetupData _gridSetupData;
    [SerializeField] private int _minNumberOfTilesToMatch = 4;
                                                
    [Header("Tiles")]       
    [SerializeField] private List<TileData> _tilesData;
    [SerializeField] private float _defaultTileFallSpeed = 2.0f;
    [SerializeField] private float _boostedTileFallSpeed = 4.0f;
    [SerializeField] private float _singleTileFallSpeed = 6.0f;
    [SerializeField] private float moveTimeBetweenColumns = 0.1f;
    [SerializeField] private float _endGameTilesFallingSpeed = 10.0f;
    [SerializeField] private float _droppingMoveDuration = 0.5f;
    
    [Header("Scores")]
    [SerializeField] private ScoreData _scoreData;

    public List<TileData> TilesData => _tilesData;
    public ScoreData ScoreData => _scoreData;
    public float EndGameScreenTime => _endGameScreenTime;
    public float SingleTileFallSpeed => _singleTileFallSpeed;
    public float DroppingMoveDuration => _droppingMoveDuration;
    public GridSetupData GridSetupData => _gridSetupData;
    public float DefaultTileFallSpeed => _defaultTileFallSpeed;
    public float BoostedTileFallSpeed => _boostedTileFallSpeed;
    public float MoveTimeBetweenColumns => moveTimeBetweenColumns;
    public int MinNumberOfTilesToMatch => _minNumberOfTilesToMatch;
    public float EndGameTilesFallingSpeed => _endGameTilesFallingSpeed;
    public string EndGameWinMessage => _endGameWinMessage;
    public string EndGameLostMessage => _endGameLostMessage;
    public string ExitGameConfirmMessage => _exitGameConfirmMessage;
    public string SinglePlayerHighScoreMessage => _singlePlayerHighScoreMessage;
    public string SinglePlayerStandardMessage => _singlePlayerStandardMessage;
}
