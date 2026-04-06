using Kruty1918.Moyva.Units.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    [CustomEditor(typeof(UnitRegistrySO))]
    public sealed class UnitRegistryEditor : UnityEditor.Editor
    {
        private const string UnitPrefabFolder = "Assets/Moyva/Prefabs/Units";

        private SerializedProperty _configs;
        private Vector2 _scroll;
        private bool _createOpen;
        private string _newId = "";
        private float _newStamina = 100f;
        private Vector2 _newStaminaRange = new(-5f, 5f);
        private Sprite _newSprite;
        private GameObject _newPrefab;

        private void OnEnable()
        {
            _configs = serializedObject.FindProperty("Configs");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RegistryEditorStyles.DrawColoredHeader("  Unit Registry", RegistryEditorStyles.Accent);
            EditorGUILayout.Space(2);

            int count = _configs?.arraySize ?? 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{count} юніт(ів)", EditorStyles.boldLabel);
            if (GUILayout.Button("Відкрити Registry Hub", GUILayout.Width(160)))
                RegistryHubWindow.Open(2);
            EditorGUILayout.EndHorizontal();

            RegistryEditorStyles.DrawSeparator();

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MaxHeight(500));

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = _configs.GetArrayElementAtIndex(i);
                string typeId  = el.FindPropertyRelative("TypeId")?.stringValue ?? "?";
                float  stamina = el.FindPropertyRelative("BaseStamina")?.floatValue ?? 0f;
                var    pfb     = el.FindPropertyRelative("Prefab")?.objectReferenceValue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);

                // Заголовок
                EditorGUILayout.BeginHorizontal();
                DrawIdLabel(typeId);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"Стаміна: {stamina:F0}", EditorStyles.miniLabel, GUILayout.Width(100));
                EditorGUILayout.LabelField(pfb ? $"\u2713" : "\u2717", EditorStyles.miniLabel, GUILayout.Width(16));
                Color prev = GUI.color;
                GUI.color = RegistryEditorStyles.ErrorCol;
                if (GUILayout.Button("\u00d7", GUILayout.Width(22), GUILayout.Height(18)))
                    removeIdx = i;
                GUI.color = prev;
                EditorGUILayout.EndHorizontal();

                // Inline editing
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(el.FindPropertyRelative("TypeId"), new GUIContent("Type ID"));
                ValidateInlineId(el.FindPropertyRelative("TypeId")?.stringValue);
                EditorGUILayout.PropertyField(el.FindPropertyRelative("BaseStamina"));
                EditorGUILayout.PropertyField(el.FindPropertyRelative("StaminaRandomRange"));
                EditorGUILayout.PropertyField(el.FindPropertyRelative("Prefab"));
                EditorGUILayout.PropertyField(el.FindPropertyRelative("AnimationSettings"), true);
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(1);
            }

            EditorGUILayout.EndScrollView();

            if (removeIdx >= 0)
            {
                string name = _configs.GetArrayElementAtIndex(removeIdx).FindPropertyRelative("TypeId")?.stringValue ?? "?";
                if (EditorUtility.DisplayDialog("Видалити", $"Видалити юніта '{name}'?", "Так", "Ні"))
                    _configs.DeleteArrayElementAtIndex(removeIdx);
            }

            RegistryEditorStyles.DrawSeparator();

            _createOpen = EditorGUILayout.Foldout(_createOpen, "\u2795 Створити нового юніта", true, EditorStyles.foldoutHeader);
            if (_createOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                _newId           = RegistryEditorStyles.IdFieldWithDuplicateCheck("Type ID", _newId, _configs, "TypeId");
                _newStamina      = EditorGUILayout.FloatField("Base Stamina", _newStamina);
                _newStaminaRange = EditorGUILayout.Vector2Field("Stamina Random Range", _newStaminaRange);
                _newSprite       = (Sprite)EditorGUILayout.ObjectField("Sprite", _newSprite, typeof(Sprite), false);
                _newPrefab       = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newPrefab, typeof(GameObject), false);
                if (!_newPrefab && !_newSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);
                EditorGUILayout.Space(4);

                bool valid = RegistryEditorStyles.ValidateIdFull(_newId, _configs, "TypeId") == null;
                EditorGUI.BeginDisabledGroup(!valid);
                if (GUILayout.Button("\u2713 Створити юніта", RegistryEditorStyles.CreateButton))
                    DoCreate();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DoCreate()
        {
            string id = _newId.Trim();
            if (RegistryEditorStyles.ValidateIdFull(id, _configs, "TypeId") != null) return;

            GameObject pfb = _newPrefab ? _newPrefab : CreatePrefab(id, _newSprite);
            if (!pfb) pfb = CreateEmptyPrefab(id);

            int idx = _configs.arraySize;
            _configs.InsertArrayElementAtIndex(idx);
            var el = _configs.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("TypeId").stringValue = id;
            el.FindPropertyRelative("BaseStamina").floatValue = _newStamina;
            el.FindPropertyRelative("StaminaRandomRange").vector2Value = _newStaminaRange;
            el.FindPropertyRelative("Prefab").objectReferenceValue = pfb;

            var anim = el.FindPropertyRelative("AnimationSettings");
            if (anim != null)
            {
                var dur = anim.FindPropertyRelative("MoveDurationPerTile");
                if (dur != null) dur.floatValue = 0.3f;
                var delay = anim.FindPropertyRelative("DelayOnTile");
                if (delay != null) delay.floatValue = 0.05f;
            }

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            _newId = ""; _newSprite = null; _newPrefab = null;
        }

        private bool ContainsId(string id)
        {
            for (int i = 0; i < _configs.arraySize; i++)
            {
                string existing = _configs.GetArrayElementAtIndex(i).FindPropertyRelative("TypeId")?.stringValue;
                if (string.Equals(existing, id, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static GameObject CreatePrefab(string id, Sprite sprite)
        {
            if (!sprite) return null;
            EnsureFolder(UnitPrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{UnitPrefabFolder}/{safe}.prefab");
            var go = new GameObject(safe);
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            var pfb = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return pfb;
        }

        private static void DrawIdLabel(string id)
        {
            string err = RegistryEditorStyles.ValidateId(id);
            Color prev = GUI.color;
            if (err != null) GUI.color = RegistryEditorStyles.ErrorCol;
            EditorGUILayout.LabelField(err != null ? $"\u26a0 {id}" : id, RegistryEditorStyles.EntryTitle);
            GUI.color = prev;
        }

        private static void ValidateInlineId(string id)
        {
            if (id != null && id.Contains('_'))
            {
                Color prev = GUI.color;
                GUI.color = RegistryEditorStyles.ErrorCol;
                EditorGUILayout.HelpBox("'_' заборонений в ID.", MessageType.Error);
                GUI.color = prev;
            }
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Replace('\\', '/').TrimEnd('/').Split('/');
            if (parts.Length == 0 || parts[0] != "Assets") return;
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        private static GameObject CreateEmptyPrefab(string id)
        {
            EnsureFolder(UnitPrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{UnitPrefabFolder}/{safe}.prefab");
            var go = new GameObject(safe);
            var pfb = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return pfb;
        }
    }
}
