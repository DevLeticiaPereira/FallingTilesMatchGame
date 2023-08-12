using System;
using System.Collections;
using System.Collections.Generic;
using Grid;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GridScoreManager : MonoBehaviour
{
    [SerializeField] private ScoreData _scoreData;
    [SerializeField] private TMP_Text _gridScoreText;
    [SerializeField] private GridManager _gridManager;
    
    private int _totalScore = 0;
    //private int _comboMultiplier = 1;
    //private int _chainMultiplier = 1;

    private void Awake()
    {
        _gridScoreText.text = "0";
    }

    public void AddScoreToGrid(int tilesPopped)
    {
        _totalScore += tilesPopped * _scoreData.BaseTileScore;
        _gridScoreText.text = _totalScore.ToString();
        EventManager.InvokeEventScore(_gridManager.GridID, _totalScore);
    }

}
