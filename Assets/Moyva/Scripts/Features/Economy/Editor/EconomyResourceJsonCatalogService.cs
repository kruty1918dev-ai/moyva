#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Editor.Shared;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Editor
{
    public sealed class EconomyResourceJsonSyncResult
    {
        public int ResourceCount;
        public int ChangedCount;
        public int CreatedCount;
        public string Summary;
    }

    public static class EconomyResourceJsonCatalogService
    {
        public const string JsonPath = "Assets/Moyva/Presets/Economy/resources.json";
        private const string ExpectedSchema = "moyva.economy-resources";
        private const string ResourceRoot = "Assets/Moyva/Data/ScriptableObjects/Economy/Resources/";
        private const int SupportedVersion = 1;

        [MenuItem("Moyva/Data/JSON/Economy Resources/Validate and Sync")]
        public static void SyncFromMenu()
        {
            EconomyResourceJsonSyncResult result = SyncFromJson();
            Debug.Log($"[MoyvaEconomyJson] {result.Summary}");
        }

        [MenuItem("Moyva/Data/JSON/Economy Resources/Export Current Database")]
        public static void ExportFromMenu()
        {
            EconomyDatabaseSO database = FindOnlyDatabase();
            EconomyResourcesJsonDocument document = Export(database);
            MoyvaJsonFile.Write(JsonPath, document);
            AssetDatabase.ImportAsset(JsonPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[MoyvaEconomyJson] Exported {document.Resources.Count} resources to {JsonPath}.");
        }

        public static EconomyResourceJsonSyncResult SyncFromJson()
        {
            EconomyResourcesJsonDocument document = MoyvaJsonFile.Read<EconomyResourcesJsonDocument>(JsonPath);
            Validate(document);
            EconomyDatabaseSO database = ResolveDatabase(document.DatabaseAsset);
            var resolvedResources = new List<EconomyResourceDefinition>(document.Resources.Count);
            int changedCount = 0;
            int createdCount = 0;

            for (int index = 0; index < document.Resources.Count; index++)
            {
                EconomyResourceJsonDefinition source = document.Resources[index];
                bool created;
                bool changed;
                EconomyResourceDefinition resource = ApplyResource(source, index, out created, out changed);
                resolvedResources.Add(resource);
                if (created) createdCount++;
                if (changed) changedCount++;
            }

            AppendMissingResources(database, resolvedResources);
            AssetDatabase.SaveAssets();
            return new EconomyResourceJsonSyncResult
            {
                ResourceCount = resolvedResources.Count,
                ChangedCount = changedCount,
                CreatedCount = createdCount,
                Summary = $"schema={ExpectedSchema}@{SupportedVersion}, resources={resolvedResources.Count}, changed={changedCount}, created={createdCount}",
            };
        }

        private static EconomyResourceDefinition ApplyResource(
            EconomyResourceJsonDefinition source,
            int index,
            out bool created,
            out bool changed)
        {
            string context = $"{JsonPath}: resources[{index}] ({source.Id})";
            EconomyResourceDefinition existing = AssetDatabase.LoadAssetAtPath<EconomyResourceDefinition>(source.Asset);
            UnityEngine.Object occupied = AssetDatabase.LoadMainAssetAtPath(source.Asset);
            if (existing == null && occupied != null)
                throw new InvalidDataException($"{context}: asset path is occupied by {occupied.GetType().Name}: {source.Asset}");

            EconomyResourceDefinition staged = existing != null
                ? UnityEngine.Object.Instantiate(existing)
                : ScriptableObject.CreateInstance<EconomyResourceDefinition>();
            staged.hideFlags = HideFlags.HideAndDontSave;
            staged.name = existing != null ? existing.name : Path.GetFileNameWithoutExtension(source.Asset);
            try
            {
                var serialized = new SerializedObject(staged);
                serialized.FindProperty("_id").stringValue = source.Id;
                serialized.FindProperty("_displayName").stringValue = source.DisplayName;
                serialized.FindProperty("_category").enumValueIndex = (int)ParseCategory(source.Category, context + ".category");
                serialized.FindProperty("_icon").objectReferenceValue = MoyvaJsonAssetReferenceResolver.Resolve<Sprite>(source.Icon, false, context + ".icon");
                serialized.FindProperty("_stackLimit").intValue = source.StackLimit;
                serialized.FindProperty("_weightGrams").intValue = source.WeightGrams;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                created = existing == null;
                changed = created || !string.Equals(
                    EditorJsonUtility.ToJson(existing, false),
                    EditorJsonUtility.ToJson(staged, false),
                    StringComparison.Ordinal);

                if (created)
                {
                    staged.hideFlags = HideFlags.None;
                    AssetDatabase.CreateAsset(staged, source.Asset);
                    existing = staged;
                    staged = null;
                }
                else if (changed)
                {
                    Undo.RecordObject(existing, "Sync economy resource from JSON");
                    EditorUtility.CopySerialized(staged, existing);
                    existing.name = Path.GetFileNameWithoutExtension(source.Asset);
                    EditorUtility.SetDirty(existing);
                }

                return existing;
            }
            finally
            {
                if (staged != null)
                    UnityEngine.Object.DestroyImmediate(staged);
            }
        }

        private static void AppendMissingResources(
            EconomyDatabaseSO database,
            IReadOnlyList<EconomyResourceDefinition> catalogResources)
        {
            var serialized = new SerializedObject(database);
            SerializedProperty resources = serialized.FindProperty("_resources");
            var known = new HashSet<EconomyResourceDefinition>();
            for (int index = 0; index < resources.arraySize; index++)
            {
                if (resources.GetArrayElementAtIndex(index).objectReferenceValue is EconomyResourceDefinition resource)
                    known.Add(resource);
            }

            bool changed = false;
            for (int index = 0; index < catalogResources.Count; index++)
            {
                EconomyResourceDefinition resource = catalogResources[index];
                if (!known.Add(resource))
                    continue;

                int newIndex = resources.arraySize;
                resources.InsertArrayElementAtIndex(newIndex);
                resources.GetArrayElementAtIndex(newIndex).objectReferenceValue = resource;
                changed = true;
            }

            if (!changed)
                return;

            Undo.RecordObject(database, "Register JSON economy resources");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        private static void Validate(EconomyResourcesJsonDocument document)
        {
            if (!string.Equals(document.Schema, ExpectedSchema, StringComparison.Ordinal))
                throw new InvalidDataException($"{JsonPath}: schema must be '{ExpectedSchema}'.");
            if (document.Version != SupportedVersion)
                throw new InvalidDataException($"{JsonPath}: unsupported version {document.Version}; expected {SupportedVersion}.");
            if (document.Resources == null || document.Resources.Count == 0)
                throw new InvalidDataException($"{JsonPath}: resources must contain at least one entry.");

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < document.Resources.Count; index++)
            {
                EconomyResourceJsonDefinition resource = document.Resources[index]
                    ?? throw new InvalidDataException($"{JsonPath}: resources[{index}] is null.");
                string context = $"{JsonPath}: resources[{index}]";
                if (string.IsNullOrWhiteSpace(resource.Asset)
                    || !resource.Asset.StartsWith(ResourceRoot, StringComparison.Ordinal)
                    || !resource.Asset.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"{context}: asset must be an .asset path under {ResourceRoot}");
                }
                if (!paths.Add(resource.Asset))
                    throw new InvalidDataException($"{context}: duplicate asset path '{resource.Asset}'.");
                if (string.IsNullOrWhiteSpace(resource.Id) || !ids.Add(resource.Id))
                    throw new InvalidDataException($"{context}: id is required and must be unique (case-insensitive).");
                if (string.IsNullOrWhiteSpace(resource.DisplayName))
                    throw new InvalidDataException($"{context}: displayName is required.");
                if (resource.StackLimit < 0)
                    throw new InvalidDataException($"{context}: stackLimit cannot be negative; 0 means unlimited.");
                if (resource.WeightGrams < 1)
                    throw new InvalidDataException($"{context}: weightGrams must be >= 1.");
                ParseCategory(resource.Category, context + ".category");
            }
        }

        private static EconomyResourceCategory ParseCategory(string value, string context)
        {
            if (!Enum.TryParse(value, true, out EconomyResourceCategory category)
                || !Enum.IsDefined(typeof(EconomyResourceCategory), category))
            {
                throw new InvalidDataException($"{context}: unknown EconomyResourceCategory '{value}'.");
            }

            return category;
        }

        private static EconomyDatabaseSO ResolveDatabase(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return FindOnlyDatabase();

            EconomyDatabaseSO database = AssetDatabase.LoadAssetAtPath<EconomyDatabaseSO>(assetPath);
            if (database == null)
                throw new InvalidDataException($"{JsonPath}: databaseAsset is not an EconomyDatabaseSO: {assetPath}");
            return database;
        }

        private static EconomyDatabaseSO FindOnlyDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:EconomyDatabaseSO", new[] { "Assets/Moyva" });
            if (guids.Length != 1)
                throw new InvalidOperationException($"Expected exactly one EconomyDatabaseSO in Assets/Moyva, found {guids.Length}.");
            return AssetDatabase.LoadAssetAtPath<EconomyDatabaseSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static EconomyResourcesJsonDocument Export(EconomyDatabaseSO database)
        {
            var document = new EconomyResourcesJsonDocument
            {
                DatabaseAsset = AssetDatabase.GetAssetPath(database),
            };
            foreach (EconomyResourceDefinition resource in database.Resources)
            {
                if (resource == null)
                    continue;
                document.Resources.Add(new EconomyResourceJsonDefinition
                {
                    Asset = AssetDatabase.GetAssetPath(resource),
                    Id = resource.Id,
                    DisplayName = resource.DisplayName,
                    Category = resource.Category.ToString(),
                    Icon = MoyvaJsonAssetReferenceResolver.FromObject(resource.Icon),
                    StackLimit = resource.StackLimit,
                    WeightGrams = resource.WeightGrams,
                });
            }
            return document;
        }
    }

    internal sealed class EconomyResourceJsonAutoSync : AssetPostprocessor
    {
        private static bool _scheduled;
        private static bool _running;

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (!ContainsCatalog(imported) && !ContainsCatalog(moved) || _scheduled)
                return;
            _scheduled = true;
            EditorApplication.delayCall += Apply;
        }

        private static void Apply()
        {
            _scheduled = false;
            if (_running || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                _scheduled = true;
                EditorApplication.delayCall += Apply;
                return;
            }

            _running = true;
            try
            {
                EconomyResourceJsonSyncResult result = EconomyResourceJsonCatalogService.SyncFromJson();
                Debug.Log($"[MoyvaEconomyJson] Auto-sync {result.Summary}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[MoyvaEconomyJson] Auto-sync failed: {exception}");
            }
            finally
            {
                _running = false;
            }
        }

        private static bool ContainsCatalog(string[] paths)
        {
            if (paths == null) return false;
            for (int index = 0; index < paths.Length; index++)
                if (string.Equals(paths[index], EconomyResourceJsonCatalogService.JsonPath, StringComparison.Ordinal)) return true;
            return false;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class EconomyResourcesJsonDocument
    {
        [JsonProperty("$schema", Order = 0)] public string JsonSchema = "../Schemas/economy-resources.schema.json";
        [JsonProperty("schema", Order = 1)] public string Schema = "moyva.economy-resources";
        [JsonProperty("version", Order = 2)] public int Version = 1;
        [JsonProperty("databaseAsset", Order = 3)] public string DatabaseAsset = "Assets/Moyva/Data/ScriptableObjects/Economy/EconomyDatabase.asset";
        [JsonProperty("resources", Order = 4)] public List<EconomyResourceJsonDefinition> Resources = new List<EconomyResourceJsonDefinition>();
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class EconomyResourceJsonDefinition
    {
        [JsonProperty("asset")] public string Asset;
        [JsonProperty("id")] public string Id;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("category")] public string Category;
        [JsonProperty("icon")] public MoyvaJsonAssetReference Icon;
        [JsonProperty("stackLimit")] public int StackLimit;
        [JsonProperty("weightGrams")] public int WeightGrams = 1000;
    }
}

#endif
