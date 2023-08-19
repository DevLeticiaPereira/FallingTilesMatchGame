using System;
using System.Collections;
using UnityEngine;
using Utilities;

namespace Grid.TileStates
{
    public class FallingRootState : TileState
    {
        private Tile _beginPair;
        private readonly float _boostedFallSpeed;

        private readonly float _defaultFallSpeed;
        private Vector2 _gridCellDimensions;
        private readonly GridManager _gridManager;
        private Vector2 _inicialWorldPosition;
        private readonly bool _isPlayer;
        private bool _isRotating;
        private readonly float _moveTimeBetweenColumns;
        private Vector2 _targetWorldPosition;
        private bool _tileReachedGrid;

        public FallingRootState(Tile tileOwner, StateMachine<TileState> tileStateMachine,
            GridManager gridManager) : base(tileOwner,
            tileStateMachine)
        {
            _isPlayer = gridManager.IsPlayer;
            _gridManager = gridManager;
            var _gameSettings = GameManager.Instance.GameSettings;
            _defaultFallSpeed = _gameSettings.DefaultTileFallSpeed;
            _boostedFallSpeed = _gameSettings.BoostedTileFallSpeed;
            _moveTimeBetweenColumns = _gameSettings.MoveTimeBetweenColumns;
        }

        public float CurrentFallSpeed { get; private set; }

        public override void Enter()
        {
            base.Enter();
            if (_isPlayer)
            {
                EventManager.EventMoveHorizontal += MoveHorizontal;
                EventManager.EventAccelerate += Accelerate;
            }

            EventManager.EventTileReachedGrid += TileReachedGrid;
            _beginPair = TileOwner.BeginPairTile;
            TileOwner.SetFallingRootSprite();
            CurrentFallSpeed = _defaultFallSpeed;
            _gridCellDimensions = _gridManager.GridInfo.BlockDimensions;

            UpdateGridTarget();
        }

        public override void Exit()
        {
            base.Exit();
            if (_isPlayer)
            {
                EventManager.EventMoveHorizontal -= MoveHorizontal;
                EventManager.EventAccelerate -= Accelerate;
            }

            EventManager.EventTileReachedGrid -= TileReachedGrid;
        }

        public override void Update()
        {
            if (GameManager.Instance.StateMachine.CurrentState != GameManager.Instance.RunningState) return;

            base.Update();

            if (TryUpdateTileTemporaryGridPosition()) UpdateGridTarget();

            if (Vector2.Distance(TileOwner.transform.position, _targetWorldPosition) <
                CurrentFallSpeed * Time.deltaTime)
            {
                HandleTileReachedGrid();
                return;
            }

            var movementDirection = (_targetWorldPosition - (Vector2)TileOwner.transform.position).normalized;
            var stepY = CurrentFallSpeed * Time.deltaTime * movementDirection.y;
            TileOwner.transform.position += new Vector3(0, stepY, 0f);
        }

        private void HandleTileReachedGrid()
        {
            _tileReachedGrid = true;
            TileOwner.StopCoroutine("MoveToHorizontal");
            TileOwner.transform.SetParent(_gridManager.transform);
            TileOwner.transform.position = _targetWorldPosition;
            EventManager.InvokeTileReachedGrid(_gridManager.GridID, TileOwner.TemporaryGridPosition.Value, TileOwner);
        }

        private bool TryUpdateTileTemporaryGridPosition()
        {
            var positionRelatedToGrid = TileOwner.transform.position - _gridManager.transform.position;
            var currentGridPositionY = Mathf.FloorToInt(positionRelatedToGrid.y / _gridCellDimensions.y);
            var currentGridPositionX = Mathf.FloorToInt(positionRelatedToGrid.x / _gridCellDimensions.x);
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

            var positionComparisonRange = 0.1f;
            if (TileOwner.transform.position.y > _beginPair.transform.position.y &&
                Mathf.Abs(TileOwner.transform.position.y - _beginPair.transform.position.y) > positionComparisonRange)
                firstAvailablePosition += new Vector2Int(0, 1);
            _targetWorldPosition = GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, firstAvailablePosition);
        }

        private Vector2Int GetNextPossibleHorizontalGridPosition(
            InputManager.DragDirection dragDirection, Vector2Int gridPosition)
        {
            var newTargetGridPosition = gridPosition;
            --newTargetGridPosition.y;
            switch (dragDirection)
            {
                case InputManager.DragDirection.Left:
                    --newTargetGridPosition.x;
                    break;
                case InputManager.DragDirection.Right:
                    ++newTargetGridPosition.x;
                    break;
            }

            return newTargetGridPosition;
        }

        #region Events Callback

        private void TileReachedGrid(Guid gridId, Vector2Int gridPosition, Tile tile)
        {
            if (gridId != _gridManager.GridID) return;

            if (tile == _beginPair && !_tileReachedGrid)
            {
                TileOwner.TileStateMachine.ChangeState(TileOwner.FallingTileState);
                TileOwner.StopCoroutine("MoveHorizontal");
            }
        }

        private void Accelerate(bool active)
        {
            if (_tileReachedGrid) return;
            
            if (active)
                CurrentFallSpeed = _boostedFallSpeed;
            else
                CurrentFallSpeed = _defaultFallSpeed;
        }

        private void MoveHorizontal(InputManager.DragDirection dragDirection)
        {
            if (!_isPlayer) return;
            if (_tileReachedGrid) return;

            var newTargetGridPosition =
                GetNextPossibleHorizontalGridPosition(dragDirection, TileOwner.TemporaryGridPosition.Value);
            if (!GridUtilities.IsGridPositionAvailable(_gridManager.Grid, newTargetGridPosition)) return;

            var newTargetChildGridPosition =
                GetNextPossibleHorizontalGridPosition(dragDirection, _beginPair.TemporaryGridPosition.Value);
            if (!GridUtilities.IsGridPositionAvailable(_gridManager.Grid, newTargetChildGridPosition)) return;

            _targetWorldPosition = GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, newTargetGridPosition);
            TileOwner.StartCoroutine(MoveToHorizontal(_moveTimeBetweenColumns, _targetWorldPosition.x));
        }

        private IEnumerator MoveToHorizontal(float moveDuration, float targetHorizontalPosition)
        {
            var initialPosition = TileOwner.transform.position;
            float elapsedTime = 0;

            while (elapsedTime < moveDuration)
            {
                var t = elapsedTime / moveDuration;
                var target = new Vector3(targetHorizontalPosition, TileOwner.transform.position.y, 0);
                TileOwner.transform.position = Vector3.Lerp(initialPosition, target, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            TileOwner.transform.position = new Vector3(targetHorizontalPosition, TileOwner.transform.position.y, 0);
        }

        #endregion
    }
}