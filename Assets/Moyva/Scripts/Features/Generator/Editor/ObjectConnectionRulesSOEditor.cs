using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(ObjectConnectionRulesSO))]
    public sealed class ObjectConnectionRulesSOEditor : UnityEditor.Editor
    {
        private bool _validationFoldout;
        private readonly List<string> _missingIds = new();
        private bool _validated;
        private Vector2 _scrollPos;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            DrawValidation();
        }

        private void DrawValidation()
        {
            _validationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
                _validationFoldout, "Валідація реєстру об'єктів");

            if (_validationFoldout)
            {
                EditorGUILayout.Space(2);

                if (GUILayout.Button("Перевірити реєстр", GUILayout.Height(24)))
                    Validate();

                if (_validated)
                    DrawResults();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void Validate()
        {
            var rules = (ObjectConnectionRulesSO)target;
            _missingIds.Clear();

            var registry = FindAsset<MapObjectRegistrySO>();
            if (registry == null)
            {
                EditorUtility.DisplayDialog("Помилка",
                    "MapObjectRegistrySO не знайдено в проєкті.", "OK");
                _validated = false;
                return;
            }

            var registered = new HashSet<string>();
            if (registry.Definitions != null)
            {
                foreach (var def in registry.Definitions)
                {
                    if (!string.IsNullOrEmpty(def.Id))
                        registered.Add(def.Id);
                }
            }

            var needed = new HashSet<string>();
            foreach (var group in rules.Groups)
            {
                if (!string.IsNullOrEmpty(group.BaseObjectId))
                    needed.Add(group.BaseObjectId);
                if (!string.IsNullOrEmpty(group.FallbackId))
                    needed.Add(group.FallbackId);
                foreach (var v in group.Variants)
                {
                    if (!string.IsNullOrEmpty(v.VariantId))
                        needed.Add(v.VariantId);
                }
            }

            foreach (var id in needed)
            {
                if (!registered.Contains(id))
                    _missingIds.Add(id);
            }

            _missingIds.Sort();
            _validated = true;
        }

        private void DrawResults()
        {
            EditorGUILayout.Space(4);

            if (_missingIds.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Усі ID з правил зареєстровано в MapObjectRegistrySO.",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(
                $"Відсутніх ID у реєстрі: {_missingIds.Count}",
                MessageType.Warning);

            _scrollPos = EditorGUILayout.BeginScrollView(
                _scrollPos, GUILayout.MaxHeight(200));
            foreach (var id in _missingIds)
                EditorGUILayout.LabelField("  \u2022 " + id);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);

            if (GUILayout.Button(
                $"Створити усі відсутні ({_missingIds.Count})",
                GUILayout.Height(28)))
            {
                CreateMissingObjects();
            }
        }

        private void CreateMissingObjects()
        {
            var registry = FindAsset<MapObjectRegistrySO>();
            if (registry == null) return;

            var so = new SerializedObject(registry);
            var arr = so.FindProperty("_definitions");

            foreach (var id in _missingIds)
            {
                arr.arraySize++;
                var elem = arr.GetArrayElementAtIndex(arr.arraySize - 1);
                elem.FindPropertyRelative("_id").stringValue = id;
                elem.FindPropertyRelative("_visualPrefab").objectReferenceValue = null;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();

            Debug.Log($"[ObjectConnectionRules] Створено {_missingIds.Count} записів у MapObjectRegistrySO.");
            _missingIds.Clear();
            _validated = false;
        }

        private static T FindAsset<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<T>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
