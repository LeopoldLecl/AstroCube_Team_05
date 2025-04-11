using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossfadeTransition : MonoBehaviour
{


    private void OnEnable()
    {
        EventManager.OnPlayerWin += StartFade;
        EventManager.OnStartSequence += FadeOut;
        EventManager.OnEndSequence += FadeIn;
    }   
    private void OnDisable()
    {
        EventManager.OnPlayerWin -= StartFade;
        EventManager.OnStartSequence -= FadeOut;
        EventManager.OnEndSequence -= FadeIn;
    }

    void StartFade()
    {
        GetComponent<Animator>().SetTrigger("StartFade");
    }

    void FadeOut()
    {
        GetComponent<Animator>().SetTrigger("FadeOut");
    }

    void FadeIn()
    {
        GetComponent<Animator>().SetTrigger("FadeIn");
    }

    public void ChangeSceneAfterAnimation()
    {
        EventManager.TriggerSceneChange();
    }
}
