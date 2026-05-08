using Kruty1918.Moyva.Generator.Runtime.Nodes;
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

            EditorGUILayout.HelpBox("Єдине зерно генерації для всього графа. Інші ноди не мають власних seed-полів.", MessageType.Info);
            EditorGUILayout.PropertyField(_seedProperty, new GUIContent("Seed"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}