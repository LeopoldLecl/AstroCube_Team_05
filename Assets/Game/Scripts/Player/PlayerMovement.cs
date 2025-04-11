using AK.Wwise;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    [Header("Scene Requirments")]
    [SerializeField] CharacterController _controller;
    [SerializeField] Transform _camera;
    [SerializeField] Transform _floorCheck;
    [SerializeField] LayerMask _floorLayer;

    bool _hasGravity = true;

    [Header("Jump")]
    [SerializeField] bool _canJump = true;

    [Header("Crouch")]
    [SerializeField] bool _canCrouch = true;

    [Header("Slipping")]
    [SerializeField][Range(0.0f, 0.1f)] float _slippingMovementControl = 0.01f;

    [Header("GravityRotation")]
    [SerializeField] bool _enableGravityRotation = true;

    [Header("NoClip")]
    [SerializeField] bool _resetRotationWhenNoClip = false;

    [Header("WISE")]
    [SerializeField] AK.Wwise.Event AKWiseEvent;



    Vector3 _gravityDirection;

    float _floorDistance = 0.1f;

    float _currentMoveSpeed;
    Vector3 _verticalVelocity;
    Vector3 _horizontalVelocity;
    bool _isGrounded;

    float _defaultCameraHeight;
    float _defaultControllerHeight;
    Vector3 _defaultControllerCenter;

    float _xInput = 0;
    float _zInput = 0;
    float _yInput = 0;//noclip
    bool _jumpInput = false;
    bool _crouchInput = false;

    bool _isSlipping = false;
    Vector3 _pastHorizontalVelocity;
    GameSettings _gameSettings;

    // HeadBobbing
    float _walkingDuration;
    float _startWalkingDuration;
    float _stopWalkingDuration;
    bool _isWalking;

    Vector3 newCamPos;

    Vector3 _externallyAppliedMovement = Vector3.zero;

    public float defaultSpeed { get; private set; }


    private float _timerBeforeNextStep = 0;
    public float _timerTNextStep = 1;

    


    void Start()
    {
        _gameSettings = GameManager.Instance.Settings;
        GetComponent<DetectNewParent>().DoGravityRotation = _gameSettings.EnableGravityRotation;

        _defaultCameraHeight = _camera.transform.localPosition.y;
        _defaultControllerHeight = _controller.height;
        _defaultControllerCenter = _controller.center;

        defaultSpeed = _gameSettings.PlayerMoveSpeed;
        _currentMoveSpeed = defaultSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        // collect player inputs
        _xInput = Input.GetAxis("Horizontal");
        _zInput = Input.GetAxis("Vertical");
        if (_canJump) _jumpInput = Input.GetButtonDown("Jump");
        if (_canCrouch) _crouchInput = Input.GetKey(KeyCode.LeftShift);
        */

        //check player state
       _isGrounded = Physics.CheckSphere(_floorCheck.position, _floorDistance, _floorLayer);

        //apply gravity
        if (_hasGravity)
        {
            _gravityDirection = transform.up;
            _verticalVelocity += _gravityDirection * _gameSettings.Gravity * Time.deltaTime;
            if (_isGrounded)
            {
                _verticalVelocity = Vector3.zero;
            }
        }

        //_gravityDirection = transform.up;
        //_verticalVelocity = _gravityDirection * _gameSettings.Gravity * Time.deltaTime;

        // movePlayer (walking around)
        if (_isSlipping) _pastHorizontalVelocity = _horizontalVelocity;
        _horizontalVelocity = transform.right * _xInput + transform.forward * _zInput;

        if (_isSlipping)
        {
            _horizontalVelocity = _horizontalVelocity * _gameSettings.SlippingMovementControl + _pastHorizontalVelocity;

            //clamp
            _horizontalVelocity.x = _horizontalVelocity.x > 1 ? 1 : _horizontalVelocity.x;
            _horizontalVelocity.x = _horizontalVelocity.x < -1 ? -1 : _horizontalVelocity.x;
            _horizontalVelocity.z = _horizontalVelocity.z > 1 ? 1 : _horizontalVelocity.z;
            _horizontalVelocity.z = _horizontalVelocity.z < -1 ? -1 : _horizontalVelocity.z;
        }

        // jump
        if (_jumpInput && _isGrounded)
        {
            _verticalVelocity = transform.up * Mathf.Sqrt(_gameSettings.JumpHeight * -2f * _gameSettings.Gravity);
        }
        _jumpInput = false;

        // crouch
        if (_crouchInput)
        {
            _controller.height *= _gameSettings.CrouchHeight;
            _controller.center = Vector3.up * _gameSettings.CrouchHeight * -1;

            newCamPos = _camera.transform.localPosition;
            newCamPos.y = _defaultCameraHeight * _gameSettings.CrouchHeight;
        }
        else
        {
            _controller.height = _defaultControllerHeight;
            _controller.center = _defaultControllerCenter;
            newCamPos = _camera.transform.localPosition;
            newCamPos.y = _defaultCameraHeight;
        }
        _crouchInput = false;

        // no clip
        _horizontalVelocity += transform.up * _yInput;

        // apply calculated Movement
        if (_hasGravity)
        {
            _controller.Move(_horizontalVelocity *
                             (_crouchInput ? _currentMoveSpeed : _currentMoveSpeed / _gameSettings.CrouchSpeed) * Time.deltaTime
                             + _externallyAppliedMovement);
            _controller.Move(_verticalVelocity * Time.deltaTime);
        } 
        else // no clip
        {
            _controller.Move(_horizontalVelocity *
                             (_currentMoveSpeed/10) * Time.deltaTime
                             + _externallyAppliedMovement);
        }
        _ApplyCameraHeight(newCamPos.y);
        ExecuteFootStep();
    }

    void ExecuteFootStep()
    {
        if (_isWalking)
        {
            _timerBeforeNextStep += Time.deltaTime;
        }
        else
        {
            _timerBeforeNextStep = 0;
        }

        if (_timerBeforeNextStep >= _timerTNextStep)
        {
            _timerBeforeNextStep = 0;
            AKWiseEvent.Post(gameObject);
        }
    }

    #region Inputs
    public void ActionMovement(Vector2 direction)
    {
        //Debug.Log("actionMovement direction is " + direction);
        _xInput = direction.x;
        _zInput = direction.y;
    }
    public void ActionJump()
    {
        _jumpInput = _canJump;
    }
    public void ActionCrouch()
    {
        _crouchInput = _canCrouch;
    }
    #endregion

    public void SetSpeed(float newSpeed)
    {
        _currentMoveSpeed = newSpeed * Time.deltaTime;
    }

    public void SetSpeedToDefault()
    {
        _currentMoveSpeed = defaultSpeed;
    }

    public void SetSlippingState(bool isSlipping)
    {
        _isSlipping = isSlipping;
    }

    private void _ApplyCameraHeight(float currentDefaultHeight)
    {
        Vector3 newCameraHeight;
        _isWalking = _horizontalVelocity != Vector3.zero;
        if (_isWalking && !_isSlipping)
        {
            if (_startWalkingDuration <= _gameSettings.StartWalkingTransitionDuration)
            {
                _stopWalkingDuration = 0.0f;
                _startWalkingDuration += Time.deltaTime;
                newCameraHeight = Vector3.up * Mathf.Lerp(_camera.transform.localPosition.y,
                                                            currentDefaultHeight + _gameSettings.HeadBobbingCurve.Evaluate(0.0f) * _gameSettings.HeadBobbingAmount,
                                                            _startWalkingDuration / _gameSettings.StartWalkingTransitionDuration);
            }
            else
            {
                _walkingDuration += Time.deltaTime;
                newCameraHeight = Vector3.up * (currentDefaultHeight + _gameSettings.HeadBobbingCurve.Evaluate((_walkingDuration * _gameSettings.HeadBobbingSpeed) % 1) * _gameSettings.HeadBobbingAmount);
            }
        }
        else
        {
            _walkingDuration = 0.0f;
            if (_stopWalkingDuration <= _gameSettings.StopWalkingTransitionDuration)
            {
                _startWalkingDuration = 0.0f;
                _stopWalkingDuration += Time.deltaTime;
                newCameraHeight = Vector3.up * Mathf.Lerp(_camera.transform.localPosition.y,
                                                            currentDefaultHeight,
                                                            _stopWalkingDuration / _gameSettings.StopWalkingTransitionDuration);
            }
            else
            {
                newCameraHeight = Vector3.up * currentDefaultHeight;
            }
        }
        _camera.transform.localPosition = newCameraHeight;
    }

    //NoClip
    public void ActivateNoClip()
    {
        GetComponent<CharacterController>().excludeLayers = Physics.AllLayers;
        _hasGravity = false;
        _verticalVelocity = Vector3.zero;
        _controller.Move(Vector3.zero);
        transform.SetParent(null);
        if (_resetRotationWhenNoClip)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
        }
    }
    public void DeactivateNoClip()
    {
        GetComponent<CharacterController>().excludeLayers = 0;
        _hasGravity = true;
    }
    public void ActionVerticalMovement(float direction)
    {
        _yInput = direction;
    }

    public void SetExternallyAppliedMovement(Vector3 directon, float speed = 1)
    {
        _externallyAppliedMovement = directon * speed;
    }
}
