using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class InputHandler : MonoBehaviour
{
    [SerializeField] PlayerHold _playerHold;
    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] DetectNewParent _parentChanger;

    RubiksCubeController _controller;

    PlayerInput _playerInput;

    public static InputHandler Instance => instance;
    private static InputHandler instance;

    public Vector2 CameraMovement => _cameraMovement;
    private Vector2 _cameraMovement;

    public bool CanMove
    {
        get => _canMove;

        set
        {
            _canMove = value;
            ToggleCanMove(value);
        }
    }
    [SerializeField, ReadOnly] private bool _canMove;

    void Awake()
    {
        if (instance) Destroy(this);
        else instance = this;
        _canMove = true;
        _controller = GetComponent<RubiksCubeController>();
        _playerInput = GetComponent<PlayerInput>();
        InputActionMap _actionMap = _playerInput.actions.FindActionMap("PlayerMovement");
        if (_actionMap != null)
        {
            if (_playerMovement != null) _actionMap.Enable();
            else Debug.LogError("playerMovement script is missing from InputHandler Inspector");
        }
        else Debug.LogError("playerMovment InputMap not found.");

        _playerInput.actions.FindActionMap("OtherActions").Enable();
        _parentChanger = _playerMovement.GetComponent<DetectNewParent>();

        if (!GameManager.Instance.IsRubiksCubeEnabled)
        {
            _playerInput.actions.FindActionMap("RubiksCube").Disable();
        }
    }

    private void ToggleCanMove(bool canMove)
    {
        switch (canMove)
        {
            default:
            case true:
                _playerInput.actions.FindActionMap("PlayerMovement").Enable();
                break;
            case false: 
                _playerInput.actions.FindActionMap("PlayerMovement").Disable();
                break;
        }
    }

    #region Rubiks Cube Inputs
    public void OnSwitchColumnsLineLeft(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            _controller.ActionSwitchLineCols(true);
        }
    }
    public void OnSwitchColumnsLineRight(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            _controller.ActionSwitchLineCols(false);
        }
    }
    public void OnClockWise(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            _controller.ActionMakeTurn(false);
        }
    }
    public void OnCounterClockWise(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            _controller.ActionMakeTurn(true);
        }
    }

    public void OnMoveOverlayCube(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            _controller.ActionRotateCubeUI(callbackContext.ReadValue<Vector2>());
        }
    }

    public void OnResetRoom(InputAction.CallbackContext callbackContext)
    {
        //Hold 
        print(callbackContext.time - callbackContext.startTime);
        if (callbackContext.performed)
        {
            if (!_controller.ControlledScript.IsReversing) EventManager.Instance.TriggerReset();

        }
        //TAP 
        else if (callbackContext.canceled && callbackContext.time - callbackContext.startTime < .5f)
        {
            if (!_controller.ControlledScript.IsReversing && _controller.ControlledScript.Moves.Count > 0) EventManager.Instance.TriggerResetOnce();
        }
    }
    #endregion

    public void OnInteract(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            _playerHold.TryHold();
            //if (_playerHold.IsHolding) _playerHold.TryRelease();
        }
    }

    public void OnGamePause(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {            
            if (EventManager.gamePaused == false) EventManager.TriggerGamePause();
            else EventManager.TriggerGameUnpause();
        }
    }

    #region Player Movement & NoClip Movement
    public void OnMovement(InputAction.CallbackContext callbackContext) //also used for NoClip
    {
        if(!_controller.ControlledScript.IsReversing)
        _playerMovement.ActionMovement(callbackContext.ReadValue<Vector2>());
    }
    public void OnJump(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed && !_controller.ControlledScript.IsReversing)
        {
            _playerMovement.ActionJump();
        }
    }
    public void OnCrouch(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed && !_controller.ControlledScript.IsReversing)
        {
            _playerMovement.ActionCrouch();
        }
    }
    #endregion

    #region Noclip
    public void OnVerticalMovement(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed)
        {
            _playerMovement.ActionVerticalMovement(callbackContext.ReadValue<float>());
        }
    }
    #endregion

    #region UI
    public void OnShowStripLayer(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            _controller.ShowStripLayerToPlayer = !_controller.ShowStripLayerToPlayer;
        }
    }
    #endregion

}
