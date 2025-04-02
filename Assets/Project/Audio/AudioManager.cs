using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[Serializable]
public class Music
{
    public string name;
    [HideInInspector] public AudioSource source;
    public AudioClip clip;
    public AudioMixerGroup Out;
    [SerializeField, Range(0, 1)] public float volume = 1f;
    [SerializeField, Range(-3, 3)] public float pitch = 1f;
    public bool loop;
}

public class AudioManager : MonoBehaviour
{

    [HideInInspector] public static AudioManager Instance { get; private set; }

    public Music CurrentMusicPlaying { get; private set; }
    [Space(10)]
    [SerializeField] private List<Music> _musics = new List<Music>();
    public List<Music> musicList { get; private set; } = new List<Music>();

    private void Awake()
    {
        SceneManager.activeSceneChanged += (previousScene, newScene) => CheckScenes(previousScene, newScene);

        if (Instance == null)
        {
            Instance = this;
        }

        musicList = _musics;

        foreach (Music m in musicList)
        {
            m.source = gameObject.AddComponent<AudioSource>();

            if (m.source is null) Debug.LogError(m.name + "'s source is null !");
            m.source.clip = m.clip;
            m.source.outputAudioMixerGroup = m.Out;

            m.source.volume = m.volume;
            m.source.pitch = m.pitch;
            m.source.loop = m.loop;

        }
    }
    private void CheckScenes(Scene prevS, Scene newS)
    {
        Debug.Log($"Checking scenes... previous : {prevS.name}, new :{newS.name}");
        switch (newS.buildIndex)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 6:
            case 9:
                PlayMusic("MainTheme");
                break;
            case 4:
            case 5:
                Debug.Log($"CASE NERD SCENE");
                PlayMusic("Nerd");
                break;
            case 7:
            case 8:
                Debug.Log($"CASE BULLY SCENE");
                PlayMusic("Bully");
                break;
            case 10:
            case 11:
            case 12:
                Debug.Log($"CASE EMO SCENE");
                PlayMusic("Emo");
                break;
            case 13:
            case 14:
            case 15:
                Debug.Log($"CASE GAME OVER");
                break;
        }

    }

    public bool PlayMusic(string name)
    {
        if (CurrentMusicPlaying != null)
        {
            if(CurrentMusicPlaying.name == name) return false;
            StopMusic(CurrentMusicPlaying.name);
            CurrentMusicPlaying = null;
        }

        Music s = musicList.Find(mus => mus.name == name);
        if (s == null)
        {
            Debug.Log("Can't find " + name);
            return false;
        }
        s.source.Play();

        if (s.source.isPlaying == true)
        {
            CurrentMusicPlaying = s;
            Debug.Log($"<b><color=green> '{s.name}' PLAYING WOOOOOOOOOOOOOOOOOOOOOOOOOOO </color></b>");
        }
        else Debug.LogError($"\"<b><color=red> AAAAAAAAAAAAAAAAAAAH {s.name} NOT PLAYING </color></b>\"");

        return s.source.isPlaying;
    }

    public void StopMusic(string name)
    {
        Music s = musicList.Find(mus => mus.name == name);
        if (s == null)
        {
            Debug.LogError($"Can't find {name}");
            return;
        }
        s.source.Stop();
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= (prevS, newS) => CheckScenes(prevS, newS);
    }
}
