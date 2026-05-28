using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    public sealed class RegistryFactoryEditorWindow : EditorWindow
    {
        private const string TilePrefabFolder = "Assets/Moyva/Prefabs/Tiles";
        private const string ObjectPrefabFolder = "Assets/Moyva/Prefabs/Objects";
        private const string UnitPrefabFolder = "Assets/Moyva/Prefabs/Units";

        private TileRegistrySO _tileRegistry;
        private string _tileId = string.Empty;
        private float _tileMovementCost = 1f;
        private Sprite _tileSprite;
        private GameObject _tilePrefab;

        private MapObjectRegistrySO _objectRegistry;
        private string _objectId = string.Empty;
        private Sprite _objectSprite;
        private GameObject _objectPrefab;

        private UnitRegistrySO _unitRegistry;
        private string _unitTypeId = string.Empty;
        private float _unitBaseStamina = 100f;
        private Vector2 _unitStaminaRandomRange = new Vector2(-5f, 5f);
        private Sprite _unitSprite;
        private GameObject _unitPrefab;

        private Vector2 _scroll;

        private static void Open()
        {
            // Нова версія: відкривається Registry Hub
            RegistryHubWindow.Open();
        }

        private void OnEnable()
        {
            _tileRegistry = FindFirstAsset<TileRegistrySO>();
            _objectRegistry = FindFirstAsset<MapObjectRegistrySO>();
            _unitRegistry = FindFirstAsset<UnitRegistrySO>();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Registry Factory", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Швидке додавання записів у Tile/Object/Unit реєстри з перевіркою дубльованих ID.", MessageType.Info);

            DrawTileSection();
            DrawObjectSection();
            DrawUnitSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawTileSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Додати Tile", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Автогенерація prefab", TilePrefabFolder, EditorStyles.miniLabel);

            _tileRegistry = (TileRegistrySO)EditorGUILayout.ObjectField("Tile Registry", _tileRegistry, typeof(TileRegistrySO), false);
            _tileId = EditorGUILayout.TextField("Tile ID", _tileId);
            _tileMovementCost = EditorGUILayout.FloatField("Movement Cost", _tileMovementCost);
            _tileSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _tileSprite, typeof(Sprite), false);
            _tilePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab (optional)", _tilePrefab, typeof(GameObject), false);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Додати Tile", GUILayout.Width(160f)))
            {
                AddTileDefinition();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawObjectSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Додати Map Object", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Автогенерація prefab", ObjectPrefabFolder, EditorStyles.miniLabel);
            EditorGUILayout.HelpBox(
                "Зараз для Map Object немає окремих definition-полів для прохідності, блокування зору чи дозволу на будівництво. " +
                "Фактично object займає клітинку і тому блокує pathfinding та preview будівництва через ObjectsMap. Fog of War blocking API ще відсутній.",
                MessageType.Info);

            _objectRegistry = (MapObjectRegistrySO)EditorGUILayout.ObjectField("Object Registry", _objectRegistry, typeof(MapObjectRegistrySO), false);
            _objectId = EditorGUILayout.TextField("Object ID", _objectId);
            _objectSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _objectSprite, typeof(Sprite), false);
            _objectPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab (optional)", _objectPrefab, typeof(GameObject), false);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Додати Object", GUILayout.Width(160f)))
            {
                AddObjectDefinition();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawUnitSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Додати Unit Type", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Автогенерація prefab", UnitPrefabFolder, EditorStyles.miniLabel);

            _unitRegistry = (UnitRegistrySO)EditorGUILayout.ObjectField("Unit Registry", _unitRegistry, typeof(UnitRegistrySO), false);
            _unitTypeId = EditorGUILayout.TextField("Unit Type ID", _unitTypeId);
            _unitBaseStamina = EditorGUILayout.FloatField("Base Stamina", _unitBaseStamina);
            _unitStaminaRandomRange = EditorGUILayout.Vector2Field("Stamina Random Range", _unitStaminaRandomRange);
            _unitSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _unitSprite, typeof(Sprite), false);
            _unitPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab (optional)", _unitPrefab, typeof(GameObject), false);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Додати Unit", GUILayout.Width(160f)))
            {
                AddUnitDefinition();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void AddTileDefinition()
        {
            if (_tileRegistry == null)
            {
                EditorUtility.DisplayDialog("Registry Factory", "TileRegistrySO не призначений.", "OK");
                return;
            }

            string id = NormalizeId(_tileId);
            if (string.IsNullOrEmpty(id))
            {
                EditorUtility.DisplayDialog("Registry Factory", "Tile ID не може бути порожнім.", "OK");
                return;
            }

            var so = new SerializedObject(_tileRegistry);
            var definitions = so.FindProperty("_definitions");
            if (definitions == null)
            {
                EditorUtility.DisplayDialog("Registry Factory", "Не знайдено поле _definitions у TileRegistrySO.", "OK");
                return;
            }

            if (ContainsId(definitions, "_id", id))
            {
                EditorUtility.DisplayDialog("Registry Factory", $"Tile ID '{id}' вже існує.", "OK");
                return;
            }

            GameObject prefab = ResolvePrefab(id, _tilePrefab, _tileSprite, TilePrefabFolder);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Registry Factory", "Потрібен Sprite або Prefab для Tile.", "OK");
                return;
            }

            int index = definitions.arraySize;
            definitions.InsertArrayElementAtIndex(index);
            SerializedProperty element = definitions.GetArrayElementAtIndex(index);

            element.FindPropertyRelative("_id").stringValue = id;
            element.FindPropertyRelative("_movementCost").floatValue = _tileMovementCost;
            element.FindPropertyRelative("_visualPrefab").objectReferenceValue = prefab;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_tileRegistry);
            AssetDatabase.SaveAssets();

            _tileId = string.Empty;
            _tileSprite = null;
            _tilePrefab = null;

            ShowNotification(new GUIContent($"Tile '{id}' додано"));
        }

        private void AddObjectDefinition()
        {
            if (_objectRegistry == null)
            {
                EditorUtility.DisplayDialog("Registry Factory", "MapObjectRegistrySO не призначений.", "OK");
                return;
            }

            string id = NormalizeId(_objectId);
            if (string.IsNullOrEmpty(id))
            {
                EditorUtility.DisplayDialog("Registry Factory", "Object ID не може бути порожнім.", "OK");
                return;
            }

            var so = new SerializedObject(_objectRegistry);
            var definitions = so.FindProperty("_definitions");
            if (definitions == null)
            {
                EditorUtility.DisplayDialog("Registry Factory", "Не знайдено поле _definitions у MapObjectRegistrySO.", "OK");
                return;
            }

            if (ContainsId(definitions, "_id", id))
            {
                EditorUtility.DisplayDialog("Registry Factory", $"Object ID '{id}' вже існує.", "OK");
                return;
            }

            GameObject prefab = ResolvePrefab(id, _objectPrefab, _objectSprite, ObjectPrefabFolder);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Registry Factory", "Потрібен Sprite або Prefab для Object.", "OK");
                return;
            }

            // Future extension point:
            // When MapObjectDefinition gets gameplay metadata, populate it here.
            // Examples requested by design but not supported in runtime yet:
            // - per-object walkability override
            // - per-object fog-of-war vision blocking
            // - per-object buildable / player-constructible flag

            int index = definitions.arraySize;
            definitions.InsertArrayElementAtIndex(index);
            SerializedProperty element = definitions.GetArrayElementAtIndex(index);

            element.FindPropertyRelative("_id").stringValue = id;
            element.FindPropertyRelative("_visualPrefab").objectReferenceValue = prefab;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_objectRegistry);
            AssetDatabase.SaveAssets();

            _objectId = string.Empty;
            _objectSprite = null;
            _objectPrefab = null;

            ShowNotification(new GUIContent($"Object '{id}' додано"));
        }

        private void AddUnitDefinition()
        {
            if (_unitRegistry == null)
            {
                EditorUtility.DisplayDialog("Registry Factory", "UnitRegistrySO не призначений.", "OK");
                return;
            }

            string id = NormalizeId(_unitTypeId);
            if (string.IsNullOrEmpty(id))
            {
                EditorUtility.DisplayDialog("Registry Factory", "Unit Type ID не може бути порожнім.", "OK");
                return;
            }

            _unitRegistry.Configs ??= new List<UnitClassConfig>();
            if (_unitRegistry.Configs.Exists(c => c != null && string.Equals(c.TypeId, id, StringComparison.OrdinalIgnoreCase)))
            {
                EditorUtility.DisplayDialog("Registry Factory", $"Unit Type ID '{id}' вже існує.", "OK");
                return;
            }

            GameObject prefab = ResolvePrefab(id, _unitPrefab, _unitSprite, UnitPrefabFolder);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Registry Factory", "Потрібен Sprite або Prefab для Unit.", "OK");
                return;
            }

            _unitRegistry.Configs.Add(new UnitClassConfig
            {
                TypeId = id,
                BaseStamina = _unitBaseStamina,
                StaminaRandomRange = _unitStaminaRandomRange,
                Prefab = prefab
            });

            EditorUtility.SetDirty(_unitRegistry);
            AssetDatabase.SaveAssets();

            _unitTypeId = string.Empty;
            _unitSprite = null;
            _unitPrefab = null;

            ShowNotification(new GUIContent($"Unit '{id}' додано"));
        }

        private GameObject ResolvePrefab(string id, GameObject prefabOverride, Sprite sprite, string targetFolder)
        {
            if (prefabOverride != null)
                return prefabOverride;

            if (sprite == null)
                return null;

            return CreatePrefabFromSprite(id, sprite, targetFolder);
        }

        private GameObject CreatePrefabFromSprite(string id, Sprite sprite, string targetFolder)
        {
            EnsureFolderExists(targetFolder);
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                Debug.LogError($"[RegistryFactory] Invalid prefab folder: {targetFolder}");
                return null;
            }

            string safeId = SanitizeForFileName(id);
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{safeId}.prefab");

            var temp = new GameObject(safeId);
            var renderer = temp.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            DestroyImmediate(temp);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return prefab;
        }

        private static T FindFirstAsset<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static bool ContainsId(SerializedProperty arrayProperty, string idPropertyName, string id)
        {
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                SerializedProperty item = arrayProperty.GetArrayElementAtIndex(i);
                string existingId = item.FindPropertyRelative(idPropertyName)?.stringValue;
                if (string.Equals(existingId, id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string NormalizeId(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            string normalized = folderPath.Replace('\\', '/').TrimEnd('/');
            if (!normalized.StartsWith("Assets", StringComparison.Ordinal))
                return;

            string[] parts = normalized.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }

        private static string SanitizeForFileName(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "item";

            char[] invalid = System.IO.Path.GetInvalidFileNameChars();
            char[] chars = id.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(invalid, chars[i]) >= 0 || chars[i] == '/')
                    chars[i] = '_';
            }

            return new string(chars);
        }
    }
}