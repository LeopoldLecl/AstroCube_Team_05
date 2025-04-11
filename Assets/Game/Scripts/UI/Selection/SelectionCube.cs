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
    int _defaultRenderingLayerMask, _cubeObjectSelectionRenderingLayerMask = 9, _axisObjectSelectionRenderingLayerMask = 10, _cubeSelectionRenderingLayerMask, _axisSelectionRenderingLayerMask, _axisLockRenderingLayerMask = 6, _playerOnTileRenderingLayerMask = 5, _objectLockRenderingLayerMask = 11;

    private Renderer[] _renderers;
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
        else Unselect();
    }

    public void SetTileMatToLock()
    {
        _isTileLocked = true;
        allOldMat = new Material[_renderers.Length];

        int i = 0;
        foreach (Renderer r in _renderers)
        {
            allOldMat[i++] = r.material;
            if (r.transform.CompareTag("Floor"))
                r.renderingLayerMask = (uint)Mathf.Pow(2, _axisLockRenderingLayerMask);
            else
                r.renderingLayerMask = (uint)Mathf.Pow(2, _objectLockRenderingLayerMask);
        }
    }

    public void Select(SelectionMode mode)
    {
        foreach (var renderer in _renderers)
        {
            switch (mode)
            {
                case SelectionMode.AXIS:
                    if (renderer.transform.CompareTag("Floor"))
                        renderer.renderingLayerMask = (uint)Mathf.Pow(2, _axisSelectionRenderingLayerMask);
                    else
                        renderer.renderingLayerMask = (uint)Mathf.Pow(2, _axisObjectSelectionRenderingLayerMask);                    
                    break;
                case SelectionMode.CUBE:
                    if (renderer.transform.CompareTag("Floor"))
                        renderer.renderingLayerMask = (uint)Mathf.Pow(2, _cubeSelectionRenderingLayerMask);
                    else
                        renderer.renderingLayerMask = (uint)Mathf.Pow(2, _cubeObjectSelectionRenderingLayerMask);                    
                    break;
                case SelectionMode.LOCKED:
                    if (renderer.transform.CompareTag("Floor"))
                        renderer.renderingLayerMask = (uint)Mathf.Pow(2, _axisLockRenderingLayerMask);
                    else
                        renderer.renderingLayerMask = (uint)Mathf.Pow(2, _objectLockRenderingLayerMask);
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
