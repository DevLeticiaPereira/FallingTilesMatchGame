using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Grid.TileStates
{
    public class PlacedOnGridState : TileState
    {
        /// <summary>
        /// Valid connections that tile has to neighbor tiles - The color matters
        /// Setup when grid has changes at the tile position or on its neighbors
        /// </summary>
        public TileData.TileConnections Connections { get; private set; }
        /// <summary>
        /// Neighbor grid positions and its connection direction
        /// Color are no considered
        /// setup in the Enter and used later for faster color connection check
        /// </summary>
        private Dictionary<Vector2Int, TileData.TileConnections> _neighborConnectionMap = new Dictionary<Vector2Int, TileData.TileConnections>();
        
        private readonly GridManager _gridManager;
        private bool _waitingToFall = false;

        public PlacedOnGridState(Tile tileOwner, StateMachine<TileState> tileStateMachine, GridManager gridManager) : base(tileOwner, tileStateMachine)
        {
            _gridManager = gridManager;
        }
        
        public override void Enter()
        {
            base.Enter();
            EventManager.EventTilesDroppedFromGrid += OnDroppedFromGrid;
            EventManager.EventTilesMatched += OnTilesMatched;
            EventManager.EventTilesAddedToGrid += OnTilesAddedToGrid;
            if (!TileOwner.GridPosition.HasValue)
            {
                return;
            }
            _neighborConnectionMap = GetNeighborGridPositions(TileOwner.GridPosition.Value);
        }

        private void OnTilesAddedToGrid(Guid gridId, HashSet<Vector2Int> changedPositions)
        {
            if (gridId != _gridManager.GridID)
            {
                return;
            }
            
            if (HasNeighborChanged(changedPositions) || changedPositions.Contains(TileOwner.GridPosition.Value))
            {
                Connections = GetTileConnections();
                TileOwner.UpdateTileSpriteWithConnections(Connections);
            }
        }

        private void OnTilesMatched(Guid gridId, HashSet<Vector2Int> changedPositions)
        {
            if (gridId != _gridManager.GridID)
            {
                return;
            }
            if (changedPositions.Contains(TileOwner.GridPosition.Value))
            {
                TileOwner.TileStateMachine.ChangeState(TileOwner.MatchedTileState);
                return;
            }

            if (HasNeighborChanged(changedPositions))
            {
                Connections = GetTileConnections();
                TileOwner.UpdateTileSpriteWithConnections(Connections);
                CheckForDownTile();
            }
        }

        private void OnDroppedFromGrid(Guid gridId, Dictionary<Vector2Int, Vector2Int> positionsChangedMap)
        {
            if (gridId != _gridManager.GridID)
            {
                return;
            }
            
            if (positionsChangedMap.ContainsKey(TileOwner.GridPosition.Value))
            {
                TileOwner.SetDefaultSprite();
                _neighborConnectionMap = GetNeighborGridPositions(positionsChangedMap[TileOwner.GridPosition.Value]);
                TileOwner.StartToMoveGridPosition(0.5f, positionsChangedMap[TileOwner.GridPosition.Value]);
                return;
            }

            if (HasNeighborChanged(positionsChangedMap, /*check for key*/ true))
            {
                Connections = GetTileConnections();
                TileOwner.UpdateTileSpriteWithConnections(Connections);
                CheckForDownTile();
            }
        }
        
        private bool HasNeighborChanged(HashSet<Vector2Int> changedGridPositions)
        {
            foreach (var position in changedGridPositions)
            {
                if (_neighborConnectionMap.ContainsKey(position))
                {
                    return true;
                }
            }
            return false;
        }
        
        private bool HasNeighborChanged(Dictionary<Vector2Int,Vector2Int> changedGridPositions, bool checkForKey)
        {
            foreach (var position in changedGridPositions)
            {
                var positionToCheck = checkForKey ? position.Key : position.Value;
                if (_neighborConnectionMap.ContainsKey(positionToCheck))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckForDownTile()
        {
            Vector2Int downTile = new Vector2Int(TileOwner.GridPosition.Value.x, TileOwner.GridPosition.Value.y - 1);
            if (!GridUtilities.IsGridPositionAvailable(_gridManager.Grid, downTile))
            {
                return false;
            }
            
            EventManager.InvokeEventTileShouldFallFromPosition(_gridManager.GridID, TileOwner.GridPosition.Value);
            return true;
        }

        private Dictionary<Vector2Int, TileData.TileConnections> GetNeighborGridPositions(Vector2Int gridPosition)
        {
            Dictionary<Vector2Int, TileData.TileConnections> adjacentGridPositions = new Dictionary<Vector2Int, TileData.TileConnections>();
            foreach (TileData.TileConnections direction in System.Enum.GetValues(typeof(TileData.TileConnections)))
            {
                if (direction == TileData.TileConnections.None)
                {
                    continue;
                }
                
                var adjacentGridPosition = GridUtilities.GetAdjacentGridPosition(gridPosition, direction);
                if (GridUtilities.IsPositionPartOfGrid(_gridManager.Grid, adjacentGridPosition))
                {
                    adjacentGridPositions.Add(adjacentGridPosition, direction);
                }
            }
            return adjacentGridPositions;
        }
        
        private TileData.TileConnections GetTileConnections()
        {
            TileData.TileConnections newTileConnections = TileData.TileConnections.None;
            TileData.TileColor tileColor = TileOwner.Data.ColorTile;

            foreach (var neighborConnectionPair in _neighborConnectionMap)
            {
                if (!TryToConnectToNeighbor(neighborConnectionPair.Key, tileColor))
                {
                    continue;
                }
                newTileConnections |= neighborConnectionPair.Value;
            }
            return newTileConnections;
        }
        
        private bool TryToConnectToNeighbor(Vector2Int adjacentGridPosition, TileData.TileColor tileColor)
        {
            if (!_gridManager.Grid.TryGetValue(adjacentGridPosition, out var cellInfo))
            {
                return false;
            }

            if (cellInfo.TileColor != tileColor)
            {
                return false;
            }
            
            return true;
        }
        
        public override void Exit()
        {
            base.Exit();
            EventManager.EventTilesDroppedFromGrid -= OnDroppedFromGrid;
            EventManager.EventTilesMatched -= OnTilesMatched;
            EventManager.EventTilesAddedToGrid -= OnTilesAddedToGrid;
        }
    }
}
