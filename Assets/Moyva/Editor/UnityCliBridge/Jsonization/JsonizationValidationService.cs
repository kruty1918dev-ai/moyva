using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    public static class JsonizationValidationService
    {
        [Serializable] private sealed class ValidationReport
        {
            public string generatedUtc;
            public int jsonFiles;
            public int schemas;
            public int duplicateIds;
            public int unresolvedConfigRefs;
            public int unresolvedAssetRefs;
            public int deserializationFailures;
            public int projectScriptableObjectTypes;
            public int projectScriptableObjectAssets;
            public string fingerprint;
            public List<string> errors = new();
            public List<string> remainingTypes = new();
        }

        public static string ValidateAuthoring(string reportPath)
        {
            var report = new ValidationReport { generatedUtc = DateTime.UtcNow.ToString("O") };
            var byModelId = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var assetKeys = LoadCatalogKeys();
            var docs = new List<JObject>();

            foreach (string file in Directory.GetFiles(JsonizationEditorUtil.PresetsRoot, "*.json", SearchOption.AllDirectories))
            {
                string norm = file.Replace('\\','/');
                if (norm.Contains("/Schemas/")) { report.schemas++; continue; }
                JObject root;
                try { root = JObject.Parse(File.ReadAllText(file)); }
                catch (Exception ex)
                {
                    report.errors.Add($"{norm}: invalid JSON: {ex.Message}");
                    continue;
                }
                if (root.Property("schema") == null || root.Property("id") == null || root.Property("model") == null)
                    continue; // non-Moyva auxiliary JSON

                report.jsonFiles++;
                docs.Add(root);
                string schema = root.Value<string>("schema");
                string id = root.Value<string>("id");
                string model = root.Value<string>("model");
                int? version = root.Value<int?>("version");
                if (string.IsNullOrWhiteSpace(schema) ||
                    string.IsNullOrWhiteSpace(id) ||
                    string.IsNullOrWhiteSpace(model) ||
                    version is null or < 1)
                {
                    report.errors.Add($"{norm}: root metadata schema/version/id/model must be non-empty and version must be >= 1");
                    continue;
                }

                string key = model + "|" + id;
                if (!byModelId.Add(key)) report.duplicateIds++;

                string schemaRel = root.Value<string>("$schema");
                if (string.IsNullOrWhiteSpace(schemaRel))
                    report.errors.Add($"{norm}: missing $schema");
                else
                {
                    string schemaPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), schemaRel));
                    if (!File.Exists(schemaPath)) report.errors.Add($"{norm}: schema not found {schemaRel}");
                }
            }

            foreach (JObject root in docs)
            {
                foreach (JObject obj in root.DescendantsAndSelf().OfType<JObject>())
                {
                    if (obj["$config"] is JObject cref)
                    {
                        string model = cref.Value<string>("model");
                        string id = cref.Value<string>("id");
                        if (!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(id) &&
                            !byModelId.Contains(model + "|" + id))
                        {
                            report.unresolvedConfigRefs++;
                            report.errors.Add($"Unresolved config ref {model}/{id}");
                        }
                    }
                    if (obj.Property("$asset") != null)
                    {
                        string key = obj.Value<string>("$asset");
                        bool required = obj.Value<bool?>("required") ?? true;
                        if (required && !string.IsNullOrWhiteSpace(key) && !assetKeys.Contains(key))
                        {
                            report.unresolvedAssetRefs++;
                            report.errors.Add($"Unresolved required asset {key}");
                        }
                    }
                }
            }

            JsonizationEditorUtil.WriteJson(reportPath, report);
            string summary = $"MOYVA_JSON_VALIDATE files={report.jsonFiles} schemas={report.schemas} duplicates={report.duplicateIds} configRefs={report.unresolvedConfigRefs} assetRefs={report.unresolvedAssetRefs} errors={report.errors.Count}";
            if (report.errors.Count > 0 || report.duplicateIds > 0)
                throw new InvalidOperationException(summary);
            return summary;
        }

        public static string RuntimeDeserializeSmoke(string reportPath)
        {
            var report = new ValidationReport { generatedUtc = DateTime.UtcNow.ToString("O") };
            try
            {
                MoyvaJsonRuntime.ResetForExplicitReload();
                MoyvaJsonRuntime.EnsureLoaded();
                report.fingerprint = MoyvaJsonRuntime.ConfigFingerprint;

                foreach (TextAsset file in Resources.LoadAll<TextAsset>("MoyvaConfigGenerated"))
                {
                    if (file == null || string.IsNullOrWhiteSpace(file.text)) continue;
                    JObject root;
                    try { root = JObject.Parse(file.text); }
                    catch { continue; }
                    report.jsonFiles++;
                    string schema = root.Value<string>("schema");
                    string model = root.Value<string>("model");
                    string id = root.Value<string>("id");
                    if (string.IsNullOrWhiteSpace(schema) || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(id)) continue;
                    Type type = MoyvaJsonTypeRegistry.ResolveConfigModel(model, schema);
                    if (type == null)
                    {
                        report.deserializationFailures++;
                        report.errors.Add($"No type for {schema}/{model}/{id}");
                        continue;
                    }
                    try
                    {
                        object value = MoyvaJsonRuntime.Get(type, id);
                        if (value == null) throw new InvalidOperationException("returned null");
                    }
                    catch (Exception ex)
                    {
                        report.deserializationFailures++;
                        report.errors.Add($"{schema}/{id}: {ex.GetType().Name}:{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                report.deserializationFailures++;
                report.errors.Add(ex.ToString());
            }

            JsonizationEditorUtil.WriteJson(reportPath, report);
            string summary = $"MOYVA_JSON_RUNTIME_SMOKE fingerprint={report.fingerprint} failures={report.deserializationFailures}";
            if (report.deserializationFailures > 0) throw new InvalidOperationException(summary);
            return summary;
        }

        public static string FinalNoScriptableObjectsAudit(string reportPath)
        {
            var report = new ValidationReport { generatedUtc = DateTime.UtcNow.ToString("O") };
            foreach (Type type in TypeCache.GetTypesDerivedFrom<ScriptableObject>())
            {
                if (!JsonizationEditorUtil.IsProjectOwned(type)) continue;
                report.projectScriptableObjectTypes++;
                report.remainingTypes.Add(type.FullName);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Moyva" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path) || path.Contains("/Plugins/")) continue;
                UnityEngine.Object[] assets;
                try { assets = AssetDatabase.LoadAllAssetsAtPath(path); }
                catch { continue; }
                if (assets.Any(a => a != null && JsonizationEditorUtil.IsProjectOwned(a.GetType())))
                    report.projectScriptableObjectAssets++;
            }

            JsonizationEditorUtil.WriteJson(reportPath, report);
            string summary = $"MOYVA_JSON_NO_SO types={report.projectScriptableObjectTypes} assets={report.projectScriptableObjectAssets}";
            if (report.projectScriptableObjectTypes > 0 || report.projectScriptableObjectAssets > 0)
                throw new InvalidOperationException(summary + " remaining=" + string.Join(",", report.remainingTypes.Take(20)));
            return summary;
        }

        private static HashSet<string> LoadCatalogKeys()
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(JsonizationEditorUtil.CatalogPath);
            var catalog = prefab != null ? prefab.GetComponent<MoyvaJsonAssetCatalog>() : null;
            if (catalog == null) return result;
            foreach (var entry in catalog.Entries)
                if (entry != null && !string.IsNullOrWhiteSpace(entry.Key) && entry.Asset != null)
                    result.Add(entry.Key);
            return result;
        }
    }
}
