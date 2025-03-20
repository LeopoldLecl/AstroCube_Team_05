using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class InputHandler : MonoBehaviour
{
    [SerializeField] PlayerHold _playerHold;
    [SerializeField] PlayerMovement _playerMovement;
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
        if (callbackContext.performed)
        {
            EventManager.Instance.TriggerReset();
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
        _playerMovement.ActionMovement(callbackContext.ReadValue<Vector2>());
    }
    public void OnJump(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed)
        {
            _playerMovement.ActionJump();
        }
    }
    public void OnCrouch(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed)
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
