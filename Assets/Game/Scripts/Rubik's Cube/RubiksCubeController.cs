using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RubiksStatic;
using System.Linq;

public class RubiksCubeController : MonoBehaviour
{

    [SerializeField] GameObject _controlledCube;
    RubiksMovement _controlledScript;


    bool _cameraPlayerReversed = false;

    [SerializeField] Transform _overlayTransform;
    bool _isCameraRotating = false;

    [SerializeField] List<GameObject> ReplicatedCube = new List<GameObject>();
    [SerializeField] SelectionCube ActualFace;

    [SerializeField] bool _ShowStripLayerToPlayer = true;

    List<RubiksMovement> _replicatedScript = new List<RubiksMovement>();

    [SerializeField] Transform _player;
    [SerializeField] DetectNewParent _detectParentForGroundRotation;

    SliceAxis _selectedSlice = 0;
    private GameSettings _gameSettings;

    bool _canPlayerMoveAxis = true;

    public LayerMask _detectableLayer;

    #region Accesseur

    public bool CameraPlayerReversed { get => _cameraPlayerReversed; set => _cameraPlayerReversed = value; }
    public bool ShowStripLayerToPlayer { get => _ShowStripLayerToPlayer; set => _ShowStripLayerToPlayer = value; }
    public RubiksMovement ControlledScript { get => _controlledScript;}

    #endregion

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _detectParentForGroundRotation = _player.GetComponentInChildren<DetectNewParent>();
        if (_controlledCube != null) _controlledScript = _controlledCube.GetComponentInChildren<RubiksMovement>();
        foreach (GameObject go in ReplicatedCube)
        {
            _replicatedScript.Add(go.GetComponentInChildren<RubiksMovement>());
        }
        _gameSettings = GameManager.Instance.Settings;
        if(GameManager.Instance.IsRubiksCubeEnabled)
            ActionSwitchLineCols(true);
    }


    /* OLD
    public void SetActualCube(Transform newFace)
    {
        ShutDownFace();
        GameObject newCube = null;
        if (newFace.parent.parent && newFace.parent.parent.CompareTag("Rubiks")) newCube = newFace.parent.parent.gameObject;
        else if (newFace.parent.CompareTag("Rubiks")) newCube = newFace.parent.gameObject;

        if (newCube && newCube != controlledCube)
        {
            if ((ReplicatedCube.Contains(newCube)))
            {
                print("bahoui connard");
                return;
            }
            controlledCube = newCube;
            _controlledScript = controlledCube.GetComponentInChildren<RubiksMovement>();
        }

        if (_controlledScript == null) return;
        if (ActualFace) ActualFace.enabled = false;
        ActualFace = newFace.GetComponent<Outline>();

        IlluminateFace(_selectedSlice, SelectionCube.SelectionMode.CUBE);        

        ActualFace.OutlineColor = Color.red;
        ActualFace.OutlineWidth = 15;
        ActualFace.enabled = true;

        _controlledScript.GetAxisFromCube(ActualFace.transform, _selectedSlice);
    }
    */

    public void SetActualCube(Transform newFace)
    {
        ShutDownFace();
        if (newFace.parent.parent && newFace.parent.parent.CompareTag("Rubiks")) _controlledCube = newFace.parent.parent.gameObject;
        else if (newFace.parent.CompareTag("Rubiks")) _controlledCube = newFace.parent.gameObject;

        _controlledScript = _controlledCube.GetComponentInChildren<RubiksMovement>();

        if (_controlledScript == null) return;
        if (_controlledScript.IsRotating) return;

        if (ActualFace) ActualFace.enabled = false;
        ActualFace = newFace.GetComponent<SelectionCube>();

        if (_ShowStripLayerToPlayer && TryIlluminateFace(_selectedSlice, SelectionCube.SelectionMode.AXIS))
        {
            ActualFace.Select(SelectionCube.SelectionMode.CUBE);
            _canPlayerMoveAxis = true;
        }
        else
        {
            _canPlayerMoveAxis = false;
        }

        _controlledScript.GetAxisFromCube(ActualFace.transform, _selectedSlice);
    }

    public void ActionSwitchLineCols(bool isLeft)
    {

        _selectedSlice = (SliceAxis)(((int)_selectedSlice + (isLeft ? -1 : +1) + 3) % 3);
        switch (_selectedSlice)
        {
            case SliceAxis.X:
                _detectParentForGroundRotation.DoGroundRotation = false;
                if (_controlledScript.IsLockXAxis)
                {
                    ActionSwitchLineCols(true);
                    return;
                }
                break;
            case SliceAxis.Y:
                _detectParentForGroundRotation.DoGroundRotation = true;
                if (_controlledScript.IsLockYAxis)
                {
                    ActionSwitchLineCols(true);
                    return;
                }
                break;
            case SliceAxis.Z:
                _detectParentForGroundRotation.DoGroundRotation = false;
                if (_controlledScript.IsLockZAxis)
                {
                    ActionSwitchLineCols(true);
                    return;
                }
                break;
        }
        if (ActualFace) SetActualCube(ActualFace.transform);
    }

    public void ActionMakeTurn(bool clockwise)
    {
        if (_controlledScript && _controlledScript.IsRotating == false && _canPlayerMoveAxis)
        {
            if (_cameraPlayerReversed)
            {
                clockwise = !clockwise;
            }

            ShutDownFace();

            if (_controlledScript == null) return;

            GameManager.Instance.Screenshake(1, 0.2f, 12);

            foreach (RubiksMovement cube in _replicatedScript)
            {
                if (!cube) continue;

                // HAHAHAHAHAHAHA LA LIGNE EST HORRIBLE ALED JEROME J'T'EN SUPPLIE
                Transform equivalence = cube.transform.GetComponentsInChildren<Transform>().First(t => t.GetComponentIndex() == ActualFace.transform.GetComponentIndex());

                // Transform equivalence = cube.transform.GetChild(ActualFace.transform.GetComponentIndex());
                // why won't work ???

                // Get The index of the children 
                // Find the Other child at the index in other cube
                // Move it

                cube.RotateAxis(cube.GetAxisFromCube(equivalence, _selectedSlice), ActualFace.transform, clockwise, _gameSettings.RubikscCubeAxisRotationDuration, _selectedSlice);
            }

            _controlledScript.RotateAxis(_controlledScript.GetAxisFromCube(ActualFace.transform, _selectedSlice), ActualFace.transform, clockwise, _gameSettings.RubikscCubeAxisRotationDuration, _selectedSlice);
            EventManager.TriggerCubeRotated();
        }
    }
    public void ActionRotateCubeUI(Vector2 direction)
    {
        StartCoroutine(RotateCubeUI(direction));
    }

    IEnumerator RotateCubeUI(Vector2 direction)
    {
        if (!_isCameraRotating)
        {

            _isCameraRotating = true;
            float elapsedTime = 0;
            Quaternion startRotation = _overlayTransform.rotation;
            Quaternion targetRotation = startRotation * Quaternion.AngleAxis(-90, new Vector2(direction.y, direction.x));

            while (elapsedTime < _gameSettings.UiRubikscCubeRotationDuration)
            {
                elapsedTime += Time.deltaTime;
                _overlayTransform.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / _gameSettings.UiRubikscCubeRotationDuration);
                yield return null;
            }

            _overlayTransform.transform.rotation = targetRotation;

            _isCameraRotating = false;
        }
    }

    /* OLD
    void IlluminateFace(SliceAxis sliceAxis)
    {
        Color hey = new Color(1, 0.5f, 0, 1);
        if(_controlledScript != null)
        foreach (Transform go in _controlledScript.GetCubesFromFace(ActualFace.transform, sliceAxis))   
        {
            Outline outline = go.GetComponent<Outline>();
            outline.OutlineColor = hey;
            outline.OutlineWidth = 10;
            outline.enabled = true;
        }
    */

    /// <summary>
    /// return True if Player Can Move Axis;
    /// </summary>
    /// <param name="sliceAxis"></param>
    /// <param name="mode">Should normally never be set to Locked</param>
    /// <returns></returns>
    bool TryIlluminateFace(SliceAxis sliceAxis, SelectionCube.SelectionMode mode)
    {
        List<SelectionCube> selectionCubes = new List<SelectionCube>();
        bool isOneTileLocked = false;

        if (_controlledScript != null)
            foreach (Transform go in _controlledScript.GetCubesFromFace(ActualFace.transform, sliceAxis))
            {
                SelectionCube selection = go.GetComponent<SelectionCube>();
                if (selection == null) continue;

                selectionCubes.Add(selection);
                if (selection.IsTileLocked) isOneTileLocked = true;
            }

        foreach (SelectionCube selection in selectionCubes) selection.Select(isOneTileLocked ? SelectionCube.SelectionMode.LOCKED : mode);

        return !isOneTileLocked;
    }

    /* OLD
    void ShutDownFace()
    {
        foreach (Transform go in controlledCube.transform)
        {
            Outline outOutline;
            if (go.TryGetComponent<Outline>(out outOutline)) outOutline.enabled = false;
        }
    }
    */

    void ShutDownFace()
    {
        foreach (Transform go in _controlledCube.transform)
        {
            SelectionCube selection = go.GetComponent<SelectionCube>();
            if (selection == null) continue;
            selection.Unselect();
        }
    }

}


