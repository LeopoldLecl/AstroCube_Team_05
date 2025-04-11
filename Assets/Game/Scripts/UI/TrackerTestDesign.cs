using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class TrackerTestDesign : MonoBehaviour
{
    public int rotationTracker { get; private set; } = 0;
    public float timeTracker { get; private set; } = 0f;    

    [SerializeField] private TextMeshProUGUI timeTrackerText;
    [SerializeField] private TextMeshProUGUI rotationTrackerText;

    private bool isTimerRunning = false;



    private void OnEnable()
    {
        EventManager.OnSceneStart += StartTimer;
        EventManager.OnPlayerWin += StopTimer;
        EventManager.OnCubeRotated += IncrementRotation;
    }

    private void OnDisable()
    {
        EventManager.OnSceneStart -= StartTimer;
        EventManager.OnPlayerWin -= StopTimer;
        EventManager.OnCubeRotated -= IncrementRotation;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            timeTracker += Time.deltaTime;
            UpdateUI();
        }
    }

    private void StartTimer()
    {
        isTimerRunning = true;
    }

    private void StopTimer()
    {
        print("Tester Took : " + timeTrackerText.text + " on level " + SceneManager.GetActiveScene().name);
        isTimerRunning = false;
    }

    private void ResetTracker()
    {
        isTimerRunning = false;
        timeTracker = 0f;
        rotationTracker = 0;
        UpdateUI();
    }

    private void IncrementRotation()
    {
        rotationTracker++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        TimeSpan UITimeAsTS = TimeSpan.FromSeconds(timeTracker);
        if (timeTrackerText != null)
            timeTrackerText.text = "Time : " + (UITimeAsTS.Minutes < 10 ? "0" + UITimeAsTS.Minutes : UITimeAsTS.Minutes) + ":" + (UITimeAsTS.Seconds < 10 ? "0" + UITimeAsTS.Seconds : UITimeAsTS.Seconds) + "," + UITimeAsTS.Milliseconds;

        if (rotationTrackerText != null)
            rotationTrackerText.text = "Rotations : " + rotationTracker;
    }
}
