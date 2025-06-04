using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public bool _isDoorOpenAtStart = true;

    public static ExitDoor Instance => _instance;
    public static ExitDoor _instance;

    [SerializeField] private GameObject _door;
    [SerializeField] private GameObject _stencil;
    [SerializeField] private float _endScaleStencil = 5.0f;

    [Header("Camera Focus to end")]
    [SerializeField] private CameraFocusAttractor _cameraFocusAttractor;
    [SerializeField] private float focusStrength = 0.7f;
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private float focusDuration = 2.0f;

    private GameSettings _gameSettings;
    private bool _isShowing = false;

    [Header("FOV")]
    [SerializeField] private float _MaxFOV_END = 150.0f;


    private void Awake()
    {
        if (_instance) Destroy(this);
        else _instance = this;

        if (_isDoorOpenAtStart)
            OpenDoor();
        else
            CloseDoor();
    }

    private void Start()
    {
        _gameSettings = GameManager.Instance.Settings;
        _isShowing = false;
        SeeExitThroughWalls();
    }

    private void OnEnable()
    {
        EventManager.OnSeeExit += SeeExitThroughWalls;
        EventManager.OnSeeExit += FocusCameraToExit;
    }

    private void OnDisable()
    {
        EventManager.OnSeeExit -= SeeExitThroughWalls;
        EventManager.OnSeeExit -= FocusCameraToExit;
    }

    public void OpenDoor()
    {
        _door.SetActive(true);
    }

    public void CloseDoor()
    {
        _door.SetActive(false);
    }

    public void SeeExitThroughWalls()
    {
        if (_isShowing) return;
        StartCoroutine(ShowExit());
    }

    public void FocusCameraToExit()
    {
        if (_cameraFocusAttractor == null)
        {
            Debug.LogWarning("GameActionFocusCamera: Missing references!");
            return;
        }

        _cameraFocusAttractor.SetPointOfInterest(transform);
        _cameraFocusAttractor.SetFocusParameters(transitionDuration, focusDuration, focusStrength);
        _cameraFocusAttractor.StartFocus();
    }

    private IEnumerator ShowExit()
    {
        _isShowing = true;
        _stencil.SetActive(true);
        yield return _stencil.transform.DOScale(_endScaleStencil, _gameSettings.StencilFadeInDuration).WaitForCompletion();
        yield return new WaitForSeconds(_gameSettings.StencilStayDuration);
        yield return _stencil.transform.DOScale(0, _gameSettings.StencilFadeOutDuration).WaitForCompletion();
        _stencil.SetActive(false);
        _isShowing = false;
    }
}
