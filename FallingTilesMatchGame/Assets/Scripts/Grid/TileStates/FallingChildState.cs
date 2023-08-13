using Grid;
using Grid.TileStates;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

public class FallingChildState : TileState
{
    private Vector2 _targetWorldPosition;
    private Vector2Int _currentGridTemporaryPosition;
    private float _currentFallSpeed;

    //todo add to inspector, to we can set it up
    private readonly float _defaultFallSpeed = 2.0f;
    private readonly float _boostedFallSpeed = 4.0f;
    private readonly float _horizontalMoveSpeed = 8.0f;
    private readonly float _rotateMoveSpeed = 8.0f;
    private Vector2 _gridCellDimensions;
    private bool _isAiControlled;
    private GridManager _gridManager;
    private Tile _beginPair;
    private bool _isRotating;
    public TileData.TileConnections _currentPositionRelatedToTileRoot = TileData.TileConnections.Up;
    
    public FallingChildState(Tile tileOwner, StateMachine<TileState> tileStateMachine, GridManager gridManager) : base(tileOwner, tileStateMachine)
    {
        _isAiControlled = gridManager.IsAiControlled;
        _gridManager = gridManager;
        
    }

    public override void Enter()
    {
        base.Enter();
        _beginPair = TileOwner.BeginPairTile;
        _gridCellDimensions = _gridManager.GridInfo.BlockDimensions;
        if (!TileOwner.TemporaryGridPosition.HasValue)
        {
            Debug.Log("dont have value");
        }
        
        if (!_isAiControlled)
        {
            InputManager.Rotate += Rotate;
        }
        UpdateGridTarget();
    }
    
    public override void Update()
    {
        if (GameManager.Instance.StateMachine.CurrentState != GameManager.Instance.RunningState)
        {
            return;
        }

        base.Update();

        var currentGridPositionY = Mathf.CeilToInt(TileOwner.transform.position.y / _gridCellDimensions.y);
        var currentGridPositionX = Mathf.CeilToInt(TileOwner.transform.position.x / _gridCellDimensions.x);
        var newTemporaryGridPosition = new Vector2Int(currentGridPositionX, currentGridPositionY);
        if (newTemporaryGridPosition != TileOwner.TemporaryGridPosition.Value)
        {
            //Debug.Log($"Reached grid position {currentGridPositionX} : {currentGridPositionY} is root {TileOwner.IsRoot}");
            TileOwner.SetTemporaryGridPosition(newTemporaryGridPosition);
            UpdateGridTarget();
        }

        if (Vector2.Distance(TileOwner.transform.position, _targetWorldPosition) > 0.05)
        {
            return;
        }
        
        TileOwner.transform.SetParent(_gridManager.transform);
        TileOwner.transform.position = _targetWorldPosition;
        EventManager.InvokeTileReachedGrid(_gridManager.GridID, TileOwner.TemporaryGridPosition.Value, TileOwner);
    }
    
    private void Rotate()
    {
        var nextRotationDirection = GetNexRotateDirection();
        if (nextRotationDirection == TileData.TileConnections.None)
        {
            return;
        }

        var possibleGridPosition = GridUtilities.GetAdjacentGridPosition(_beginPair.TemporaryGridPosition.Value, nextRotationDirection);
        if (!GridUtilities.IsGridPositionAvailable(_gridManager.Grid, possibleGridPosition))
        {
            return;
        }
        _currentPositionRelatedToTileRoot = nextRotationDirection;
            
        var nextRotatePosition = new Vector3(); 
        switch (nextRotationDirection)
        {
            case TileData.TileConnections.Right:
                nextRotatePosition = new Vector3(TileOwner.transform.localPosition.y, 0);
                break;
            case TileData.TileConnections.Left:
                nextRotatePosition = new Vector3(TileOwner.transform.localPosition.y, 0);
                break;
            case TileData.TileConnections.Up:
                nextRotatePosition = new Vector3( 0,-TileOwner.transform.localPosition.x);
                break;
            case TileData.TileConnections.Down:
                nextRotatePosition = new Vector3( 0,-TileOwner.transform.localPosition.x);
                break;
        }
        TileOwner.transform.localPosition = nextRotatePosition;
    }

    public void UpdateGridTarget()
    {
        bool foundValidPos = false;
        Vector2Int firstAvailablePosition = new Vector2Int(); 
        for (int i = 0; i < TileOwner.TemporaryGridPosition.Value.y; ++i)
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
        Debug.Log($"_targetWorldPosition {firstAvailablePosition} isRoot: {TileOwner.IsRoot}");
        _targetWorldPosition = GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, firstAvailablePosition);
    }

    private TileData.TileConnections GetNexRotateDirection()
    {
        switch (_currentPositionRelatedToTileRoot)
        {
            case TileData.TileConnections.Right:
                return TileData.TileConnections.Down;
            case TileData.TileConnections.Left:
                return TileData.TileConnections.Up;
            case TileData.TileConnections.Up:
                return TileData.TileConnections.Right;
            case TileData.TileConnections.Down:
                return TileData.TileConnections.Left;
        }

        return TileData.TileConnections.None;
    }
    
    public override void Exit()
    {
        base.Exit();
        InputManager.Rotate -= Rotate;
    }
}
