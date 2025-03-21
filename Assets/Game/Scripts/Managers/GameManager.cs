using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameSettings Settings => settings;
    [SerializeField] private GameSettings settings;

    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    [SerializeField] string nextScene;

    public static GameManager Instance => instance;
    private static GameManager instance;

    private void Awake()
    {
        if (instance) Destroy(this);
        else instance = this;
    }

    private void OnEnable()
    {
        EventManager.OnSceneChange += ChangeScene;
    }

    private void OnDisable()
    {
        EventManager.OnSceneChange -= ChangeScene;
        
        EventManager.OnPlayerWin -= ShowWinScreen;
        EventManager.OnPlayerLose -= ShowLoseScreen;
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
}
