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
            EventManager.EventUpdateTilesWithGridChanges += OnEventUpdateTilesWithGridChanged;
            if (!TileOwner.GridPosition.HasValue)
            {
                return;
            }
            _neighborConnectionMap = GetNeighborGridPositions(TileOwner.GridPosition.Value);
        }
        
        private void OnEventUpdateTilesWithGridChanged(Guid gridID, HashSet<Vector2Int> gridPositionsChanged, GridUtilities.GridChangedReason gridChangedReason)
        {
            if (gridID != _gridManager.GridID)
            {
                return;
            }
            
            if (gridPositionsChanged.Contains(TileOwner.GridPosition.Value) && gridChangedReason == GridUtilities.GridChangedReason.TileDropped)
            {
                TileOwner.StartToMoveGridPosition(5.0f);
                return;
            }
            
            // if tile's position changed and grids position is now empty, change state to matched
            if (gridPositionsChanged.Contains(TileOwner.GridPosition.Value) && gridChangedReason == GridUtilities.GridChangedReason.TileMatched)
            {
                TileOwner.TileStateMachine.ChangeState(TileOwner.MatchedTileState);
                return;
            }
            
            if (gridPositionsChanged.Contains(TileOwner.GridPosition.Value) && gridChangedReason == GridUtilities.GridChangedReason.TileAdded)
            {
                Connections = GetTileConnections();
                TileOwner.UpdateTileSpriteWithConnections(Connections);
                return;
            }
            
            //if any of the changed position is one of neighbors positions update connections and sprites
            //check for down tile
            foreach (var gridPositionChanged in gridPositionsChanged)
            {
                if (_neighborConnectionMap.ContainsKey(gridPositionChanged))
                {
                    Connections = GetTileConnections();
                    TileOwner.UpdateTileSpriteWithConnections(Connections);
                    CheckForDownTile();
                    return;
                }
            }
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
            EventManager.EventUpdateTilesWithGridChanges -= OnEventUpdateTilesWithGridChanged;
        }
    }
}
