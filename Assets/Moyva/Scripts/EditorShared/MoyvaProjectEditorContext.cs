using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class MoyvaProjectEditorContext
    {
        private const string KeyPrefix = "Moyva.ProjectEditorContext.ScriptableObject.";

        public static event Action<string, UnityEngine.Object> AssetChanged;

        public static T Get<T>() where T : UnityEngine.Object
        {
            return Get(typeof(T).Name, typeof(T)) as T;
        }

        public static UnityEngine.Object Get(string typeName, Type expectedType = null)
        {
            string guid = EditorPrefs.GetString(BuildKey(typeName), string.Empty);
            if (string.IsNullOrWhiteSpace(guid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (expectedType != null)
                return AssetDatabase.LoadAssetAtPath(path, expectedType);

            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        }

        public static T GetOrFindFirst<T>(bool rememberFoundAsset = true) where T : UnityEngine.Object
        {
            var selected = Get<T>();
            if (selected != null)
                return selected;

            selected = FindFirstAsset<T>();
            if (selected != null && rememberFoundAsset)
                Set(selected);

            return selected;
        }

        public static void Set<T>(T asset) where T : UnityEngine.Object
        {
            Set(typeof(T).Name, asset);
        }

        public static void Set(string typeName, UnityEngine.Object asset)
        {
            string key = BuildKey(typeName);
            if (asset == null)
            {
                EditorPrefs.DeleteKey(key);
                AssetChanged?.Invoke(typeName, null);
                return;
            }

            string path = AssetDatabase.GetAssetPath(asset);
            string guid = string.IsNullOrWhiteSpace(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrWhiteSpace(guid))
            {
                EditorPrefs.DeleteKey(key);
                AssetChanged?.Invoke(typeName, null);
                return;
            }

            EditorPrefs.SetString(key, guid);
            AssetChanged?.Invoke(typeName, asset);
        }

        public static T FindFirstAsset<T>() where T : UnityEngine.Object
        {
            var asset = FindFirstAsset(typeof(T));
            return asset as T;
        }

        public static UnityEngine.Object FindFirstAsset(Type assetType)
        {
            if (assetType == null)
                return null;

            string[] guids = AssetDatabase.FindAssets($"t:{assetType.Name}");
            Array.Sort(guids, StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                var asset = AssetDatabase.LoadAssetAtPath(path, assetType);
                if (asset != null)
                    return asset;
            }

            return null;
        }

        public static Type ResolveScriptableObjectType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .FirstOrDefault(type => string.Equals(type.Name, typeName, StringComparison.Ordinal)
                    || string.Equals(type.FullName, typeName, StringComparison.Ordinal));
        }

        public static string GetAssetPath(UnityEngine.Object asset)
        {
            return asset != null ? AssetDatabase.GetAssetPath(asset) : string.Empty;
        }

        private static string BuildKey(string typeName)
        {
            return KeyPrefix + (string.IsNullOrWhiteSpace(typeName) ? "Unknown" : typeName.Trim());
        }
    }
}
