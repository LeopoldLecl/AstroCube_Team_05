using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GoToNextScene : MonoBehaviour
{
    [SerializeField] GameObject _text;
    VideoPlayer _videoPlayer;

    bool _firstKeyPress = true;

    private void OnEnable()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.loopPointReached += ChangeScene;
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (_firstKeyPress)
            {
                _text.SetActive(true);
                _firstKeyPress = false;                
            } else
            {
                VideoPlayer source = _videoPlayer;
                ChangeScene(source);
            }
        }
    }

    void ChangeScene(VideoPlayer source)
    {
        SceneManager.LoadScene(1);
    }
}
