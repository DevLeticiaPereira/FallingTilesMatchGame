using UnityEngine;
using Utilities;

namespace Grid.TileStates
{
    public class FallingState : TileState
    {
        private Vector2 _targetWorldPosition;
        private Vector2Int _currentGridTemporaryPosition;
        private float _currentFallSpeed;

        //todo add to inspector, to we can set it up
        private readonly float _defaultFallSpeed = 2.0f;
        private readonly float _boostedFallSpeed = 4.0f;
        private readonly float _horizontalMoveSpeed = 8.0f;

        private bool _isAiControlled;
        private GridManager _gridManager;
        
        public FallingState(Tile tileOwner, StateMachine<TileState> tileStateMachine, GridManager gridManager) : base(tileOwner,
            tileStateMachine)
        {
            _isAiControlled = gridManager.IsAiControlled;
            _gridManager = gridManager;
        }
        
        public override void Enter()
        {
            base.Enter();
            if (!TryUpdateGridTarget())
            {
                _tileStateMachine.ChangeState(TileOwner.PlacedOnGridTileState);
                return;
            }

            if (TileOwner.IsRoot)
            {
                TileOwner.SetFallingRootSprite();
            }
            _currentFallSpeed = _defaultFallSpeed;

            if (!_isAiControlled)
            {
                InputManager.MoveHorizontal += MoveHorizontal;
                InputManager.Rotate += Rotate;
            }
        }

        private void Rotate()
        {

        }

        public override void Update()
        {
            if (GameManager.Instance.StateMachine.CurrentState != GameManager.Instance.RunningState)
            {
                return;
            }

            base.Update();

            if (!TileOwner.TemporaryGridPosition.HasValue)
            {
                return;
            }
            
            // Calculate the movement direction towards the _nextWorldPosition
            Vector2 movementDirection = (_targetWorldPosition - (Vector2)TileOwner.transform.position).normalized;

            // Calculate the step for both x and y directions based on separate speeds
            float stepX = _horizontalMoveSpeed * Time.deltaTime * movementDirection.x;
            float stepY = _currentFallSpeed * Time.deltaTime * movementDirection.y;

            // Move the tile towards the _nextWorldPosition
            TileOwner.transform.position += new Vector3(stepX, stepY, 0f);

            // Check if the tile has reached the destination position
            if (Vector2.Distance(TileOwner.transform.position, _targetWorldPosition) > 0.05)
            {
                return;
            }
            
            // Check if next destination is available
            TileOwner.transform.position = _targetWorldPosition;
            if (TryUpdateGridTarget())
            {
                return;
            }
            // tile reached destination
            EventManager.InvokeTileReachedGrid(_gridManager.GridID, TileOwner.TemporaryGridPosition.Value, TileOwner);
        }

        private bool TryUpdateGridTarget()
        {
            if (!TileOwner.TemporaryGridPosition.HasValue)
                return false;

            var temporaryGridPosition = TileOwner.TemporaryGridPosition.Value;
            Vector2Int nextGridPosition = new Vector2Int(temporaryGridPosition.x, temporaryGridPosition.y - 1);
            if (!GridUtilities.IsGridPositionAvailable(_gridManager.Grid, nextGridPosition))
            {
                return false;
            }

            TileOwner.SetTemporaryGridPosition(nextGridPosition, false);
            _targetWorldPosition = GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, nextGridPosition);
            return true;
        }

        private void MoveHorizontal(InputManager.DragHorizontalDirection dragHorizontalDirection)
        {
            if (_isAiControlled) return;
            if (!TileOwner.TemporaryGridPosition.HasValue)
                return;
            Vector2Int newTargetGridPosition = TileOwner.TemporaryGridPosition.Value;
            
            switch (dragHorizontalDirection)
            {
                case InputManager.DragHorizontalDirection.Left:
                    --newTargetGridPosition.x;
                    break;
                case InputManager.DragHorizontalDirection.Right:
                    ++newTargetGridPosition.x;
                    break;
            }

            if (GridUtilities.IsGridPositionAvailable(_gridManager.Grid, newTargetGridPosition))
            {
                // Update the next grid and world positions based on the new grid position
                TileOwner.SetTemporaryGridPosition(newTargetGridPosition, false);
                _targetWorldPosition = GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, newTargetGridPosition);
            }
        }

        public override void Exit()
        {
            if (_isAiControlled)
            {
                InputManager.MoveHorizontal -= MoveHorizontal;
            }
        }
    }
}