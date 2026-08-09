using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Editor.UnityCliBridge
{
    public sealed class MoyvaUnityCliBridgeWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _lastOutput = string.Empty;

        [MenuItem("Moyva/Tools/Unity CLI Bridge", priority = 18)]
        public static void Open()
        {
            var window = GetWindow<MoyvaUnityCliBridgeWindow>("Unity CLI Bridge");
            window.minSize = new Vector2(720f, 520f);
            window.Show();
        }

        private void OnGUI()
        {
            Scene scene = SceneManager.GetActiveScene();

            EditorGUILayout.LabelField("Moyva · Unity CLI / Pipeline Bridge", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Project", Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath);
                EditorGUILayout.LabelField("Unity", Application.unityVersion);
                EditorGUILayout.LabelField("Scene", scene.IsValid() ? $"{scene.name}  ({scene.path})" : "<invalid>");
                EditorGUILayout.LabelField("Playing", EditorApplication.isPlaying.ToString());
                EditorGUILayout.LabelField("Compiling", EditorApplication.isCompiling.ToString());
                EditorGUILayout.LabelField("Bridge", "Registered through com.unity.pipeline [CliCommand]");
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Read-only inspection", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Project status"))
                    _lastOutput = MoyvaUnityCliQuery.ProjectStatusJson();

                if (GUILayout.Button("Scene tree"))
                    _lastOutput = MoyvaUnityCliQuery.SceneTreeJson();

                if (GUILayout.Button("UI audit"))
                    _lastOutput = MoyvaUnityCliQuery.UiAuditJson();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Terminal quick start", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy: list commands"))
                    Copy("cd \"" + ProjectRoot + "\" && unity command --project-path=\"" + ProjectRoot + "\"");

                if (GUILayout.Button("Copy: UI audit"))
                    Copy("cd \"" + ProjectRoot + "\" && unity command moyva-ui-audit --project-path=\"" + ProjectRoot + "\" --format json");

                if (GUILayout.Button("Copy: MCP help"))
                    Copy("unity mcp --help");
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open CLI documentation"))
                    OpenProjectFile("docs/tools/unity-cli/README.md");

                if (GUILayout.Button("Open UI workflow"))
                    OpenProjectFile("docs/tools/unity-cli/UI_AUTOMATION.md");

                if (GUILayout.Button("Open agent protocol"))
                    OpenProjectFile("docs/tools/unity-cli/AI_AGENT_PROTOCOL.md");
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Mutation commands deliberately leave the scene dirty. Run moyva-ui-save-scene only after inspection; it creates an external backup before saving.",
                MessageType.Info);

            EditorGUILayout.LabelField("Last local audit result", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(
                string.IsNullOrWhiteSpace(_lastOutput)
                    ? "Use Project status / Scene tree / UI audit."
                    : _lastOutput,
                GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private static string ProjectRoot =>
            Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;

        private static void Copy(string value)
        {
            EditorGUIUtility.systemCopyBuffer = value;
            Debug.Log("[MoyvaUnityCliBridge] Copied terminal command: " + value);
        }

        private static void OpenProjectFile(string relativePath)
        {
            string absolute = Path.Combine(
                ProjectRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(absolute))
            {
                Debug.LogWarning("[MoyvaUnityCliBridge] File not found: " + absolute);
                return;
            }

            EditorUtility.OpenWithDefaultApp(absolute);
        }
    }
}
