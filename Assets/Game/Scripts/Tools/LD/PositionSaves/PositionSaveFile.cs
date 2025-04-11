using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Test", fileName = "Yippie")]
public class PositionSaveFile : ScriptableObject
{
    public List<GameObject> objects;
    public List<Vector3> positions;
    public List<Vector3> eulerAngles;
    public List<Vector3> scales;
}
