using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoldable 
{
    public void OnHold();
    public void OnRelease();
    public Transform GetTransform();
    public Transform GetOriginalParent();
}
