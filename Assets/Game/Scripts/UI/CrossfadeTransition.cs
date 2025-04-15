using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossfadeTransition : MonoBehaviour
{
    public float currentOpacity;
    public float segmentCount;
    [SerializeField] GameObject _kaleidoscopeCam;
    Image _screen;
    float _oldOpacity = 1;

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
        //if (currentOpacity == _oldOpacity) return;

        _screen.material.SetFloat("_Alpha", currentOpacity);
        _screen.material.SetFloat("_SegmentCount", segmentCount);

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

        _oldOpacity = currentOpacity;
    }
}
