//using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossfadeTransition : MonoBehaviour
{
    public float currentOpacity;

    [SerializeField] GameObject _kaleidoscopeCam;

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

        if (currentOpacity == 0 && _kaleidoscopeCam.activeSelf == true)
        {
            _screen.enabled = false;
            _kaleidoscopeCam.SetActive(false);
        }
        
        if (currentOpacity != 0 && _kaleidoscopeCam.activeSelf == false)
        {
            _screen.enabled = true;
            _kaleidoscopeCam.SetActive(true);
        }
    }
}
