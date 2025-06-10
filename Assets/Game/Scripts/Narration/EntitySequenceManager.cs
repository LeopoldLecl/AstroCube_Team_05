using System.Collections.Generic;
using UnityEngine;

public class EntitySequenceManager : MonoBehaviour
{
    [SerializeField] private TextAnimation _textAnimation;
    [SerializeField] List<GameObject> _objectToDisable;

    public void StartAnim() => _ToggleObjects(false);
    public void StopAnim()
    {
        _ToggleObjects(true);
        gameObject.SetActive(false);
    }

    private void _ToggleObjects(bool isActive)
    {
        foreach (var obj in _objectToDisable)
        {
            if (obj)
                obj.gameObject.SetActive(isActive);
        }

    }
    public void DisplayText()
    {
        _textAnimation.DisplayText();
    }

    public void DistortScreen(float duration)
    {
        PostProcessManager.Instance.SetScreenDistortion(1.0f, duration);
    }

    public void SetDistortion(float amount)
    {
        PostProcessManager.Instance.SetScreenDistortion(amount);
    }

    private void OnDisable()
    {
        PostProcessManager.Instance.SetScreenDistortion(0.0f);
    }
}
