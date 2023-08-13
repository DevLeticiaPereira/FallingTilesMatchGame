using System;
using System.Collections.Generic;
using Grid;
using UnityEngine;

public static class EventManager
{
    public static event Action<Guid, Vector2Int, Tile> EventTileReachedGrid;
    public static event Action<Guid, Vector2Int, Tile> EventDroppedTileReachedGrid;
    public static event Action<Guid, HashSet<Tile>> EventPlacedTileAtGridPosition;
    public static event Action<Guid, HashSet<Vector2Int>> EventTilesAddedToGrid;
    public static event Action<Guid, Dictionary<Vector2Int, Vector2Int>> EventTilesDroppedFromGrid;
    public static event Action<Guid, HashSet<Vector2Int>> EventTilesMatched;
    public static event Action<Guid> EventGridGameOver;
    public static event Action<Guid> EventTileDestroyed;
    public static event Action<Guid, Vector2Int> EventShouldFallFromPosition;
    public static event Action<Guid, int> EventScore;

    
    public static void InvokeTileReachedGrid(Guid gridID, Vector2Int gridPosition, Tile tile)
    {
        EventTileReachedGrid?.Invoke(gridID, gridPosition, tile);
    }
    
    public static void InvokeDroppedTileReachedGrid(Guid gridID, Vector2Int gridPosition, Tile tile)
    {
        EventDroppedTileReachedGrid?.Invoke(gridID, gridPosition, tile);
    }

    public static void InvokePlacedTileAtGridStartPoint(Guid gridID, HashSet<Tile> tilesMoved)
    {
        EventPlacedTileAtGridPosition?.Invoke(gridID, tilesMoved);
    }

    public static void InvokeTilesMatched(Guid gridID, HashSet<Vector2Int> gridPositions)
    {
        EventTilesMatched?.Invoke(gridID, gridPositions);
    }
    
    public static void InvokeTilesDroppedFromGrid(Guid gridID, Dictionary<Vector2Int, Vector2Int> gridPositions)
    {
        EventTilesDroppedFromGrid?.Invoke(gridID, gridPositions);
    }
    
    public static void InvokeTilesAddedToGrid(Guid gridID, HashSet<Vector2Int> gridPositions)
    {
        EventTilesAddedToGrid?.Invoke(gridID, gridPositions);
    }

    public static void InvokeGridGameOver(Guid gridID)
    {
        EventGridGameOver?.Invoke(gridID);
    }

    public static void InvokeEventTileDestroyed(Guid gridID)
    {
        EventTileDestroyed?.Invoke(gridID);
    }

    public static void InvokeEventTileShouldFallFromPosition(Guid gridID, Vector2Int gridPosition)
    {
        EventShouldFallFromPosition?.Invoke(gridID, gridPosition);
    }

    public static void InvokeEventScore(Guid gridID, int newScore)
    {
        EventScore?.Invoke(gridID, newScore);
    }
}

