using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeakerDatabase", menuName = "Database/Speaker", order = 1)]
public class SpeakerDatabase : ScriptableObject
{
    public List<SpeakerData> speakerDatas = new();
}
