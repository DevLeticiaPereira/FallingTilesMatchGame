using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class InputManager : Singleton<InputManager>
{
    public enum DragDirection
    {
        None,
        Left,
        Right,
        Down
    }

    private bool _draggingDown;
    private float _horizontalMovementCooldown;
    private InputAction _moveInputAction;
    private bool _movingHorizontal;

    private PlayerInput _playerInput;
    private InputAction _singleTouchInputAction;
    private Vector2 _touchLastPosition;
    private Guid _playerGridID;
    
    public bool PlayerInputEnabled { get; private set; }

    #region Public Methods

    public void EnablePlayerInput(bool enable)
    {
        PlayerInputEnabled = enable;
    }

    #endregion

    #region Monobehavior

    protected override void Awake()
    {
        base.Awake();
        _playerInput = GetComponent<PlayerInput>();
        _singleTouchInputAction = _playerInput.actions.FindAction("SingleTouch");
        _moveInputAction = _playerInput.actions.FindAction("Move");
    }

    private void Start()
    {
        PlayerInputEnabled = false;
        _horizontalMovementCooldown = GameManager.Instance.GameSettings.MoveTimeBetweenColumns;
    }

    private void OnEnable()
    {
        EventManager.EventExitedGameplayScene += OnExitedGameplayScene;
        _singleTouchInputAction.performed += TryToRotate;
        _moveInputAction.performed += TryToMove;

        RunningGameState.OnGameStartRunning += () => { EnablePlayerInput(true); };
        RunningGameState.OnGameStopRunning += () => { EnablePlayerInput(false); };
    }

    private void OnDestroy()
    {
        EventManager.EventExitedGameplayScene -= OnExitedGameplayScene;
        _singleTouchInputAction.performed -= TryToRotate;
        _moveInputAction.performed -= TryToMove;
        RunningGameState.OnGameStartRunning -= () => { EnablePlayerInput(true); };
        RunningGameState.OnGameStopRunning -= () => { EnablePlayerInput(false); };
    }

    #endregion

    public void SetPlayerGridID(Guid gridID)
    {
        _playerGridID = gridID;
    }

    #region Input Handling - Events

    private void TryToRotate(InputAction.CallbackContext context)
    {
        if (!PlayerInputEnabled) return;

        var touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        if (!IsTouchOverUI(touchPosition) && PlayerInputEnabled) EventManager.InvokeRotate(_playerGridID);
    }

    private void TryToMove(InputAction.CallbackContext context)
    {
        if (!PlayerInputEnabled) return;

        var touchPhase = Touchscreen.current.primaryTouch.phase.ReadValue();
        var currentTouchPos = context.ReadValue<Vector2>();

        if (touchPhase is TouchPhase.Ended or TouchPhase.Canceled && _draggingDown)
        {
            EventManager.InvokeAccelerate(_playerGridID, false);
            _draggingDown = false;
            return;
        }

        if (IsTouchOverUI(currentTouchPos)) return;

        if (touchPhase == TouchPhase.Began)
        {
            _touchLastPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            return;
        }

        if (_movingHorizontal)
        {
            return;
        }
        
        var direction =  currentTouchPos -_touchLastPosition;
        var dragDirection = GetDragDirection(direction);
        if (dragDirection is DragDirection.Left or DragDirection.Right)
        {
            _draggingDown = false;
            EventManager.InvokeAccelerate(_playerGridID, false);
            _movingHorizontal = true;
            EventManager.InvokeMoveHorizontal(_playerGridID, dragDirection);
            StartCoroutine(HorizontalMovementCooldown());
        }
        else if (dragDirection == DragDirection.Down && !_draggingDown)
        {
            EventManager.InvokeAccelerate(_playerGridID,true);
            _draggingDown = true;
        }
        _touchLastPosition = currentTouchPos;
    }

    private IEnumerator HorizontalMovementCooldown()
    {
        yield return new WaitForSeconds(_horizontalMovementCooldown);
        _movingHorizontal = false;
    }

    private void OnExitedGameplayScene()
    {
        EventManager.UnsubscribeAllRotate();
        EventManager.UnsubscribeAllMoveHorizontal();
    }

    #endregion

    #region Auxiliary Functions

    private static bool IsTouchOverUI(Vector2 touchPosition)
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = touchPosition
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Any(result => result.gameObject.GetComponent<Graphic>() != null);
    }

    private static DragDirection GetDragDirection(Vector2 value)
    {
        if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
        {
            if (value.x < 0) return DragDirection.Left;
            if (value.x > 0) return DragDirection.Right;
        }
        else
        {
            if (value.y < 0)
            {
                return DragDirection.Down;
            }
        }
        return DragDirection.None;
    }

    #endregion
}