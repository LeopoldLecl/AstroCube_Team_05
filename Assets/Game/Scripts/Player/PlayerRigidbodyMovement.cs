using UnityEngine;

public class PlayerRigidbodyMovement : MonoBehaviour
{
    [Header("Scene Requirments")]
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] Transform _camera;
    [SerializeField] Transform _floorCheck;
    [SerializeField] LayerMask _floorLayer;

    bool _hasGravity = true;

    [Header("Movement Modifiers")]
    [SerializeField, Range(0.0f, 10f)] float _speedMultiplier = 1.0f;

    [Header("Slipping")]
    [SerializeField][Range(0.0f, 0.1f)] float _slippingMovementControl = 0.01f;

    [Header("GravityRotation")]
    [SerializeField] bool _enableGravityRotation = true;

    [Header("NoClip")]
    [SerializeField] bool _resetRotationWhenNoClip = false;

    bool _canMove = true;

    Vector3 _gravityDirection;

    private GroundTypePlayerIsWalkingOn _currentGroundType = GroundTypePlayerIsWalkingOn.Default;

    float _floorDistance = 0.1f;

    float _currentMoveSpeed;
    float _currentMoveSpeedFactor = 1f;
    Vector3 _verticalVelocity;
    Vector3 _horizontalVelocity;
    bool _isGrounded;

    float _defaultCameraHeight;
    float _defaultControllerHeight;
    Vector3 _defaultControllerCenter;

    float _xInput = 0;
    float _zInput = 0;
    float _yInput = 0; //noclip
    bool _jumpInput = false;
    bool _crouchInput = false;

    bool _isSlipping = false;
    Vector3 _pastHorizontalVelocity;
    GameSettings _gameSettings;

    float _walkingDuration;
    float _startWalkingDuration;
    float _stopWalkingDuration;
    bool _isWalking;

    Vector3 newCamPos;
    Vector3 _externallyAppliedMovement = Vector3.zero;

    public bool isOnDefaultGround;
    public float defaultSpeed { get; private set; }
    public bool HasGravity { get => _hasGravity; set => _hasGravity = value; }

    private float _timerBeforeNextStep = 0;
    public float _timerTNextStep = 1;

    private void OnEnable()
    {
        EventManager.OnStartCubeRotation += DisableMovement;
        EventManager.OnEndCubeRotation += EnableMovement;
    }

    private void OnDisable()
    {
        EventManager.OnStartCubeRotation -= DisableMovement;
        EventManager.OnEndCubeRotation -= EnableMovement;
    }

    public void EnableMovement() => _canMove = true;
    public void DisableMovement() => _canMove = false;

    void Start()
    {
        _gameSettings = GameManager.Instance.Settings;
        GetComponent<DetectNewParent>().DoGravityRotation = _gameSettings.EnableGravityRotation;

        _defaultCameraHeight = _camera.transform.localPosition.y;
        defaultSpeed = _gameSettings.PlayerMoveSpeed * _speedMultiplier;
        _currentMoveSpeed = defaultSpeed;

        _rigidbody.freezeRotation = true;
    }

    void Update()
    {
        if (!_canMove) return;

        _isGrounded = Physics.CheckSphere(_floorCheck.position, _floorDistance, _floorLayer);

        _gravityDirection = transform.up;
        _xInput = Input.GetAxis("Horizontal");
        _zInput = Input.GetAxis("Vertical");

        _horizontalVelocity = transform.right * _xInput + transform.forward * _zInput;

        if (_isSlipping)
        {
            _horizontalVelocity = _horizontalVelocity * _gameSettings.SlippingMovementControl + _pastHorizontalVelocity;
            _horizontalVelocity = Vector3.ClampMagnitude(_horizontalVelocity, 1);
        }


        if (_crouchInput)
        {
            newCamPos = _camera.transform.localPosition;
            newCamPos.y = _defaultCameraHeight * _gameSettings.CrouchHeight;
        }
        else
        {
            newCamPos = _camera.transform.localPosition;
            newCamPos.y = _defaultCameraHeight;
        }

        _ApplyCameraHeight(newCamPos.y);
        ExecuteFootStep();
    }

    void FixedUpdate()
    {
        if (!_canMove) return;

        Vector3 move = _horizontalVelocity + transform.up * _yInput;
        float moveSpeed = _currentMoveSpeed * _currentMoveSpeedFactor;

        if (_hasGravity)
        {
            Vector3 velocity = move * (_crouchInput ? moveSpeed / _gameSettings.CrouchSpeed : moveSpeed);
            velocity += _externallyAppliedMovement / Time.fixedDeltaTime;

            Vector3 currentVel = _rigidbody.velocity;
            velocity.y = currentVel.y;

            _rigidbody.velocity = velocity;
        }
        else
        {
            Vector3 velocity = move * (moveSpeed / 10);
            velocity += _externallyAppliedMovement / Time.fixedDeltaTime;
            _rigidbody.velocity = velocity;
        }
    }

    void ExecuteFootStep()
    {
        _isWalking = _horizontalVelocity != Vector3.zero;
        if (_isWalking)
        {
            _timerBeforeNextStep += Time.deltaTime;
        }
        else
        {
            _timerBeforeNextStep = 0;
        }

        float stepDuration = _timerTNextStep / _currentMoveSpeedFactor;
        if (_timerBeforeNextStep >= stepDuration)
        {
            _timerBeforeNextStep = 0;
            UpdateGroundType();
            EventManager.TriggerPlayerFootSteps(_currentGroundType);
        }
    }

    public void ActionMovement(Vector2 direction)
    {
        _xInput = direction.x;
        _zInput = direction.y;
    }


    public void SetSpeed(float newSpeed)
    {
        _currentMoveSpeed = newSpeed;
    }

    public void SetSpeedFactor(float speedFactor)
    {
        _currentMoveSpeedFactor = speedFactor;
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
        if (_isWalking && !_isSlipping)
        {
            EventManager.TriggerPlayerFootSteps(_currentGroundType);

            if (_startWalkingDuration <= _gameSettings.StartWalkingTransitionDuration)
            {
                _stopWalkingDuration = 0.0f;
                _startWalkingDuration += Time.deltaTime;
                newCameraHeight = Vector3.up * Mathf.Lerp(_camera.localPosition.y,
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
                newCameraHeight = Vector3.up * Mathf.Lerp(_camera.localPosition.y,
                    currentDefaultHeight,
                    _stopWalkingDuration / _gameSettings.StopWalkingTransitionDuration);
            }
            else
            {
                newCameraHeight = Vector3.up * currentDefaultHeight;
            }
        }

        _camera.localPosition = newCameraHeight;
    }

    public void ActivateNoClip()
    {
        _hasGravity = false;
        _verticalVelocity = Vector3.zero;
        transform.SetParent(null);
        if (_resetRotationWhenNoClip)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
        }
    }

    public void DeactivateNoClip()
    {
        _hasGravity = true;
    }

    public void ActionVerticalMovement(float direction)
    {
        _yInput = direction;
    }

    public void SetExternallyAppliedMovement(Vector3 direction, float speed = 1)
    {
        _externallyAppliedMovement = direction * speed;
    }

    private void UpdateGroundType()
    {
        Ray ray = new Ray(_floorCheck.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, _floorDistance + 0.2f, _floorLayer))
        {
            string groundTag = hit.collider.tag;
            switch (groundTag)
            {
                case "Floor_Default":
                default:
                    _currentGroundType = GroundTypePlayerIsWalkingOn.Default;
                    break;
                case "Floor_Grass":
                    _currentGroundType = GroundTypePlayerIsWalkingOn.Grass;
                    break;
            }
        }
        else
        {
            _currentGroundType = GroundTypePlayerIsWalkingOn.Default;
        }
    }

}