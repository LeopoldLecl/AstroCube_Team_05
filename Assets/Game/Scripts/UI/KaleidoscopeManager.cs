using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KaleidoscopeManager : MonoBehaviour
{
    [SerializeField] GameObject _sourceCamera;
    [SerializeField] float _fadeSpeed = 0.1f;

    Image _screen;
    Material _kaleidoscopeMat;
    bool _isEnabled = false;
    float _currentOpacity = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _screen = GetComponent<Image>();
        _kaleidoscopeMat = GetComponent<Image>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            _isEnabled = !_isEnabled;
        }

        if (_isEnabled && _currentOpacity < 1)
        {
            if (_sourceCamera.activeInHierarchy == false) _sourceCamera.SetActive(true);
            if (_screen.enabled == false) _screen.enabled = true;

            if (_currentOpacity < 1) 
            {
                _currentOpacity += _fadeSpeed;
                _currentOpacity = _currentOpacity >= 1 ? 1 : _currentOpacity;
                _screen.material.SetFloat("_Alpha", _currentOpacity);
            }
        } 

        if (!_isEnabled && _currentOpacity > 0) 
        {
            if (_currentOpacity > 0)
            {
                _currentOpacity -= _fadeSpeed;
                _currentOpacity = _currentOpacity <= 0 ? 0 : _currentOpacity;
                _screen.material.SetFloat("_Alpha", _currentOpacity);

                if (_currentOpacity == 0)
                {
                    _sourceCamera.SetActive(false);
                    _screen.enabled = false;
                }
            }
        }
    }

    public void IsEnabled(bool isEnabled)
    {
        _isEnabled = isEnabled;
    }
}

