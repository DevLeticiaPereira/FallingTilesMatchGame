using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using Utilities;

namespace Grid
{
    public class GridManager : MonoBehaviour
    {
       #region Serialized Fields

        [Header("Grid setup and configurations")] [SerializeField]
        private GridSetupData _gridSetupData;

        [SerializeField] private GridScoreManager _gridScoreManager;

        [SerializeField] private List<TileData> _tilesDataList;
        [SerializeField] private GameObject _spawnMarkerPrefab;
        [SerializeField] private List<Transform> _waitingPairSpawnPoints;
        [SerializeField] private bool _isAiControlled;

        #endregion

        #region Properties

        public Guid GridID { get; private set; }
        public GridSetupData GridInfo => _gridSetupData;

        //dictionary that links grid position to each cell info
        public Dictionary<Vector2Int, GridUtilities.CellInfo> Grid { get; private set; } =
            new Dictionary<Vector2Int, GridUtilities.CellInfo>();

        //Dictionary that links the tile type to its data
        private Dictionary<TileData.TileColor, TileData> _tilesDataDictionary = new Dictionary<TileData.TileColor, TileData>();

        //Dictionary that links the type type to its calculated spawn probability.
        private Dictionary<float, TileData> _tilesSpawnCumulativeProbability = new Dictionary<float, TileData>();

        //List of pairs that are displayed in the game as the next pair to enter the grid
        private List<(Tile, Tile)> _waitingPairs = new List<(Tile, Tile)>();
        //The pair that is taking from the waiting list and add at the top of the grid
        //private (Tile, Tile) _activatedFallingPair = (null, null);
        //All tiles that are current falling
        private int _numberOfFallingTiles = 0;
        private int _tileOnMatchingState = 0;
        private int _numberOfTilesDropping = 0;
        private HashSet<Vector2Int> _gridPositionsToCheck = new HashSet<Vector2Int>();
        private Dictionary<Vector2Int, Tile> _bufferForDroppingTiles = new Dictionary<Vector2Int, Tile>();
        private List<Vector2Int> _gridWaitingToDropPositions = new List<Vector2Int>();
        
        
        
        private Vector2Int _startGridPositionTile1;
        private Vector2Int _startGridPositionTile2;
        private GameObject _spawnMarker;

        public bool IsAiControlled => _isAiControlled;

        #endregion

        #region Monobehavior Methods

        private void OnEnable()
        {
            StartGameState.OnGameStart += ActivateWaitingPair;
            EventManager.EventTileReachedGrid += OnTileReachedGrid;
            EventManager.EventDroppedTileReachedGrid += OnDroppedTileReachedGrid;
            EventManager.EventTileDestroyed += OnTileDestroyed;
            EventManager.EventShouldFallFromPosition += OnShouldFallFromPosition;
        }
        
        private void OnDisable()
        {
            StartGameState.OnGameStart -= ActivateWaitingPair;
            EventManager.EventTileReachedGrid -= OnTileReachedGrid;
            EventManager.EventDroppedTileReachedGrid -= OnDroppedTileReachedGrid;
            EventManager.EventTileDestroyed -= OnTileDestroyed;
            EventManager.EventShouldFallFromPosition -= OnShouldFallFromPosition;
        }

        private void Awake()
        {
            foreach (var tileData in _tilesDataList)
            {
                if (!_tilesDataDictionary.ContainsKey(tileData.ColorTile))
                {
                    _tilesDataDictionary[tileData.ColorTile] = tileData;
                }
            }
            
            GridID = Guid.NewGuid();
            GameManager.Instance.SignUpGridToGame(GridID, _isAiControlled);
            LoadTilesSpawnProbability();
            Grid = GridUtilities.GenerateGridCells(this.transform.position, _gridSetupData);
            SetupStartPosition();
            UpdateGridWaitingPairs();
        }

        #endregion

        #region Event callback fucntions
        private void OnTileReachedGrid(Guid id, Vector2Int gridPosition, Tile tile)
        {
            if (id != GridID)
            {
                return; 
            }
            // ADD TILE THAT REACHED GRID
            if (!TryAddTileToGrid(gridPosition, tile))
            {
                return;
            }
            tile.SetGridPosition(gridPosition);
            
            // GAME OVER CHECK
            if (!GridUtilities.IsGridPositionAvailable(Grid, _startGridPositionTile1))
            {
                EventManager.InvokeGridGameOver(GridID);
                return;
            }
            
            //UPDATE FLOW CONTROLLER VARIABLES
            --_numberOfFallingTiles;
            _gridPositionsToCheck.Add(gridPosition);
            
            //IF ANY TILE STILL FALLING DISABLE INPUT AND WAIT FOR NEXT TILE TO REACH GRID
            if (_numberOfFallingTiles > 0)
            {
                InputManager.Instance.EnablePlayerInput(false);
                return;
            }
            
            //WARN TILES ABOUT GRID ADDED TILES AND UPDATE ITS CONNECTIONS
            EventManager.InvokeUpdateTilesWithGridChanges(GridID, _gridPositionsToCheck, GridUtilities.GridChangedReason.TileAdded);
            
            //HANDLE CHECK FOR MATCH AND WARN TILES IF ANY TILE WAS REMOVED
            var gridPositionsMatched = CheckForMatches();
            foreach (var gridPositionMatched in gridPositionsMatched)
            {
                RemoveTileFromGrid(gridPositionMatched);
            }
            
            _gridPositionsToCheck.Clear();
            _tileOnMatchingState = gridPositionsMatched.Count;
            
            if (gridPositionsMatched.Count>0)
            {
                _gridScoreManager.AddScoreToGrid(gridPositionsMatched.Count);
                EventManager.InvokeUpdateTilesWithGridChanges(GridID, gridPositionsMatched, GridUtilities.GridChangedReason.TileMatched);
            }
            else
            {
                InputManager.Instance.EnablePlayerInput(true);
                ActivateWaitingPair();
            }
        }
        
        private void OnDroppedTileReachedGrid(Guid gridId, Vector2Int gridPosition, Tile tile)
        {
            if (gridId != GridID)
            {
                return; 
            }

            --_numberOfTilesDropping;
            if (_numberOfTilesDropping > 0)
            {
                return;
            }
            
            foreach (var droppingTile in _bufferForDroppingTiles)
            {
                _gridPositionsToCheck.Add(droppingTile.Key);
                TryAddTileToGrid(droppingTile.Key, droppingTile.Value);
            }
            _bufferForDroppingTiles.Clear();
            
            //WARN TILES ABOUT GRID ADDED TILES AND UPDATE ITS CONNECTIONS
            EventManager.InvokeUpdateTilesWithGridChanges(GridID, _gridPositionsToCheck, GridUtilities.GridChangedReason.TileAdded);
            
            //HANDLE CHECK FOR MATCH AND WARN TILES IF ANY TILE WAS REMOVED
            var gridMatchedPositions = CheckForMatches();
            foreach (var gridMatchedPosition in gridMatchedPositions)
            {
                RemoveTileFromGrid(gridMatchedPosition);
            }
            
            _gridPositionsToCheck.Clear();
            _tileOnMatchingState = gridMatchedPositions.Count;
            
            if (gridMatchedPositions.Count>0)
            {
                _gridScoreManager.AddScoreToGrid(gridMatchedPositions.Count);
                EventManager.InvokeUpdateTilesWithGridChanges(GridID, gridMatchedPositions, GridUtilities.GridChangedReason.TileMatched);
            }
            else
            {
                InputManager.Instance.EnablePlayerInput(true);
                ActivateWaitingPair();
            }
        }
        
        private void OnTileDestroyed(Guid id)
        {
            if (id != GridID)
            {
                return; 
            }
            
            --_tileOnMatchingState;

            if (_tileOnMatchingState > 0 || _numberOfFallingTiles > 0)
            {
                return;
            }
            
            if (_gridWaitingToDropPositions.Count <= 0)
            {
                InputManager.Instance.EnablePlayerInput(true);
                ActivateWaitingPair();
            }
            
            ProcessTileDrop();
        }

        private void ProcessTileDrop()
        {
            foreach (var gridWaitingToDropPosition in _gridWaitingToDropPositions)
            {
                if (!Grid.ContainsKey(gridWaitingToDropPosition))
                {
                    continue;
                }

                var cell = Grid[gridWaitingToDropPosition];
                var newPosition = new Vector2Int(-1, -1);
                for (int i = 0; i < _gridSetupData.Rows; i++)
                {
                    newPosition = new Vector2Int(gridWaitingToDropPosition.x, i);
                    if (GridUtilities.IsGridPositionAvailable(Grid, newPosition) 
                        && !_bufferForDroppingTiles.ContainsKey(newPosition))
                    {
                        break;
                    }
                }

                if (newPosition == new Vector2Int(-1, -1))
                {
                    Debug.LogError("cant find a valid place to drop tile.");
                }
                cell.Tile.SetGridPosition(newPosition);
                _bufferForDroppingTiles.Add(newPosition, cell.Tile);
                RemoveTileFromGrid(gridWaitingToDropPosition);
            }
            
            _numberOfTilesDropping = _bufferForDroppingTiles.Count;
            EventManager.InvokeUpdateTilesWithGridChanges(GridID, _gridWaitingToDropPositions.ToHashSet(), GridUtilities.GridChangedReason.TileDropped);
        }

        private void OnShouldFallFromPosition(Guid id, Vector2Int gridPosition)
        {
            if (id != GridID)
            {
                return; 
            }

            for (int i = gridPosition.y; i < _gridSetupData.Rows; i++)
            {
                var positionToFall = new Vector2Int(gridPosition.x, i);
                if (GridUtilities.IsGridPositionAvailable(Grid, positionToFall))
                {
                    break;   
                }
                Utilities.Utilities.AddUniqueToList( ref _gridWaitingToDropPositions, positionToFall);
            }
        }

        #endregion

        #region Private Methods
        
        private HashSet<Vector2Int> CheckForMatches()
        {
            HashSet<Vector2Int> gridPositionsMatched = new HashSet<Vector2Int>();
            foreach (var gridPositionToCheck in _gridPositionsToCheck)
            {
                var connectedGridPosition = GridUtilities.GetChainConnectedTiles(Grid, gridPositionToCheck);
                if (connectedGridPosition.Count >= GameManager.Instance.MinNumberOfTilesToMatch)
                {
                    gridPositionsMatched.UnionWith(connectedGridPosition);
                }
            }

            return gridPositionsMatched;
        }

        private bool TryAddTileToGrid(Vector2Int gridPosition, Tile tile)
        {
            if (!Grid.TryGetValue(gridPosition, out GridUtilities.CellInfo cell) || cell.Tile != null)
            {
                return false;
            }
            cell.Tile = tile;
            cell.TileColor = tile.Data.ColorTile;
            return true;
        }

        private void RemoveTileFromGrid(Vector2Int gridPosition)
        {
            GridUtilities.CellInfo cell = Grid[gridPosition];
            cell.Tile = null;
            cell.TileColor = TileData.TileColor.None;
        }

        private void ActivateWaitingPair()
        {
            if (_waitingPairs.Count == 0)
            {
                Debug.LogWarning("No waiting pairs available.");
                return;
            }

            var tile1 = _waitingPairs[0].Item1; 
            var tile2 = _waitingPairs[0].Item2; 
            _waitingPairs.RemoveAt(0);

            _numberOfFallingTiles += 2;
            
            var tileToMoveInfo =  new HashSet<Tile>
            {
                tile1, tile2
            };
            
            EventManager.InvokePlacedTileAtGridStartPoint(GridID, tileToMoveInfo);
            UpdateGridWaitingPairs();
        }

        private void UpdateGridWaitingPairs()
        {
            //reorder pairs
            for (int i = 0; i < _waitingPairs.Count; ++i)
            {
                Vector2 newPosition = _waitingPairSpawnPoints[i].position;
                _waitingPairs[i].Item1.transform.position = newPosition;
                _waitingPairs[i].Item2.transform.position =
                    newPosition + new Vector2(0, _gridSetupData.BlockDimensions.y);
            }

            //add new pair if position is empty
            while (_waitingPairs.Count < _waitingPairSpawnPoints.Count)
            {
                TileData tileData1 = GetRandomWeightedTileData();
                Vector2 tilePosition1 = _waitingPairSpawnPoints[_waitingPairs.Count].position;
                Tile tile1 = SpawnTile(tileData1, tilePosition1, true, _startGridPositionTile1, this.transform);

                TileData tileData2 = GetRandomWeightedTileData();
                Vector2 tilePosition2 = tilePosition1 + new Vector2(0, _gridSetupData.BlockDimensions.y);
                Tile tile2 = SpawnTile(tileData2, tilePosition2, false, _startGridPositionTile2, tile1.transform);

                tile1.SetBeginPair(tile2);
                tile2.SetBeginPair(tile1);
                
                (Tile,Tile) newPair = new (tile1, tile2);
                _waitingPairs.Add(newPair);
            }
        }

        private Tile SpawnTile(TileData tileData, Vector2 worldPosition, bool isRoot, Vector2Int initialGridPosition, Transform parent)
        {
            GameObject spawnedTileObject =
                Instantiate(_gridSetupData.TilePrefab, worldPosition, Quaternion.identity, parent);
            spawnedTileObject.TryGetComponent(out Tile tile);
            Assert.IsNotNull(tile, "Tile's prefab missing Tile script component");
            tile.InitializeTile(this, tileData, initialGridPosition, isRoot);
            spawnedTileObject.name = $"Tile {tileData.ColorTile}";
            return tile;
        }

        #endregion

        #region InitialGridSetups

        private TileData GetRandomWeightedTileData()
        {
            float randomTileProbability = Random.Range(0.0f, 1.0f);

            TileData newTileData = null;

            // Handle the case where the _tilesSpawnCumulativeProbability dictionary is empty
            if (_tilesSpawnCumulativeProbability.Count == 0)
            {
                Debug.LogError(
                    "No tile spawn probabilities available. Please make sure _tilesSpawnCumulativeProbability is properly set.");
                return newTileData;
            }

            foreach (KeyValuePair<float, TileData> tileSpawnProbability in _tilesSpawnCumulativeProbability)
            {
                if (randomTileProbability <= tileSpawnProbability.Key)
                {
                    return tileSpawnProbability.Value;
                }
            }

            // If no tile data is found (e.g., due to incorrect probability setup),
            // select a random tile from _tilesDataDictionary
            if (newTileData == null)
            {
                Debug.LogWarning("Failed to find a weighted tile data. Selecting a random tile.");
                newTileData =
                    _tilesDataDictionary[
                        _tilesDataDictionary.Keys.ToArray()[Random.Range(0, _tilesDataDictionary.Count)]];
            }

            return newTileData;
        }
        private void LoadTilesSpawnProbability()
        {
            // Calculate Total Spawn Weights
            List<TilesSpawnWeight> tileWeights = _gridSetupData.TilesSpawnWeight;
            float sumWeights = 0;
            foreach (TilesSpawnWeight tileWeight in tileWeights)
            {
                sumWeights += tileWeight.spawnWeight;
            }

            if (sumWeights == 0)
            {
                Debug.LogError("Sum of weights for tile spawn probabilities is zero. Please set non-zero weights.");
                return;
            }

            //Populate Tiles Spawn Cumulative Probability
            float cumulativeProbability = 0;
            foreach (TilesSpawnWeight tileWeight in tileWeights)
            {
                TileData tileData = _tilesDataDictionary[tileWeight.tileType];
                if (tileData == null)
                {
                    Debug.LogError("Missing Tile data for Tile Type" + tileWeight.tileType);
                    continue;
                }

                if (tileWeight.spawnWeight > 0)
                {
                    cumulativeProbability += tileWeight.spawnWeight / sumWeights;
                    _tilesSpawnCumulativeProbability[cumulativeProbability] = tileData;
                }
            }
        }
        private void SetupStartPosition()
        {
            _startGridPositionTile1 = new Vector2Int(_gridSetupData.ColumnToSpawn, _gridSetupData.Rows - 2);
            _startGridPositionTile2 = new Vector2Int(_gridSetupData.ColumnToSpawn, _gridSetupData.Rows - 1);
            var startWorldPositionTile = GridUtilities.GetGridCellWorldPosition(Grid, _startGridPositionTile1);
            _spawnMarker = Instantiate(_spawnMarkerPrefab, startWorldPositionTile, Quaternion.identity,
                this.transform);
        }

        #endregion
    }
}