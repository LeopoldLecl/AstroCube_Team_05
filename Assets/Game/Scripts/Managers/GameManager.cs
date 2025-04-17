using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameSettings Settings => settings;
    [SerializeField] private GameSettings settings;

    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    [SerializeField][Scene] string nextScene;

    [Header("Entity Sequence")]
    [SerializeField] EntitySequenceManager _entitySequenceManager;
    [SerializeField] Image _fade;
    [SerializeField] float _sequenceDuration;
    [SerializeField] List<GameObject> _objectToDisable;

    public static GameManager Instance => instance;
    private static GameManager instance;


    [SerializeField] private GameObject _rubiksCubeUI;
    public bool IsRubiksCubeEnabled => _isRubiksCubeEnabled;
    [SerializeField, ReadOnly] private bool _isRubiksCubeEnabled;


    private void Awake()
    {
        if (instance) Destroy(this);
        else instance = this;
    }

    public enum EScreenshakeMode
    {
        RUBIKS_CUBE_ROTATION
    }

    public void Screenshake(EScreenshakeMode mode)
    {
        switch (mode)
        {
            default:
                break;
            case EScreenshakeMode.RUBIKS_CUBE_ROTATION:
                Camera.main.DOShakePosition(settings.RubiksCubeRotationScreenshakeSettings.x,
                                            settings.RubiksCubeRotationScreenshakeSettings.y,
                                            (int)settings.RubiksCubeRotationScreenshakeSettings.z,
                                            settings.RubiksCubeRotationScreenshakeSettings.w);
                break;
        }
    }
    void ScreenshakeCubeRotation() => Screenshake(EScreenshakeMode.RUBIKS_CUBE_ROTATION);

    private void OnEnable()
    {
        EventManager.OnSceneChange += ChangeScene;

        EventManager.OnStartCubeRotation += ScreenshakeCubeRotation;

        EventManager.OnGamePause += StopDeltTime;
        EventManager.OnGamePause += UnlockMouse;
        EventManager.OnGameUnpause += ResetDeltaTime;
        EventManager.OnGameUnpause += LockMouse;
    }

    private void OnDisable()
    {
        EventManager.OnSceneChange -= ChangeScene;

        EventManager.OnStartCubeRotation -= ScreenshakeCubeRotation;

        EventManager.OnGamePause -= StopDeltTime;
        EventManager.OnGamePause -= UnlockMouse;
        EventManager.OnGameUnpause -= ResetDeltaTime;
        EventManager.OnGameUnpause -= LockMouse;
    }

    private void Start()
    {
        EventManager.TriggerSceneStart();
    }

    void ShowWinScreen()
    {
        winScreen.SetActive(true);
        Debug.Log("Victoire !");
    }

    void ShowLoseScreen()
    {
        loseScreen.SetActive(true);
        Debug.Log("D�faite !");
    }

    void ChangeScene()
    {
        SceneManager.LoadScene(nextScene);
    }

    void StopDeltTime()
    {
        Time.timeScale = 0;
    }    
    
    void ResetDeltaTime()
    {
        Time.timeScale = 1f;
    }

    void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartSequence() => StartCoroutine(ShowSequence());
    public IEnumerator ShowSequence()
    {
        EventManager.TriggerNarrativeSequenceStart();

        yield return DOTween.To(() => new Color(0, 0, 0, 0), x => _fade.color = x, new Color(0, 0, 0, 1.0f), 1.0f).WaitForCompletion();
        _fade.color = new Color(0, 0, 0, 0);

        //yield return new WaitForSeconds(1.0f);

        foreach (var obj in _objectToDisable)
        {
            obj.gameObject.SetActive(false);
        }

        _entitySequenceManager.gameObject.SetActive(true);

        yield return new WaitForSeconds(_sequenceDuration);

        _entitySequenceManager.gameObject.SetActive(false);
        Debug.Log("DisableEntity"); 
        foreach (var obj in _objectToDisable)
        {
            obj.gameObject.SetActive(true);
        }

        EventManager.TriggerNarrativeSequenceEnd();
        _fade.color = new Color(0, 0, 0, 1.0f);
        yield return DOTween.To(() => new Color(0, 0, 0, 1.0f), x => _fade.color = x, new Color(0, 0, 0, 0.0f), 1.0f).WaitForCompletion();

        InputHandler.Instance.CanMove = true;
    }

    [Button("Enable Rubik's Cube")]
    public void EnableRubiksCube() => ToggleRubiksCube(true);

    [Button("Disable Rubik's Cube")]
    public void DisableRubiksCube() => ToggleRubiksCube(false);
    
    private void ToggleRubiksCube(bool isEnabled)
    {
        _isRubiksCubeEnabled = isEnabled;
        _rubiksCubeUI.SetActive(isEnabled);
    }
}
