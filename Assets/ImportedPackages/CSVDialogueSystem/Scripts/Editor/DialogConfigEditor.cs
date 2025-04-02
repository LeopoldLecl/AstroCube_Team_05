using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static DialogConfig;

[CustomEditor(typeof(DialogConfig))]
[CanEditMultipleObjects]
public class DialogConfigEditor : Editor
{
    private DialogConfig Source => target as DialogConfig;
    private GUIStyle _titleStyle;

    #region INSPECTOR
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        InitStyle();
        DrawSpeakersDatabasePanel();

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(Source.speakerDatabases.Count == 0 || Source.speakerDatabases.Exists(x => x == null));
        DrawSpeakersPanel();

        DrawSentencePanel();

        EditorGUI.EndDisabledGroup();

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(Source);
    }

    private void DrawSpeakersDatabasePanel()
    {
        EditorGUILayout.BeginVertical("box");

        DrawHeader();
        DrawBody();
        DrawFooter();

        EditorGUILayout.EndVertical();

        void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Speakers Database", _titleStyle);
            if (GUILayout.Button(new GUIContent("X", "Clear all Database"), GUILayout.Width(30)))
            {
                if (EditorUtility.DisplayDialog("Delete all database", "Do you want delete all speakers database?", "Yes", "No"))
                    Source.speakerDatabases.Clear();
            }

            EditorGUILayout.EndHorizontal();
        }
        void DrawBody()
        {
            if (Source.speakerDatabases.Count != 0)
            {
                for (int i = 0; i < Source.speakerDatabases.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Source.speakerDatabases[i] = EditorGUILayout.ObjectField(Source.speakerDatabases[i], typeof(SpeakerDatabase), false) as SpeakerDatabase;

                    if (GUILayout.Button(new GUIContent("X", "Remove database"), GUILayout.Width(30)))
                    {
                        Source.speakerDatabases.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        void DrawFooter()
        {
            if (GUILayout.Button(new GUIContent("Add new database", "")))
            {
                Source.speakerDatabases.Add(null);
            }
        }
    }

    private void DrawSpeakersPanel()
    {
        EditorGUILayout.BeginVertical("box");

        DrawHeader();
        DrawBody();
        DrawFooter();

        EditorGUILayout.EndVertical();

        void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Speakers", _titleStyle);
            if (GUILayout.Button(new GUIContent("X", "Clear all speakers"), GUILayout.Width(30)))
            {
                if (EditorUtility.DisplayDialog("Delete all speakers", "Do you want delete all speakers ?", "Yes", "No"))
                    Source.speakers.Clear();
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawBody()
        {
            if (Source.speakers.Count != 0)
            {
                for (int i = 0; i < Source.speakers.Count; i++)
                {
                    SpeakerConfig config = Source.speakers[i];

                    EditorGUILayout.BeginHorizontal();

                    if (Source.speakerDatabases.Count != 0)
                    {
                        if (Source.speakerDatabases.Count > 1)
                        {
                            List<string> alldatabaseLabel = new();
                            foreach (SpeakerDatabase sd in Source.speakerDatabases)
                                alldatabaseLabel.Add(sd?.name);

                            int idDatabate = Source.speakerDatabases.FindIndex(x => x == config.speakerDatabase);

                            idDatabate = EditorGUILayout.Popup(idDatabate < 0 ? 0 : idDatabate, alldatabaseLabel.ToArray());

                            config.speakerDatabase = Source.speakerDatabases[idDatabate];
                        }
                        else
                        {
                            config.speakerDatabase = Source.speakerDatabases.First();
                        }
                    }

                    if (config.speakerDatabase != null)
                    {
                        List<string> alldataLabel = new();
                        foreach (SpeakerData sd in config.speakerDatabase.speakerDatas)
                            alldataLabel.Add(sd?.label);

                        int idData = config.speakerDatabase.speakerDatas.FindIndex(x => x.id == config.speakerData.id);

                        idData = EditorGUILayout.Popup(idData < 0 ? 0 : idData, alldataLabel.ToArray());

                        config.speakerData = config.speakerDatabase.speakerDatas[idData];
                    }

                    config.position = (SpeakerConfig.POSITION)EditorGUILayout.EnumPopup(config.position);

                    if (GUILayout.Button(new GUIContent("X", "Remove speeker"), GUILayout.Width(30)))
                    {
                        Source.speakers.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                    Source.speakers[i] = config;
                }
            }
        }
        void DrawFooter()
        {
            if (GUILayout.Button(new GUIContent("Add new speaker", "")))
            {
                Source.speakers.Add(new DialogConfig.SpeakerConfig());
            }
        }
    }

    private void DrawSentencePanel()
    {
        EditorGUILayout.BeginVertical("box");

        DrawHeader();
        DrawBody();
        DrawFooter();

        EditorGUILayout.EndVertical();

        void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Sentence", _titleStyle);
            //if (GUILayout.Button(new GUIContent("X", "Clear all sentences"), GUILayout.Width(30)))
            //{
            //    if (EditorUtility.DisplayDialog("Delete all sentences", "Do you want delete all sentences ?", "Yes", "No"))
            //        _source.speakers.Clear();
            //}

            EditorGUILayout.EndHorizontal();
        }

        void DrawBody()
        {
            EditorGUILayout.BeginHorizontal();

            Source.csvDialog = (TextAsset)EditorGUILayout.ObjectField("File : ", Source.csvDialog, typeof(TextAsset), false);

            if(Source.csvDialog != null && !Source.table.IsLoaded())
                Source.table.Load(Source.csvDialog);

            EditorGUILayout.EndHorizontal();

            if (Source.table.GetRowList().Count == 0)
                return;

            List<string> allRowSentence = new();

            foreach (var r in Source.table.GetRowList())
                allRowSentence.Add(r.KEY);

            var allSpeaker = Source.speakers.Select(x => x.speakerData).ToList();

            for (int i = 0; i < Source.sentenceConfig.Count; i++)
            {
                var sentenceConfig = Source.sentenceConfig[i];

                int idRowSentence = Source.table.GetRowList().FindIndex(x => x.KEY == sentenceConfig.key);
                int idSpeaker = Source.speakers.FindIndex(x => x.speakerData.id == sentenceConfig.speakerData.id);

                EditorGUILayout.BeginHorizontal();

                idSpeaker = EditorGUILayout.Popup(idSpeaker < 0 ? 0 : idSpeaker, allSpeaker.Select(x => x.label).ToArray());
                sentenceConfig.speakerData = allSpeaker[idSpeaker];

                idRowSentence = EditorGUILayout.Popup(idRowSentence < 0 ? 0 : idRowSentence, allRowSentence.ToArray());
                sentenceConfig.key = allRowSentence[idRowSentence];

                Source.sentenceConfig[i] = sentenceConfig;

                if (GUILayout.Button(new GUIContent("X", "Delete sentence"), GUILayout.Width(20)))
                    Source.sentenceConfig.RemoveAt(i);

                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawFooter()
        {
            if (GUILayout.Button(new GUIContent("Add new sentence", "")))
                Source.sentenceConfig.Add(new(string.Empty, null));
        }
    }
    #endregion

    #region STYLE
    private void InitStyle()
    {
        _titleStyle = GUI.skin.label;
        _titleStyle.alignment = TextAnchor.MiddleCenter;
        _titleStyle.fontStyle = FontStyle.Bold;
    }
    #endregion
}
