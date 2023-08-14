using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class InputManager : Singleton<InputManager>
{
	public bool PlayerInputEnabled { get; private set; }
	
	[SerializeField]
	private float _moveDistanceThreshold  = 5.0f;
	
	private PlayerInput _playerInput;
	private InputAction _singleTouchInputAction;
	private InputAction _moveInputAction;
	private InputAction _accelerateInputAction;
	private Vector2 _touchLastPosition;
	
	public enum DragHorizontalDirection
	{
		None,
		Left,
		Right
	}
	protected override void Awake()
	{
		base.Awake();
		
		PlayerInputEnabled = false;
		
		_playerInput = GetComponent<PlayerInput>();
		_singleTouchInputAction = _playerInput.actions.FindAction("SingleTouch");
		_moveInputAction = _playerInput.actions.FindAction("Move");
		_accelerateInputAction = _playerInput.actions.FindAction("Accelerate");
	}

	private void OnEnable()
	{
		EventManager.EventExitedGameplayScene += OnExitedGameplayScene;
		_singleTouchInputAction.performed += TryToRotate;
		_moveInputAction.performed += TryToMoveHorizontal;
		RunningGameState.OnGameStartRunning += () => {EnablePlayerInput(true);};
		RunningGameState.OnGameStopRunning += () => {EnablePlayerInput(false);};
	}
	private void OnDestroy()
	{
		EventManager.EventExitedGameplayScene -= OnExitedGameplayScene;
		_singleTouchInputAction.performed -= TryToRotate;
		_moveInputAction.performed -= TryToMoveHorizontal;
		RunningGameState.OnGameStartRunning -= () => {EnablePlayerInput(true);};
		RunningGameState.OnGameStopRunning -= () => {EnablePlayerInput(false);};
	}

	private void OnExitedGameplayScene()
	{
		EventManager.UnsubscribeAllRotate();
		EventManager.UnsubscribeAllMoveHorizontal();
	}

	public void EnablePlayerInput(bool enable)
	{
		PlayerInputEnabled = enable;
	}

	private void TryToRotate(InputAction.CallbackContext context)
	{
		if (!PlayerInputEnabled)
		{
			return;	
		}
		
		Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
		if (!IsTouchOverUI(touchPosition) && PlayerInputEnabled)
		{
			EventManager.InvokeRotate();
		}
	}
	
	private void TryToMoveHorizontal(InputAction.CallbackContext context)
	{
		if (!PlayerInputEnabled)
		{
			return;	
		}
		
		if (Touchscreen.current.primaryTouch.phase.ReadValue() == TouchPhase.Began)
		{
			// If the touch just started, record the touch position
			_touchLastPosition = Touchscreen.current.primaryTouch.position.ReadValue();
		}
		else
		{
			if (IsTouchOverUI(_touchLastPosition))
			{
				return;
			}
			Vector2 currentTouchPos = context.ReadValue<Vector2>();
			// Calculate the distance moved since the last frame
			float distanceMoved = Vector2.Distance(currentTouchPos, _touchLastPosition);
			// Check if the distance moved is greater than the threshold to send move action
			if (distanceMoved >= _moveDistanceThreshold)
			{
				Vector2 direction = _touchLastPosition - currentTouchPos;
				if (direction.magnitude > 0)
				{
					DragHorizontalDirection dragHorizontalDirection = GetDragDirection(direction);
					EventManager.InvokeMoveHorizontal(dragHorizontalDirection);
				}
				// Update the last touch position for the next frame
				_touchLastPosition = currentTouchPos;
			}
		}
	}

	private static bool IsTouchOverUI(Vector2 touchPosition)
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = touchPosition;
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, results);
		foreach (RaycastResult result in results)
		{
			if (result.gameObject.GetComponent<Graphic>() != null)
			{
				return true;
			}
		}
		return false;
	}

	private static DragHorizontalDirection GetDragDirection(Vector2 value)
	{
		if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
		{
			if (value.x > 0)
			{
				return DragHorizontalDirection.Left;
			} 
			
			if (value.x < 0){
				return DragHorizontalDirection.Right;
			}
		}
		return DragHorizontalDirection.None;
	}
}

