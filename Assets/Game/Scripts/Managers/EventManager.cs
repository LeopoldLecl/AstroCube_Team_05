using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance => instance;
    private static EventManager instance;

    private GameSettings _gameSettings;

    public static bool gamePaused = false;

    private void Awake()
    {
        if (instance) Destroy(this);
        else instance = this;

        _gameSettings = GameManager.Instance.Settings;
    }

    //Game Events
    public static event Action OnSceneStart;
    public static event Action OnSceneEnd;

    public static event Action OnPlayerWin;
    public static event Action OnPlayerLose;

    public static event Action OnSceneChange;

    public static event Action OnGamePause;
    public static event Action OnGameUnpause;


    //Rubik's Cube Events
    public static event Action OnCubeRotated;

    //Object Events
    public static event Action OnButtonPressed;
    public static event Action OnButtonReleased;

    //Player Events
    public static event Action<float> OnPlayerReset;
    public static event Action<float> OnPlayerResetOnce;

    public static event Action OnStartSequence;
    public static event Action OnEndSequence;

    public static void TriggerPlayerWin()
    {
        OnPlayerWin?.Invoke();
    }

    public void TriggerPlayerLose()
    {
        Debug.Log("Lose Event Triggered!");
        OnPlayerLose?.Invoke();
        TriggerReset();
    }    
    
    public static void TriggerSceneChange()
    {
        OnSceneChange?.Invoke();
    }

    public static void TriggerGamePause()
    {
        OnGamePause?.Invoke();
        gamePaused = true;
    }

    public static void TriggerGameUnpause()
    {
        OnGameUnpause?.Invoke();
        gamePaused = false;
    }

    public static void TriggerButtonPressed()
    {
        OnButtonPressed?.Invoke();
    }

    public static void TriggerButtonReleased()
    {
        OnButtonReleased?.Invoke();
    }

    public void TriggerReset()
    {
        OnPlayerReset?.Invoke(_gameSettings.ResetDuration);
    }
    public void TriggerResetOnce()
    {
        OnPlayerResetOnce?.Invoke(_gameSettings.ResetDuration/4);
    }

    public static void TriggerSceneStart()
    {
        OnSceneStart?.Invoke();
    }

    public static void TriggerSceneEnd() {
        OnSceneEnd?.Invoke();
    }

    public static void TriggerSequenceStart()
    {
        OnStartSequence?.Invoke();
    }

    public static void TriggerSequenceEnd()
    {
        OnEndSequence?.Invoke();
    }

    public static void TriggerCubeRotated()
    {
        OnCubeRotated?.Invoke();
    }
}
