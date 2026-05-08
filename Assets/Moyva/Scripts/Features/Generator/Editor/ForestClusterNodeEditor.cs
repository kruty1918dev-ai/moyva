using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(ForestClusterNode))]
    public sealed class ForestClusterNodeEditor : UnityEditor.Editor
    {
        private SerializedProperty _spawnObjects;

        private int _objectIndex;
        private Texture2D _previewTexture;

        private void OnEnable()
        {
            _spawnObjects = serializedObject.FindProperty("_spawnObjects");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Object Cluster Utilities", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Автоматизація для одного набору об'єктів: рандомізація параметрів, шансів та нормалізація.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Randomize Cluster"))
                    RunAndPersist(node => node.RandomizeClusterSettings());

                if (GUILayout.Button("Randomize All Chances"))
                    RunAndPersist(node => node.RandomizeAllObjectChances());
            }

            if (GUILayout.Button("Normalize All Chances"))
                RunAndPersist(node => node.NormalizeAllObjectChances());

            DrawSingleObjectUtilities();
            DrawChanceOverview();
            DrawMaskNotes();
            DrawPreview();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSingleObjectUtilities()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Single Object Utility", EditorStyles.boldLabel);

            if (_spawnObjects == null || _spawnObjects.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Об'єкти не налаштовані.", MessageType.None);
                return;
            }

            _objectIndex = Mathf.Clamp(_objectIndex, 0, _spawnObjects.arraySize - 1);
            var objectNames = new string[_spawnObjects.arraySize];
            for (int i = 0; i < _spawnObjects.arraySize; i++)
            {
                var objectProp = _spawnObjects.GetArrayElementAtIndex(i);
                var idProp = objectProp.FindPropertyRelative("ObjectId");
                objectNames[i] = string.IsNullOrWhiteSpace(idProp?.stringValue)
                    ? $"Object {i + 1}"
                    : idProp.stringValue;
            }

            _objectIndex = EditorGUILayout.Popup("Object", _objectIndex, objectNames);
            if (GUILayout.Button("Randomize Selected Object Chance"))
            {
                RunAndPersist(node => node.RandomizeSingleObjectChance(_objectIndex));
            }
        }

        private void DrawChanceOverview()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Chance Overview", EditorStyles.boldLabel);

            if (_spawnObjects == null || _spawnObjects.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Немає об'єктів для візуалізації шансів.", MessageType.None);
                return;
            }

            float sum = 0f;
            for (int i = 0; i < _spawnObjects.arraySize; i++)
            {
                var objectProp = _spawnObjects.GetArrayElementAtIndex(i);
                var enabledProp = objectProp.FindPropertyRelative("Enabled");
                var idProp = objectProp.FindPropertyRelative("ObjectId");
                var chanceProp = objectProp.FindPropertyRelative("Chance");
                bool enabled = enabledProp == null || enabledProp.boolValue;
                if (!enabled || string.IsNullOrWhiteSpace(idProp?.stringValue))
                    continue;
                sum += Mathf.Max(0f, chanceProp != null ? chanceProp.floatValue : 0f);
            }

            if (sum <= 0f)
            {
                EditorGUILayout.HelpBox("Сума шансів = 0. Натисни Normalize All Chances, щоб розподілити шанс між валідними об'єктами.", MessageType.Warning);
                return;
            }

            for (int i = 0; i < _spawnObjects.arraySize; i++)
            {
                var objectProp = _spawnObjects.GetArrayElementAtIndex(i);
                var enabledProp = objectProp.FindPropertyRelative("Enabled");
                var idProp = objectProp.FindPropertyRelative("ObjectId");
                var chanceProp = objectProp.FindPropertyRelative("Chance");
                bool enabled = enabledProp == null || enabledProp.boolValue;
                if (!enabled || string.IsNullOrWhiteSpace(idProp?.stringValue))
                    continue;

                float chance = Mathf.Max(0f, chanceProp != null ? chanceProp.floatValue : 0f);
                float normalized = chance / sum;
                Rect rect = GUILayoutUtility.GetRect(18f, 18f, "TextField");
                EditorGUI.ProgressBar(rect, normalized, $"{idProp.stringValue}  ({normalized:P0})");
            }
        }

        private void DrawMaskNotes()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Mask Visualization", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Mask використовується як заборона спавну без будь-яких змін: true = табу, об'єкт у цій клітинці не ставиться.",
                MessageType.None);
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh Preview"))
                    RefreshPreview();
            }

            if (_previewTexture == null)
                RefreshPreview();

            if (_previewTexture == null)
            {
                EditorGUILayout.HelpBox("Preview буде доступний після виконання ноди або з noise-патерну за замовчуванням.", MessageType.None);
                return;
            }

            Rect rect = GUILayoutUtility.GetRect(192f, 192f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, _previewTexture, null, UnityEngine.ScaleMode.ScaleToFit);
        }

        private void RefreshPreview()
        {
            var node = (ForestClusterNode)target;
            _previewTexture = node.GeneratePreview(192, 192);
        }

        private void RunAndPersist(System.Action<ForestClusterNode> action)
        {
            serializedObject.ApplyModifiedProperties();

            var node = (ForestClusterNode)target;
            Undo.RecordObject(node, "Object Cluster Utility");
            action(node);
            EditorUtility.SetDirty(node);

            serializedObject.Update();
            RefreshPreview();
            Repaint();
        }
    }
}
