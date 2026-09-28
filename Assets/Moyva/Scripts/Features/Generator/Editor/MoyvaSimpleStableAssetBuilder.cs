using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Builds the simple-stable-v1 decoration assets: one wrapper prefab per
    /// allowlisted source under Assets/Moyva/Prefabs/Environment/SimpleStable,
    /// the shared SW_* fallback materials, and matching "sw-*" definitions in
    /// the map-object registry JSON so the profile decoration config's asset
    /// pools resolve through IMapObjectRegistryService.
    ///
    /// Wrappers keep the source prefab as a nested instance, so source meshes
    /// and materials stay authoritative; the wrapper only provides the stable
    /// sw-* identity and a material fallback where a source renderer is
    /// unassigned.
    ///
    /// Batch entry:
    ///   -executeMethod Kruty1918.Moyva.Generator.Editor.MoyvaSimpleStableAssetBuilder.Build
    /// </summary>
    internal static class MoyvaSimpleStableAssetBuilder
    {
        private const string WrapperFolder = "Assets/Moyva/Prefabs/Environment/SimpleStable";
        private const string MaterialFolder = "Assets/Moyva/Art/Materials";
        private const string RegistryJsonPath =
            "Assets/Moyva/Presets/Generator/map-object-registry/mapobjectregistry.json";
        private const string ReportPath = "Temp/ai/simple-stable-assets.txt";

        private sealed class WrapperSpec
        {
            public string Id;
            public string SourceRegistryId;
            public string FallbackMaterialName;
        }

        private static readonly WrapperSpec[] Specs =
        {
            new WrapperSpec { Id = "sw-tree-single-a",  SourceRegistryId = "kaykit-tree-single-a",  FallbackMaterialName = "SW_Wood" },
            new WrapperSpec { Id = "sw-tree-single-b",  SourceRegistryId = "kaykit-tree-single-b",  FallbackMaterialName = "SW_Wood" },
            new WrapperSpec { Id = "sw-rock-single-a",  SourceRegistryId = "kaykit-rock-single-a",  FallbackMaterialName = "SW_Stone" },
            new WrapperSpec { Id = "sw-rock-single-c",  SourceRegistryId = "kaykit-rock-single-c",  FallbackMaterialName = "SW_Stone" },
            new WrapperSpec { Id = "sw-rock-single-e",  SourceRegistryId = "kaykit-rock-single-e",  FallbackMaterialName = "SW_Stone" },
            new WrapperSpec { Id = "sw-grass-cross-a",  SourceRegistryId = "veg-grass-cross-a",     FallbackMaterialName = "SW_Foliage" },
            new WrapperSpec { Id = "sw-grass-tri-a",    SourceRegistryId = "veg-grass-tri-a",       FallbackMaterialName = "SW_Foliage" },
            new WrapperSpec { Id = "sw-grass-tall-a",   SourceRegistryId = "veg-grass-tall-a",      FallbackMaterialName = "SW_Foliage" },
            new WrapperSpec { Id = "sw-fern-a",         SourceRegistryId = "veg-fern-a",            FallbackMaterialName = "SW_Foliage" },
            new WrapperSpec { Id = "sw-flower-a",       SourceRegistryId = "veg-flower-a",          FallbackMaterialName = "SW_Foliage" },
            new WrapperSpec { Id = "sw-flower-b",       SourceRegistryId = "veg-flower-b",          FallbackMaterialName = "SW_Foliage" },
            new WrapperSpec { Id = "sw-sedge-a",        SourceRegistryId = "veg-sedge-a",           FallbackMaterialName = "SW_Foliage" },
            new WrapperSpec { Id = "sw-waterlily-a",    SourceRegistryId = "kaykit-waterlily-a",    FallbackMaterialName = "SW_Foliage" },
        };

        [MenuItem("Tools/Moyva/Simple Stable/Build Decoration Wrappers")]
        public static void BuildMenu()
        {
            Build();
        }

        /// <summary>Batch-safe entry; exits non-zero on any failure.</summary>
        public static void Build()
        {
            try
            {
                var report = new StringBuilder();
                EnsureFolder(WrapperFolder);
                EnsureFolder(MaterialFolder);

                var registry = JObject.Parse(File.ReadAllText(RegistryJsonPath));
                var definitions = registry["definitions"] as JArray;
                if (definitions == null)
                    throw new InvalidOperationException(
                        $"{RegistryJsonPath}: no 'definitions' array.");

                var existingIds = new HashSet<string>(StringComparer.Ordinal);
                var sourcePaths = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (JObject entry in definitions.Children<JObject>())
                {
                    string id = entry.Value<string>("id");
                    if (!string.IsNullOrEmpty(id))
                        existingIds.Add(id);
                    string editorPath = entry["visualPrefab"]?.Value<string>("editorPath");
                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(editorPath))
                        sourcePaths[id] = editorPath;
                }

                EnsureFallbackMaterials();

                int created = 0;
                int updated = 0;
                int registryAdded = 0;
                foreach (WrapperSpec spec in Specs)
                {
                    report.Append(spec.Id).Append(": ");
                    if (!sourcePaths.TryGetValue(spec.SourceRegistryId, out string sourcePath))
                    {
                        report.AppendLine($"FAILED - source '{spec.SourceRegistryId}' missing from registry");
                        throw new InvalidOperationException(
                            $"Source registry id '{spec.SourceRegistryId}' not found.");
                    }

                    GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                    if (source == null)
                    {
                        report.AppendLine($"FAILED - source prefab missing at {sourcePath}");
                        throw new InvalidOperationException($"Missing source prefab: {sourcePath}");
                    }

                    string wrapperPath = $"{WrapperFolder}/{spec.Id}.prefab";
                    bool existed = AssetDatabase.LoadAssetAtPath<GameObject>(wrapperPath) != null;
                    string wrapperGuid = BuildWrapper(source, spec, wrapperPath, report);
                    if (existed) updated++; else created++;

                    if (!existingIds.Contains(spec.Id))
                    {
                        definitions.Add(new JObject
                        {
                            ["id"] = spec.Id,
                            ["visualPrefab"] = new JObject
                            {
                                ["$asset"] = $"asset.game-object.{spec.Id}.{wrapperGuid}",
                                ["editorPath"] = wrapperPath,
                                ["required"] = true,
                            },
                        });
                        existingIds.Add(spec.Id);
                        registryAdded++;
                    }
                    else
                    {
                        // Keep the key in sync with the on-disk prefab GUID.
                        foreach (JObject entry in definitions.Children<JObject>())
                        {
                            if (!string.Equals(entry.Value<string>("id"), spec.Id, StringComparison.Ordinal))
                                continue;
                            entry["visualPrefab"]["$asset"] = $"asset.game-object.{spec.Id}.{wrapperGuid}";
                            entry["visualPrefab"]["editorPath"] = wrapperPath;
                        }
                    }

                    report.AppendLine();
                }

                WriteRegistry(registry);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // The new sw-* $asset references only resolve at runtime once
                // the catalog prefab and Resources copies include them.
                string catalogReport = Kruty1918.Moyva.Jsonization.Editor
                    .JsonizationExportService.BuildAssetCatalog(
                        "Temp/ai/simple-stable-catalog.json");
                string syncReport = Kruty1918.Moyva.Jsonization.Editor
                    .JsonizationExportService.SyncGeneratedResources(
                        "Temp/ai/simple-stable-sync.json");
                report.AppendLine(catalogReport).AppendLine(syncReport);

                report.AppendLine(
                    $"wrappers created={created} updated={updated} registryAdded={registryAdded}");
                WriteReport(report.ToString());
                Debug.Log($"[SimpleStableAssets] {report}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SimpleStableAssets] Build failed: {ex}");
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                else
                    throw;
            }
        }

        /// <summary>
        /// Validation-only pass (Step A): every allowlisted source prefab and
        /// every sw-* wrapper must load with at least one valid mesh renderer.
        /// </summary>
        [MenuItem("Tools/Moyva/Simple Stable/Validate Allowlist Assets")]
        public static void Validate()
        {
            var report = new StringBuilder();
            int failures = 0;
            foreach (WrapperSpec spec in Specs)
            {
                string wrapperPath = $"{WrapperFolder}/{spec.Id}.prefab";
                GameObject wrapper = AssetDatabase.LoadAssetAtPath<GameObject>(wrapperPath);
                if (wrapper == null)
                {
                    report.AppendLine($"{spec.Id}: MISSING {wrapperPath}");
                    failures++;
                    continue;
                }

                int meshRenderers = 0;
                int missingMaterials = 0;
                foreach (var renderer in wrapper.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null || filter.sharedMesh.vertexCount == 0)
                    {
                        failures++;
                        report.AppendLine($"{spec.Id}: renderer without mesh on {renderer.name}");
                        continue;
                    }
                    meshRenderers++;
                    foreach (Material material in renderer.sharedMaterials)
                        if (material == null)
                            missingMaterials++;
                }

                if (missingMaterials > 0)
                {
                    failures++;
                    report.AppendLine($"{spec.Id}: {missingMaterials} null material slot(s)");
                }

                report.AppendLine($"{spec.Id}: ok renderers={meshRenderers}");
            }

            report.AppendLine($"validate failures={failures}");
            WriteReport(report.ToString());
            Debug.Log($"[SimpleStableAssets] Validation:\n{report}");
            if (failures > 0 && Application.isBatchMode)
                EditorApplication.Exit(1);
        }

        private static string BuildWrapper(
            GameObject source,
            WrapperSpec spec,
            string wrapperPath,
            StringBuilder report)
        {
            var root = new GameObject(spec.Id);
            try
            {
                var child = (GameObject)PrefabUtility.InstantiatePrefab(source);
                child.name = source.name;
                child.transform.SetParent(root.transform, false);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;

                int renderers = 0;
                int fallbacks = 0;
                Material fallback = AssetDatabase.LoadAssetAtPath<Material>(
                    $"{MaterialFolder}/{spec.FallbackMaterialName}.mat");
                foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    renderers++;
                    var shared = renderer.sharedMaterials;
                    for (int i = 0; i < shared.Length; i++)
                    {
                        if (shared[i] != null)
                            continue;
                        shared[i] = fallback;
                        fallbacks++;
                    }
                    renderer.sharedMaterials = shared;
                }

                report.Append($"source={source.name} renderers={renderers} fallbacks={fallbacks} ");
                PrefabUtility.SaveAsPrefabAsset(root, wrapperPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            string guid = AssetDatabase.AssetPathToGUID(wrapperPath);
            return guid.Substring(0, Math.Min(8, guid.Length));
        }

        private static void EnsureFallbackMaterials()
        {
            EnsureMaterial("SW_Foliage", new Color(0.32f, 0.55f, 0.24f, 1f));
            EnsureMaterial("SW_Wood", new Color(0.45f, 0.32f, 0.2f, 1f));
            EnsureMaterial("SW_Stone", new Color(0.55f, 0.55f, 0.58f, 1f));
        }

        private static void EnsureMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
                return;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            AssetDatabase.CreateAsset(material, path);
        }

        private static void WriteRegistry(JObject registry)
        {
            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                // Match the checked-in file's one-space indent so the diff only
                // shows the appended definitions.
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 1;
                writer.IndentChar = ' ';
                registry.WriteTo(writer);
            }
            File.WriteAllText(RegistryJsonPath, sb.ToString() + "\n");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void WriteReport(string text)
        {
            string full = Path.Combine(Directory.GetCurrentDirectory(), ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, text);
        }
    }
}
