using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    [CustomEditor(typeof(BuildingRegistrySO))]
    public sealed class BuildingRegistryEditor : UnityEditor.Editor
    {
        private const string BuildingPrefabFolder = "Assets/Moyva/Prefabs/Buildings";
        private SerializedProperty _buildings;
        private Vector2 _scroll;
        private bool _createOpen;
        private string _newId = "";
        private string _newDisplayName = "";
        private BuildingCategory _newCategory;
        private GameObject _newPrefab;
        private Sprite _newSprite;

        private void OnEnable()
        {
            _buildings = serializedObject.FindProperty("Buildings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RegistryEditorStyles.DrawColoredHeader("  Building Registry", RegistryEditorStyles.Accent);
            EditorGUILayout.Space(2);

            int count = _buildings?.arraySize ?? 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{count} будівл(ь)", EditorStyles.boldLabel);
            if (GUILayout.Button("Відкрити Registry Hub", GUILayout.Width(160)))
                RegistryHubWindow.Open(3);
            if (GUILayout.Button("Rebuild previews", GUILayout.Width(140)))
            {
                BuildingPrefabPreviewCacheUtility.RebuildSerializedRegistryPreviews(serializedObject, target);
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();

            RegistryEditorStyles.DrawSeparator();

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MaxHeight(500));

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = _buildings.GetArrayElementAtIndex(i);
                string id      = el.FindPropertyRelative("Id")?.stringValue ?? "?";
                string display = el.FindPropertyRelative("DisplayName")?.stringValue ?? "";
                int    catIdx  = el.FindPropertyRelative("Category")?.enumValueIndex ?? 0;
                var    pfb     = el.FindPropertyRelative("Prefab")?.objectReferenceValue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);

                // Заголовок
                EditorGUILayout.BeginHorizontal();
                DrawIdLabel(id);
                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(display))
                    EditorGUILayout.LabelField($"\"{display}\"", EditorStyles.miniLabel, GUILayout.Width(120));

                string catName = ((BuildingCategory)catIdx).ToString();
                RegistryEditorStyles.DrawBadge(catName, CategoryColor(catIdx));

                EditorGUILayout.LabelField(pfb ? "\u2713" : "\u2717", EditorStyles.miniLabel, GUILayout.Width(16));

                Color prev = GUI.color;
                GUI.color = RegistryEditorStyles.ErrorCol;
                if (GUILayout.Button("\u00d7", GUILayout.Width(22), GUILayout.Height(18)))
                    removeIdx = i;
                GUI.color = prev;
                EditorGUILayout.EndHorizontal();

                // Inline editing
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(el.FindPropertyRelative("Id"), new GUIContent("ID"));
                ValidateInlineId(el.FindPropertyRelative("Id")?.stringValue);
                EditorGUILayout.PropertyField(el.FindPropertyRelative("DisplayName"));
                EditorGUILayout.PropertyField(el.FindPropertyRelative("Category"));
                var prefabProp = el.FindPropertyRelative("Prefab");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(prefabProp);
                if (EditorGUI.EndChangeCheck())
                    BuildingPrefabPreviewCacheUtility.RebuildSerializedBuildingPreview(el, target);
                using (new EditorGUI.DisabledScope(true))
                {
                    var previewProp = el.FindPropertyRelative("RuntimePreview");
                    if (previewProp != null)
                        EditorGUILayout.PropertyField(previewProp, new GUIContent("Cached Preview"));
                }
                EditorGUILayout.LabelField("Вартість будівництва", EditorStyles.boldLabel);
                BuildingConstructionCostEditorShared.DrawCostList(
                    el.FindPropertyRelative("ConstructionCost"),
                    "Додати ресурс для будівництва");
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(1);
            }

            EditorGUILayout.EndScrollView();

            if (removeIdx >= 0)
            {
                string name = _buildings.GetArrayElementAtIndex(removeIdx).FindPropertyRelative("Id")?.stringValue ?? "?";
                if (EditorUtility.DisplayDialog("Видалити", $"Видалити будівлю '{name}'?", "Так", "Ні"))
                    _buildings.DeleteArrayElementAtIndex(removeIdx);
            }

            RegistryEditorStyles.DrawSeparator();

            _createOpen = EditorGUILayout.Foldout(_createOpen, "\u2795 Створити нову будівлю", true, EditorStyles.foldoutHeader);
            if (_createOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                _newId          = RegistryEditorStyles.IdFieldWithDuplicateCheck("ID", _newId, _buildings, "Id");
                _newDisplayName = EditorGUILayout.TextField("Display Name", _newDisplayName);
                _newCategory    = (BuildingCategory)EditorGUILayout.EnumPopup("Category", _newCategory);
                _newSprite      = (Sprite)EditorGUILayout.ObjectField("Sprite", _newSprite, typeof(Sprite), false);
                _newPrefab      = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newPrefab, typeof(GameObject), false);
                if (!_newPrefab && !_newSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);
                EditorGUILayout.Space(4);

                bool valid = RegistryEditorStyles.ValidateIdFull(_newId, _buildings, "Id") == null;
                EditorGUI.BeginDisabledGroup(!valid);
                if (GUILayout.Button("\u2713 Створити будівлю", RegistryEditorStyles.CreateButton))
                    DoCreate();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DoCreate()
        {
            string id = _newId.Trim();
            if (RegistryEditorStyles.ValidateIdFull(id, _buildings, "Id") != null) return;

            GameObject pfb = ResolvePrefab(id);

            int idx = _buildings.arraySize;
            _buildings.InsertArrayElementAtIndex(idx);
            var el = _buildings.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("Id").stringValue = id;
            el.FindPropertyRelative("DisplayName").stringValue = _newDisplayName;
            el.FindPropertyRelative("Category").enumValueIndex = (int)_newCategory;
            var iconProp = el.FindPropertyRelative("Icon");
            if (iconProp != null)
                iconProp.objectReferenceValue = _newSprite;
            el.FindPropertyRelative("Prefab").objectReferenceValue = pfb;
            BuildingPrefabPreviewCacheUtility.RebuildSerializedBuildingPreview(el, target);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            _newId = ""; _newDisplayName = ""; _newPrefab = null; _newSprite = null;
        }

        private GameObject ResolvePrefab(string id)
        {
            if (_newPrefab) return _newPrefab;
            if (_newSprite) return CreatePrefabFromSprite(id, _newSprite);
            return CreateEmptyPrefab(id);
        }

        private static GameObject CreatePrefabFromSprite(string id, Sprite sprite)
        {
            EnsureFolder(BuildingPrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{BuildingPrefabFolder}/{safe}.prefab");
            var go = new GameObject(safe);
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            var pfb = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return pfb;
        }

        private static GameObject CreateEmptyPrefab(string id)
        {
            EnsureFolder(BuildingPrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{BuildingPrefabFolder}/{safe}.prefab");
            var go = new GameObject(safe);
            var pfb = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return pfb;
        }

        private bool ContainsId(string id)
        {
            for (int i = 0; i < _buildings.arraySize; i++)
            {
                string existing = _buildings.GetArrayElementAtIndex(i).FindPropertyRelative("Id")?.stringValue;
                if (string.Equals(existing, id, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
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

        private static Color CategoryColor(int idx) => idx switch
        {
            0 => new Color(0.85f, 0.30f, 0.30f),
            1 => new Color(0.35f, 0.70f, 0.35f),
            2 => new Color(0.40f, 0.50f, 0.80f),
            _ => Color.grey,
        };

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
    }
}
