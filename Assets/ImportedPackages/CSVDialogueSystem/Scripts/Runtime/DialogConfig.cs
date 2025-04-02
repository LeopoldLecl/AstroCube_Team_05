using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogConfig : MonoBehaviour
{
    public List<SpeakerDatabase> speakerDatabases = new();

    [System.Serializable]
    public struct SpeakerConfig
    {
        public enum POSITION
        {
            LEFT,
            MIDDLE,
            RIGHT
        }
        public POSITION position;
        public SpeakerDatabase speakerDatabase;
        public SpeakerData speakerData;

        public void SetPosition(POSITION newPosition)
        {
            this.position = newPosition;
        }
    }

    public List<SpeakerConfig> speakers = new();

    [System.Serializable]
    public struct SentenceConfig
    {
        public string key;
        public SpeakerData speakerData;
        public AudioClip audioClip;

        public SentenceConfig(string key, SpeakerData speaker)
        {
            this.key = key;
            speakerData = speaker;
            audioClip = null;
        }
    }

    public TextAsset csvDialog;
    
    public CSVTable table;
    [Header("SENTENCES")]
    public List<SentenceConfig> sentenceConfig = new();

    private void Awake()
    {
        if(csvDialog != null && !table.IsLoaded())
            table.Load(csvDialog);
    }
}
