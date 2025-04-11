using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static UnityEngine.Timeline.TimelineAsset;

public class EntitySequenceManager : MonoBehaviour
{
    [SerializeField] private TextAnimation _textAnimation;
    [SerializeField] private Material _distortMat;
    [SerializeField] private UniversalRendererData _data;
    FullScreenPassRendererFeature _distortionRenderFeature;

    // Start is called before the first frame update
    void Start()
    {
        _distortionRenderFeature =  _data.rendererFeatures.First(x => x.name == "Distort") as FullScreenPassRendererFeature;
        _distortionRenderFeature.passMaterial.SetFloat("_DistortionAmount", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayText()
    {
        StartCoroutine(_textAnimation.StartDisplayText());
    }

    public void DistortScreen(float duration)
    {
        DOTween.To(() => _distortionRenderFeature.passMaterial.GetFloat("_DistortionAmount"), x => _distortionRenderFeature.passMaterial.SetFloat("_DistortionAmount", x), 5f, duration);
    }

    public void SetDistortion(float amount)
    {
        _distortionRenderFeature = _data.rendererFeatures.First(x => x.name == "Distort") as FullScreenPassRendererFeature;
        _distortionRenderFeature.passMaterial.SetFloat("_DistortionAmount", amount);
    }

    private void OnDisable()
    {
        _distortionRenderFeature.passMaterial.SetFloat("_DistortionAmount", 1);
    }
}
