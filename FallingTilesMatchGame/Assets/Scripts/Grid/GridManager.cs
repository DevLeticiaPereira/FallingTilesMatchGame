using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Utilities;
using Random = UnityEngine.Random;

namespace Grid
{
    public class GridManager : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GridScoreManager _gridScoreManager;
        [SerializeField] private GameObject _spawnMarkerPrefab;
        [SerializeField] private List<Transform> _waitingPairSpawnPoints;
        [SerializeField] private bool _isPlayer;
        [SerializeField] private TMP_Text _gridOwnerText;
        #endregion

        #region Properties

        public Guid GridID { get; private set; }
        public GridSetupData GridInfo { get; private set; }

        //dictionary that links grid position to each cell info
        public Dictionary<Vector2Int, GridUtilities.CellInfo> Grid { get; private set; } = new();

        //Dictionary that links the tile type to its data
        private readonly Dictionary<TileData.TileColor, TileData> _tilesDataDictionary = new();

        //Dictionary that links the type type to its calculated spawn probability.
        private readonly Dictionary<float, TileData> _tilesSpawnCumulativeProbability = new();

        //List of pairs that are displayed in the game as the next pair to enter the grid
        private readonly List<(Tile, Tile)> _waitingPairs = new();

        private int _minNumberToMatch = 4;
        private int _numberOfFallingTiles;
        private int _tileOnMatchingState;
        private int _numberOfTilesDropping;
        private readonly HashSet<Vector2Int> _gridPositionsToCheck = new();
        private List<Vector2Int> _gridWaitingToDropPositions = new();
        private Vector2Int _startGridPositionTile1;
        private Vector2Int _startGridPositionTile2;
        private GameObject _spawnMarker;
        private AIController _aiControllerComponent;

        public bool IsPlayer => _isPlayer;

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
            InitializeGameSettings();
            SignupGridToGame();
            if (!_isPlayer)
            {
                _aiControllerComponent = gameObject.AddComponent<AIController>();
                _aiControllerComponent.Initialize(this);
            }
            SetupGridOwnerName();
            LoadTilesSpawnProbability();
            Grid = GridUtilities.GenerateGridCells(transform.position, GridInfo);
            SetupStartPosition();
            UpdateGridWaitingPairs();
        }
        #endregion

        #region Event callback fucntions

        private void OnTileReachedGrid(Guid id, Vector2Int gridPosition, Tile tile)
        {
            if (id != GridID) return;
            // ADD TILE THAT REACHED GRID
            if (!TryAddTileToGrid(gridPosition, tile)) return;
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
            EventManager.InvokeTilesAddedToGrid(GridID, _gridPositionsToCheck);
            //HANDLE CHECK FOR MATCH AND WARN TILES IF ANY TILE WAS REMOVED
            if (!TryToFindMatches())
            {
                InputManager.Instance.EnablePlayerInput(true);
                ActivateWaitingPair();
            }
        }

        private void OnDroppedTileReachedGrid(Guid gridId, Vector2Int gridPosition, Tile tile)
        {
            if (gridId != GridID) return;

            if (!TryAddTileToGrid(gridPosition, tile)) return;
            tile.SetGridPosition(gridPosition);

            //UPDATE FLOW CONTROLLER VARIABLES
            _gridPositionsToCheck.Add(gridPosition);
            --_numberOfTilesDropping;

            if (_numberOfTilesDropping > 0) return;

            //WARN TILES ABOUT GRID ADDED TILES AND UPDATE ITS CONNECTIONS
            EventManager.InvokeTilesAddedToGrid(GridID, _gridPositionsToCheck);

            //HANDLE CHECK FOR MATCH AND WARN TILES IF ANY TILE WAS REMOVED
            if (!TryToFindMatches())
            {
                InputManager.Instance.EnablePlayerInput(true);
                ActivateWaitingPair();
            }
        }


        //callback received when a tile is destroyed, once all tiles that were predicted to be destroyed reaches here,
        //it will check if any tile wants to drop and if not restart the loop with a new tile on the grid top
        private void OnTileDestroyed(Guid id)
        {
            if (id != GridID) return;

            --_tileOnMatchingState;

            if (_tileOnMatchingState > 0 || _numberOfFallingTiles > 0) return;

            if (_gridWaitingToDropPositions.Count <= 0)
            {
                InputManager.Instance.EnablePlayerInput(true);
                ActivateWaitingPair();
            }

            ProcessTileDrop();
        }

        //When a tile loose a down connection it send this event to enter the _gridWaitingToDropPositions list.
        //All tiles that are above this will also enter that list
        //they will be waiting for all matched tiles to be destroyed so it can begin to drop
        private void OnShouldFallFromPosition(Guid id, Vector2Int gridPosition)
        {
            if (id != GridID) return;

            for (var i = gridPosition.y; i < GridInfo.Rows; i++)
            {
                var positionToFall = new Vector2Int(gridPosition.x, i);
                if (GridUtilities.IsGridPositionAvailable(Grid, positionToFall)) break;
                Utilities.Utilities.AddUniqueToList(ref _gridWaitingToDropPositions, positionToFall);
            }
        }

        #endregion

        #region Private Methods

        //Assign and store new position for all dropped tiles 
        //Dropped tiles will start will receive the even and start to move to new position
        //other tiles will will receive the event as well and updates its connections
        private void ProcessTileDrop()
        {
            var oldToNewPositionMap = new Dictionary<Vector2Int, Vector2Int>();
            foreach (var gridWaitingToDropPosition in _gridWaitingToDropPositions)
            {
                if (!Grid.ContainsKey(gridWaitingToDropPosition)) continue;

                var cell = Grid[gridWaitingToDropPosition];
                if (!TryGetNewAvailableDropPosition(gridWaitingToDropPosition, oldToNewPositionMap,
                        out var newPosition))
                {
                    Debug.LogError("cant find a valid place to drop tile.");
                    continue;
                }

                oldToNewPositionMap[gridWaitingToDropPosition] = newPosition;
                RemoveTileFromGrid(gridWaitingToDropPosition);
            }

            _numberOfTilesDropping = oldToNewPositionMap.Count;
            EventManager.InvokeTilesDroppedFromGrid(GridID, oldToNewPositionMap);
            _gridWaitingToDropPositions.Clear();
        }

        //look for an empty position on the grid starting from the bottom 
        //consider grip and also the dropped tiles that had already been assign a new position on the grid
        private bool TryGetNewAvailableDropPosition(Vector2Int gridWaitingToDropPosition,
            Dictionary<Vector2Int, Vector2Int> oldToNewPositionMap, out Vector2Int newPosition)
        {
            newPosition = new Vector2Int(-1, -1);
            for (var i = 0; i < GridInfo.Rows; i++)
            {
                newPosition = new Vector2Int(gridWaitingToDropPosition.x, i);
                if (GridUtilities.IsGridPositionAvailable(Grid, newPosition)
                    && !oldToNewPositionMap.ContainsValue(newPosition))
                    return true;
            }

            return false;
        }

        private bool TryToFindMatches()
        {
            var gridMatchedPositions = GridUtilities.CheckForMatches(Grid, _gridPositionsToCheck, _minNumberToMatch);
            if (gridMatchedPositions.Count > 0)
            {
                foreach (var gridMatchedPosition in gridMatchedPositions) RemoveTileFromGrid(gridMatchedPosition);
                _gridScoreManager.AddScoreToGrid(gridMatchedPositions.Count);
                EventManager.InvokeTilesMatched(GridID, gridMatchedPositions);
            }

            _gridPositionsToCheck.Clear();
            _tileOnMatchingState = gridMatchedPositions.Count;
            return gridMatchedPositions.Count > 0;
        }

        private bool TryAddTileToGrid(Vector2Int gridPosition, Tile tile)
        {
            if (!Grid.TryGetValue(gridPosition, out var cell) || cell.Tile != null) return false;
            cell.Tile = tile;
            cell.TileColor = tile.Data.ColorTile;
            return true;
        }

        private void RemoveTileFromGrid(Vector2Int gridPosition)
        {
            var cell = Grid[gridPosition];
            cell.Tile = null;
            cell.TileColor = TileData.TileColor.None;
        }

        //Get pair of tiles waiting on the grid board and activate them in game
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

            var tileToMoveInfo = new HashSet<Tile>
            {
                tile1, tile2
            };

            EventManager.InvokePlacedTileAtGridStartPoint(GridID, tileToMoveInfo);
            UpdateGridWaitingPairs();
        }

        //Move waiting pair waiting to empty position em fill up the spawn now waiting tiles
        private void UpdateGridWaitingPairs()
        {
            //reorder pairs
            for (var i = 0; i < _waitingPairs.Count; ++i)
            {
                Vector2 newPosition = _waitingPairSpawnPoints[i].position;
                _waitingPairs[i].Item1.transform.position = newPosition;
                _waitingPairs[i].Item2.transform.position =
                    newPosition + new Vector2(0, GridInfo.BlockDimensions.y);
            }

            //add new pair if position is empty
            while (_waitingPairs.Count < _waitingPairSpawnPoints.Count)
            {
                var tileData1 = GetRandomWeightedTileData();
                Vector2 tilePosition1 = _waitingPairSpawnPoints[_waitingPairs.Count].position;
                var tile1 = SpawnTile(tileData1, tilePosition1, true, _startGridPositionTile1, transform);

                var tileData2 = GetRandomWeightedTileData();
                var tilePosition2 = tilePosition1 + new Vector2(0, GridInfo.BlockDimensions.y);
                var tile2 = SpawnTile(tileData2, tilePosition2, false, _startGridPositionTile2, tile1.transform);

                tile1.SetBeginPair(tile2);
                tile2.SetBeginPair(tile1);

                (Tile, Tile) newPair = new(tile1, tile2);
                _waitingPairs.Add(newPair);
            }
        }

        private Tile SpawnTile(TileData tileData, Vector2 worldPosition, bool isRoot, Vector2Int initialGridPosition,
            Transform parent)
        {
            var spawnedTileObject =
                Instantiate(GridInfo.TilePrefab, worldPosition, Quaternion.identity, parent);
            spawnedTileObject.TryGetComponent(out Tile tile);
            Assert.IsNotNull(tile, "Tile's prefab missing Tile script component");
            tile.InitializeTile(this, tileData, initialGridPosition, isRoot);
            spawnedTileObject.name = $"Tile {tileData.ColorTile}";
            return tile;
        }

        #endregion

        #region InitialGridSetups

        //When trying to spawn a tile take into consideration its weigh probability to spawn. 
        private TileData GetRandomWeightedTileData()
        {
            var randomTileProbability = Random.Range(0.0f, 1.0f);

            TileData newTileData = null;

            // Handle the case where the _tilesSpawnCumulativeProbability dictionary is empty
            if (_tilesSpawnCumulativeProbability.Count == 0)
            {
                Debug.LogError(
                    "No tile spawn probabilities available. Please make sure _tilesSpawnCumulativeProbability is properly set.");
                return newTileData;
            }

            foreach (var tileSpawnProbability in _tilesSpawnCumulativeProbability)
                if (randomTileProbability <= tileSpawnProbability.Key)
                    return tileSpawnProbability.Value;

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
        //calculate tiles spawn probability and store it to easy access for gameplay
        private void LoadTilesSpawnProbability()
        {
            // Calculate Total Spawn Weights
            var tileWeights = GridInfo.TilesSpawnWeight;
            float sumWeights = 0;
            foreach (var tileWeight in tileWeights) sumWeights += tileWeight.spawnWeight;

            if (sumWeights == 0)
            {
                Debug.LogError("Sum of weights for tile spawn probabilities is zero. Please set non-zero weights.");
                return;
            }

            //Populate Tiles Spawn Cumulative Probability
            float cumulativeProbability = 0;
            foreach (var tileWeight in tileWeights)
            {
                var tileData = _tilesDataDictionary[tileWeight.tileType];
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
        //setup tiles start position on the grid and instantiate game object that marks it
        private void SetupStartPosition()
        {
            _startGridPositionTile1 = new Vector2Int(GridInfo.ColumnToSpawn, GridInfo.Rows - 2);
            _startGridPositionTile2 = new Vector2Int(GridInfo.ColumnToSpawn, GridInfo.Rows - 1);
            var startWorldPositionTile = GridUtilities.GetGridCellWorldPosition(Grid, _startGridPositionTile1);
            _spawnMarker = Instantiate(_spawnMarkerPrefab, startWorldPositionTile, Quaternion.identity,
                transform);
        }
        private void SetupGridOwnerName()
        {
            if (GameManager.Instance.NumberOfPlayers == 1)
            {
                _gridOwnerText.gameObject.SetActive(false);
            }
            else if (_isPlayer)
            {
                _gridOwnerText.text = "Player";
            }
            else
            {
                _gridOwnerText.text = "Enemy";
            }
        }
        private void InitializeGameSettings()
        {
            GridInfo = GameManager.Instance.GameSettings.GridSetupData;
            _minNumberToMatch = GameManager.Instance.GameSettings.MinNumberOfTilesToMatch;
            var tilesDataList = GameManager.Instance.GameSettings.TilesData;
            
            foreach (var tileData in tilesDataList)
                if (!_tilesDataDictionary.ContainsKey(tileData.ColorTile))
                    _tilesDataDictionary[tileData.ColorTile] = tileData;
        }
        private void SignupGridToGame()
        {
            GridID = Guid.NewGuid();
            if (!GameManager.Instance.SignUpGridToGame(GridID, _isPlayer))
            {
                _isPlayer = !_isPlayer;
            }
        }

        #endregion
    }
}