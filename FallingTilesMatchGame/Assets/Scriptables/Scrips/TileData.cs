using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TileData", order = 1)]
public class TileData : ScriptableObject
{
    //todo: move enums to utility folder
    #region Enuns
    [Flags]
    public enum TileConnections
    {
        None = 0,
        Right = 1 << 0,
        Left = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3,
    }
    
    public enum TileColor
    {
        None,
        Red,
        Green,
        Blue,
        Yellow,
        Purple
    }
    #endregion
    
    #region Serializable Fields
    [Serializable] public class TileConnectionData
    {
        [SerializeField] private TileConnections _tileConnection;
        [SerializeField] private Sprite _sprite;

        public TileConnections TileConnection => _tileConnection;
        public Sprite Sprite => _sprite;
    }
    [SerializeField] private TileColor _colorTile;
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _rootTileSprite;
    [SerializeField] private AnimationClip _deathAnimation;
    [SerializeField] private List<TileConnectionData> _tilesConnectionData;
    
    #endregion
    
    #region Properties
    public TileColor ColorTile => _colorTile;
    public List<TileConnectionData> TilesConnectionData => _tilesConnectionData;
    public Sprite DefaultSprite => _defaultSprite;
    public Sprite RootTileSprite => _rootTileSprite;
    public AnimationClip DeathAnimation => _deathAnimation;
    
    #endregion Properties

    #region Methods
    
    public Sprite GetSpriteForConnection(TileConnections connections)
    {
      //todo Fix bug not getting right sprite for tile with all flags
        foreach (var tileConnectionData in _tilesConnectionData)
        {
            if (tileConnectionData.TileConnection == connections)
            {
                return tileConnectionData.Sprite;
            }
        }
        // Return null or a default sprite if the combination is not found
        return _defaultSprite;
    }

    #endregion
}

    

