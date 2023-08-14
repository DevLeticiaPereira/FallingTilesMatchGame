using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TilesSpawnWeight
{
    public TileData.TileColor tileType;

    [Range(0.0f, 1.0f)]
    public float spawnWeight;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GridSetupData", order = 1)]
public class GridSetupData : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] private int _rows = 16;
    [SerializeField] private int _columns = 8;
    [SerializeField] private Vector2 _blockDimensions;
    [SerializeField] private Vector2 _blockSpaceBetween;
    [SerializeField] private GameObject _blockBackgroundPrefabType1;
    [SerializeField] private GameObject _blockBackgroundPrefabType2;
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private int _columnToSpawn;

    [NonReorderable] 
    public List<TilesSpawnWeight> TilesSpawnWeight;
    public int Rows => _rows;
    public int Columns => _columns;
    public Vector2 BlockDimensions => _blockDimensions;
    public Vector2 BlockSpaceBetween => _blockSpaceBetween;
    public GameObject BlockBackgroundPrefabType1 => _blockBackgroundPrefabType1;
    public GameObject BlockBackgroundPrefabType2 => _blockBackgroundPrefabType2;
    public GameObject TilePrefab => _tilePrefab;
    public int ColumnToSpawn => _columnToSpawn;


    public void OnBeforeSerialize()
    {
        if (TilesSpawnWeight.Count >= Enum.GetValues(typeof(TileData.TileColor)).Length)
        {
            return;
        }
        
        foreach (TileData.TileColor enumValue in Enum.GetValues(typeof(TileData.TileColor)))
        {
            if (enumValue == TileData.TileColor.None)
            {
                continue;
            }
            if (TilesSpawnWeight.All(tileWeight => tileWeight.tileType != enumValue))
            {
                TilesSpawnWeight newTileWeight = new TilesSpawnWeight();
                newTileWeight.tileType = enumValue;
                TilesSpawnWeight.Add(newTileWeight);
                Console.WriteLine(enumValue);
            }
        }
    }

    public void OnAfterDeserialize()
    {
    }
}
