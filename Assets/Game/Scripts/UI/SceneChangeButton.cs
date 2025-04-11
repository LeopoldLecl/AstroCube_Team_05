using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeButton : MonoBehaviour
{
    [SerializeField] string _sceneName;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(_sceneName);
    } 
}
