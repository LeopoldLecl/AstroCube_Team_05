using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] PlayerHold _playerHold;
    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] DetectNewParent _parentChanger;

    RubiksCubeController _controller;

    PlayerInput _playerInput;

    void Awake()
    {

        _controller = GetComponent<RubiksCubeController>();
        _playerInput = GetComponent<PlayerInput>();
        InputActionMap _actionMap = _playerInput.actions.FindActionMap("PlayerMovement");
        if (_actionMap != null)
        {
            if (_playerMovement != null) _actionMap.Enable();
            else Debug.LogError("playerMovement script is missing from InputHandler Inspector");
        }
        else Debug.LogError("playerMovment InputMap not found.");

        _parentChanger = _playerMovement.GetComponent<DetectNewParent>();
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
            if(!_controller.ControlledScript.IsReversing && _controller.ControlledScript.Moves.Count > 0) EventManager.Instance.TriggerResetOnce();
        }
    }
    #endregion
    public void OnInteract(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            if (_playerHold.IsHolding) _playerHold.TryRelease();
            else _playerHold.TryHold();
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
