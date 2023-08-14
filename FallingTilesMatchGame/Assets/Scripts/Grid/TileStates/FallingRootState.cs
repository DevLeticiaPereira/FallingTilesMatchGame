using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities;

namespace Grid.TileStates
{
    public class FallingRootState : TileState
    {
        private Vector2 _targetWorldPosition;
        private Vector2 _inicialWorldPosition;
        
       private float _defaultFallSpeed = 2.0f; 
       private float _moveTimeBetweenColumns = 8.0f;

       private float _currentFallSpeed;
        private bool _isAiControlled;
        private GridManager _gridManager;
        private Tile _beginPair;
        private bool _isRotating;
        private Vector2 _gridCellDimensions;
        private bool _tileReachedGrid = false;

        public FallingRootState(Tile tileOwner, StateMachine<TileState> tileStateMachine, GridManager gridManager/*, Tile beginPair*/) : base(tileOwner,
            tileStateMachine)
        {
            _isAiControlled = gridManager.IsAiControlled;
            _gridManager = gridManager;
            var _gameSettings = GameManager.Instance.GameSettings;
            _defaultFallSpeed = _gameSettings.DefaultTileFallSpeed;
            _moveTimeBetweenColumns = _gameSettings.MoveTimeBetweenColumns;
        }

       
        public override void Enter()
        {
            base.Enter();
            if (!_isAiControlled)
            {
                EventManager.EventMoveHorizontal += MoveHorizontal;
            }
            
            EventManager.EventTileReachedGrid += TileReachedGrid;
            _beginPair = TileOwner.BeginPairTile;
            TileOwner.SetFallingRootSprite();
            _currentFallSpeed = _defaultFallSpeed;
            _gridCellDimensions = _gridManager.GridInfo.BlockDimensions;
            
            UpdateGridTarget();
        }

        private void TileReachedGrid(Guid gridId, Vector2Int gridPosition, Tile tile)
        {
            if (gridId != _gridManager.GridID)
            {
                return;
            }

            if (tile == _beginPair && !_tileReachedGrid)
            {
                TileOwner.TileStateMachine.ChangeState(TileOwner.FallingTileState);
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (!_isAiControlled)
            {
                EventManager.EventMoveHorizontal -= MoveHorizontal;
            }
            EventManager.EventTileReachedGrid -= TileReachedGrid;
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
            
            float stepY = _currentFallSpeed * Time.deltaTime /** movementDirection.y*/;
            TileOwner.transform.position -= new Vector3(0, stepY, 0f);
        }
        
        private void HandleTileReachedGrid()
        {
            _tileReachedGrid = true;
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
            
            float positionComparisonRange = 0.1f;
            if (TileOwner.transform.position.y > _beginPair.transform.position.y && Mathf.Abs(TileOwner.transform.position.y-_beginPair.transform.position.y) > positionComparisonRange)
            {
                firstAvailablePosition += new Vector2Int(0, 1);
            }
            
            _targetWorldPosition = GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, firstAvailablePosition);
        }

        private void MoveHorizontal(InputManager.DragHorizontalDirection dragHorizontalDirection)
        {
            if (_isAiControlled) return;
            
            var newTargetGridPosition = GetNextPossibleHorizontalGridPosition(dragHorizontalDirection,TileOwner.TemporaryGridPosition.Value);
            if (!GridUtilities.IsGridPositionAvailable(_gridManager.Grid, newTargetGridPosition))
            {
                return;
            }
            
            var newTargetChildGridPosition = GetNextPossibleHorizontalGridPosition(dragHorizontalDirection,_beginPair.TemporaryGridPosition.Value);
            if (!GridUtilities.IsGridPositionAvailable(_gridManager.Grid, newTargetChildGridPosition))
            {
                return;
            }
            
            _targetWorldPosition = GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, newTargetGridPosition);
            TileOwner.StartToMoveHorizontal(_moveTimeBetweenColumns, _targetWorldPosition.x);
        }

        private Vector2Int GetNextPossibleHorizontalGridPosition(InputManager.DragHorizontalDirection dragHorizontalDirection, Vector2Int gridPosition)
        {
            Vector2Int newTargetGridPosition = gridPosition;
            --newTargetGridPosition.y;
            switch (dragHorizontalDirection)
            {
                case InputManager.DragHorizontalDirection.Left:
                    --newTargetGridPosition.x;
                    break;
                case InputManager.DragHorizontalDirection.Right:
                    ++newTargetGridPosition.x;
                    break;
            }

            return newTargetGridPosition;
        }
    }
}