using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

public class RubicsCube : MonoBehaviour
{

    [SerializeField] private GameObject[] _tilesPerFaces = new GameObject[6 * 9];
    [SerializeField] private Transform[] _foldoutTransforms = new Transform[6];
    [SerializeField] private static List<Vector3> _positionsSaves = new();


    public static GameObject[] Selection {  get => _selection; set => _selection = value; }
    private static GameObject[] _selection = { null };

    public static EIsolationMode IsolationMode { get => _isolationMode; set => _isolationMode = value; }
    private static EIsolationMode _isolationMode = EIsolationMode.TILE;

    public static UnityAction OnReset;


    public void Reset()
    {
        OnReset?.Invoke();
        if (Selection[0] != null)
            DeIsolate(_isolationMode, RubicsCube.Selection[0].transform, transform);
        ChangeWallsVisibility(transform, true);
    }


    public void ChangeWallsVisibility(Transform parent, bool visibility)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var cube = parent.GetChild(i);
            for (int j = 0; j < cube.childCount; j++)
            {
                var tile = cube.GetChild(j);
                if (tile.gameObject.layer == LayerMask.NameToLayer("Tile"))
                {
                    _ChangeFloorVisibily(tile, visibility);
                }
            }            
        }
        if (!visibility)
        {
            for (int tile = 0; tile < 9; tile++)
            {
                Transform floor = _tilesPerFaces[tile].transform;
                _ChangeFloorVisibily(floor, true);
            }            
        }
    }

    private void _ChangeFloorVisibily(Transform parent, bool visibility)
    {
        for (int k = 0; k < parent.childCount; k++)
        {
            if (parent.GetChild(k).gameObject.layer == LayerMask.NameToLayer("Floor"))
            {
                parent.GetChild(k).gameObject.SetActive(visibility);
            }
        }
    }

    #region Isolate

    public void Isolate(EIsolationMode mode, Transform selection, Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            switch (mode)
            {
                case EIsolationMode.CUBE:
                    if (i != selection.GetSiblingIndex())
                        parent.GetChild(i).gameObject.SetActive(false);
                    break;
                case EIsolationMode.TILE:
                    Transform selectionCube = selection.parent;
                    if (i != selectionCube.GetSiblingIndex())
                        parent.GetChild(i).gameObject.SetActive(false);
                    else
                    {
                        for (int j = 0; j < selectionCube.childCount; j++)
                        {
                            if (j != selection.GetSiblingIndex() && selectionCube.GetChild(j).gameObject.layer == LayerMask.NameToLayer("Tile"))
                                selectionCube.GetChild(j).gameObject.SetActive(false);
                        }
                    }
                    break;
            }
        }
    }

    public void DeIsolate(EIsolationMode mode, Transform selection, Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            switch (mode)
            {
                case EIsolationMode.CUBE:
                    if (i != selection.GetSiblingIndex())
                        parent.GetChild(i).gameObject.SetActive(true);
                    break;
                case EIsolationMode.TILE:
                    Transform selectionCube = selection.parent;
                    if (i != selectionCube.GetSiblingIndex())
                        parent.GetChild(i).gameObject.SetActive(true);
                    else
                    {
                        for (int j = 0; j < selectionCube.childCount; j++)
                        {
                            if (j != selection.GetSiblingIndex() && selectionCube.GetChild(j).gameObject.layer == LayerMask.NameToLayer("Tile"))
                                selectionCube.GetChild(j).gameObject.SetActive(true);
                        }
                    }
                    break;
            }
        }
    }

    #endregion

    private void OnDestroy()
    {
        Reset();
    }

    private void OnApplicationQuit()
    {
        Reset();
    }


}
