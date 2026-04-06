using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(ObjectAutoTileNode))]
    public sealed class ObjectAutoTileNodeEditor : UnityEditor.Editor
    {
        private bool _rulesFoldout;
        private UnityEditor.Editor _rulesEditor;
        private bool _validationFoldout;
        private readonly List<string> _missingIds = new();
        private bool _validated;
        private Vector2 _scrollPos;

        private void OnDisable()
        {
            if (_rulesEditor != null)
                DestroyImmediate(_rulesEditor);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawNodeHeader();
            DrawNodeProperties();
            EditorGUILayout.Space(8);
            DrawValidation();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawNodeHeader()
        {
            var node = (NodeBase)target;
            var info = (NodeInfoAttribute)System.Attribute.GetCustomAttribute(
                node.GetType(), typeof(NodeInfoAttribute));
            if (info != null && !string.IsNullOrWhiteSpace(info.Description))
            {
                EditorGUILayout.HelpBox(info.Description, MessageType.Info);
                EditorGUILayout.Space(4);
            }
        }

        private void DrawNodeProperties()
        {
            var prop = serializedObject.GetIterator();
            prop.NextVisible(true);
            while (prop.NextVisible(false))
            {
                if (prop.name is "_nodeId" or "_editorPosition") continue;
                EditorGUILayout.PropertyField(prop, true);

                if (prop.propertyType == SerializedPropertyType.ObjectReference
                    && prop.objectReferenceValue is ScriptableObject so)
                {
                    DrawNestedEditor(so);
                }
            }
        }

        private void DrawNestedEditor(ScriptableObject so)
        {
            _rulesFoldout = EditorGUILayout.Foldout(_rulesFoldout,
                $"    \u25B6 Rules: {so.name}", true);
            if (!_rulesFoldout) return;

            if (_rulesEditor == null || _rulesEditor.target != so)
            {
                if (_rulesEditor != null) DestroyImmediate(_rulesEditor);
                _rulesEditor = CreateEditor(so);
            }

            EditorGUI.indentLevel++;
            _rulesEditor.OnInspectorGUI();
            EditorGUI.indentLevel--;
        }

        private void DrawValidation()
        {
            _validationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
                _validationFoldout, "Валідація реєстру об'єктів");

            if (_validationFoldout)
            {
                EditorGUILayout.Space(2);

                var rulesProp = serializedObject.FindProperty("_rules");
                var rules = rulesProp?.objectReferenceValue as ObjectConnectionRulesSO;

                if (rules == null)
                {
                    EditorGUILayout.HelpBox(
                        "Rules не призначено — валідацію неможливо виконати.",
                        MessageType.Warning);
                }
                else
                {
                    if (GUILayout.Button("Перевірити реєстр", GUILayout.Height(24)))
                        Validate(rules);

                    if (_validated)
                        DrawResults();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void Validate(ObjectConnectionRulesSO rules)
        {
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

            Debug.Log($"[ObjectAutoTile] Створено {_missingIds.Count} записів у MapObjectRegistrySO.");
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
