using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Editor.Shared
{
    /// <summary>
    /// Deterministic editor-only renderer for gameplay catalogue icons.
    /// JSON remains the authoring source; the ScriptableObjects are updated only
    /// so the result is immediately visible before the next JSON sync.
    /// </summary>
    public static class DeterministicPrefabIconBaker
    {
        public const int IconSize = 512;
        public const string GeneratedRoot = "Assets/Moyva/Art/UI/Icons/Generated";
        public const string BuildingRoot = GeneratedRoot + "/Buildings";
        public const string UnitRoot = GeneratedRoot + "/Units";
        public const string ManifestPath = GeneratedRoot + "/icon-manifest.json";

        private const string UnitJsonPath = "Assets/Moyva/Presets/Units/unit-registry.json";
        private const string BuildingJsonRoot = "Assets/Moyva/Presets/Buildings";
        private const string RenderSettingsVersion = "orthographic-512-three-quarter-v1";

        [MenuItem("Moyva/Content/Icons/Bake Buildings")]
        public static void BakeBuildingsFromMenu()
        {
            Bake(buildings: true, units: false, replaceGenerated: false);
        }

        [MenuItem("Moyva/Content/Icons/Bake Units")]
        public static void BakeUnitsFromMenu()
        {
            Bake(buildings: false, units: true, replaceGenerated: false);
        }

        [MenuItem("Moyva/Content/Icons/Bake All")]
        public static void BakeAllFromMenu()
        {
            Bake(buildings: true, units: true, replaceGenerated: false);
        }

        [MenuItem("Moyva/Content/Icons/Rebake All Generated")]
        public static void RebakeAllFromMenu()
        {
            Bake(buildings: true, units: true, replaceGenerated: true);
        }

        [MenuItem("Moyva/Content/Icons/Validate Generated Icons")]
        public static void ValidateFromMenu()
        {
            IconBakeManifest manifest = LoadManifest();
            var errors = new List<string>();

            ValidateBuildings(manifest, errors);
            ValidateUnits(manifest, errors);

            if (errors.Count > 0)
                throw new InvalidOperationException("Generated icon validation failed:\n- " + string.Join("\n- ", errors));

            Debug.Log($"[MoyvaIconBake] VALIDATION_OK entries={manifest.Entries.Count}, size={IconSize}.");
        }

        public static IconBakeResult Bake(bool buildings, bool units, bool replaceGenerated)
        {
            EnsureAssetFolder(GeneratedRoot);
            IconBakeManifest manifest = LoadManifest();
            int rendered = 0;
            int reused = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                if (buildings)
                    BakeBuildings(manifest, replaceGenerated, ref rendered, ref reused);
                if (units)
                    BakeUnits(manifest, replaceGenerated, ref rendered, ref reused);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssignGeneratedSprites(buildings, units);
            WriteManifest(manifest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var result = new IconBakeResult
            {
                Rendered = rendered,
                Reused = reused,
                Total = rendered + reused,
            };
            Debug.Log($"[MoyvaIconBake] COMPLETE rendered={rendered}, reused={reused}, total={result.Total}.");
            return result;
        }

        private static void BakeBuildings(
            IconBakeManifest manifest,
            bool replaceGenerated,
            ref int rendered,
            ref int reused)
        {
            EnsureAssetFolder(BuildingRoot);
            string[] guids = AssetDatabase.FindAssets("t:BuildingDefinitionAsset", new[] { "Assets/Moyva" });
            Array.Sort(guids, StringComparer.Ordinal);
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BuildingDefinitionAsset definition = AssetDatabase.LoadAssetAtPath<BuildingDefinitionAsset>(assetPath);
                if (definition == null || definition.Presentation?.Prefab == null || string.IsNullOrWhiteSpace(definition.Id))
                    continue;

                string id = SanitizeFileName(definition.Id);
                string outputPath = $"{BuildingRoot}/{id}.png";
                BakeOne("building", definition.Id, definition.Presentation.Prefab, outputPath, manifest, replaceGenerated, ref rendered, ref reused);
            }
        }

        private static void BakeUnits(
            IconBakeManifest manifest,
            bool replaceGenerated,
            ref int rendered,
            ref int reused)
        {
            EnsureAssetFolder(UnitRoot);
            string[] guids = AssetDatabase.FindAssets("t:UnitRegistrySO", new[] { "Assets/Moyva" });
            if (guids.Length != 1)
                throw new InvalidOperationException($"Expected one UnitRegistrySO, found {guids.Length}.");

            UnitRegistrySO registry = AssetDatabase.LoadAssetAtPath<UnitRegistrySO>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (registry?.Configs == null)
                return;

            foreach (var config in registry.Configs)
            {
                if (config?.Prefab == null || string.IsNullOrWhiteSpace(config.TypeId))
                    continue;

                string id = SanitizeFileName(config.TypeId);
                string outputPath = $"{UnitRoot}/{id}.png";
                BakeOne("unit", config.TypeId, config.Prefab, outputPath, manifest, replaceGenerated, ref rendered, ref reused);
            }
        }

        private static void BakeOne(
            string scope,
            string id,
            GameObject prefab,
            string outputPath,
            IconBakeManifest manifest,
            bool replaceGenerated,
            ref int rendered,
            ref int reused)
        {
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
            string dependencyHash = AssetDatabase.GetAssetDependencyHash(prefabPath).ToString();
            IconBakeEntry existing = manifest.Find(scope, id);
            bool isCurrent = existing != null
                             && existing.SourcePrefabGuid == prefabGuid
                             && existing.DependencyHash == dependencyHash
                             && existing.RenderSettings == RenderSettingsVersion
                             && existing.OutputPath == outputPath
                             && File.Exists(ToFullPath(outputPath));
            if (isCurrent && !replaceGenerated)
            {
                reused++;
                return;
            }

            Texture2D texture = Render(prefab);
            try
            {
                File.WriteAllBytes(ToFullPath(outputPath), texture.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }

            manifest.Upsert(new IconBakeEntry
            {
                Scope = scope,
                Id = id,
                SourcePrefabGuid = prefabGuid,
                DependencyHash = dependencyHash,
                OutputPath = outputPath,
                RenderSettings = RenderSettingsVersion,
                Width = IconSize,
                Height = IconSize,
            });
            rendered++;
        }

        private static Texture2D Render(GameObject prefab)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                GameObject instance = preview.InstantiatePrefabInScene(prefab);
                Bounds bounds = CalculateBounds(instance);
                Vector3 center = bounds.center;
                float radius = Mathf.Max(0.25f, bounds.extents.magnitude);
                Vector3 viewDirection = new Vector3(1f, 0.72f, -1f).normalized;

                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = Color.clear;
                preview.camera.orthographic = true;
                preview.camera.orthographicSize = Mathf.Max(bounds.extents.y, radius * 0.72f) / 0.9f;
                preview.camera.nearClipPlane = 0.01f;
                preview.camera.farClipPlane = radius * 8f + 10f;
                preview.camera.transform.position = center - viewDirection * (radius * 3f + 2f);
                preview.camera.transform.rotation = Quaternion.LookRotation(center - preview.camera.transform.position, Vector3.up);
                preview.camera.allowHDR = false;
                preview.camera.allowMSAA = true;

                preview.lights[0].intensity = 1.2f;
                preview.lights[0].color = new Color(1f, 0.91f, 0.76f);
                preview.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
                preview.lights[1].intensity = 0.55f;
                preview.lights[1].color = new Color(0.68f, 0.78f, 1f);
                preview.lights[1].transform.rotation = Quaternion.Euler(340f, 215f, 0f);
                preview.ambientColor = new Color(0.32f, 0.34f, 0.37f);

                var rect = new Rect(0f, 0f, IconSize, IconSize);
                preview.BeginStaticPreview(rect);
                preview.camera.Render();
                Texture2D result = preview.EndStaticPreview();
                if (result == null)
                    throw new InvalidOperationException($"Unity returned no preview for '{prefab.name}'.");
                return result;
            }
            finally
            {
                preview.Cleanup();
            }
        }

        private static Bounds CalculateBounds(GameObject instance)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(instance.transform.position, Vector3.one);

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
                bounds.Encapsulate(renderers[index].bounds);
            return bounds;
        }

        private static void AssignGeneratedSprites(bool buildings, bool units)
        {
            if (buildings)
                AssignBuildingSprites();
            if (units)
                AssignUnitSprites();
        }

        private static void AssignBuildingSprites()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildingDefinitionAsset", new[] { "Assets/Moyva" });
            foreach (string guid in guids)
            {
                BuildingDefinitionAsset definition = AssetDatabase.LoadAssetAtPath<BuildingDefinitionAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    continue;

                string iconPath = $"{BuildingRoot}/{SanitizeFileName(definition.Id)}.png";
                Sprite sprite = ImportSprite(iconPath);
                if (sprite == null)
                    continue;

                var serialized = new SerializedObject(definition);
                SerializedProperty icon = serialized.FindProperty("Presentation.Icon");
                SerializedProperty runtimePreview = serialized.FindProperty("Presentation.RuntimePreview");
                if (icon != null)
                    icon.objectReferenceValue = sprite;
                if (runtimePreview != null)
                    runtimePreview.objectReferenceValue = sprite;
                serialized.ApplyModifiedProperties();
                UpdateBuildingJsonIcon(definition.Id, iconPath);
            }
        }

        private static void AssignUnitSprites()
        {
            string[] guids = AssetDatabase.FindAssets("t:UnitRegistrySO", new[] { "Assets/Moyva" });
            if (guids.Length != 1)
                return;

            UnitRegistrySO registry = AssetDatabase.LoadAssetAtPath<UnitRegistrySO>(AssetDatabase.GUIDToAssetPath(guids[0]));
            var serialized = new SerializedObject(registry);
            SerializedProperty configs = serialized.FindProperty("Configs");
            JObject root = JObject.Parse(File.ReadAllText(ToFullPath(UnitJsonPath), Encoding.UTF8));
            JArray units = (JArray)root["units"];

            for (int index = 0; index < configs.arraySize; index++)
            {
                SerializedProperty config = configs.GetArrayElementAtIndex(index);
                string typeId = config.FindPropertyRelative("TypeId")?.stringValue;
                if (string.IsNullOrWhiteSpace(typeId))
                    continue;

                string iconPath = $"{UnitRoot}/{SanitizeFileName(typeId)}.png";
                Sprite sprite = ImportSprite(iconPath);
                if (sprite == null)
                    continue;

                config.FindPropertyRelative("CustomSprite").objectReferenceValue = sprite;
                JObject unit = FindUnitJson(units, typeId);
                if (unit != null)
                    unit["presentation"]["customSprite"] = JObject.FromObject(MoyvaJsonAssetReferenceResolver.FromObject(sprite));
            }

            serialized.ApplyModifiedProperties();
            WriteJson(UnitJsonPath, root);
        }

        private static Sprite ImportSprite(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return null;

            bool changed = importer.textureType != TextureImporterType.Sprite
                           || importer.mipmapEnabled
                           || importer.alphaSource != TextureImporterAlphaSource.FromInput
                           || importer.wrapMode != TextureWrapMode.Clamp;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            if (changed)
                importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void UpdateBuildingJsonIcon(string id, string iconPath)
        {
            string jsonPath = $"{BuildingJsonRoot}/{id}.json";
            string fullPath = ToFullPath(jsonPath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Building JSON not found for '{id}'.", jsonPath);

            JObject root = JObject.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
            JObject presentation = (JObject)(root["presentation"] ??= new JObject());
            presentation["icon"] = new JObject
            {
                ["path"] = iconPath,
                ["required"] = true,
            };
            WriteJson(jsonPath, root);
        }

        private static JObject FindUnitJson(JArray units, string typeId)
        {
            if (units == null)
                return null;
            foreach (JToken token in units)
            {
                if (token is JObject unit
                    && string.Equals((string)unit["typeId"], typeId, StringComparison.Ordinal))
                    return unit;
            }
            return null;
        }

        private static void ValidateBuildings(IconBakeManifest manifest, List<string> errors)
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildingDefinitionAsset", new[] { "Assets/Moyva" });
            foreach (string guid in guids)
            {
                BuildingDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<BuildingDefinitionAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset == null || string.IsNullOrWhiteSpace(asset.Id))
                    continue;
                ValidateEntry("building", asset.Id, asset.Presentation?.Prefab, asset.Presentation?.Icon, manifest, errors);
            }
        }

        private static void ValidateUnits(IconBakeManifest manifest, List<string> errors)
        {
            string[] guids = AssetDatabase.FindAssets("t:UnitRegistrySO", new[] { "Assets/Moyva" });
            if (guids.Length != 1)
            {
                errors.Add($"Expected one UnitRegistrySO, found {guids.Length}.");
                return;
            }
            UnitRegistrySO registry = AssetDatabase.LoadAssetAtPath<UnitRegistrySO>(AssetDatabase.GUIDToAssetPath(guids[0]));
            foreach (var config in registry.Configs)
                ValidateEntry("unit", config.TypeId, config.Prefab, config.CustomSprite, manifest, errors);
        }

        private static void ValidateEntry(
            string scope,
            string id,
            GameObject prefab,
            Sprite sprite,
            IconBakeManifest manifest,
            List<string> errors)
        {
            if (prefab == null)
            {
                errors.Add($"{scope}:{id} has no prefab.");
                return;
            }
            IconBakeEntry entry = manifest.Find(scope, id);
            if (entry == null)
            {
                errors.Add($"{scope}:{id} has no manifest entry.");
                return;
            }
            if (sprite == null || AssetDatabase.GetAssetPath(sprite) != entry.OutputPath)
                errors.Add($"{scope}:{id} does not reference '{entry.OutputPath}'.");
            if (entry.Width != IconSize || entry.Height != IconSize || !File.Exists(ToFullPath(entry.OutputPath)))
                errors.Add($"{scope}:{id} output is missing or has invalid dimensions.");
            if (sprite != null
                && (sprite.texture == null
                    || sprite.texture.width != IconSize
                    || sprite.texture.height != IconSize))
            {
                errors.Add($"{scope}:{id} imported texture is not {IconSize}x{IconSize}.");
            }

            if (AssetImporter.GetAtPath(entry.OutputPath) is TextureImporter importer
                && !importer.DoesSourceTextureHaveAlpha())
            {
                errors.Add($"{scope}:{id} source PNG has no alpha channel.");
            }
        }

        private static IconBakeManifest LoadManifest()
        {
            string fullPath = ToFullPath(ManifestPath);
            if (!File.Exists(fullPath))
                return new IconBakeManifest();
            return JsonConvert.DeserializeObject<IconBakeManifest>(File.ReadAllText(fullPath, Encoding.UTF8))
                   ?? new IconBakeManifest();
        }

        private static void WriteManifest(IconBakeManifest manifest)
        {
            manifest.Schema = "moyva.generated-icons";
            manifest.Version = 1;
            manifest.RenderSettings = RenderSettingsVersion;
            manifest.Entries.Sort((left, right) =>
            {
                int scope = string.CompareOrdinal(left.Scope, right.Scope);
                return scope != 0 ? scope : string.CompareOrdinal(left.Id, right.Id);
            });
            WriteJson(ManifestPath, JToken.FromObject(manifest));
        }

        private static void WriteJson(string assetPath, JToken value)
        {
            File.WriteAllText(ToFullPath(assetPath), value.ToString(Formatting.Indented) + Environment.NewLine, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            string current = "Assets";
            string[] parts = assetPath.Split('/');
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }

        private static string SanitizeFileName(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (char character in value)
                builder.Append(Array.IndexOf(Path.GetInvalidFileNameChars(), character) >= 0 ? '-' : char.ToLowerInvariant(character));
            return builder.ToString();
        }

        private static string ToFullPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                                 ?? throw new InvalidOperationException("Cannot resolve project root.");
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }
    }

    public sealed class IconBakeResult
    {
        public int Rendered;
        public int Reused;
        public int Total;
    }

    [Serializable]
    public sealed class IconBakeManifest
    {
        [JsonProperty("schema")] public string Schema = "moyva.generated-icons";
        [JsonProperty("version")] public int Version = 1;
        [JsonProperty("renderSettings")] public string RenderSettings;
        [JsonProperty("entries")] public List<IconBakeEntry> Entries = new List<IconBakeEntry>();

        public IconBakeEntry Find(string scope, string id)
        {
            return Entries.Find(entry => string.Equals(entry.Scope, scope, StringComparison.Ordinal)
                                         && string.Equals(entry.Id, id, StringComparison.Ordinal));
        }

        public void Upsert(IconBakeEntry value)
        {
            IconBakeEntry current = Find(value.Scope, value.Id);
            if (current != null)
                Entries.Remove(current);
            Entries.Add(value);
        }
    }

    [Serializable]
    public sealed class IconBakeEntry
    {
        [JsonProperty("scope")] public string Scope;
        [JsonProperty("id")] public string Id;
        [JsonProperty("sourcePrefabGuid")] public string SourcePrefabGuid;
        [JsonProperty("dependencyHash")] public string DependencyHash;
        [JsonProperty("outputPath")] public string OutputPath;
        [JsonProperty("renderSettings")] public string RenderSettings;
        [JsonProperty("width")] public int Width;
        [JsonProperty("height")] public int Height;
    }
}
