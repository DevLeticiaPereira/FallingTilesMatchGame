using System;
using System.Collections;
using System.Collections.Generic;
using Grid;
using UnityEngine;
using Utilities;

public class AIController : MonoBehaviour
{
    private GridManager _gridManager;
    private int _gridRows;
    private int _gridColumns;
    private Tile _rootTile;
    private Tile _childTile;
    private float _moveHorizontalMinTime;

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
    }

    private void PlacedTileAtGridStartPoint(Guid gridID, HashSet<Tile> tilePair)
    {
        if (gridID != _gridManager.GridID)
        {
            return;
        }

        SetupRootAndChildTiles(tilePair);
        
        List<Vector2Int> firstEmptySpaceForEachColumn = GetFirstEmptySpaceForEachColumn();
        int randomIndex = UnityEngine.Random.Range(0, firstEmptySpaceForEachColumn.Count);
        var chosenPosition = firstEmptySpaceForEachColumn[randomIndex];

        StartCoroutine(MoveTileHorizontal(chosenPosition));
    }
    
    private IEnumerator MoveTileHorizontal(Vector2Int destiny)
    {
        while (_rootTile.TemporaryGridPosition.Value.x != destiny.x || _childTile.TemporaryGridPosition.Value.x != destiny.x)
        {
            if (_rootTile == null || _childTile == null)
            {
             break;   
            }
            
            if (!_rootTile.TemporaryGridPosition.HasValue || !_childTile.TemporaryGridPosition.HasValue)
            {
                break;
            }
            
            var dragDirection = _rootTile.TemporaryGridPosition.Value.x > destiny.x
                ? InputManager.DragDirection.Left
                : InputManager.DragDirection.Right;
            
            EventManager.InvokeMoveHorizontal(_gridManager.GridID, dragDirection);
            yield return new WaitForSeconds(_moveHorizontalMinTime);
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
    
    private Vector2Int CalculateBestMove()
    {
        return new Vector2Int();
    }
}
