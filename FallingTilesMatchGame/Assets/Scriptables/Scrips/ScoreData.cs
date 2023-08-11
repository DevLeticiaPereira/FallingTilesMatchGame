using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreData", menuName = "ScriptableObjects/ScoreDataData", order = 1)]
public class ScoreData : ScriptableObject
{
    [SerializeField] private int _baseTileScore = 10; // Base score for connecting Tile
    [SerializeField] private int _comboBonusBase = 50; // Base score for combo bonus
    [SerializeField] private int _comboBonusIncrement = 25; //Increment for each additional combo
    [SerializeField] private int _chainBonusBase = 100;
    [SerializeField] private int _chainBonusIncrement = 50;
    public int BaseTileScore => _baseTileScore;
    public int ComboBonusBase => _comboBonusBase;
    public int ComboBonusIncrement => _comboBonusIncrement;
    public int ChainBonusBase => _chainBonusBase;
    public int ChainBonusIncrement => _chainBonusIncrement;
}
