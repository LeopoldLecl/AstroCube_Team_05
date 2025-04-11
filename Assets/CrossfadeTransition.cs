using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossfadeTransition : MonoBehaviour
{


    private void OnEnable()
    {
        EventManager.OnPlayerWin += StartFade;
    }   
    private void OnDisable()
    {
        EventManager.OnPlayerWin -= StartFade;
    }

    void StartFade()
    {
        GetComponent<Animator>().SetTrigger("StartFade");
    }

    public void ChangeSceneAfterAnimation()
    {
        EventManager.TriggerSceneChange();

    }
}
