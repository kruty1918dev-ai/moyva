using Kruty1918.Moyva.Generator.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    [CustomEditor(typeof(MapObjectRegistrySO))]
    public sealed class MapObjectRegistryEditor : UnityEditor.Editor
    {
        private const string ObjectPrefabFolder = "Assets/Moyva/Prefabs/Objects";

        private SerializedProperty _definitions;
        private Vector2 _scroll;
        private bool _createOpen;
        private string _newId = "";
        private Sprite _newSprite;
        private GameObject _newPrefab;

        private void OnEnable()
        {
            _definitions = serializedObject.FindProperty("_definitions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RegistryEditorStyles.DrawColoredHeader("  Map Object Registry", RegistryEditorStyles.Accent);
            EditorGUILayout.Space(2);

            int count = _definitions?.arraySize ?? 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{count} об'єкт(ів)", EditorStyles.boldLabel);
            if (GUILayout.Button("Відкрити Registry Hub", GUILayout.Width(160)))
                RegistryHubWindow.Open(1);
            EditorGUILayout.EndHorizontal();

            RegistryEditorStyles.DrawSeparator();

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MaxHeight(400));

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = _definitions.GetArrayElementAtIndex(i);
                string id  = el.FindPropertyRelative("_id")?.stringValue ?? "?";
                var    pfb = el.FindPropertyRelative("_visualPrefab")?.objectReferenceValue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);

                EditorGUILayout.BeginHorizontal();
                DrawIdLabel(id);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(pfb ? $"Prefab: {pfb.name}" : "Prefab: \u2717", EditorStyles.miniLabel, GUILayout.Width(160));
                Color prev = GUI.color;
                GUI.color = RegistryEditorStyles.ErrorCol;
                if (GUILayout.Button("\u00d7", GUILayout.Width(22), GUILayout.Height(18)))
                    removeIdx = i;
                GUI.color = prev;
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(el.FindPropertyRelative("_id"), new GUIContent("ID"));
                ValidateInlineId(el.FindPropertyRelative("_id")?.stringValue);
                EditorGUILayout.PropertyField(el.FindPropertyRelative("_visualPrefab"), new GUIContent("Visual Prefab"));
                if (GUILayout.Button("Відкрити в редакторі економіки", GUILayout.Width(240f)))
                    OpenEconomyMapObjectsTab(id);
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(1);
            }

            EditorGUILayout.EndScrollView();

            if (removeIdx >= 0)
            {
                string name = _definitions.GetArrayElementAtIndex(removeIdx).FindPropertyRelative("_id")?.stringValue ?? "?";
                if (EditorUtility.DisplayDialog("Видалити", $"Видалити об'єкт '{name}'?", "Так", "Ні"))
                    _definitions.DeleteArrayElementAtIndex(removeIdx);
            }

            RegistryEditorStyles.DrawSeparator();

            _createOpen = EditorGUILayout.Foldout(_createOpen, "\u2795 Створити новий об'єкт", true, EditorStyles.foldoutHeader);
            if (_createOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                _newId     = RegistryEditorStyles.IdFieldWithDuplicateCheck("ID", _newId, _definitions, "_id");
                _newSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _newSprite, typeof(Sprite), false);
                _newPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newPrefab, typeof(GameObject), false);
                if (!_newPrefab && !_newSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);
                EditorGUILayout.Space(4);

                bool valid = RegistryEditorStyles.ValidateIdFull(_newId, _definitions, "_id") == null;
                EditorGUI.BeginDisabledGroup(!valid);
                if (GUILayout.Button("\u2713 Створити об'єкт", RegistryEditorStyles.CreateButton))
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
            EnsureFolder(ObjectPrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{ObjectPrefabFolder}/{safe}.prefab");
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
            EnsureFolder(ObjectPrefabFolder);
            string safe = id.Replace('/', '-').Replace('\\', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{ObjectPrefabFolder}/{safe}.prefab");
            var go = new GameObject(safe);
            var pfb = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return pfb;
        }

        private static void OpenEconomyMapObjectsTab(string mapObjectId)
        {
            var windowType = System.Type.GetType(
                "Kruty1918.Moyva.Economy.Editor.EconomyDesignerWindow, Kruty1918.Moyva.Economy.Editor");

            if (windowType == null)
            {
                EditorApplication.ExecuteMenuItem("Moyva/Tools/Редактор Економіки");
                return;
            }

            var method = windowType.GetMethod(
                "OpenMapObjectsTab",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (method != null)
            {
                method.Invoke(null, new object[] { mapObjectId });
                return;
            }

            EditorApplication.ExecuteMenuItem("Moyva/Tools/Редактор Економіки");
        }
    }
}
