using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScoreManager : MonoBehaviour
{
    [SerializeField] private ScoreData _scoreData;
    
    private int _totalScore = 0;
    //private int _comboMultiplier = 1;
    //private int _chainMultiplier = 1;

    private bool _IsAiControlled = false;

    private void Awake()
    {
        //GridManager
    }

    public void SetIsAiControlled(bool isAI)
    {
        _IsAiControlled = isAI;
    }

    public void AddScoreToGrid(int tilesPopped)
    {
        _totalScore = tilesPopped * _scoreData.BaseTileScore;
        
        if (!_IsAiControlled)
        {
            GameManager.Instance.UpdatePlayerScore(_totalScore);
        }
    }

}
