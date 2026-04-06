using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    [CustomEditor(typeof(TileRegistrySO))]
    public sealed class TileRegistryEditor : UnityEditor.Editor
    {
        private const string TilePrefabFolder = "Assets/Moyva/Prefabs/Tiles";

        private SerializedProperty _definitions;
        private Vector2 _scroll;
        private bool _createOpen;
        private string _newId = "";
        private float _newCost = 1f;
        private Sprite _newSprite;
        private GameObject _newPrefab;

        private void OnEnable()
        {
            _definitions = serializedObject.FindProperty("_definitions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ── Заголовок ──
            RegistryEditorStyles.DrawColoredHeader("  Tile Registry", RegistryEditorStyles.Accent);
            EditorGUILayout.Space(2);

            int count = _definitions?.arraySize ?? 0;

            // ── Кнопка хабу ──
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{count} тайл(ів)", EditorStyles.boldLabel);
            if (GUILayout.Button("Відкрити Registry Hub", GUILayout.Width(160)))
                RegistryHubWindow.Open(0);
            EditorGUILayout.EndHorizontal();

            RegistryEditorStyles.DrawSeparator();

            // ── Список записів ──
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MaxHeight(400));

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = _definitions.GetArrayElementAtIndex(i);
                string id   = el.FindPropertyRelative("_id")?.stringValue ?? "?";
                float  cost = el.FindPropertyRelative("_movementCost")?.floatValue ?? 0f;
                var    pfb  = el.FindPropertyRelative("_visualPrefab")?.objectReferenceValue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);

                // Рядок заголовка
                EditorGUILayout.BeginHorizontal();
                DrawIdLabel(id);
                GUILayout.FlexibleSpace();
                Color prev = GUI.color;
                GUI.color = RegistryEditorStyles.ErrorCol;
                if (GUILayout.Button("\u00d7", GUILayout.Width(22), GUILayout.Height(18)))
                    removeIdx = i;
                GUI.color = prev;
                EditorGUILayout.EndHorizontal();

                // Деталі
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Рух: {cost:F1}", EditorStyles.miniLabel, GUILayout.Width(80));
                EditorGUILayout.LabelField(pfb ? $"Prefab: {pfb.name}" : "Prefab: \u2717", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                // Inline редагування через PropertyField
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(el.FindPropertyRelative("_id"), new GUIContent("ID"));
                ValidateInlineId(el.FindPropertyRelative("_id")?.stringValue);
                EditorGUILayout.PropertyField(el.FindPropertyRelative("_movementCost"), new GUIContent("Movement Cost"));
                EditorGUILayout.PropertyField(el.FindPropertyRelative("_visualPrefab"), new GUIContent("Visual Prefab"));
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(1);
            }

            EditorGUILayout.EndScrollView();

            if (removeIdx >= 0)
            {
                string name = _definitions.GetArrayElementAtIndex(removeIdx).FindPropertyRelative("_id")?.stringValue ?? "?";
                if (EditorUtility.DisplayDialog("Видалити", $"Видалити тайл '{name}'?", "Так", "Ні"))
                    _definitions.DeleteArrayElementAtIndex(removeIdx);
            }

            RegistryEditorStyles.DrawSeparator();

            // ── Створення ──
            _createOpen = EditorGUILayout.Foldout(_createOpen, "\u2795 Створити новий тайл", true, EditorStyles.foldoutHeader);
            if (_createOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                _newId     = RegistryEditorStyles.IdFieldWithDuplicateCheck("ID", _newId, _definitions, "_id");
                _newCost   = EditorGUILayout.FloatField("Movement Cost", _newCost);
                _newSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _newSprite, typeof(Sprite), false);
                _newPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newPrefab, typeof(GameObject), false);
                if (!_newPrefab && !_newSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);
                EditorGUILayout.Space(4);

                bool valid = RegistryEditorStyles.ValidateIdFull(_newId, _definitions, "_id") == null;
                EditorGUI.BeginDisabledGroup(!valid);
                if (GUILayout.Button("\u2713 Створити тайл", RegistryEditorStyles.CreateButton))
                    DoCreate();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DoCreate()
        {
            string id = _newId.Trim();
            if (RegistryEditorStyles.ValidateIdFull(id, _definitions, "_id") != null) return;

            GameObject pfb = _newPrefab ? _newPrefab : CreatePrefab(id, _newSprite);
            if (!pfb) pfb = CreateEmptyPrefab(id);

            int idx = _definitions.arraySize;
            _definitions.InsertArrayElementAtIndex(idx);
            var el = _definitions.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("_id").stringValue = id;
            el.FindPropertyRelative("_movementCost").floatValue = _newCost;
            el.FindPropertyRelative("_visualPrefab").objectReferenceValue = pfb;
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            _newId = ""; _newSprite = null; _newPrefab = null;
        }

        private bool ContainsId(string id)
        {
            for (int i = 0; i < _definitions.arraySize; i++)
            {
                string existing = _definitions.GetArrayElementAtIndex(i).FindPropertyRelative("_id")?.stringValue;
                if (string.Equals(existing, id, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static GameObject CreatePrefab(string id, Sprite sprite)
        {
            if (!sprite) return null;
            EnsureFolder(TilePrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{TilePrefabFolder}/{safe}.prefab");
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
            EnsureFolder(TilePrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{TilePrefabFolder}/{safe}.prefab");
            var go = new GameObject(safe);
            var pfb = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return pfb;
        }
    }
}
