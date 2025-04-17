using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctionality : MonoBehaviour
{
    public void ContinueButton()
    {
        EventManager.TriggerGameUnpause();
    }

    public void SettingsButton()
    {
        Debug.Log("You clicked on the SETTINGS button.");
    }

    public void QuitButton()
    {
        //Application.Quit();
        SceneManager.LoadScene("StartMenu");
    }
}
