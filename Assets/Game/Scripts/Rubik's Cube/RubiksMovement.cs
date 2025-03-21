using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RubiksStatic;
using System.Linq;

public class RubiksMovement : MonoBehaviour
{

    [SerializeField] Transform middle;
    [SerializeField] Transform middleGameObject;

    [SerializeField] List<Transform> Axis = new List<Transform>();
    List<Transform> _allBlocks = new List<Transform>();

    [SerializeField] bool _doScramble = true;


    private bool _isRotating = false;

    private bool _isReversing = false;
    List<RubiksMove> _moves = new List<RubiksMove>();

    [SerializeField] bool _isLockXAxis;
    [SerializeField] bool _isLockYAxis;
    [SerializeField] bool _isLockZAxis;

    #region Accessor

    public bool IsRotating { get => _isRotating; }
    public bool IsReversing { get => _isReversing; }
    public bool IsLockXAxis { get => _isLockXAxis; }
    public bool IsLockYAxis { get => _isLockYAxis; }
    public bool IsLockZAxis { get => _isLockZAxis; }
    internal List<RubiksMove> Moves { get => _moves; }

    #endregion

    private void Awake()
    {
        EventManager.OnPlayerReset += ReverseMoves;
        EventManager.OnPlayerResetOnce += UndoMove;

        foreach (Transform t in transform.parent)
        {
            if (t.tag == "Movable") _allBlocks.Add(t);
        }
        if (_doScramble) StartCoroutine(Scramble());

    }
    void OnDisable()
    {
        EventManager.OnPlayerReset -= ReverseMoves;
        EventManager.OnPlayerResetOnce -= UndoMove;

    }
    IEnumerator Scramble()
    {
        while (_doScramble)
        {
            if (!_isRotating)
            {
                RubiksMove m = CreateRandomMove();
                RotateAxis(m, .2f);
            }
            yield return null;
        }
    }
    void ReverseMoves(float timeToReset)
    {
        _doScramble = false;
        StartCoroutine(ReverseAllMoves(timeToReset));
    }
    IEnumerator ReverseAllMoves(float time)
    {
        while (_isRotating) yield return null;
        time /= _moves.Count();
        _isReversing = true;
        while (_moves.Count > 0)
        {
            if (!_isRotating)
            {
                RubiksMove m = _moves[_moves.Count - 1];
                StartCoroutine(RotateAxisCoroutine(m.axis, m.cube, !m.clockWise, time, m.orientation));
                _moves.RemoveAt(_moves.Count - 1);
            }
            yield return null;
        }
        yield return new WaitForSeconds(time + .05f);
        _isReversing = false;
    }

    void UndoMove(float time)
    {
        if (IsReversing) return;
        StartCoroutine(ReverseOneMoves(time));
    }
    IEnumerator ReverseOneMoves(float time)
    {
        while (_isRotating)
            yield return null;

        if (Moves.Count > 0) yield return null;
            
        _isReversing = true;
        RubiksMove m = Moves[Moves.Count - 1];

        StartCoroutine(RotateAxisCoroutine(m.axis, m.cube, !m.clockWise, time, m.orientation));
        Moves.RemoveAt(Moves.Count - 1);

        yield return new WaitForSeconds(time + .05f);
        _isReversing = false;
    }
    void RotateAxis(RubiksMove move, float duration = 0.5f)
    {
        StartCoroutine(RotateAxisCoroutine(move.axis, move.cube, move.clockWise, duration, move.orientation));
    }

    /// <summary>
    /// Fonction qui Lance la coroutine qui permet de faire tourner n'importe quelle partie du cube.
    /// </summary>
    /// <param name="axis">L'un des 6 Axes X/Y/Z/-X/-Y/-Z ET le Milieu</param> 
    /// <param name="selectedCube">The cube the player is looknig at</param>
    /// <param name="clockWise">Sens de rortation de l'axe</param>
    /// <param name="duration">frere abuse un peu</param>
    /// <param name="sliceAxis">Indique autour de quelle axes X/Y/Z doit tourner la slice du cube </param>
    /// <returns></returns>
    public void RotateAxis(Transform axis, Transform selectedCube, bool clockWise, float duration = 0.5f, SliceAxis sliceAxis = SliceAxis.Useless)
    {
        StartCoroutine(RotateAxisCoroutine(axis, selectedCube, clockWise, duration, sliceAxis));
    }

    /// <summary>
    /// Fonction qui permet de faire tourner n'importe quelle partie du cube.
    /// </summary>
    /// <param name="axis">L'un des 6 Axes X/Y/Z/-X/-Y/-Z ET le Milieu</param> 
    /// <param name="selectedCube">The cube the player is looknig at</param>
    /// <param name="clockWise">Sens de rortation de l'axe</param>
    /// <param name="duration">frere abuse un peu</param>
    /// <param name="sliceAxis">Indique autour de quelle axes X/Y/Z doit tourner la slice du cube </param>
    /// <returns></returns>
    public IEnumerator RotateAxisCoroutine(Transform axis, Transform selectedCube, bool clockWise, float duration = 0.5f, SliceAxis sliceAxis = SliceAxis.Useless)
    {
        if (_isRotating) yield break;
        _isRotating = true;


        Vector3 rotationAxis = Vector3.zero;
        {
            if (Mathf.Abs(axis.localPosition.x) > 0.5f)
                rotationAxis = Vector3.right;
            else if (Mathf.Abs(axis.localPosition.y) > 0.5f)
                rotationAxis = Vector3.up;
            else if (Mathf.Abs(axis.localPosition.z) > 0.5f)
                rotationAxis = Vector3.forward;
        }

        bool isMiddle = true;

        Vector3 localAxisPos = axis.localPosition;
        Vector3 localRefPos = selectedCube.localPosition;

        List<int> blockIndexs = new List<int>();
        foreach (var block in _allBlocks)
        {
            Vector3 localBlockPos = block.transform.localPosition;

            bool isOnSamePlane =
                          (rotationAxis == Vector3.forward && Mathf.Abs(localBlockPos.z - localRefPos.z) < 0.5f)
                       || (rotationAxis == Vector3.up && Mathf.Abs(localBlockPos.y - localRefPos.y) < 0.5f)
                       || (rotationAxis == Vector3.right && Mathf.Abs(localBlockPos.x - localRefPos.x) < 0.5f);

            if (isOnSamePlane)
            {
                if (block.name == "Corner") isMiddle = false;
                block.transform.SetParent(axis, true);
                blockIndexs.Add(_allBlocks.IndexOf(block));
            }
        }

        if (isMiddle) middleGameObject.parent = axis;


        foreach (int i in blockIndexs)
        {
            if (_allBlocks[i].gameObject.name != "Middle")
            {
                var tiles = _allBlocks[i].transform.GetComponentsInChildren<Tile>().ToList();
                foreach (Tile tile in tiles)
                {
                    if (!tile.IsOccupied)
                        continue;
                    switch (sliceAxis)
                    {
                        case SliceAxis.X:
                            if (transform.localPosition.z - _allBlocks[i].transform.localPosition.z < 0 && clockWise
                                || transform.localPosition.z - _allBlocks[i].transform.localPosition.z > 0 && !clockWise)
                                tile.OnPropulsion?.Invoke(new Vector3(0, 0, transform.localPosition.z - _allBlocks[i].transform.localPosition.z).normalized);
                            break;
                        case SliceAxis.Y:
                            if (transform.localPosition.y - _allBlocks[i].transform.localPosition.y < 0 && clockWise
                                || transform.localPosition.y - _allBlocks[i].transform.localPosition.y > 0 && !clockWise)
                                tile.OnPropulsion?.Invoke(new Vector3(0, transform.localPosition.y - _allBlocks[i].transform.localPosition.y, 0).normalized);
                            break;
                        case SliceAxis.Z:
                            if (transform.localPosition.x - _allBlocks[i].transform.localPosition.x < 0 && clockWise
                                || transform.localPosition.x - _allBlocks[i].transform.localPosition.x > 0 && !clockWise)
                                tile.OnPropulsion?.Invoke(new Vector3(transform.localPosition.x - _allBlocks[i].transform.localPosition.x, 0, 0).normalized);
                            break;
                    }
                }
            }
        }

        int direction = clockWise ? 1 : -1;


        Quaternion startRotation = axis.localRotation;
        Quaternion targetRotation = Quaternion.AngleAxis(direction * 90, rotationAxis) * startRotation;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            axis.localRotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / duration);
            yield return null;
        }
        axis.localRotation = targetRotation;

        foreach (int i in blockIndexs)
        {
            Vector3 pos = _allBlocks[i].transform.localPosition;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            pos.z = Mathf.Round(pos.z);
            _allBlocks[i].transform.localPosition = pos;
            _allBlocks[i].transform.SetParent(this.transform.parent, true);

        }


        if (isMiddle)
        {
            middleGameObject.parent = transform.parent;
        }

        _isRotating = false;

        if (!_isReversing)
        {
            RubiksMove move = new()
            {
                axis = axis,
                cube = selectedCube,
                orientation = sliceAxis,
                clockWise = clockWise
            };
            _moves.Add(move);
        }
    }

    RubiksMove CreateRandomMove()
    {
        int ran = Random.Range(0, _allBlocks.Count - 1);
        RubiksMove move = new()
        {
            cube = _allBlocks[ran],
            orientation = (SliceAxis)(ran % 3),
            axis = GetAxisFromCube(_allBlocks[ran], (SliceAxis)(ran % 3)),
            clockWise = Random.Range(0, 2) % 2 == 0
        };

        return move;
    }


    public List<Transform> GetCubesFromFace(Transform cube, SliceAxis sliceAxis)
    {
        bool isMiddle = cube.name.Contains("Face");

        Vector3 rotationAxis = sliceAxis == SliceAxis.X ? Vector3.right :
                                      sliceAxis == SliceAxis.Y ? Vector3.forward :
                                      Vector3.up;

        List<Transform> result = new List<Transform>();
        foreach (var block in _allBlocks)
        {

            Vector3 localBlockPos = block.localPosition;
            Vector3 localRefPos = cube.localPosition;

            if (isMiddle)
            {
                float blockAxisValue = sliceAxis == SliceAxis.X ? localBlockPos.x :
                                      sliceAxis == SliceAxis.Y ? localBlockPos.y :
                                      localBlockPos.z; //si X use pos.x else si Y use pos.y else use pos.z

                float refAxisValue = sliceAxis == SliceAxis.X ? localRefPos.x :
                         sliceAxis == SliceAxis.Y ? localRefPos.y :
                         localRefPos.z;

                if (Mathf.Abs(blockAxisValue - refAxisValue) < 0.5f)
                {
                    result.Add(block);
                }
            }
            else
            {

                bool isOnSamePlane =
              (rotationAxis == Vector3.up && Mathf.Abs(localBlockPos.z - localRefPos.z) < 0.5f) || // Rotating around Y -> Match Z
              (rotationAxis == Vector3.forward && Mathf.Abs(localBlockPos.y - localRefPos.y) < 0.5f) || // Rotating around X -> Match Y
              (rotationAxis == Vector3.right && Mathf.Abs(localBlockPos.x - localRefPos.x) < 0.5f); // Rotating around Z -> Match X

                if (isOnSamePlane)
                {
                    result.Add(block);
                }
            }
        }
        return result;
    }

    public Transform GetAxisFromCube(Transform cube, SliceAxis sliceAxis)
    {
        if (cube.name.Contains("Face"))
        {
            return middle;
        }

        float OldDistance = float.MaxValue;
        Transform closestAxis = null;
        foreach (Transform t in Axis)
        {
            if (t != Axis[0])
            {
                if (t.name.Contains("X") && sliceAxis == SliceAxis.X
                || t.name.Contains("Y") && sliceAxis == SliceAxis.Y
                || t.name.Contains("Z") && sliceAxis == SliceAxis.Z)
                {
                    float newDistance = Vector3.Distance(t.position, cube.position);
                    if (newDistance < OldDistance)
                    {
                        OldDistance = newDistance;
                        closestAxis = t;
                    }
                }


            }
        }
        return closestAxis;
    }
    private void OnValidate()
    {
        if (IsLockXAxis && IsLockYAxis && IsLockZAxis) _isLockXAxis = false;
    }
}

namespace RubiksStatic
{
    class RubiksMove
    {
        public Transform axis;
        public Transform cube;
        public SliceAxis orientation;
        public bool clockWise;

        public void Print()
        {
            Debug.Log("Axis : " + axis + " cube : " + cube + " Orient : " + orientation + " ClockWise : " + clockWise);
        }
    }
    public enum SliceAxis { X, Y, Z, Useless }

}