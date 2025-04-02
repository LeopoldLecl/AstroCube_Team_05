using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeThisGameobjectPersistent : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
