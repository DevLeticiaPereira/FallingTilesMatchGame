using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grid.TileStates
{
    public class WaitingState : TileState
    {
        private Guid _gridID;
        public WaitingState(Tile tileOwner, StateMachine<TileState> tileStateMachine, Guid gridID) : base(tileOwner,
            tileStateMachine)
        {
            _gridID = gridID;
        }

        public override void Enter()
        {
            base.Enter();
            EventManager.EventPlacedTileAtGridPosition += OnEventPlacedTileAtGridPosition;
        }

        private void OnEventPlacedTileAtGridPosition(Guid gridID, Dictionary<Tile, Vector2Int> tilesToPlaceInfo)
        {
            if (_gridID != gridID || !tilesToPlaceInfo.ContainsKey(TileOwner))
            {
                return;
            }

            var gridPosition = tilesToPlaceInfo[TileOwner];
            TileOwner.SetTemporaryGridPosition(gridPosition, true);
            TileOwner.TileStateMachine.ChangeState(TileOwner.FallingTileState);
        }

        public override void Exit()
        {
            base.Exit();
            EventManager.EventPlacedTileAtGridPosition -= OnEventPlacedTileAtGridPosition;
        }
    }
}