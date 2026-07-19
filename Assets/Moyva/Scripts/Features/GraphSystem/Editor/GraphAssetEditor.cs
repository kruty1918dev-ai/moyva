using Kruty1918.Moyva.GraphSystem.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    [CustomEditor(typeof(GraphAsset))]
    public sealed class GraphAssetEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            var graphAsset = (GraphAsset)target;

            EditorGUILayout.LabelField("Graph Asset", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Nodes", graphAsset.Nodes.Count.ToString());
            EditorGUILayout.LabelField("Connections",
                graphAsset.Connections.Count.ToString());
            EditorGUILayout.LabelField("Version",
                graphAsset.Version.ToString());

            if (GUILayout.Button("Open Graph Editor", GUILayout.Height(32)))
            {
                GraphEditorWindow.Open(graphAsset);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Validate"))
            {
                var validator = new Runtime.GraphValidator();
                var errors = validator.Validate(graphAsset);

                if (errors.Count == 0)
                {
                    EditorUtility.DisplayDialog("Validation",
                        "No errors found.", "OK");
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (var error in errors)
                        sb.AppendLine(error.ToString());

                    EditorUtility.DisplayDialog("Validation",
                        $"{errors.Count} issue(s) found:\n\n{sb}",
                        "OK");
                }
            }

            if (GUILayout.Button("Dry Run Subasset Audit"))
            {
                var report = GraphSubassetAuditUtility.Audit(
                    graphAsset,
                    writeReport: true);
                EditorUtility.DisplayDialog(
                    "Graph subasset audit",
                    $"Reachable: {report.Reachable.Count}\n" +
                    $"Proven orphan candidates: {report.Orphaned.Count}\n" +
                    $"Cleanup safe: {report.CleanupSafe}\n\n" +
                    $"Звіт: {report.ReportPath}",
                    "OK");
            }

            EditorGUILayout.Space();
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Node List", EditorStyles.boldLabel);

            for (int i = 0; i < graphAsset.Nodes.Count; i++)
            {
                var node = graphAsset.Nodes[i];
                if (node == null)
                {
                    EditorGUILayout.LabelField($"  [{i}] (null — missing script?)");
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  [{i}] {node.Title}",
                    EditorStyles.miniLabel);

                if (GUILayout.Button("Select", GUILayout.Width(50)))
                    Selection.activeObject = node;

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
