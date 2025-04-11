using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class PlaythroughDataTxtWrite : MonoBehaviour
{
    [SerializeField] string path = Application.dataPath + "/PlaythroughData.txt";

    TrackerTestDesign _ttd;

    float _completionTime;
    int _numOfMoves;
    
    // Start is called before the first frame update
    void Start()
    {
        _ttd = GetComponent<TrackerTestDesign>();
        CreateTextFile();
    }

    void CreateTextFile()
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "PlaythoughDataLog\n\n");
        }
    }

    private void OnEnable()
    {
        EventManager.OnSceneEnd += AddDataEntry;
    }

    private void OnDisable()
    {
        EventManager.OnSceneEnd -= AddDataEntry;
    }

    void AddDataEntry()
    {
        _completionTime = _ttd.timeTracker;
        _numOfMoves = _ttd.rotationTracker;

        string contentOutline = "Date: {0}\nScene Name: {1}\nTime Completed: {2}\nNumber Of Moves: {3}\n\n";
        string content = string.Format(contentOutline, 
            System.DateTime.Now.ToString(), 
            SceneManager.GetActiveScene().name, 
            _completionTime, 
            _numOfMoves
            );
        File.AppendAllText(path, content);

        Debug.Log("DataEntry in scene " + SceneManager.GetActiveScene().name);
    }
}

/* Date: XXX
 * Scene Name: XXX
 * Time Completed: XXX 
 * Number of Moves: XXX 
 * /n
 */
