//
//  Outline.cs
//  QuickOutline
//
//  Created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class SelectionCube : MonoBehaviour
{
    [SerializeField]
    bool _isTileLocked;
    [SerializeField]
    int _defaultRenderingLayerMask, _cubeSelectionRenderingLayerMask, _axisSelectionRenderingLayerMask, _axisLockRenderingLayerMask = 6, _playerOnTileRenderingLayerMask = 5;

    private Renderer[] _renderers;
    [SerializeField] Material greyMat;
    private Material[] allOldMat = new Material[0];



    public bool IsTileLocked { get => _isTileLocked; set => _isTileLocked = value; }

    public enum SelectionMode
    {
        AXIS,
        CUBE,
        LOCKED,
        PLAYERONTILE
    }

    void Awake()
    {
        // Cache renderers
        _renderers = GetComponentsInChildren<Renderer>();

        if (_isTileLocked) SetTileMatToLock();
    }

    public void SetIsTileLock(bool locking)
    {
        IsTileLocked = locking;
        if (locking) SetTileMatToLock();
        else SetTilestoBaseMat();
    }

    public void SetTileMatToLock()
    {
        _isTileLocked = true;
        allOldMat = new Material[_renderers.Length];

        int i = 0;
        foreach (Renderer r in _renderers)
        {
            allOldMat[i++] = r.material;
            r.material = greyMat;
        }
    }

    public void SetTilestoBaseMat()
    {
        _isTileLocked = false;

        int i = 0;
        foreach (Renderer r in _renderers)
        {
            r.material = allOldMat[i++];
        }
    }

    public void Select(SelectionMode mode)
    {
        foreach (var renderer in _renderers)
        {
            switch (mode)
            {
                case SelectionMode.AXIS:
                    renderer.renderingLayerMask = (uint)Mathf.Pow(2, _axisSelectionRenderingLayerMask);
                    break;
                case SelectionMode.CUBE:
                    renderer.renderingLayerMask = (uint)Mathf.Pow(2, _cubeSelectionRenderingLayerMask);
                    break;
                case SelectionMode.LOCKED:
                    renderer.renderingLayerMask = (uint)Mathf.Pow(2, _axisLockRenderingLayerMask);
                    break;
                case SelectionMode.PLAYERONTILE:
                    renderer.renderingLayerMask = (uint)Mathf.Pow(2, _axisLockRenderingLayerMask);
                    break;
            }
        }
    }

    public void Unselect()
    {
        foreach (var renderer in _renderers)
        {
            renderer.renderingLayerMask = (uint)Mathf.Pow(2, _defaultRenderingLayerMask);
        }
    }
}
