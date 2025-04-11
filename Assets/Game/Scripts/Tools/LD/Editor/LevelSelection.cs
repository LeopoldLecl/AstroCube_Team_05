
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UIElements;
using static UnityEditor.Rendering.FilterWindow;
using static UnityEditor.Rendering.InspectorCurveEditor;

[CustomEditor(typeof(RubicsCube))]
public class LevelSelection : Editor
{
    // - EDITOR -
    [SerializeField] Font _font;
    static GUIStyle _buttonStyle;

    // - SELECTION
    static bool _isObjectSelected;
    static bool _isInIsolatedMode;

    // - FOLDOUT -
    [SerializeField] static SerializedProperty _tilesPerFaces;    
    [SerializeField] static SerializedProperty _foldoutTransforms;
    static private Pose[] _tilesOriginalTransforms = new Pose[6 * 9];
    static float _tileInterval;
    static bool _isFoldout = false;
    static bool _areWallsInvisible = false;
    static bool _showFoldoutReferences;

    // - OTHER -    
    static RubicsCube _script;
    static SerializedObject _serializedObj;

    private void OnEnable()
    {
        _isInIsolatedMode = false;
        _isObjectSelected = false;
        _script = (RubicsCube)target;
        _tileInterval = _script.transform.localScale.x;
        RubicsCube.OnReset += ResetFold;
        EditorApplication.playModeStateChanged += LaunchPlaymode;
        EditorApplication.wantsToQuit += WantsToQuit;
        EditorSceneManager.sceneSaving += SceneSaving;
    }

#region Preventions

    private static void LaunchPlaymode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode && _isFoldout)
        {
            EditorApplication.isPlaying = false;
            ResetFold();
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("WARNING", "Fold the Rubic's cube before lauching Play Mode", "OK...", "No !!");
        }
    }

    static bool WantsToQuit()
    {
        if (_isFoldout)
        {
            ResetFold();
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("WARNING", "Fold the Rubic's cube before quitting !", "OK...", "No !!");
            return false;
        }
        return true;
    }

    static void SceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
    {
        if (_isFoldout)
        {
            ResetFold();
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("WARNING", "Fold the Rubic's cube before saving scene !", "OK...", "No !!");
        }
    }

#endregion

    private void OnSceneGUI()
    {
        if (RubicsCube.Selection[0] != null)
        {
            Handles.DrawOutline(RubicsCube.Selection, Color.yellow, 1);
            Handles.PositionHandle(RubicsCube.Selection[0].transform.position, Quaternion.identity);
        }

        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(0);
        }

        if (Event.current.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hit;
            LayerMask lm = GetLayerMaskFromMode(RubicsCube.IsolationMode);
            if (Physics.Raycast(ray, out hit, 500, lm))
            {
                if(_isObjectSelected && RubicsCube.Selection[0] != hit.collider.gameObject || _isInIsolatedMode)
                {
                    _script.DeIsolate(RubicsCube.IsolationMode, RubicsCube.Selection[0].transform, _script.transform);
                    _isInIsolatedMode = false;
                }
                RubicsCube.Selection[0] = hit.collider.gameObject;
                _isObjectSelected = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {     
        GUI.skin.font = _font;
        serializedObject.Update();
        _serializedObj = serializedObject;

        EditorGUILayout.Space(10);

        InitStyles();

        ActionsDisplay();

        FoldoutDisplay();

        if (GUI.changed)
            AssetDatabase.SaveAssetIfDirty(target);

        _serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    static void InitStyles()
    {
        _buttonStyle = GUI.skin.button;
        _buttonStyle.fontSize = 13;
        _buttonStyle.fontStyle = FontStyle.Normal;
        _buttonStyle.normal.textColor = Color.black;
        _buttonStyle.hover.textColor = Color.white;
    }


#region SELECTION

    static void ActionsDisplay()
    {
        InitStyles();
        GUILayout.Label("Level Editor ", new GUIStyle(GUI.skin.label) { fontSize = 15, normal = new GUIStyleState() { textColor = Color.white } });
        GUILayout.Space(10);


        GUILayout.BeginVertical("GroupBox");


        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Isolation mode : ", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Normal, fontSize = 13, normal = new GUIStyleState() { textColor = new Color(0.9f, 0.8f, 0.6f) } });


        RubicsCube.IsolationMode = (EIsolationMode)EditorGUILayout.EnumPopup(RubicsCube.IsolationMode);

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginVertical("GroupBox");

        string labelText = RubicsCube.Selection[0] != null ? "Selected Object : " + RubicsCube.Selection[0].name : "Selected Object : No Object Selected";
        GUILayout.Label(labelText, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Normal, fontSize = 13, normal = new GUIStyleState() { textColor = new Color(0.6f, 0.7f, 1f) } });

        GUILayout.Space(10);

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUI.backgroundColor = !_isInIsolatedMode ? Color.white : Color.grey;

        if (GUILayout.Button("Isolate", _buttonStyle, GUILayout.Height(25)))
        {
            if (RubicsCube.Selection[0] != null)
            {
                _isInIsolatedMode = !_isInIsolatedMode;

                if (_isInIsolatedMode)
                    _script.Isolate(RubicsCube.IsolationMode, RubicsCube.Selection[0].transform, _script.transform);
                else
                    _script.DeIsolate(RubicsCube.IsolationMode, RubicsCube.Selection[0].transform, _script.transform);
            }

        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUI.backgroundColor = new Color(1f, 0.9f, 0.8f);
        if (GUILayout.Button("Go to selected object", _buttonStyle, GUILayout.Height(25)))
        {
            if(RubicsCube.Selection[0] != null)
                UnityEditor.Selection.activeGameObject = RubicsCube.Selection[0];
        }

        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button("Reset Editor", _buttonStyle, GUILayout.Height(25)))
        {
            _script.Reset();
            _isInIsolatedMode = false;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndVertical();


        GUILayout.BeginVertical("GroupBox");

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUI.backgroundColor = _isFoldout ? new Color(0.4f, 0.6f, 0.3f) : new Color(0.8f, 1f, 0.7f);
        if (GUILayout.Button("Foldout Rubic's Cube", _buttonStyle, GUILayout.Height(25)))
        {
            _isFoldout = !_isFoldout;
            
            if(_isFoldout)
                Foldout();
            else
                Foldin();

        }
        GUI.backgroundColor = Color.white;

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();



        GUILayout.BeginVertical("GroupBox");

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUI.backgroundColor = _areWallsInvisible ? new Color(0.4f, 0.6f, 0.3f) : new Color(0.8f, 1f, 0.7f);
        if (GUILayout.Button("Make Walls invisible", _buttonStyle, GUILayout.Height(25)))
        {
            _areWallsInvisible = !_areWallsInvisible;

            if (_areWallsInvisible)
                _script.ChangeWallsVisibility(_script.transform, false);
            else
                _script.ChangeWallsVisibility(_script.transform, true);
        }
        GUI.backgroundColor = Color.white;

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();



        GUILayout.EndVertical();
    }

    LayerMask GetLayerMaskFromMode(EIsolationMode mode)
    {
        switch (mode)
        {
            default:
            case EIsolationMode.CUBE:
            {
                LayerMask mask = LayerMask.GetMask("Cube");
                return mask;
            }
            case EIsolationMode.TILE:
            {
                LayerMask mask = LayerMask.GetMask("Tile");
                return mask;
            }
        }
    }

#endregion

#region FOLDOUT

    static void FoldoutDisplay()
    {
        _showFoldoutReferences = EditorGUILayout.BeginFoldoutHeaderGroup(_showFoldoutReferences, "Show Foldout References", new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Normal });

        if (_showFoldoutReferences)
        {
            _tilesPerFaces = _serializedObj.FindProperty("_tilesPerFaces");
            _foldoutTransforms = _serializedObj.FindProperty("_foldoutTransforms");

            for (int face = 0; face < 6; face++)
            {
                GUILayout.BeginVertical("GroupBox");
                GUILayout.Label($"Face {face + 1}", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Normal, fontSize = 13, normal = new GUIStyleState() { textColor = new Color(0.7f, 0.9f, 0.6f) } });


                for (int tile = 0; tile < 9; tile++)
                {
                    int index = face * 9 + tile;

                    SerializedProperty element = _tilesPerFaces.GetArrayElementAtIndex(index);
                    EditorGUILayout.PropertyField(element, new GUIContent($"Face {face + 1} - Tile {tile + 1} :"), true);
                }
                EditorGUILayout.Space(5);

                SerializedProperty transform = _foldoutTransforms.GetArrayElementAtIndex(face);

                GUILayout.BeginHorizontal("HelpBox");
                GUILayout.Label("Foldout Transform :", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Normal, fontSize = 12, normal = new GUIStyleState() { textColor = new Color(0.9f, 0.6f, 0.7f) } });
                EditorGUILayout.PropertyField(transform, new GUIContent(""), true);

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
        }
    }

    static void Foldout()
    {
        _tilesPerFaces = _serializedObj.FindProperty("_tilesPerFaces");
        _foldoutTransforms = _serializedObj.FindProperty("_foldoutTransforms");

        for (int face = 0; face < 6; face++)
        {
            SerializedProperty transform = _foldoutTransforms.GetArrayElementAtIndex(face);
            Transform transformFoldout = (Transform)transform.objectReferenceValue;

            for (int tile = 0; tile < 9; tile++)
            {
                int index = face * 9 + tile;

                SerializedProperty element = _tilesPerFaces.GetArrayElementAtIndex(index);
                GameObject tileGameObject = ((GameObject)element.objectReferenceValue);

                _tilesOriginalTransforms[index].position = tileGameObject.transform.position;
                _tilesOriginalTransforms[index].rotation = tileGameObject.transform.rotation;

                tileGameObject.transform.position = new Vector3(transformFoldout.position.x + ((tile % 3.0f)*_tileInterval), 
                                                               transformFoldout.position.y, 
                                                               transformFoldout.position.z - (Mathf.Floor(tile / 3.0f) * _tileInterval));
                tileGameObject.transform.rotation = transformFoldout.rotation;
            }          
        }
    }

    static void Foldin()
    {
        _tilesPerFaces = _serializedObj.FindProperty("_tilesPerFaces");

        for (int face = 0; face < 6; face++)
        {
            for (int tile = 0; tile < 9; tile++)
            {
                int index = face * 9 + tile;

                SerializedProperty element = _tilesPerFaces.GetArrayElementAtIndex(index);
                GameObject tileGameObject = ((GameObject)element.objectReferenceValue);
                tileGameObject.transform.position = _tilesOriginalTransforms[index].position;
                tileGameObject.transform.rotation = _tilesOriginalTransforms[index].rotation;                
            }
        }
    }

    static void ResetFold()
    {
        if (_isFoldout)
            Foldin();
        _isFoldout = false;
    }

#endregion

}
