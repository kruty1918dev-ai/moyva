#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiAudit
{
    public sealed class GameplayUiAuditWindow : EditorWindow
    {
        private string _lastOutput = string.Empty;
        private Vector2 _scroll;

        [MenuItem("Moyva/Tools/Unity CLI/Gameplay UI Audit", priority = 21)]
        public static void Open()
        {
            var window = GetWindow<GameplayUiAuditWindow>("Gameplay UI Audit");
            window.minSize = new Vector2(700f, 480f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Moyva · Gameplay UI Audit", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Target", GameplayUiAuditService.TargetScenePath);
            EditorGUILayout.HelpBox(
                "This tool is read-only except for safely opening the canonical Gameplay scene when no loaded scene is dirty. It writes reports outside the project. It never saves or modifies scene content.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Status"))
                    _lastOutput = GameplayUiAuditService.Status();

                if (GUILayout.Button("Prepare Gameplay"))
                    _lastOutput = GameplayUiAuditService.PrepareTargetScene();

                if (GUILayout.Button("Open docs"))
                {
                    string project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
                    string path = Path.Combine(project, "docs/tools/unity-cli/GAMEPLAY_UI_AUDIT.md");
                    if (File.Exists(path))
                        EditorUtility.OpenWithDefaultApp(path);
                }
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Terminal", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(
                "tools/unity-cli/moyva-gameplay-ui-audit",
                EditorStyles.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Last result", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(
                string.IsNullOrWhiteSpace(_lastOutput) ? "No local action yet." : _lastOutput,
                GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }
}

#endif
