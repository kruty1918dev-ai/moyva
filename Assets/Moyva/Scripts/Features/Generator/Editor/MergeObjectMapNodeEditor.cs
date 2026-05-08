using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(MergeObjectMapNode))]
    public sealed class MergeObjectMapNodeEditor : UnityEditor.Editor
    {
        private Texture2D _previewTexture;

        private void OnDisable()
        {
            if (_previewTexture != null)
                DestroyImmediate(_previewTexture);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            EditorGUILayout.Space(8f);
            DrawStats();
            DrawLegend();
            DrawPreview();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStats()
        {
            var node = (MergeObjectMapNode)target;

            EditorGUILayout.LabelField("Merge Stats", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Input A objects", node.LastAObjects.ToString());
            EditorGUILayout.LabelField("Input B objects", node.LastBObjects.ToString());
            EditorGUILayout.LabelField("Result objects", node.LastResultObjects.ToString());
            EditorGUILayout.LabelField("Conflicts", node.LastConflicts.ToString());

            if (node.LastConflicts > 0)
                EditorGUILayout.HelpBox("Конфлікт — це клітинка, де A і B мають різні непорожні object ID. Перемагає вхід, обраний у Conflict Priority.", MessageType.Info);
        }

        private void DrawLegend()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Preview Legend", EditorStyles.boldLabel);
            DrawLegendRow(new Color(0.14f, 0.42f, 0.95f, 1f), "Only A");
            DrawLegendRow(new Color(0.95f, 0.62f, 0.16f, 1f), "Only B");
            DrawLegendRow(new Color(0.10f, 0.55f, 1.00f, 1f), "Conflict, A wins");
            DrawLegendRow(new Color(1.00f, 0.62f, 0.12f, 1f), "Conflict, B wins");
            DrawLegendRow(new Color(0.08f, 0.09f, 0.12f, 1f), "Empty");
        }

        private static void DrawLegendRow(Color color, string label)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Rect swatch = GUILayoutUtility.GetRect(18f, 18f, GUILayout.Width(18f), GUILayout.Height(18f));
                EditorGUI.DrawRect(swatch, color);
                EditorGUILayout.LabelField(label);
            }
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            if (GUILayout.Button("Refresh Preview"))
                RefreshPreview();

            if (_previewTexture == null)
                RefreshPreview();

            if (_previewTexture == null)
            {
                EditorGUILayout.HelpBox("Preview з'явиться після запуску ноди.", MessageType.None);
                return;
            }

            Rect rect = GUILayoutUtility.GetRect(192f, 192f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, _previewTexture, null, UnityEngine.ScaleMode.ScaleToFit);
        }

        private void RefreshPreview()
        {
            if (_previewTexture != null)
                DestroyImmediate(_previewTexture);

            var node = (MergeObjectMapNode)target;
            _previewTexture = node.GeneratePreview(192, 192);
        }
    }
}
