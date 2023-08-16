using UnityEngine;
using Utilities;

namespace Grid.TileStates
{
    public class FallingState : TileState
    {
        private Vector2 _gridCellDimensions;
        private readonly GridManager _gridManager;
        private Vector2Int _gridPositionsOutOfBounds;
        private float _singleTileFallSpeed;
        private Vector2 _targetWorldPosition;

        public FallingState(Tile tileOwner, StateMachine<TileState> tileStateMachine, GridManager gridManager) : base(
            tileOwner, tileStateMachine)
        {
            _gridManager = gridManager;
        }

        public override void Enter()
        {
            base.Enter();
            _singleTileFallSpeed = GameManager.Instance.GameSettings.SingleTileFallSpeed;
            TileOwner.SetDefaultSprite();
            _gridCellDimensions = _gridManager.GridInfo.BlockDimensions;
            UpdateGridTarget();
        }

        private void UpdateGridTarget()
        {
            var foundValidPos = false;
            var firstAvailablePosition = new Vector2Int();
            for (var i = 0; i <= _gridManager.GridInfo.Rows; ++i)
            {
                var positionToCheck = new Vector2Int(TileOwner.TemporaryGridPosition.Value.x, i);
                if (GridUtilities.IsGridPositionAvailable(_gridManager.Grid, positionToCheck))
                {
                    firstAvailablePosition = positionToCheck;
                    foundValidPos = true;
                    break;
                }
            }

            if (!foundValidPos)
            {
                Debug.Log("Cannot find Next valid position");
                return;
            }

            _targetWorldPosition = GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, firstAvailablePosition);
        }

        public override void Update()
        {
            if (GameManager.Instance.StateMachine.CurrentState != GameManager.Instance.RunningState) return;

            base.Update();

            if (TryUpdateTileTemporaryGridPosition()) UpdateGridTarget();

            if (Vector2.Distance(TileOwner.transform.position, _targetWorldPosition) <
                _singleTileFallSpeed * Time.deltaTime)
            {
                HandleTileReachedGrid();
                return;
            }

            var movementDirection = (_targetWorldPosition - (Vector2)TileOwner.transform.position).normalized;
            var stepY = _singleTileFallSpeed * Time.deltaTime * movementDirection.y;
            TileOwner.transform.position += new Vector3(0, stepY, 0f);
        }

        private void HandleTileReachedGrid()
        {
            TileOwner.transform.SetParent(_gridManager.transform);
            TileOwner.transform.position = _targetWorldPosition;
            EventManager.InvokeTileReachedGrid(_gridManager.GridID, TileOwner.TemporaryGridPosition.Value, TileOwner);
        }

        private bool TryUpdateTileTemporaryGridPosition()
        {
            var currentGridPositionY = Mathf.CeilToInt(TileOwner.transform.position.y / _gridCellDimensions.y);
            var currentGridPositionX = Mathf.CeilToInt(TileOwner.transform.position.x / _gridCellDimensions.x);
            var newTemporaryGridPosition = new Vector2Int(currentGridPositionX, currentGridPositionY);
            if (newTemporaryGridPosition != TileOwner.TemporaryGridPosition.Value)
            {
                TileOwner.SetTemporaryGridPosition(newTemporaryGridPosition);
                return true;
            }

            return false;
        }
    }
}