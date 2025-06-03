using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCamControl : MonoBehaviour
{
    [Header("Customised Settings")]
    [SerializeField] CustomisedSettings _customSettings;

    [Header("Camera Movement")]
    [SerializeField] Transform _playerTransform;

    [Header("Raycast")]
    [SerializeField] RubiksCubeController rubiksCubeController;
    [SerializeField] LayerMask _detectableLayer;
    [SerializeField] float _maxDistance;

    [Header("Cameras")]
    [SerializeField] Camera _mainCamera;

    [Header("Options")]
    [SerializeField] bool _doReversedCam = true;

    [Header("Sensitivity")]
    [SerializeField] private float yawSensitivity = 100f;
    [SerializeField] private float pitchSensitivity = 100f;

    Transform _oldTile;

    float _yRotation;
    GameSettings _settings;
    InputHandler _inputHandler;

    Vector2 mousePos = new();

    private Quaternion _externalRotationInfluence = Quaternion.identity;
    private float _rotationInfluenceAmount = 0f;

    private float _externalYawInfluence = 0f;
    private float _yawInfluenceAmount = 0f;

    public Transform PlayerTransform => _playerTransform;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _settings = GameManager.Instance.Settings;
        UpdateCameraFOV(_customSettings.customFov);
        _inputHandler = InputHandler.Instance;
        ForceResetSelection();
    }
    public void OnCamera(Vector2 rawInput) //also used for NoClip
    {
        mousePos = new Vector2(rawInput.x * yawSensitivity * Time.deltaTime,
                               rawInput.y * pitchSensitivity * Time.deltaTime);
    }

    void Update()
    {
        UpdateSelection(false);
    }

    private void ForceResetSelection()
    {
        UpdateSelection(true);
    }

    private void UpdateSelection(bool forceNewSelection = false)
    {
        if (_inputHandler == null || !_inputHandler.CanMove)
            return;

        _yRotation -= mousePos.y;
        _yRotation = Mathf.Clamp(_yRotation, -90f, 90f);

        if (_doReversedCam)
        {
            Vector3 forward = _playerTransform.forward;
            forward.y = 0; // Ignore vertical tilt if needed

            float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            float normalizeAngle = (angle < 0) ? angle + 360 : angle; // Normalize to 0-360
            bool isReversed = normalizeAngle >= 315 || normalizeAngle < 135;

            rubiksCubeController.CameraPlayerReversed = isReversed;
        }

        Quaternion baseRotation = Quaternion.Euler(_yRotation, 0f, 0f);
        transform.localRotation = Quaternion.Slerp(baseRotation, _externalRotationInfluence, _rotationInfluenceAmount);

        float yawInput = mousePos.x; 

        float targetYaw = _playerTransform.eulerAngles.y + yawInput;
        float newYaw = Mathf.LerpAngle(targetYaw, _externalYawInfluence, _yawInfluenceAmount);

        _playerTransform.rotation = Quaternion.Euler(0f, newYaw, 0f);

        if (!GameManager.Instance.IsRubiksCubeEnabled)
            return;

        RaycastHit _raycastInfo;

        if (Physics.Raycast(transform.position, transform.forward, out _raycastInfo, _maxDistance, _detectableLayer))
        {
            GameObject collider = _raycastInfo.collider.gameObject;
            _oldTile = collider.transform;

            if (rubiksCubeController == null || _oldTile.parent == null)
                return;

            if (forceNewSelection)
                rubiksCubeController.SetActualCube(_oldTile.parent);
            else
            {
                if (rubiksCubeController.ActualFace == null || rubiksCubeController.ActualFace.transform != _oldTile.parent)
                    rubiksCubeController.SetActualCube(_oldTile.parent);
            }
        }
    }

    public float GetVerticalAngle()
    {
        return _yRotation;
    }

    public float PlayerTransformEulerY()
    {
        return _playerTransform.eulerAngles.y;
    }

    public void SetExternalPitch(float pitch, float influence)
    {
        _externalRotationInfluence = Quaternion.Euler(pitch, 0f, 0f);
        _rotationInfluenceAmount = influence;
    }

    public void SetExternalYaw(float yaw, float influence)
    {
        _externalYawInfluence = yaw;
        _yawInfluenceAmount = influence;
    }

    public void ClearExternalInfluence()
    {
        _rotationInfluenceAmount = 0f;
        _yawInfluenceAmount = 0f;
    }

    private void OnEnable()
    {
        EventManager.OnFOVChange += UpdateCameraFOV;
        EventManager.OnMouseChange += UpdateCameraMouseSensitivity;
        EventManager.OnEndNarrativeSequence += ResetMousePosition;
        EventManager.OnPlayerChangeParent += ForceResetSelection;
    }

    private void OnDisable()
    {
        EventManager.OnFOVChange -= UpdateCameraFOV;
        EventManager.OnMouseChange -= UpdateCameraMouseSensitivity;
        EventManager.OnEndNarrativeSequence -= ResetMousePosition;
        EventManager.OnPlayerChangeParent -= ForceResetSelection;
    }

    void UpdateCameraFOV(float newFOV)
    {
        _mainCamera.fieldOfView = newFOV;
    }

    void UpdateCameraMouseSensitivity(float newCamMouseSen)
    {
        yawSensitivity = newCamMouseSen;
        pitchSensitivity = newCamMouseSen;
    }

    void ResetMousePosition()
    {
        mousePos = Vector2.zero;
        _yRotation = 0.0f;
    }
}
