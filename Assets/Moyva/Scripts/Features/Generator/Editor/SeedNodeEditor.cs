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

        private void OnEnable()
        {
            _seedProperty = serializedObject.FindProperty("seed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as SeedNode;
            if (node != null)
            {
                EditorGUILayout.HelpBox(GraphNodeDocumentation.BuildInspectorHeader(node), MessageType.Info);
                EditorGUILayout.HelpBox(GraphNodeDocumentation.BuildPortsInspectorText(node), MessageType.None);
            }

            string help = GraphNodeDocumentation.GetParameterDescription(typeof(SeedNode), "seed", "Seed");
            EditorGUILayout.PropertyField(_seedProperty, new GUIContent("Seed", help));
            EditorGUILayout.HelpBox(help, MessageType.None);

            serializedObject.ApplyModifiedProperties();
        }
    }
}