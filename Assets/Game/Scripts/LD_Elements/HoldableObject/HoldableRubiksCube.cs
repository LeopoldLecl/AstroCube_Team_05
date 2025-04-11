using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableRubiksCube : MonoBehaviour, IHoldable
{
    private Transform _originalParent;
    private Transform _originalTransform;
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

    public void OnHold(Transform newParent)
    {
        StartCoroutine(HoldRubiksCube(newParent));
        //transform.position = transform.parent.position;
    }

    IEnumerator HoldRubiksCube(Transform newParent)
    {
        InputHandler.Instance.CanMove = false;
        transform.DOMove(newParent.position, 1.0f);
        transform.DOScale(newParent.localScale, 1.0f);
        yield return new WaitForSeconds(1.0f);
        transform.parent = newParent;

        GameManager.Instance.StartSequence();
    }

    public void OnRelease() { return; }
}
