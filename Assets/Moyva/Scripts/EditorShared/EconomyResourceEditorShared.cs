using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Economy.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class EconomyResourceEditorShared
    {
        private const string ResourcesProperty = "_resources";
        private const string EconomyRootFolder = "Assets/Moyva/SO/Economy";
        private const string ResourcesFolder = EconomyRootFolder + "/Resources";

        public static bool TrySyncResourceAssetName(EconomyResourceDefinition resource, out string error)
        {
            error = string.Empty;
            if (resource == null)
                return false;

            string preferredName = GetPreferredResourceAssetName(resource);
            if (string.IsNullOrWhiteSpace(preferredName))
                return false;

            if (string.Equals(resource.name, preferredName, StringComparison.Ordinal))
                return false;

            string path = AssetDatabase.GetAssetPath(resource);
            if (string.IsNullOrWhiteSpace(path))
                return false;

            string renameError = AssetDatabase.RenameAsset(path, preferredName);
            if (!string.IsNullOrWhiteSpace(renameError))
            {
                error = renameError;
                return false;
            }

            AssetDatabase.SaveAssets();
            return true;
        }

        public static string GetPreferredResourceAssetName(EconomyResourceDefinition resource)
        {
            if (resource == null)
                return string.Empty;

            string raw = !string.IsNullOrWhiteSpace(resource.DisplayName)
                ? resource.DisplayName
                : resource.Id;

            return SanitizeAssetName(raw);
        }

        public static List<EconomyResourceDefinition> LoadResources()
        {
            var resources = new List<EconomyResourceDefinition>();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(EconomyResourceDefinition)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<EconomyResourceDefinition>(path);
                if (asset != null)
                    resources.Add(asset);
            }

            return resources
                .OrderBy(r => r.Id ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(r => r.name ?? string.Empty, StringComparer.Ordinal)
                .ToList();
        }

        public static List<string> LoadResourceIds()
        {
            return LoadResources()
                .Select(r => (r.Id ?? string.Empty).Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
        }

        public static List<EconomyResourceDefinition> LoadResourcesNotInDatabase(EconomyDatabaseSO database)
        {
            var resources = LoadResources();
            if (database == null)
                return resources;

            return resources
                .Where(r => !HasResourceInDatabase(database, r))
                .ToList();
        }

        public static EconomyResourceDefinition CreateResourceAssetInteractive(string defaultFileName = "EconomyResourceDefinition")
        {
            return CreateResourceAssetInProjectFolder(defaultFileName);
        }

        public static EconomyResourceDefinition CreateResourceAssetInProjectFolder(string defaultFileName = "EconomyResourceDefinition")
        {
            EnsureFolder(EconomyRootFolder);
            EnsureFolder(ResourcesFolder);

            string baseName = SanitizeAssetName(defaultFileName);
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "EconomyResourceDefinition";

            string path = AssetDatabase.GenerateUniqueAssetPath($"{ResourcesFolder}/{baseName}.asset");

            var asset = ScriptableObject.CreateInstance<EconomyResourceDefinition>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static int AddAllResourcesToDatabase(EconomyDatabaseSO database)
        {
            if (database == null)
                return 0;

            var toAdd = LoadResourcesNotInDatabase(database);
            int added = 0;
            for (int i = 0; i < toAdd.Count; i++)
            {
                if (AddResourceToDatabase(database, toAdd[i]))
                    added++;
            }

            return added;
        }

        public static bool HasResourceInDatabase(EconomyDatabaseSO database, EconomyResourceDefinition resource)
        {
            if (database == null || resource == null)
                return false;

            var so = new SerializedObject(database);
            so.Update();
            var list = so.FindProperty(ResourcesProperty);
            if (list == null || !list.isArray)
                return false;

            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == resource)
                    return true;
            }

            return false;
        }

        public static bool AddResourceToDatabase(EconomyDatabaseSO database, EconomyResourceDefinition resource)
        {
            if (database == null || resource == null)
                return false;

            var so = new SerializedObject(database);
            so.Update();
            var list = so.FindProperty(ResourcesProperty);
            if (list == null || !list.isArray)
                return false;

            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == resource)
                    return false;
            }

            Undo.RecordObject(database, "Економіка: додати ресурс");
            int idx = list.arraySize;
            list.InsertArrayElementAtIndex(idx);
            list.GetArrayElementAtIndex(idx).objectReferenceValue = resource;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            return true;
        }

        public static bool RemoveResourceFromDatabase(EconomyDatabaseSO database, EconomyResourceDefinition resource)
        {
            if (database == null || resource == null)
                return false;

            var so = new SerializedObject(database);
            so.Update();
            var list = so.FindProperty(ResourcesProperty);
            if (list == null || !list.isArray)
                return false;

            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue != resource)
                    continue;

                Undo.RecordObject(database, "Економіка: видалити ресурс");
                list.DeleteArrayElementAtIndex(i);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                return true;
            }

            return false;
        }

        public static bool DeleteResourceAsset(EconomyDatabaseSO database, EconomyResourceDefinition resource, out string error)
        {
            error = string.Empty;
            if (resource == null)
            {
                error = "Ресурс не заданий.";
                return false;
            }

            if (database != null)
                RemoveResourceFromDatabase(database, resource);

            string path = AssetDatabase.GetAssetPath(resource);
            if (string.IsNullOrWhiteSpace(path))
            {
                error = "Не вдалось визначити шлях до asset.";
                return false;
            }

            bool deleted = AssetDatabase.DeleteAsset(path);
            if (!deleted)
            {
                error = $"Не вдалось видалити asset за шляхом '{path}'.";
                return false;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        private static string SanitizeAssetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string trimmed = value.Trim();
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var chars = trimmed.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (invalidChars.Contains(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars).Trim();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            string[] parts = folderPath.Split('/');
            if (parts.Length == 0)
                return;

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
