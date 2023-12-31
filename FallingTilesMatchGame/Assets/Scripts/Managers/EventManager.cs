using System;
using System.Collections.Generic;
using Grid;
using UnityEngine;

public static class EventManager
{
    #region INPUTS

    public static event Action<Guid> EventRotate;
    public static event Action<Guid, InputManager.DragDirection> EventMoveHorizontal;
    public static event Action<Guid, bool> EventAccelerate;

    #endregion INPUTS

    #region GRID/TILE COMMUNICATION

    public static event Action<Guid, Vector2Int, Tile> EventTileReachedGrid;
    public static event Action<Guid, Vector2Int, Tile> EventDroppedTileReachedGrid;
    public static event Action<Guid, HashSet<Tile>> EventPlacedTileAtGridStartPoint;
    public static event Action<Guid, HashSet<Vector2Int>> EventTilesAddedToGrid;
    public static event Action<Guid, Dictionary<Vector2Int, Vector2Int>> EventTilesDroppedFromGrid;
    public static event Action<Guid, HashSet<Vector2Int>> EventTilesMatched;
    public static event Action<Guid> EventTileDestroyed;
    public static event Action<Guid, Vector2Int> EventShouldFallFromPosition;

    #endregion

    #region GAME GENERAL EVENTS

    public static event Action EventSinglePlayerGameMode;
    public static event Action EventMultiPlayerGameMode;
    public static event Action<Guid> EventGridGameOver;
    public static event Action<Guid, int> EventScore;
    public static event Action<int> EventUpdateHighScore;
    public static event Action EventExitedGameplayScene;

    #endregion

    #region INVOKE FUNCTIONS

    public static void InvokeEventSinglePlayerGameMode()
    {
        EventSinglePlayerGameMode?.Invoke();
    }
    
    public static void InvokeEventMultiPlayerGameMode()
    {
        EventMultiPlayerGameMode?.Invoke();
    }

    public static void InvokeMoveHorizontal(Guid gridID, InputManager.DragDirection dragDirection)
    {
        EventMoveHorizontal?.Invoke(gridID, dragDirection);
    }

    public static void InvokeAccelerate(Guid gridID, bool active)
    {
        EventAccelerate?.Invoke(gridID, active);
    }

    public static void InvokeRotate(Guid gridID)
    {
        EventRotate?.Invoke(gridID);
    }

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
        EventPlacedTileAtGridStartPoint?.Invoke(gridID, tilesMoved);
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

    public static void InvokeExitedGameplayScene()
    {
        EventExitedGameplayScene?.Invoke();
    }

    public static void InvokeEventUpdateHighScore(int highScore)
    {
        EventUpdateHighScore?.Invoke(highScore);
    }

    #endregion

    #region UNSUBSCRIBE ALL FUNCTIONS

    public static void UnsubscribeAllRotate()
    {
        if (EventRotate == null) return;

        var delegates = EventRotate?.GetInvocationList();
        foreach (var del in delegates)
            if (del is Action<Guid> listener)
                EventRotate -= listener;
    }

    public static void UnsubscribeAllMoveHorizontal()
    {
        if (EventMoveHorizontal == null) return;
        var delegates = EventMoveHorizontal?.GetInvocationList();
        foreach (var del in delegates)
            if (del is Action<Guid, InputManager.DragDirection> listener)
                EventMoveHorizontal -= listener;
    }

    #endregion
}