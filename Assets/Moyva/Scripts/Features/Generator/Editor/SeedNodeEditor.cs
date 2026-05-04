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

            if (GUILayout.Button("Random Seed"))
            {
                Undo.RecordObject(target, "Randomize Seed");
                _seedProperty.intValue = GenerateRandomSeed();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                return;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static int GenerateRandomSeed()
        {
            int value;
            do
            {
                value = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            while (value == 0);

            return value;
        }
    }
}