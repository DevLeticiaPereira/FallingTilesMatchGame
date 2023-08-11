using System;
using System.Collections.Generic;
using Grid;
using UnityEngine;

public static class EventManager
{
    public static event Action<Guid, Vector2Int, Tile> EventTileReachedGrid;
    public static event Action<Guid, Dictionary<Tile, Vector2Int>> EventPlacedTileAtGridPosition;
    public static event Action<Guid, Vector2Int, TileData.TileColor> EventGridHasChanged;
    public static event Action<Guid> EventGridGameOver;

    public static void InvokeTileReachedGrid(Guid gridID, Vector2Int gridPosition, Tile tile)
    {
        EventTileReachedGrid?.Invoke(gridID, gridPosition, tile);
    }

    public static void InvokePlacedTileAtGridPosition(Guid gridID, Dictionary<Tile, Vector2Int> tilesMoved)
    {
        EventPlacedTileAtGridPosition?.Invoke(gridID, tilesMoved);
    }
    
    public static void InvokeGridHasChanged(Guid gridID, Vector2Int gridPosition, TileData.TileColor color)
    {
        EventGridHasChanged?.Invoke(gridID, gridPosition, color);
    }

    public static void InvokeGridGameOver(Guid gridID)
    {
        EventGridGameOver?.Invoke(gridID);
    }
}