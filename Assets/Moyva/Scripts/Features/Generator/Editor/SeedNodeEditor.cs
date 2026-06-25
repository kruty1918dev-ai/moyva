using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(SeedNode))]
    public sealed class SeedNodeEditor : UnityEditor.Editor
    {
        private SerializedProperty _seedProperty;
        private string _headerText;
        private string _portsText;
        private string _seedHelp;

        private void OnEnable()
        {
            _seedProperty = serializedObject.FindProperty("seed");
            var node = target as SeedNode;
            _headerText = node != null ? GraphNodeDocumentation.BuildInspectorHeader(node) : null;
            _portsText = node != null ? GraphNodeDocumentation.BuildPortsInspectorText(node) : null;
            _seedHelp = GraphNodeDocumentation.GetParameterDescription(typeof(SeedNode), "seed", "Seed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as SeedNode;
            if (node != null)
            {
                EditorGUILayout.HelpBox(_headerText, MessageType.Info);
                EditorGUILayout.HelpBox(_portsText, MessageType.None);
            }

            EditorGUILayout.PropertyField(_seedProperty, new GUIContent("Seed", _seedHelp));
            EditorGUILayout.HelpBox(_seedHelp, MessageType.None);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
