using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private float _moveDistanceThreshold  = 150.0f;

    private PlayerInput _playerInput;
    private InputAction _singleTouchInputAction;
    private Vector2 _touchLastPosition;
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

    #region Input Handling - Events

    private void TryToRotate(InputAction.CallbackContext context)
    {
        if (!PlayerInputEnabled) return;

        var touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        if (!IsTouchOverUI(touchPosition) && PlayerInputEnabled) EventManager.InvokeRotate();
    }

    private void TryToMove(InputAction.CallbackContext context)
    {
        if (!PlayerInputEnabled) return;

        var touchPhase = Touchscreen.current.primaryTouch.phase.ReadValue();
        var currentTouchPos = context.ReadValue<Vector2>();

        if (touchPhase is TouchPhase.Ended or TouchPhase.Canceled && _draggingDown)
        {
            EventManager.InvokeAccelerate(false);
            _draggingDown = false;
            return;
        }

        if (IsTouchOverUI(currentTouchPos)) return;

        if (touchPhase == TouchPhase.Began)
        {
            _touchLastPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            return;
        }

        // Calculate the distance moved since the last frame
        var distanceMoved = Vector2.Distance(currentTouchPos, _touchLastPosition);
        // Check if the distance moved is greater than the threshold to send move action
        if (distanceMoved < _moveDistanceThreshold)
        {
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
            EventManager.InvokeAccelerate(false);
            _movingHorizontal = true;
            EventManager.InvokeMoveHorizontal(dragDirection);
            StartCoroutine(HorizontalMovementCooldown());
        }
        else if (dragDirection == DragDirection.Down && !_draggingDown)
        {
            EventManager.InvokeAccelerate(true);
            _draggingDown = true;
        }

        // Update the last touch position for the next frame
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
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = touchPosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
            if (result.gameObject.GetComponent<Graphic>() != null)
                return true;
        return false;
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