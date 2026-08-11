using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    internal static class JsonizationEditorUtil
    {
        internal const string MoyvaNamespace = "Kruty1918.Moyva";
        internal const string PresetsRoot = "Assets/Moyva/Presets";
        internal const string SchemasRoot = "Assets/Moyva/Presets/Schemas";
        internal const string GeneratedResourcesRoot = "Assets/Moyva/Resources/MoyvaConfigGenerated";
        internal const string CatalogPath = "Assets/Moyva/Resources/MoyvaRuntimeAssetCatalog.prefab";

        internal static bool IsProjectOwned(Type type)
        {
            if (type == null) return false;
            string ns = type.Namespace ?? string.Empty;
            return ns.StartsWith(MoyvaNamespace, StringComparison.Ordinal) &&
                   !ns.StartsWith("Kruty1918.Moyva.Jsonization", StringComparison.Ordinal);
        }

        internal static bool IsProjectConfigType(Type type)
        {
            if (type == null || !IsProjectOwned(type)) return false;
            return typeof(ScriptableObject).IsAssignableFrom(type) ||
                   typeof(MoyvaJsonConfigObject).IsAssignableFrom(type);
        }

        internal static string StableId(UnityEngine.Object obj, Type type = null)
        {
            if (obj == null) return string.Empty;
            type ??= obj.GetType();
            string[] memberNames = { "Id", "TypeId", "NodeId", "Guid", "ID" };
            foreach (string memberName in memberNames)
            {
                var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.PropertyType == typeof(string) && property.GetIndexParameters().Length == 0)
                {
                    try
                    {
                        string value = property.GetValue(obj) as string;
                        if (!string.IsNullOrWhiteSpace(value)) return Slug(value);
                    }
                    catch { }
                }

                var field = FindField(type, memberName) ?? FindField(type, "_" + char.ToLowerInvariant(memberName[0]) + memberName.Substring(1));
                if (field?.FieldType == typeof(string))
                {
                    try
                    {
                        string value = field.GetValue(obj) as string;
                        if (!string.IsNullOrWhiteSpace(value)) return Slug(value);
                    }
                    catch { }
                }
            }

            string path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrWhiteSpace(path))
                return Slug(Path.GetFileNameWithoutExtension(path));

            return Slug(obj.name);
        }

        internal static string StableIdForPlain(object value, Type type)
        {
            if (value == null || type == null) return string.Empty;
            string[] memberNames = { "Id", "TypeId", "NodeId", "Guid", "ID" };
            foreach (string memberName in memberNames)
            {
                var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.PropertyType == typeof(string) && property.GetIndexParameters().Length == 0)
                {
                    try
                    {
                        string text = property.GetValue(value) as string;
                        if (!string.IsNullOrWhiteSpace(text)) return Slug(text);
                    }
                    catch { }
                }
                var field = FindField(type, memberName) ?? FindField(type, "_" + char.ToLowerInvariant(memberName[0]) + memberName.Substring(1));
                if (field?.FieldType == typeof(string))
                {
                    try
                    {
                        string text = field.GetValue(value) as string;
                        if (!string.IsNullOrWhiteSpace(text)) return Slug(text);
                    }
                    catch { }
                }
            }
            return string.Empty;
        }

        internal static string Slug(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "config";
            var chars = new List<char>();
            bool dash = false;
            foreach (char raw in value.Trim())
            {
                char c = char.ToLowerInvariant(raw);
                if (char.IsLetterOrDigit(c))
                {
                    chars.Add(c);
                    dash = false;
                }
                else if (!dash && chars.Count > 0)
                {
                    chars.Add('-');
                    dash = true;
                }
            }
            while (chars.Count > 0 && chars[chars.Count - 1] == '-') chars.RemoveAt(chars.Count - 1);
            return chars.Count == 0 ? "config" : new string(chars.ToArray());
        }

        internal static string SchemaName(Type type)
        {
            if (type == null) return "moyva.unknown";
            string n = type.Name;
            if (n == "BuildingDefinitionAsset") return "moyva.building";
            if (n == "UnitClassConfig") return "moyva.unit";
            if (n == "GraphAsset") return "moyva.generator-graph";
            if (n == "WorldCreationDefaultsSO") return "moyva.world-creation";
            string id = MoyvaJsonTypeRegistry.StableId(type);
            return "moyva." + (string.IsNullOrWhiteSpace(id) ? Slug(type.Name) : id);
        }

        internal static string SchemaFileName(Type type)
        {
            string schema = SchemaName(type);
            string suffix = schema.StartsWith("moyva.", StringComparison.Ordinal) ? schema.Substring(6) : schema;
            return suffix + ".schema.json";
        }

        internal static string DomainFolder(Type type)
        {
            string full = type?.FullName ?? string.Empty;
            string name = type?.Name ?? "Unknown";
            if (name == "BuildingDefinitionAsset" || full.Contains("Construction")) return "Buildings";
            if (name == "UnitClassConfig" || full.Contains(".Units.")) return "Units";
            if (name == "GraphAsset" || full.Contains("GraphSystem")) return "Graphs";
            if (full.Contains("Generator")) return "Generator";
            if (full.Contains("Economy")) return "Economy";
            if (full.Contains("WorldCreation")) return "WorldCreation";
            if (full.Contains("Grid") || full.Contains("Tile")) return "Grid";
            return "Systems";
        }

        internal static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
                type = type.BaseType;
            }
            return null;
        }

        internal static IEnumerable<FieldInfo> SerializedFields(Type type)
        {
            var seen = new HashSet<string>();
            for (Type current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                foreach (FieldInfo field in current.GetFields(
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.IsStatic || field.IsNotSerialized || !seen.Add(current.FullName + "." + field.Name)) continue;
                    if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null || field.GetCustomAttribute<SerializeReference>() != null)
                        yield return field;
                }
            }
        }

        internal static string FindScriptPath(Type type)
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Moyva" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                Type cls = null;
                try { cls = script != null ? script.GetClass() : null; } catch { }
                if (cls == type) return path;
            }
            return string.Empty;
        }

        internal static void WriteJson(string path, object value)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonConvert.SerializeObject(value, Formatting.Indented) + "\n");
        }

        internal static void WriteText(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text ?? string.Empty);
        }

        internal static string AssetKey(UnityEngine.Object asset)
        {
            if (asset == null) return string.Empty;
            string path = AssetDatabase.GetAssetPath(asset);
            string guid = string.IsNullOrWhiteSpace(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
            string typeId = MoyvaJsonTypeRegistry.StableId(asset.GetType());
            string baseName = Slug(asset.name);
            string guidPart = string.IsNullOrWhiteSpace(guid) ? "scene" : guid.Substring(0, Math.Min(8, guid.Length));
            return $"asset.{typeId}.{baseName}.{guidPart}";
        }

        internal static bool IsMainAsset(UnityEngine.Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrWhiteSpace(path)) return false;
            return AssetDatabase.LoadMainAssetAtPath(path) == obj;
        }

        internal static IEnumerable<Type> ProjectConfigTypes()
        {
            return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(IsProjectOwned)
                .OrderBy(t => t.FullName, StringComparer.Ordinal);
        }

        internal static string RelativeSchemaPath(Type type, string jsonFile)
        {
            string schema = Path.Combine(SchemasRoot, SchemaFileName(type)).Replace('\\', '/');
            string jsonDir = Path.GetDirectoryName(jsonFile)?.Replace('\\', '/') ?? PresetsRoot;
            string from = Path.GetFullPath(jsonDir);
            string to = Path.GetFullPath(schema);
            Uri fromUri = new Uri(from.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
            Uri toUri = new Uri(to);
            return Uri.UnescapeDataString(fromUri.MakeRelativeUri(toUri).ToString());
        }

        internal static string HierarchyPath(Transform transform)
        {
            if (transform == null) return string.Empty;
            var parts = new List<string>();
            while (transform != null)
            {
                parts.Add(transform.name);
                transform = transform.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
