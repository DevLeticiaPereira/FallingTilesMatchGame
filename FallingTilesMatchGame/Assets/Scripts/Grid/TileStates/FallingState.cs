using System;
using UnityEngine;
using Utilities;

namespace Grid.TileStates
{
    public class FallingState : TileState
    {
        private readonly float _droppingFallSpeed = 5.0f;
        
        private GridManager _gridManager;
        private Vector2 _gridCellDimensions;
        private Vector2 _targetWorldPosition;
        private Vector2Int _gridPositionsOutOfBounds;
        
        public FallingState(Tile tileOwner, StateMachine<TileState> tileStateMachine, GridManager gridManager) : base(tileOwner, tileStateMachine)
        {
            _gridManager = gridManager;
        }
        public override void Enter()
        {
            base.Enter();
            TileOwner.SetDefaultSprite();
            _gridCellDimensions = _gridManager.GridInfo.BlockDimensions;
            EventManager.EventTileReachedGrid += TileReachedGrid;
             UpdateGridTarget();
        }

        public override void Exit()
        {
            base.Exit();
            EventManager.EventTileReachedGrid -= TileReachedGrid;
        }

        private void TileReachedGrid(Guid gridId, Vector2Int gridPosition, Tile tile)
        {
            if (gridId != _gridManager.GridID)
            {
                return;
            }

        }
        
        private void UpdateGridTarget()
        {
            bool foundValidPos = false;
            Vector2Int firstAvailablePosition = new Vector2Int(); 
            for (int i = 0; i <= TileOwner.TemporaryGridPosition.Value.y; ++i)
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
            if (GameManager.Instance.StateMachine.CurrentState != GameManager.Instance.RunningState)
            {
                return;
            }

            base.Update();

            if (Vector2.Distance(TileOwner.transform.position, _targetWorldPosition) < 0.05)
            {
                HandleTileReachedGrid();
                return;
            }
            
            if (TryUpdateTileTemporaryGridPosition())
            {
                UpdateGridTarget();
            }
            
            ///Vector2 movementDirection = (_targetWorldPosition - (Vector2)TileOwner.transform.position).normalized;
            float stepY = _droppingFallSpeed * Time.deltaTime /** movementDirection.y*/;
            TileOwner.transform.position -= new Vector3(0, stepY, 0f);
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