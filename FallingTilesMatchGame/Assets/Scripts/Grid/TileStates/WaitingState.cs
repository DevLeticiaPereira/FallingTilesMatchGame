using System;
using System.Collections.Generic;
using Utilities;

namespace Grid.TileStates
{
    public class WaitingState : TileState
    {
        private readonly GridManager _gridManager;

        public WaitingState(Tile tileOwner, StateMachine<TileState> tileStateMachine, GridManager gridManager) : base(
            tileOwner,
            tileStateMachine)
        {
            _gridManager = gridManager;
        }

        public override void Enter()
        {
            base.Enter();
            EventManager.EventPlacedTileAtGridStartPoint += OnEventPlacedTileAtGridStartPoint;
        }

        private void OnEventPlacedTileAtGridStartPoint(Guid gridID, HashSet<Tile> tilesToPlaceInfo)
        {
            if (_gridManager.GridID != gridID || !tilesToPlaceInfo.Contains(TileOwner)) return;

            TileOwner.transform.position =
                GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, TileOwner.TemporaryGridPosition.Value);

            if (TileOwner.IsRoot)
            {
                TileOwner.TileStateMachine.ChangeState(TileOwner.FallingRootTileState);
                return;
            }

            TileOwner.TileStateMachine.ChangeState(TileOwner.FallingTileChildState);
        }

        public override void Exit()
        {
            base.Exit();
            EventManager.EventPlacedTileAtGridStartPoint -= OnEventPlacedTileAtGridStartPoint;
        }
    }
}