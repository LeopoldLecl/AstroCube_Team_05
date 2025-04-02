using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 0.5f;
    [SerializeField] public string sceneName;

    private Coroutine coroutine = null;

    public static Action SceneChanged;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void FadeToTheNextScene()
    {
        coroutine = StartCoroutine(FadeToScene(sceneName));
    }

    public IEnumerator FadeToNextScene()
    {
        Debug.Log($"Fading to scene {sceneName}" );
        Time.timeScale = 0.35f;

        // Commence la coroutine pour le fondu sortant
        yield return StartCoroutine(FadeOut());

        // Change de scène une fois que le fondu sortant est terminé
        SceneChanged.Invoke();
        SceneManager.LoadScene(sceneName);

        // Commence le fondu entrant sur la nouvelle scène
        yield return StartCoroutine(FadeIn());
        Time.timeScale = 1;
    }



    public IEnumerator FadeToScene(string sceneName)
    {
        Debug.Log($"Fading to scene {sceneName}");
        Time.timeScale = 0.35f;

        // Commence la coroutine pour le fondu sortant
        yield return StartCoroutine(FadeOut());

        // Change de scène une fois que le fondu sortant est terminé
        SceneManager.LoadScene(sceneName);

        // Commence le fondu entrant sur la nouvelle scène
        yield return StartCoroutine(FadeIn());
        Time.timeScale = 1;
    }

    private IEnumerator FadeOut()
    {
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);

        while (fadeImage.color.a < 1f)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, fadeImage.color.a + (fadeSpeed * Time.deltaTime));
            yield return null;
        }
    }

    private IEnumerator FadeIn()    
    {
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1f);

        while (fadeImage.color.a > 0f)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, fadeImage.color.a - (fadeSpeed * Time.deltaTime));
            yield return null;
        }

        // Marque la coroutine comme terminée pour permettre à la nouvelle scène de se charger
        Destroy(gameObject);
    }
}
