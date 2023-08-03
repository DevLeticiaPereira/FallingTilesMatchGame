using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class InputManager : Singleton<InputManager>
{
	private PlayerInput _playerInput;

	private InputAction _singleTouchInputAction;
	private InputAction _dragInputAction;
	
	private GameManager _gameManager;

	private Action<DragDirection, DragState> _dragAction;

	public enum DragDirection
	{
		None,
		Down,
		Left,
		Right
	}
	
	public enum DragState
	{
		DragStart,
		Dragging,
		DragEnd
	}

	protected override void Awake()
	{
		base.Awake();
		_playerInput = GetComponent<PlayerInput>();
		_singleTouchInputAction = _playerInput.actions.FindAction("SingleTouch");
		_dragInputAction = _playerInput.actions.FindAction("Drag");
	}

	private void OnEnable()
	{
		_singleTouchInputAction.performed += PerformSingleTouch;
		_dragInputAction.started += StartDrag;
		_dragInputAction.performed += PerformDrag;
		_dragInputAction.canceled += CancelDrag;
	}

	private void OnDisable()
	{
		_singleTouchInputAction.performed -= PerformSingleTouch;
		_dragInputAction.started -= StartDrag;
		_dragInputAction.performed -= PerformDrag;
		_dragInputAction.canceled -= CancelDrag;
	}
	
	private void PerformSingleTouch(InputAction.CallbackContext context)
	{
		var value = context.ReadValueAsButton();
		Debug.Log("touched" + value);
	}

	private void StartDrag(InputAction.CallbackContext context)
	{
		Debug.Log("drag start");
		DragDirection dragDirection = DragDirection.None;
		var value = context.ReadValue<Vector2>();
		dragDirection = GetDragDirection(value);
		_dragAction?.Invoke(dragDirection, DragState.DragStart);
	}

	private void CancelDrag(InputAction.CallbackContext context)
	{
		Debug.Log("drag cancel");
		_dragAction?.Invoke(DragDirection.None, DragState.DragEnd);
	}

	private void PerformDrag(InputAction.CallbackContext context)
	{
		DragDirection dragDirection = DragDirection.None;
		var value = context.ReadValue<Vector2>();
		dragDirection = GetDragDirection(value);
		Debug.Log("dragging");
		_dragAction?.Invoke(dragDirection, DragState.Dragging);
	}
	
	private static DragDirection GetDragDirection(Vector2 value)
	{
		if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
		{
			if (value.x > 0)
			{
				return DragDirection.Right;
			}
			else if (value.x < 0)
			{
				return DragDirection.Left;
			}
		}
		else if (Mathf.Abs(value.y) > Mathf.Abs(value.x) && value.y < 0)
		{
			return DragDirection.Down;
		}

		return DragDirection.None;
	}

}

