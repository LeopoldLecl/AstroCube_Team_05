using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour
{
    [field: SerializeField] public CustomisedSettings CustomSettings { get; private set; }
    public static GlobalManager Instance { get; private set; }
    private GameManager _gm;

    private void Awake()
    {
        _gm = GameManager.Instance;

        if (GlobalManager.Instance)
        {
            DestroyImmediate(this);
            return;
        }
        else
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
    }
    private void Start()
    {
        _gm.PreviewRubiksCube.gameObject.SetActive(CustomSettings);
    }

    private void OnEnable()
    {
        EventManager.OnPreviewChange += TogglePreview;
    }
    private void OnDisable()
    {
        EventManager.OnPreviewChange -= TogglePreview;
    }

    public void TogglePreview(bool isEnabled)
    {
        if (_gm.PreviewRubiksCube == null) return;

        Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);

        _gm.PreviewRubiksCube.gameObject.SetActive(isEnabled);
    }
}
