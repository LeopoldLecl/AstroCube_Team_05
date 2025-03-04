using NaughtyAttributes;
using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private void Awake()
    {
        if(Instance) Destroy(this);
        else Instance = this;
    }

    //Game Events
    public static event Action OnSceneStart;
    public static event Action OnSceneEnd;

    public static event Action OnPlayerWin;
    public static event Action OnPlayerLose;


    //Rubik's Cube Events
    public static event Action OnCubeRotated;

    //Object Events
    public static event Action OnButtonPressed;
    public static event Action OnButtonReleased;

    //Player Events
    public static event Action OnPlayerReset;

    public static void TriggerPlayerWin()
    {
        OnPlayerWin?.Invoke();
    }

    public static void TriggerPlayerLose()
    {
        OnPlayerLose?.Invoke();
    }

    public static void TriggerButtonPressed()
    {
        OnButtonPressed?.Invoke();
    }

    public static void TriggerButtonReleased()
    {
        OnButtonReleased?.Invoke();
    }

    public static void TriggerReset()
    {
        OnPlayerReset?.Invoke();
    }

    public static void TriggerSceneStart()
    {
        OnSceneStart?.Invoke();
    }

    public static void TriggerSceneEnd() {
        OnSceneEnd?.Invoke();
    }

    public static void TriggerCubeRotated()
    {
        OnCubeRotated?.Invoke();
    }
}
