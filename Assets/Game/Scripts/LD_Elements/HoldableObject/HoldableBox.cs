using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableBox : MonoBehaviour, IHoldable
{
    private Transform _originalParent;
    private Rigidbody _rb;

    void Start()
    {
        _originalParent = transform.parent;
        LayerMask maskCube = LayerMask.GetMask("Holdable");
        _rb = GetComponent<Rigidbody>();
    }

    public Transform GetOriginalParent()
    {
        return _originalParent;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void OnHold()
    {
        transform.position = transform.parent.position;
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezePosition;
    }

    public void OnRelease()
    {
        _rb.useGravity = true;
        _rb.constraints = RigidbodyConstraints.None;
    }
}
