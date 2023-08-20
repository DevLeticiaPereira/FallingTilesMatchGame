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
    private float _moveHorizontalMinTime;

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
        _moveHorizontalMinTime = GameManager.Instance.GameSettings.MoveTimeBetweenColumns + 0.5f;
    }

    private void TileReachedGrid(Guid gridID, Vector2Int gridPosition, Tile tile)
    {
        if (gridID != _gridManager.GridID)
        {
            return;
        }
        
        StopCoroutine("MoveTileHorizontal");
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
            Debug.Log("Random tile and place");
        }
        Debug.Log($"Maching Tile {(tileToPlace.IsRoot ? "root" : "child")} to grid position {positionToPlace}");
        StartCoroutine(ControlTile(tileToPlace, positionToPlace));
    }

    private IEnumerator ControlTile(Tile tile, Vector2Int targetPosition)
    {
        Tile pair = tile == _rootTile ? _childTile : _rootTile;
      
        while (TileUtilities.GetTileGridTarget(_gridManager, tile, pair) != targetPosition)
        {
            if (_rootTile == null || _childTile == null)
            {
             break;   
            }
            
            if (!_rootTile.TemporaryGridPosition.HasValue || !_childTile.TemporaryGridPosition.HasValue)
            {
                break;
            }

            if (tile.TemporaryGridPosition.Value.x != targetPosition.x)
            {
                var dragDirection = tile.TemporaryGridPosition.Value.x > targetPosition.x
                    ? InputManager.DragDirection.Left
                    : InputManager.DragDirection.Right;
                EventManager.InvokeMoveHorizontal(_gridManager.GridID, dragDirection);
            }
            else
            {
                EventManager.InvokeRotate(_gridManager.GridID);
               
            }
            yield return new WaitForSeconds(_moveHorizontalMinTime + 0.1f);
        }
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
    
    private List<Vector2Int> GetTopTilesPositionsInColumns()
    {
        List<Vector2Int> TopTilesInColumns = new List<Vector2Int>();
        for (int i = 0; i < _gridColumns; i++)
        {
            for (int j = 0; j < _gridRows + 1; j++)
            {
                var positionToCheck = new Vector2Int(i, j);
                var tileAbove = new Vector2Int(i, j + 1);
                
                if (GridUtilities.IsGridPositionAvailable(GridCopy, tileAbove))
                {
                    TopTilesInColumns.Add(positionToCheck);
                    break;
                }
            }
        }

        return TopTilesInColumns;
    }
    
    private Tuple<Vector2Int, Tile> ChoseTileToPlaceAndPosition()
    {
        List<Vector2Int> topTilesPositionsInColumns = GetTopTilesPositionsInColumns();
        int numberOfMatches = 0;
        Vector2Int positionToPlace = new Vector2Int(-1,-1);
        Tile tileMatching = null;

        foreach (var tilePosition in topTilesPositionsInColumns)
        {
            Dictionary<Vector2Int, GridUtilities.CellInfo> simulatedGrid = new Dictionary<Vector2Int, GridUtilities.CellInfo>(GridCopy);
            if (!GridUtilities.TryGetTileAtGridPosition(simulatedGrid, tilePosition, out Tile tile))
            {
                continue;
            }

            if (tile.Data.ColorTile == _rootTile.Data.ColorTile)
            {
                var matches = GridUtilities.CheckForMatches(simulatedGrid, new HashSet<Vector2Int>(){tilePosition}, 1);
                if (matches.Count > numberOfMatches)
                {
                    numberOfMatches = matches.Count;
                    positionToPlace = tilePosition + new Vector2Int(0,1);
                    tileMatching = _rootTile;
                }

                int shouldReplace = UnityEngine.Random.Range(0, 2);
                if (matches.Count == numberOfMatches && shouldReplace == 1)
                {
                    numberOfMatches = matches.Count;
                    positionToPlace = tilePosition + new Vector2Int(0,1);
                    tileMatching = _rootTile;
                }
            }
            
            if (tile.Data.ColorTile == _childTile.Data.ColorTile)
            {
                var matches = GridUtilities.CheckForMatches(simulatedGrid, new HashSet<Vector2Int>(){tilePosition}, 1);
                if (matches.Count > numberOfMatches)
                {
                    numberOfMatches = matches.Count;
                    positionToPlace = tilePosition + new Vector2Int(0,1);
                    tileMatching = _childTile;
                }
                
                int shouldReplace = UnityEngine.Random.Range(0, 2);
                if (matches.Count == numberOfMatches && shouldReplace == 1)
                {
                    numberOfMatches = matches.Count;
                    positionToPlace = tilePosition + new Vector2Int(0,1);
                    tileMatching = _childTile;
                }
            }
        }
       
        return new Tuple<Vector2Int, Tile>(positionToPlace, tileMatching);
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
