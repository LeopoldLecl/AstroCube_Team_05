using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.UI;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(PlayModePositionSaver))]
public class PlayModePositionSaverEditor : Editor
{
    [SerializeField] Font _font;

    public override void OnInspectorGUI()
    {
        GUI.skin.font = _font;


        GUI.backgroundColor = Color.yellow;
        GUI.backgroundColor = new Color(0.8f, 1f, 0.7f);
        if (GUILayout.Button("Save Positions", new GUIStyle(GUI.skin.button)))
        {
            SavePositions();
        }

        GUI.backgroundColor = new Color(1f, 0.9f, 0.5f);
        if (GUILayout.Button("Apply Saved Positions", new GUIStyle(GUI.skin.button)))
        {
            LoadPositions();
        }

        GUI.backgroundColor = new Color(1f, 0.7f, 0.8f);
        if (GUILayout.Button("Clear Positions", new GUIStyle(GUI.skin.button)))
        {
            ClearPosition();
        }

        if(PlayModePositionSaver.PositionsSave != null)
        {
            GUI.backgroundColor = Color.white;
            GUILayout.Label($"Positions saved : {PlayModePositionSaver.PositionsSave.positions.Count}");
        }

        GUI.backgroundColor = new Color(0.6f, 0.8f, 1f);
        GUILayout.BeginHorizontal("GroupBox");

        GUILayout.Label("Save File : ");
        PlayModePositionSaver.PositionsSave = EditorGUILayout.ObjectField(PlayModePositionSaver.PositionsSave, typeof(PositionSaveFile), true) as PositionSaveFile;

        GUILayout.EndHorizontal();
    }

    private void SavePositions()
    {
        EditorUtility.SetDirty(PlayModePositionSaver.PositionsSave);
        PlayModePositionSaver.PositionsSave.objects = new();
        PlayModePositionSaver.PositionsSave.positions = new();
        PlayModePositionSaver.PositionsSave.eulerAngles = new();
        PlayModePositionSaver.PositionsSave.scales = new();


        List<Tile> allTiles = ((PlayModePositionSaver)target).GetComponentsInChildren<Tile>().ToList();
        allTiles.RemoveAt(0);

        for (int tile = 0; tile < allTiles.Count; tile++)
        {
            List<Transform> transforms = allTiles[tile].GetComponentsInChildren<Transform>().ToList();
            transforms.RemoveAt(0);
            foreach (Transform transform in transforms) if (transform != ((PlayModePositionSaver)target).transform)
                {
                    PlayModePositionSaver.PositionsSave.objects.Add(transform.gameObject);
                    PlayModePositionSaver.PositionsSave.positions.Add(transform.localPosition);
                    PlayModePositionSaver.PositionsSave.eulerAngles.Add(transform.localEulerAngles);
                    PlayModePositionSaver.PositionsSave.scales.Add(transform.localScale);
                }
        }

        AssetDatabase.SaveAssetIfDirty(PlayModePositionSaver.PositionsSave);
        EditorUtility.ClearDirty(PlayModePositionSaver.PositionsSave);
    }

    private void LoadPositions()
    {
        for (int i = 0; i < PlayModePositionSaver.PositionsSave.objects.Count; i++)
        {
            PlayModePositionSaver.PositionsSave.objects[i].transform.localPosition = PlayModePositionSaver.PositionsSave.positions[i];
            PlayModePositionSaver.PositionsSave.objects[i].transform.localEulerAngles = PlayModePositionSaver.PositionsSave.eulerAngles[i];
            PlayModePositionSaver.PositionsSave.objects[i].transform.localScale = PlayModePositionSaver.PositionsSave.scales[i];
        }
    }

    private void ClearPosition()
    {
        EditorUtility.SetDirty(PlayModePositionSaver.PositionsSave);

        PlayModePositionSaver.PositionsSave.objects = new();
        PlayModePositionSaver.PositionsSave.positions = new();
        PlayModePositionSaver.PositionsSave.eulerAngles = new();
        PlayModePositionSaver.PositionsSave.scales = new();

        AssetDatabase.SaveAssetIfDirty(PlayModePositionSaver.PositionsSave);
        EditorUtility.ClearDirty(PlayModePositionSaver.PositionsSave);
    }
}