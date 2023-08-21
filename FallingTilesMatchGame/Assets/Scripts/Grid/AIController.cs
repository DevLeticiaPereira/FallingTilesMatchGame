using System;
using System.Collections;
using System.Collections.Generic;
using Grid;
using UnityEngine;
using Utilities;
using Random = Unity.Mathematics.Random;

public class AIController : MonoBehaviour
{
    private GridManager _gridManager;
    private int _gridRows;
    private int _gridColumns;
    private Tile _rootTile;
    private Tile _childTile;
    private float _timeBetweenInput;

    private Dictionary<Vector2Int, GridUtilities.CellInfo> GridCopy = new Dictionary<Vector2Int, GridUtilities.CellInfo>();

    private void OnEnable()
    {
        EventManager.EventPlacedTileAtGridStartPoint += PlacedTileAtGridStartPoint;
        EventManager.EventTileReachedGrid += TileReachedGrid;
    }

    private void OnDisable()
    {
        EventManager.EventPlacedTileAtGridStartPoint -= PlacedTileAtGridStartPoint;
        EventManager.EventTileReachedGrid -= TileReachedGrid;
    }

    public void Initialize(GridManager gridManager)
    {
        _gridManager = gridManager;
        _gridRows = _gridManager.GridInfo.Rows;
        _gridColumns = _gridManager.GridInfo.Columns;
        _timeBetweenInput = GameManager.Instance.GameSettings.MoveTimeBetweenColumns * 2;
    }

    private void TileReachedGrid(Guid gridID, Vector2Int gridPosition, Tile tile)
    {
        if (gridID != _gridManager.GridID)
        {
            return;
        }
        
        StopCoroutine("ControlTile");
        _rootTile = null;
        _childTile = null;
        GridCopy.Clear();
    }

    private void PlacedTileAtGridStartPoint(Guid gridID, HashSet<Tile> tilePair)
    {
        if (gridID != _gridManager.GridID)
        {
            return;
        }

        foreach (var gridPair in _gridManager.Grid)
        {
            GridUtilities.CellInfo newCell = new GridUtilities.CellInfo(gridPair.Value.WorldPosition);
            newCell.Tile = gridPair.Value.Tile;
            newCell.TileColor = gridPair.Value.TileColor;
            GridCopy.Add(gridPair.Key, newCell);
        }
        
        
        SetupRootAndChildTiles(tilePair);
        var tileToPlacePosition = ChoseTileToPlaceAndPosition();
        var positionToPlace = tileToPlacePosition.Item1;
        var tileToPlace = tileToPlacePosition.Item2;

        if (tileToPlace == null)
        {
            List<Vector2Int> tilesPossiblePositions = GetFirstEmptySpaceForEachColumn();
            if (positionToPlace ==  new Vector2Int(-1,-1))
            {
                int randomIndex = UnityEngine.Random.Range(0, tilesPossiblePositions.Count);
                positionToPlace = tilesPossiblePositions[randomIndex];
            }
            int randomTileHelper = UnityEngine.Random.Range(0, 2);
            tileToPlace = randomTileHelper == 0 ? _rootTile : _childTile;
        }
        StartCoroutine(ControlTile(tileToPlace, positionToPlace));
    }

    private IEnumerator ControlTile(Tile tile, Vector2Int targetPosition)
    {
        TileData.TileConnections targetPositionRelatedToTileRoot = GetTargetDirection(tile, targetPosition);

        while (_childTile != null && _childTile.FallingTileChildState.CurrentPositionRelatedToTileRoot != targetPositionRelatedToTileRoot 
               && targetPosition.y < tile.TemporaryGridPosition.Value.y)
        {
            EventManager.InvokeRotate(_gridManager.GridID);
            yield return new WaitForSeconds(_timeBetweenInput);
        }
       
        var dragDirection = tile.TemporaryGridPosition.Value.x > targetPosition.x
            ? InputManager.DragDirection.Left
            : InputManager.DragDirection.Right;
        int numberOfInteractions = Mathf.Abs(tile.TemporaryGridPosition.Value.x - targetPosition.x);
        
        while (numberOfInteractions > 0 && targetPosition.y < tile.TemporaryGridPosition.Value.y)
        {
            EventManager.InvokeMoveHorizontal(_gridManager.GridID, dragDirection);
            --numberOfInteractions;
            yield return new WaitForSeconds(_timeBetweenInput);
        }
    }

    private TileData.TileConnections GetTargetDirection(Tile tile, Vector2Int targetPosition)
    {
        List<TileData.TileConnections> possiblePosition = new List<TileData.TileConnections>()
        {
            TileData.TileConnections.Left,
            TileData.TileConnections.Down,
            TileData.TileConnections.Right,
            TileData.TileConnections.Up
        };

        if (tile == _childTile)
        {
            possiblePosition.Remove(TileData.TileConnections.Down);
            if (targetPosition.x == _gridColumns - 1)
            {
                possiblePosition.Remove(TileData.TileConnections.Left);
            }
        }

        if (tile == _rootTile)
        {
            possiblePosition.Remove(TileData.TileConnections.Up);
            if (targetPosition.x == 0)
            {
                possiblePosition.Remove(TileData.TileConnections.Right);
            }
        }

        int indexToGet = UnityEngine.Random.Range(0, possiblePosition.Count);
        return possiblePosition[indexToGet];
    }

    private List<Vector2Int> GetFirstEmptySpaceForEachColumn()
    {
        List<Vector2Int> FirstEmptySpaceColumns = new List<Vector2Int>();
        for (int i = 0; i < _gridColumns; i++)
        {
            for (int j = 0; j < _gridRows; j++)
            {
                var positionToCheck = new Vector2Int(i, j);
                if (GridUtilities.IsGridPositionAvailable(_gridManager.Grid, positionToCheck))
                {
                    FirstEmptySpaceColumns.Add(positionToCheck);
                    break;
                }
            }
        }

        return FirstEmptySpaceColumns;
    }

    private Tuple<Vector2Int, Tile> ChoseTileToPlaceAndPosition()
    {
        List<Vector2Int> firstEmptyPositionForEachColumn = GetFirstEmptySpaceForEachColumn();
        int numberOfMatches = 0;
        Vector2Int positionToPlace = new Vector2Int(-1,-1);
        Tile tileMatching = null;

        foreach (var position in firstEmptyPositionForEachColumn)
        {
            var neighborsTiles = GetNeighborsTiles(position, GridCopy);
            foreach (var neighborsTile in neighborsTiles)
            {
                HandleNeighboursMatches(neighborsTile, position, _rootTile);
                HandleNeighboursMatches(neighborsTile, position, _childTile);
            }
        }

        void HandleNeighboursMatches(Tuple<Vector2Int, Tile> tuple, Vector2Int targetPosition, Tile tilePairToPlace)
        {
            if (tuple.Item2.Data.ColorTile == tilePairToPlace.Data.ColorTile)
            {
                var matches = GridUtilities.CheckForMatches(GridCopy, new HashSet<Vector2Int>() { tuple.Item1 }, 1);
                if (matches.Count > numberOfMatches)
                {
                    numberOfMatches = matches.Count;
                    positionToPlace = targetPosition;
                    tileMatching = tilePairToPlace;
                }
            }
        }
        
        return new Tuple<Vector2Int, Tile>(positionToPlace, tileMatching);
    }

    private static HashSet<Tuple<Vector2Int,Tile>> GetNeighborsTiles(Vector2Int tilePosition, Dictionary<Vector2Int, GridUtilities.CellInfo> simulatedGrid)
    {
        var neighborsTiles = new HashSet<Tuple<Vector2Int,Tile>>();
        foreach (TileData.TileConnections direction in Enum.GetValues(typeof(TileData.TileConnections)))
        {
            if (direction == TileData.TileConnections.Up || direction == TileData.TileConnections.None)
            {
                continue;
            }

            var adjacentTilePosition = GridUtilities.GetAdjacentGridPosition(tilePosition, direction);
            if (!GridUtilities.TryGetTileAtGridPosition(simulatedGrid, adjacentTilePosition, out Tile adjacentTile))
            {
                continue;
            }

            neighborsTiles.Add(new Tuple<Vector2Int, Tile>(adjacentTilePosition, adjacentTile));
        }

        return neighborsTiles;
    }
    
    private void SetupRootAndChildTiles(HashSet<Tile> tilePair)
    {
        foreach (var tile in tilePair)
        {
            if (tile.IsRoot)
            {
                _rootTile = tile;
            }
            
            else
            {
                _childTile = tile;
            }
            
            if (_childTile != null && _rootTile != null)
            {
                break;
            }
        }
    }
}
