//using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossfadeTransition : MonoBehaviour
{
    public float currentOpacity;

    Image _screen;
    
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

    private void Start()
    {
        _screen = GetComponent<Image>();
    }

    private void Update()
    {
        _screen.material.SetFloat("_Alpha", currentOpacity);
    }
}
