using System;
using System.Collections.Generic;
using Grid;
using UnityEngine;

namespace Utilities
{
    public static class Utilities
    {
        public static void AddUniqueToList<T>(ref List<T> list, T element)
        {
            if (!list.Contains(element)) list.Add(element);
        }
    }

    public static class TileUtilities
    {
        public static Vector2Int GetTileGridTarget(GridManager gridManager, Tile tileToCheck, Tile pairTile)
        {
            var firstAvailablePosition = new Vector2Int(-1,-1);
            for (var i = 0; i <= gridManager.GridInfo.Rows; ++i)
            {
                var positionToCheck = new Vector2Int(tileToCheck.TemporaryGridPosition.Value.x, i);
                if (GridUtilities.IsGridPositionAvailable(gridManager.Grid, positionToCheck))
                {
                    firstAvailablePosition = positionToCheck;
                    break;
                }
            }

            if (firstAvailablePosition != new Vector2Int(-1,-1))
            {
                var positionComparisonRange = 0.1f;
                if (tileToCheck.transform.position.y > pairTile.transform.position.y &&
                    Mathf.Abs(tileToCheck.transform.position.y - pairTile.transform.position.y) > positionComparisonRange)
                    firstAvailablePosition += new Vector2Int(0, 1);
            }
            return firstAvailablePosition;
        }
    }

    public static class GridUtilities
    {
        public static Vector2Int GetAdjacentGridPosition(Vector2Int gridPosition, TileData.TileConnections direction)
        {
            var x = gridPosition.x;
            var y = gridPosition.y;

            switch (direction)
            {
                case TileData.TileConnections.Right:
                    x++;
                    break;
                case TileData.TileConnections.Left:
                    x--;
                    break;
                case TileData.TileConnections.Up:
                    y++;
                    break;
                case TileData.TileConnections.Down:
                    y--;
                    break;
            }

            return new Vector2Int(x, y);
        }

        public static TileData.TileConnections GetOppositeTileConnection(TileData.TileConnections tileConnections)
        {
            var connections = TileData.TileConnections.None;

            switch (tileConnections)
            {
                case TileData.TileConnections.Right:
                    connections |= TileData.TileConnections.Left;
                    break;
                case TileData.TileConnections.Left:
                    connections |= TileData.TileConnections.Right;
                    break;
                case TileData.TileConnections.Up:
                    connections |= TileData.TileConnections.Down;
                    break;
                case TileData.TileConnections.Down:
                    connections |= TileData.TileConnections.Up;
                    break;
            }

            return connections;
        }

        public static HashSet<Vector2Int> GetChainConnectedTiles(Dictionary<Vector2Int, CellInfo> grid,
            Vector2Int startGridPosition, HashSet<Vector2Int> connectedTiles = null)
        {
            connectedTiles ??= new HashSet<Vector2Int>();

            if (!TryGetTileAtGridPosition(grid, startGridPosition, out var startTile)) return connectedTiles;

            if (!connectedTiles.Contains(startGridPosition))
                connectedTiles.Add(startGridPosition);
            else
                return connectedTiles;

            var startTileConnection = startTile.PlacedOnGridTileState.Connections;
            // Loop through each direction (right, left, up, down) and check for connected tiles
            foreach (TileData.TileConnections direction in Enum.GetValues(typeof(TileData.TileConnections)))
            {
                if ((startTileConnection & direction) == 0) continue;

                var adjacentTilePosition = GetAdjacentGridPosition(startGridPosition, direction);
                connectedTiles.UnionWith(GetChainConnectedTiles(grid, adjacentTilePosition, connectedTiles));
            }

            return connectedTiles;
        }

        public static bool IsGridPositionAvailable(Dictionary<Vector2Int, CellInfo> grid, Vector2Int gridPosition)
        {
            if (!grid.ContainsKey(gridPosition)) return false;

            var cellInfo = grid[gridPosition];
            return cellInfo.Tile == null;
        }

        public static Vector2 GetGridCellWorldPosition(Dictionary<Vector2Int, CellInfo> grid, Vector2Int gridPosition)
        {
            if (!grid.ContainsKey(gridPosition))
                //handle error
                return Vector2.zero;

            var cellInfo = grid[gridPosition];
            return cellInfo.WorldPosition;
        }

        public static Dictionary<Vector2Int, CellInfo> GenerateGridCells(Vector2 initialPosition, Vector3 parentScale,
            GridSetupData gridSetupData)
        {
            var newGrid = new Dictionary<Vector2Int, CellInfo>();
            for (var row = 0; row < gridSetupData.Rows; row++)
            for (var col = 0; col < gridSetupData.Columns; col++)
            {
                var gridPos = new Vector2Int(col, row);
                // Calculate the position of the current block
                var x = initialPosition.x + gridSetupData.BlockDimensions.x * col * parentScale.x +
                        gridSetupData.BlockSpaceBetween.x * col;
                var y = initialPosition.y + gridSetupData.BlockDimensions.y * row * parentScale.y +
                        gridSetupData.BlockSpaceBetween.y * row;
                Vector2 worldPosition = new Vector3(x, y);
                var cell = new CellInfo(worldPosition);
                newGrid[gridPos] = cell;
            }

            return newGrid;
        }

        public static bool IsPositionPartOfGrid(Dictionary<Vector2Int, CellInfo> grid, Vector2Int gridPosition)
        {
            return grid.ContainsKey(gridPosition);
        }

        public static bool TryGetTileAtGridPosition(Dictionary<Vector2Int, CellInfo> grid, Vector2Int gridPosition,
            out Tile tile)
        {
            tile = null;
            if (!grid.TryGetValue(gridPosition, out var cellInfo)) return false;

            tile = cellInfo.Tile;
            return tile != null;
        }
        
        //return all matched positions for matches that are made int the grid with the grid Positions To Check hashset
         public static HashSet<Vector2Int> CheckForMatches(Dictionary<Vector2Int, CellInfo> grid, HashSet<Vector2Int> gridPositionsToCheck, int minNumberToMatch)
         {
             var gridPositionsMatched = new HashSet<Vector2Int>();
             foreach (var gridPositionToCheck in gridPositionsToCheck)
             {
                 var connectedGridPosition = GetChainConnectedTiles(grid, gridPositionToCheck);
                 if (connectedGridPosition.Count >= minNumberToMatch)
                     gridPositionsMatched.UnionWith(connectedGridPosition);
             }
        
             return gridPositionsMatched;
         }

        public class CellInfo
        {
            public Tile Tile;

            public TileData.TileColor TileColor = TileData.TileColor.None;
            public Vector2 WorldPosition;

            public CellInfo(Vector2 worldPosition)
            {
                WorldPosition = worldPosition;
            }
        }
    }
}