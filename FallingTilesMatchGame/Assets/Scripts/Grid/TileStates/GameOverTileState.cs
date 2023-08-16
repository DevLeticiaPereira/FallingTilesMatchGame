using UnityEngine;
using Utilities;

namespace Grid.TileStates
{
    public class GameOverTileState : TileState
    {
        private readonly float _speedFallingOutOFBounds = 10.0f;
        private readonly GridManager _gridManager;
        private float _yLimitPosition;

        public GameOverTileState(Tile tileOwner, StateMachine<TileState> tileStateMachine, GridManager gridManager) :
            base(tileOwner, tileStateMachine)
        {
            _gridManager = gridManager;
        }

        public override void Enter()
        {
            base.Enter();
            var gridDownCellPosition = GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, new Vector2Int(0, 0));
            _yLimitPosition = gridDownCellPosition.y;
            TileOwner.SetDefaultSprite();
        }

        public override void Update()
        {
            base.Update();

            // Move the tile towards the yLimitPosition 
            TileOwner.transform.position = new Vector3(TileOwner.transform.position.x,
                TileOwner.transform.position.y - _speedFallingOutOFBounds * Time.deltaTime, 0f);

            // Check if the tile has reached the destination position
            if (TileOwner.transform.position.y < _yLimitPosition)
            {
                TileOwner.Destroy();
            }
        }
    }
}