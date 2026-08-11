using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    public static class JsonizationExportService
    {
        [Serializable] private sealed class ExportedRecord
        {
            public string type;
            public string id;
            public string jsonPath;
            public string assetPath;
            public string schema;
            public string hash;
        }

        [Serializable] private sealed class ExportReport
        {
            public string generatedUtc;
            public int roots;
            public int unitDefinitions;
            public int schemas;
            public int assetReferences;
            public int errors;
            public List<ExportedRecord> exported = new();
            public List<string> errorList = new();
        }

        private sealed class ExportContext
        {
            public string RootAssetPath;
            public readonly Dictionary<string, UnityEngine.Object> Assets =
                new(StringComparer.OrdinalIgnoreCase);
            public readonly HashSet<object> Stack = new(ReferenceEqualityComparer.Instance);
        }


        private static Type[] _projectOwnedConcreteTypes;
        private static readonly Dictionary<Type, string[]> ConcreteIdsCache =
            new();
        private static readonly Dictionary<Type, string[]> DerivedIdsCache =
            new();

        private static Type[] ProjectOwnedConcreteTypes()
        {
            if (_projectOwnedConcreteTypes != null)
                return _projectOwnedConcreteTypes;

            _projectOwnedConcreteTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(SafeTypes)
                .Where(t =>
                    t != null &&
                    !t.IsAbstract &&
                    JsonizationEditorUtil.IsProjectOwned(t))
                .Distinct()
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToArray();

            return _projectOwnedConcreteTypes;
        }

        public static string GenerateSchemasAndExport(string reportPath)
        {
            var report = new ExportReport { generatedUtc = DateTime.UtcNow.ToString("O") };
            Directory.CreateDirectory(JsonizationEditorUtil.PresetsRoot);
            Directory.CreateDirectory(JsonizationEditorUtil.SchemasRoot);

            Type[] rootTypes = JsonizationEditorUtil.ProjectConfigTypes().ToArray();
            foreach (Type type in rootTypes)
            {
                try
                {
                    JObject schema = BuildRootSchema(type);
                    string schemaPath = Path.Combine(
                        JsonizationEditorUtil.SchemasRoot,
                        JsonizationEditorUtil.SchemaFileName(type)).Replace('\\', '/');
                    File.WriteAllText(schemaPath, schema.ToString(Formatting.Indented) + "\n");
                    report.schemas++;
                }
                catch (Exception ex)
                {
                    report.errors++;
                    report.errorList.Add($"SCHEMA {type.FullName}: {ex.GetType().Name}:{ex.Message}");
                }
            }

            // UnitClassConfig is plain already, but is a first-class JSON definition domain.
            Type unitConfigType = ResolveType("Kruty1918.Moyva.Units.API.UnitClassConfig");
            if (unitConfigType != null)
            {
                try
                {
                    JObject schema = BuildRootSchema(unitConfigType);
                    string schemaPath = Path.Combine(JsonizationEditorUtil.SchemasRoot, "unit.schema.json").Replace('\\', '/');
                    File.WriteAllText(schemaPath, schema.ToString(Formatting.Indented) + "\n");
                    report.schemas++;
                }
                catch (Exception ex)
                {
                    report.errors++;
                    report.errorList.Add($"SCHEMA UnitClassConfig: {ex.GetType().Name}:{ex.Message}");
                }
            }

            string[] assetGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Moyva" });
            var exportedMainAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path) || path.Contains("/Plugins/")) continue;
                UnityEngine.Object main = AssetDatabase.LoadMainAssetAtPath(path);
                if (main == null || !JsonizationEditorUtil.IsProjectConfigType(main.GetType())) continue;
                if (!exportedMainAssets.Add(path)) continue;

                try
                {
                    ExportRoot(main, path, report);
                    report.roots++;

                    if (main.GetType().Name == "UnitRegistrySO")
                        report.unitDefinitions += ExportUnitsFromRegistry(main, report);
                }
                catch (Exception ex)
                {
                    report.errors++;
                    report.errorList.Add($"EXPORT {path}: {ex.GetType().Name}:{ex.Message}");
                }
            }

            // Schema for all exported plain UnitClassConfig files is already generated.
            // v5: do not refresh/import here. The exporter only writes JSON;
            // triggering AssetDatabase.Refresh from Pipeline can disconnect the
            // Pipeline server and freeze the interactive Editor.
            report.assetReferences = CollectAllAssetReferences(report).Count;
            JsonizationEditorUtil.WriteJson(reportPath, report);

            string summary =
                $"MOYVA_JSON_EXPORT roots={report.roots} units={report.unitDefinitions} " +
                $"schemas={report.schemas} assetRefs={report.assetReferences} errors={report.errors}";
            Debug.Log("[MoyvaJson] " + summary);
            if (report.errors > 0)
                throw new InvalidOperationException(summary + " — see export report.");
            return summary;
        }

        public static string BuildAssetCatalog(string reportPath)
        {
            var references = new Dictionary<string, UnityEngine.Object>(StringComparer.OrdinalIgnoreCase);
            string[] jsonFiles = Directory.Exists(JsonizationEditorUtil.PresetsRoot)
                ? Directory.GetFiles(JsonizationEditorUtil.PresetsRoot, "*.json", SearchOption.AllDirectories)
                : Array.Empty<string>();

            foreach (string jsonFile in jsonFiles)
            {
                if (jsonFile.Replace('\\','/').Contains("/Schemas/")) continue;
                JObject root;
                try { root = JObject.Parse(File.ReadAllText(jsonFile)); }
                catch { continue; }

                foreach (JObject assetRef in root.DescendantsAndSelf().OfType<JObject>()
                             .Where(o => o.Property("$asset") != null))
                {
                    string key = assetRef.Value<string>("$asset");
                    string editorPath = assetRef.Value<string>("editorPath");
                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(editorPath)) continue;
                    UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(editorPath);
                    if (asset != null) references[key] = asset;
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(JsonizationEditorUtil.CatalogPath));
            var go = new GameObject("MoyvaRuntimeAssetCatalog");
            try
            {
                var catalog = go.AddComponent<MoyvaJsonAssetCatalog>();
                catalog.ReplaceEntries(references
                    .OrderBy(x => x.Key, StringComparer.Ordinal)
                    .Select(x => new MoyvaJsonAssetCatalog.Entry { Key = x.Key, Asset = x.Value }));
                PrefabUtility.SaveAsPrefabAsset(go, JsonizationEditorUtil.CatalogPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }

            var report = new
            {
                ok = true,
                entries = references.Count,
                keys = references.Keys
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray(),
                catalog = JsonizationEditorUtil.CatalogPath,
                missing = jsonFiles.Length == 0 ? 1 : 0
            };
            JsonizationEditorUtil.WriteJson(reportPath, report);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return $"MOYVA_JSON_ASSET_CATALOG entries={references.Count}";
        }

        public static string SyncGeneratedResources(string reportPath)
        {
            if (Directory.Exists(JsonizationEditorUtil.GeneratedResourcesRoot))
                Directory.Delete(JsonizationEditorUtil.GeneratedResourcesRoot, true);
            Directory.CreateDirectory(JsonizationEditorUtil.GeneratedResourcesRoot);

            int copied = 0;
            foreach (string source in Directory.GetFiles(JsonizationEditorUtil.PresetsRoot, "*.json", SearchOption.AllDirectories))
            {
                string norm = source.Replace('\\', '/');
                if (norm.Contains("/Schemas/")) continue;
                if (norm.Contains("/MoyvaConfigGenerated/")) continue;

                JObject root;
                try { root = JObject.Parse(File.ReadAllText(source)); }
                catch { continue; }
                if (root.Property("schema") == null || root.Property("id") == null || root.Property("model") == null)
                    continue;

                string domain = JsonizationEditorUtil.Slug(root.Value<string>("schema") ?? "config");
                string id = JsonizationEditorUtil.Slug(root.Value<string>("id") ?? Path.GetFileNameWithoutExtension(source));
                string dest = Path.Combine(JsonizationEditorUtil.GeneratedResourcesRoot, $"{domain}--{id}.json");
                File.WriteAllText(dest, root.ToString(Formatting.Indented) + "\n");
                copied++;
            }

            AssetDatabase.Refresh();
            JsonizationEditorUtil.WriteJson(reportPath, new
            {
                ok = copied > 0,
                copied,
                destination = JsonizationEditorUtil.GeneratedResourcesRoot
            });
            if (copied == 0)
                throw new InvalidOperationException("No canonical JSON files were copied to runtime Resources.");
            return $"MOYVA_JSON_RUNTIME_SYNC files={copied}";
        }

        private static void ExportRoot(UnityEngine.Object root, string assetPath, ExportReport report)
        {
            Type type = root.GetType();
            string id = JsonizationEditorUtil.StableId(root, type);
            string jsonPath = OutputPath(type, id);
            var context = new ExportContext { RootAssetPath = assetPath };
            JObject data = SerializeObjectFields(root, type, context, type);
            JObject doc = CreateRootDocument(type, id, jsonPath, assetPath, data);
            string hash = HashToken(data);
            doc["migration"] = new JObject
            {
                ["sourceAssetGuid"] = AssetDatabase.AssetPathToGUID(assetPath),
                ["sourceAssetPath"] = assetPath,
                ["sourceHash"] = hash
            };
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllText(jsonPath, doc.ToString(Formatting.Indented) + "\n");
            report.exported.Add(new ExportedRecord
            {
                type = type.FullName,
                id = id,
                jsonPath = jsonPath.Replace('\\','/'),
                assetPath = assetPath,
                schema = JsonizationEditorUtil.SchemaName(type),
                hash = hash
            });
        }

        private static int ExportUnitsFromRegistry(UnityEngine.Object registry, ExportReport report)
        {
            FieldInfo configsField = JsonizationEditorUtil.FindField(registry.GetType(), "Configs") ??
                                     JsonizationEditorUtil.FindField(registry.GetType(), "_configs");
            if (configsField == null) return 0;
            if (!(configsField.GetValue(registry) is IEnumerable configs)) return 0;
            int count = 0;
            foreach (object config in configs)
            {
                if (config == null) continue;
                Type type = config.GetType();
                string id = JsonizationEditorUtil.StableIdForPlain(config, type);
                if (string.IsNullOrWhiteSpace(id)) id = "unit-" + count;
                string jsonPath = Path.Combine(JsonizationEditorUtil.PresetsRoot, "Units", id + ".json").Replace('\\','/');
                var context = new ExportContext { RootAssetPath = AssetDatabase.GetAssetPath(registry) };
                JObject data = SerializeObjectFields(config, type, context, type);
                JObject doc = CreateRootDocument(type, id, jsonPath, AssetDatabase.GetAssetPath(registry), data);
                string hash = HashToken(data);
                doc["migration"] = new JObject
                {
                    ["sourceRegistry"] = AssetDatabase.GetAssetPath(registry),
                    ["sourceHash"] = hash
                };
                Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
                File.WriteAllText(jsonPath, doc.ToString(Formatting.Indented) + "\n");
                report.exported.Add(new ExportedRecord
                {
                    type = type.FullName,
                    id = id,
                    jsonPath = jsonPath,
                    assetPath = AssetDatabase.GetAssetPath(registry),
                    schema = JsonizationEditorUtil.SchemaName(type),
                    hash = hash
                });
                count++;
            }
            return count;
        }

        private static JObject CreateRootDocument(Type type, string id, string jsonPath, string sourceAssetPath, JObject data)
        {
            var doc = new JObject
            {
                ["$schema"] = JsonizationEditorUtil.RelativeSchemaPath(type, jsonPath),
                ["schema"] = JsonizationEditorUtil.SchemaName(type),
                ["version"] = 1,
                ["id"] = id,
                ["model"] = MoyvaJsonTypeRegistry.StableId(type)
            };
            foreach (JProperty property in data.Properties())
                doc[property.Name] = property.Value;
            return doc;
        }

        private static JObject SerializeObjectFields(object value, Type actualType, ExportContext context, Type declaredType)
        {
            var result = new JObject();
            if (declaredType != null && (declaredType.IsAbstract || declaredType.IsInterface || declaredType != actualType))
                result["$type"] = MoyvaJsonTypeRegistry.StableId(actualType);

            bool track = !actualType.IsValueType && !(value is string);
            if (track && !context.Stack.Add(value))
                throw new InvalidOperationException($"Inline serialization cycle at {actualType.FullName}");
            try
            {
                foreach (FieldInfo field in JsonizationEditorUtil.SerializedFields(actualType))
                {
                    if (actualType.Name == "BuildingRegistrySO" &&
                        (field.Name == "_buildingAssets" || field.Name == "Buildings"))
                        continue;
                    if (actualType.Name == "UnitRegistrySO" && field.Name == "Configs")
                        continue;
                    object fieldValue = field.GetValue(value);
                    string name = Camel(field.Name.TrimStart('_'));
                    result[name] = SerializeValue(fieldValue, field.FieldType, context);
                }
            }
            finally
            {
                if (track) context.Stack.Remove(value);
            }
            return result;
        }

        private static JToken SerializeValue(object value, Type declaredType, ExportContext context)
        {
            if (value == null) return JValue.CreateNull();
            Type actualType = value.GetType();
            Type nullable = Nullable.GetUnderlyingType(declaredType);
            if (nullable != null) declaredType = nullable;

            if (actualType.IsEnum) return new JValue(value.ToString());
            if (value is string || value is char || value is bool ||
                value is byte || value is sbyte || value is short || value is ushort ||
                value is int || value is uint || value is long || value is ulong ||
                value is float || value is double || value is decimal)
                return JToken.FromObject(value);

            if (value is Vector2 v2) return new JObject { ["x"] = v2.x, ["y"] = v2.y };
            if (value is Vector2Int v2i) return new JObject { ["x"] = v2i.x, ["y"] = v2i.y };
            if (value is Vector3 v3) return new JObject { ["x"] = v3.x, ["y"] = v3.y, ["z"] = v3.z };
            if (value is Vector3Int v3i) return new JObject { ["x"] = v3i.x, ["y"] = v3i.y, ["z"] = v3i.z };
            if (value is Vector4 v4) return new JObject { ["x"] = v4.x, ["y"] = v4.y, ["z"] = v4.z, ["w"] = v4.w };
            if (value is Quaternion q) return new JObject { ["x"] = q.x, ["y"] = q.y, ["z"] = q.z, ["w"] = q.w };
            if (value is Color c) return new JObject { ["r"] = c.r, ["g"] = c.g, ["b"] = c.b, ["a"] = c.a };
            if (value is Color32 c32) return new JObject { ["r"] = c32.r, ["g"] = c32.g, ["b"] = c32.b, ["a"] = c32.a };
            if (value is Rect rect) return new JObject { ["x"] = rect.x, ["y"] = rect.y, ["width"] = rect.width, ["height"] = rect.height };
            if (value is RectInt recti) return new JObject { ["x"] = recti.x, ["y"] = recti.y, ["width"] = recti.width, ["height"] = recti.height };
            if (value is Bounds bounds) return new JObject { ["center"] = SerializeValue(bounds.center, typeof(Vector3), context), ["size"] = SerializeValue(bounds.size, typeof(Vector3), context) };
            if (value is BoundsInt boundsi) return new JObject { ["position"] = SerializeValue(boundsi.position, typeof(Vector3Int), context), ["size"] = SerializeValue(boundsi.size, typeof(Vector3Int), context) };
            if (value is Gradient gradient)
            {
                var colors = new JArray();
                foreach (GradientColorKey k in gradient.colorKeys) colors.Add(new JObject { ["color"] = SerializeValue(k.color, typeof(Color), context), ["time"] = k.time });
                var alphas = new JArray();
                foreach (GradientAlphaKey k in gradient.alphaKeys) alphas.Add(new JObject { ["alpha"] = k.alpha, ["time"] = k.time });
                return new JObject { ["colorKeys"] = colors, ["alphaKeys"] = alphas, ["mode"] = gradient.mode.ToString() };
            }
            if (value is LayerMask lm) return new JValue(lm.value);
            if (value is AnimationCurve curve)
            {
                var keys = new JArray();
                foreach (Keyframe k in curve.keys)
                    keys.Add(new JObject
                    {
                        ["time"] = k.time, ["value"] = k.value,
                        ["inTangent"] = k.inTangent, ["outTangent"] = k.outTangent,
                        ["inWeight"] = k.inWeight, ["outWeight"] = k.outWeight,
                        ["weightedMode"] = k.weightedMode.ToString()
                    });
                return new JObject
                {
                    ["keys"] = keys,
                    ["preWrapMode"] = curve.preWrapMode.ToString(),
                    ["postWrapMode"] = curve.postWrapMode.ToString()
                };
            }

            if (value is UnityEngine.Object unityObject)
            {
                Type type = unityObject.GetType();
                string assetPath = AssetDatabase.GetAssetPath(unityObject);
                if (JsonizationEditorUtil.IsProjectConfigType(type))
                {
                    if (!string.IsNullOrWhiteSpace(assetPath) &&
                        string.Equals(assetPath, context.RootAssetPath, StringComparison.OrdinalIgnoreCase) &&
                        !JsonizationEditorUtil.IsMainAsset(unityObject))
                    {
                        return SerializeObjectFields(unityObject, type, context, declaredType);
                    }

                    return new JObject
                    {
                        ["$config"] = new JObject
                        {
                            ["model"] = MoyvaJsonTypeRegistry.StableId(type),
                            ["id"] = JsonizationEditorUtil.StableId(unityObject, type)
                        }
                    };
                }

                string key = JsonizationEditorUtil.AssetKey(unityObject);
                context.Assets[key] = unityObject;
                return new JObject
                {
                    ["$asset"] = key,
                    ["editorPath"] = assetPath,
                    ["required"] = true
                };
            }

            if (value is IDictionary dictionary)
            {
                var obj = new JObject();
                foreach (DictionaryEntry pair in dictionary)
                    obj[Convert.ToString(pair.Key, System.Globalization.CultureInfo.InvariantCulture)] =
                        SerializeValue(pair.Value, pair.Value?.GetType() ?? typeof(object), context);
                return obj;
            }

            if (value is IEnumerable enumerable && !(value is string))
            {
                var array = new JArray();
                Type itemType = declaredType.IsArray
                    ? declaredType.GetElementType()
                    : declaredType.IsGenericType ? declaredType.GetGenericArguments().FirstOrDefault() : typeof(object);
                foreach (object item in enumerable)
                    array.Add(SerializeValue(item, itemType ?? item?.GetType() ?? typeof(object), context));
                return array;
            }

            return SerializeObjectFields(value, actualType, context, declaredType);
        }

        private static JObject BuildRootSchema(Type type)
        {
            var properties = new JObject
            {
                ["$schema"] = new JObject { ["type"] = "string", ["title"] = "JSON Schema" },
                ["schema"] = new JObject { ["const"] = JsonizationEditorUtil.SchemaName(type) },
                ["version"] = new JObject { ["type"] = "integer", ["minimum"] = 1, ["default"] = 1 },
                ["id"] = new JObject
                {
                    ["type"] = "string",
                    ["minLength"] = 1,
                    ["pattern"] = "^[a-z0-9]+(?:-[a-z0-9]+)*$",
                    ["title"] = "Стабільний ID",
                    ["description"] = "Стабільний ідентифікатор. Використовуйте дефіси; ID не залежить від filename або C# class name."
                },
                ["model"] = new JObject { ["const"] = MoyvaJsonTypeRegistry.StableId(type) },
                ["migration"] = new JObject { ["type"] = "object", ["additionalProperties"] = true }
            };

            var stack = new HashSet<Type>();
            foreach (FieldInfo field in JsonizationEditorUtil.SerializedFields(type))
            {
                if (type.Name == "BuildingRegistrySO" &&
                    (field.Name == "_buildingAssets" || field.Name == "Buildings"))
                    continue;
                if (type.Name == "UnitRegistrySO" && field.Name == "Configs")
                    continue;
                string name = Camel(field.Name.TrimStart('_'));
                properties[name] = SchemaForField(field, stack);
            }

            return new JObject
            {
                ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
                ["$id"] = "https://moyva.local/schemas/" + JsonizationEditorUtil.SchemaFileName(type),
                ["title"] = "Moyva — " + ObjectNames.NicifyVariableName(type.Name),
                ["description"] = "JSON source of truth для " + type.FullName + ". Згенеровано з фактичного Unity serialization contract; runtime читає JSON напряму.",
                ["type"] = "object",
                ["additionalProperties"] = false,
                ["required"] = new JArray("schema", "version", "id", "model"),
                ["properties"] = properties
            };
        }

        private static JToken SchemaForField(FieldInfo field, HashSet<Type> stack)
        {
            JObject schema = SchemaForType(field.FieldType, stack);
            string title = ObjectNames.NicifyVariableName(field.Name.TrimStart('_'));
            schema["title"] = title;
            string description = field.GetCustomAttribute<TooltipAttribute>()?.tooltip;
            if (!string.IsNullOrWhiteSpace(description)) schema["description"] = description;
            var min = field.GetCustomAttribute<MinAttribute>();
            if (min != null)
            {
                schema["minimum"] = min.min;
                schema["x-moyva-editorMinimum"] = min.min;
            }
            var range = field.GetCustomAttribute<RangeAttribute>();
            if (range != null)
            {
                schema["minimum"] = range.min;
                schema["maximum"] = range.max;
                schema["x-moyva-editorMinimum"] = range.min;
                schema["x-moyva-editorMaximum"] = range.max;
            }
            return schema;
        }

        private static JObject SchemaForType(Type type, HashSet<Type> stack)
        {
            if (type == null)
                return new JObject();

            Type nullable = Nullable.GetUnderlyingType(type);
            Type coreType = nullable ?? type;

            // SerializeValue emits JSON null for every null managed reference
            // and Nullable<T>. The schema must describe the actual exported
            // representation, not only the nominal CLR type.
            bool allowNull = nullable != null || !coreType.IsValueType;

            JObject schema = SchemaForNonNullableType(coreType, stack);
            if (allowNull)
                AddNullType(schema);

            return schema;
        }

        private static JObject SchemaForNonNullableType(
            Type type,
            HashSet<Type> stack)
        {
            if (type == typeof(object))
            {
                // A serialized `object` field can legally contain any JSON
                // shape. Concrete project polymorphism is constrained below
                // whenever a stronger declared type is available.
                return new JObject();
            }

            if (type == typeof(string) || type == typeof(char))
                return new JObject { ["type"] = "string" };

            if (type == typeof(bool))
                return new JObject { ["type"] = "boolean" };

            if (type.IsEnum)
                return EnumSchema(type);

            if (type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal))
            {
                return new JObject { ["type"] = "number" };
            }

            if (type.IsPrimitive)
                return new JObject { ["type"] = "integer" };

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (JsonizationEditorUtil.IsProjectConfigType(type))
                {
                    // A project-owned Unity config can appear in JSON in two
                    // legitimate forms. External/main assets are stable $config
                    // references, while sub-assets owned by the current root
                    // (Graph NodeBase is the important example) are serialized
                    // inline with a safe $type discriminator. The serializer
                    // decides which form is used from the actual asset path, so
                    // the declared-type schema must accept BOTH forms.
                    return new JObject
                    {
                        ["anyOf"] = new JArray
                        {
                            ProjectConfigReferenceSchema(),
                            InlineProjectConfigSchema(type, stack)
                        },
                        ["description"] =
                            "Moyva config: stable $config reference або inline sub-asset з allow-listed $type."
                    };
                }

                // Fields declared as the generic Unity base types (notably
                // ScriptableObject integrations in ConstructionSystemProfileSO)
                // may legally reference either an ordinary Unity asset OR a
                // Moyva project config. SerializeValue chooses from the ACTUAL
                // runtime type, so the declared-type schema must accept the same
                // union instead of forcing every ScriptableObject to be $asset.
                if (type == typeof(ScriptableObject) ||
                    type == typeof(UnityEngine.Object))
                {
                    return GenericUnityObjectReferenceSchema(type);
                }

                return AssetReferenceSchema();
            }

            if (type == typeof(Vector2) || type == typeof(Vector2Int))
                return VectorSchema("x", "y");
            if (type == typeof(Vector3) || type == typeof(Vector3Int))
                return VectorSchema("x", "y", "z");
            if (type == typeof(Vector4) || type == typeof(Quaternion))
                return VectorSchema("x", "y", "z", "w");
            if (type == typeof(Color) || type == typeof(Color32))
                return VectorSchema("r", "g", "b", "a");
            if (type == typeof(Rect) || type == typeof(RectInt))
                return VectorSchema("x", "y", "width", "height");

            if (type == typeof(Bounds) ||
                type == typeof(BoundsInt) ||
                type == typeof(Gradient) ||
                type == typeof(AnimationCurve))
            {
                return new JObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = true
                };
            }

            if (type == typeof(LayerMask))
                return new JObject { ["type"] = "integer" };

            // SerializeValue handles IDictionary BEFORE IEnumerable and writes
            // it as a JSON object. Schema generation must use the same order.
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                Type valueType = typeof(object);
                if (type.IsGenericType)
                {
                    Type[] genericArguments = type.GetGenericArguments();
                    if (genericArguments.Length >= 2)
                        valueType = genericArguments[1];
                }

                return new JObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] =
                        SchemaForType(valueType, stack)
                };
            }

            if (typeof(IEnumerable).IsAssignableFrom(type) &&
                type != typeof(string))
            {
                Type item = type.IsArray
                    ? type.GetElementType()
                    : type.IsGenericType
                        ? type.GetGenericArguments().FirstOrDefault()
                        : typeof(object);

                return new JObject
                {
                    ["type"] = "array",
                    ["items"] = SchemaForType(
                        item ?? typeof(object),
                        stack)
                };
            }

            if (type.IsAbstract || type.IsInterface)
            {
                JArray ids = ConcreteTypeIds(type);

                return new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["$type"] = new JObject
                        {
                            ["type"] = "string",
                            ["enum"] = ids
                        }
                    },
                    ["required"] = new JArray("$type"),
                    // Derived fields belong to the allow-listed concrete type.
                    // The semantic validator/runtime type registry validates
                    // the discriminator later.
                    ["additionalProperties"] = true
                };
            }

            if (!stack.Add(type))
            {
                return new JObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = true
                };
            }

            try
            {
                var properties = new JObject();

                foreach (FieldInfo field in
                         JsonizationEditorUtil.SerializedFields(type))
                {
                    properties[Camel(field.Name.TrimStart('_'))] =
                        SchemaForField(field, stack);
                }

                // SerializeObjectFields adds $type whenever a concrete runtime
                // subtype differs from the declared non-sealed base type.
                JArray derivedIds = ConcreteDerivedTypeIds(type);
                bool hasDerivedTypes = derivedIds.Count > 0;

                if (hasDerivedTypes)
                {
                    properties["$type"] = new JObject
                    {
                        ["type"] = "string",
                        ["enum"] = derivedIds
                    };
                }

                return new JObject
                {
                    ["type"] = "object",
                    ["properties"] = properties,
                    ["additionalProperties"] = hasDerivedTypes
                };
            }
            finally
            {
                stack.Remove(type);
            }
        }

        private static JObject ProjectConfigReferenceSchema()
        {
            return new JObject
            {
                ["type"] = "object",
                ["properties"] = new JObject
                {
                    ["$config"] = new JObject
                    {
                        ["type"] = "object",
                        ["required"] = new JArray("id"),
                        ["properties"] = new JObject
                        {
                            ["model"] = new JObject { ["type"] = "string" },
                            ["id"] = new JObject { ["type"] = "string" }
                        },
                        ["additionalProperties"] = true
                    }
                },
                ["required"] = new JArray("$config"),
                ["additionalProperties"] = false
            };
        }

        private static JObject InlineProjectConfigSchema(
            Type declaredType,
            HashSet<Type> stack)
        {
            // Abstract/interface project config types (for example NodeBase)
            // are emitted inline with $type + concrete serialized fields.
            // Enumerate the allow-listed discriminator and let the concrete
            // payload fields through; runtime semantic/type validation checks
            // the concrete contract. This avoids pretending an inline subasset
            // is an external $config reference.
            if (declaredType.IsAbstract || declaredType.IsInterface)
            {
                return new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["$type"] = new JObject
                        {
                            ["type"] = "string",
                            ["enum"] = ConcreteTypeIds(declaredType)
                        }
                    },
                    ["required"] = new JArray("$type"),
                    ["additionalProperties"] = true
                };
            }

            if (!stack.Add(declaredType))
            {
                return new JObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = true
                };
            }

            try
            {
                var properties = new JObject();
                foreach (FieldInfo field in
                         JsonizationEditorUtil.SerializedFields(declaredType))
                {
                    properties[Camel(field.Name.TrimStart('_'))] =
                        SchemaForField(field, stack);
                }

                JArray derivedIds = ConcreteDerivedTypeIds(declaredType);
                bool polymorphic = derivedIds.Count > 0;
                if (polymorphic)
                {
                    properties["$type"] = new JObject
                    {
                        ["type"] = "string",
                        ["enum"] = derivedIds
                    };
                }

                return new JObject
                {
                    ["type"] = "object",
                    ["properties"] = properties,
                    ["additionalProperties"] = polymorphic
                };
            }
            finally
            {
                stack.Remove(declaredType);
            }
        }

        private static JObject EnumSchema(Type type)
        {
            string[] names = Enum.GetNames(type);
            string escapedNames = names.Length == 0
                ? string.Empty
                : string.Join("|", names.Select(Regex.Escape));

            bool flags =
                type.GetCustomAttribute<FlagsAttribute>() != null;

            // Enum.ToString() is the canonical exporter format:
            //   normal enum       -> "ValueName"
            //   Flags enum        -> "A, B, C"
            //   undefined legacy  -> "0" / "-1"
            // Therefore a plain JSON-Schema enum list is insufficient for
            // migration parity. Keep enum names as editor metadata and
            // validate the actual string representation with a pattern.
            string numeric = @"-?\d+";
            string pattern;

            if (string.IsNullOrEmpty(escapedNames))
            {
                pattern = "^" + numeric + "$";
            }
            else if (flags)
            {
                pattern =
                    "^(?:" + numeric +
                    "|(?:" + escapedNames + ")" +
                    "(?:, (?:" + escapedNames + "))*)$";
            }
            else
            {
                pattern =
                    "^(?:" + numeric +
                    "|(?:" + escapedNames + "))$";
            }

            return new JObject
            {
                ["type"] = "string",
                ["pattern"] = pattern,
                ["x-enumNames"] = new JArray(names),
                ["x-flags"] = flags
            };
        }

        private static void AddNullType(JObject schema)
        {
            if (schema == null)
                return;

            // Composition schemas (notably project config reference OR inline
            // subasset) have no top-level `type`. Add an explicit null branch.
            if (schema["anyOf"] is JArray anyOf)
            {
                bool alreadyAllowsNull = anyOf
                    .OfType<JObject>()
                    .Any(x => x["type"]?.Type == JTokenType.String &&
                              string.Equals(x["type"]?.Value<string>(), "null", StringComparison.Ordinal));
                if (!alreadyAllowsNull)
                    anyOf.Add(new JObject { ["type"] = "null" });
                return;
            }

            JToken typeToken = schema["type"];

            // Empty schema already accepts null and every other JSON value.
            if (typeToken == null)
                return;

            if (typeToken.Type == JTokenType.String)
            {
                string current = typeToken.Value<string>();
                if (!string.Equals(
                        current,
                        "null",
                        StringComparison.Ordinal))
                {
                    schema["type"] =
                        new JArray(current, "null");
                }

                return;
            }

            if (typeToken is JArray types &&
                !types.Values<string>().Contains(
                    "null",
                    StringComparer.Ordinal))
            {
                types.Add("null");
            }
        }

        private static JArray ConcreteTypeIds(Type baseType)
        {
            if (!ConcreteIdsCache.TryGetValue(baseType, out string[] ids))
            {
                ids = ProjectOwnedConcreteTypes()
                    .Where(t => baseType.IsAssignableFrom(t))
                    .Select(MoyvaJsonTypeRegistry.StableId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray();

                ConcreteIdsCache[baseType] = ids;
            }

            return new JArray(ids);
        }

        private static JArray ConcreteDerivedTypeIds(Type baseType)
        {
            if (baseType == null || baseType.IsSealed)
                return new JArray();

            if (!DerivedIdsCache.TryGetValue(baseType, out string[] ids))
            {
                ids = ProjectOwnedConcreteTypes()
                    .Where(t =>
                        t != baseType &&
                        baseType.IsAssignableFrom(t))
                    .Select(MoyvaJsonTypeRegistry.StableId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray();

                DerivedIdsCache[baseType] = ids;
            }

            return new JArray(ids);
        }

        private static JObject GenericUnityObjectReferenceSchema(Type declaredType)
        {
            var configTypeIds = new JArray(
                ProjectOwnedConcreteTypes()
                    .Where(t =>
                        declaredType.IsAssignableFrom(t) &&
                        JsonizationEditorUtil.IsProjectConfigType(t))
                    .Select(MoyvaJsonTypeRegistry.StableId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(x => x, StringComparer.Ordinal));

            var inlineConfig = new JObject
            {
                ["type"] = "object",
                ["properties"] = new JObject
                {
                    ["$type"] = new JObject
                    {
                        ["type"] = "string",
                        ["enum"] = configTypeIds
                    }
                },
                ["required"] = new JArray("$type"),
                ["additionalProperties"] = true
            };

            return new JObject
            {
                ["anyOf"] = new JArray
                {
                    AssetReferenceSchema(),
                    ProjectConfigReferenceSchema(),
                    inlineConfig
                },
                ["description"] =
                    "Generic Unity object reference: $asset, Moyva $config або inline project config sub-asset."
            };
        }

        private static JObject AssetReferenceSchema() => new JObject
        {
            ["type"] = new JArray("object", "null"),
            ["description"] = "Opaque Unity asset reference. Gameplay values не зберігаються в Unity asset.",
            ["properties"] = new JObject
            {
                ["$asset"] = new JObject { ["type"] = "string", ["title"] = "Asset ID" },
                ["editorPath"] = new JObject { ["type"] = "string", ["description"] = "Editor-only path для діагностики/регенерації catalog." },
                ["required"] = new JObject { ["type"] = "boolean", ["default"] = true }
            },
            ["required"] = new JArray("$asset")
        };

        private static JObject VectorSchema(params string[] names)
        {
            var props = new JObject();
            foreach (string name in names) props[name] = new JObject { ["type"] = "number" };
            return new JObject { ["type"] = "object", ["properties"] = props, ["required"] = new JArray(names), ["additionalProperties"] = false };
        }

        private static IEnumerable<Type> SafeTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch { return Array.Empty<Type>(); }
        }

        private static Type ResolveType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }

        private static string OutputPath(Type type, string id)
        {
            string domain = JsonizationEditorUtil.DomainFolder(type);
            if (type.Name == "BuildingDefinitionAsset") return Path.Combine(JsonizationEditorUtil.PresetsRoot, "Buildings", id + ".json").Replace('\\','/');
            if (type.Name == "GraphAsset") return Path.Combine(JsonizationEditorUtil.PresetsRoot, "Graphs", id + ".json").Replace('\\','/');
            string typeFolder = MoyvaJsonTypeRegistry.StableId(type);
            return Path.Combine(JsonizationEditorUtil.PresetsRoot, domain, typeFolder, id + ".json").Replace('\\','/');
        }

        private static string Camel(string name)
        {
            if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0])) return name;
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        private static string HashToken(JToken token)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(token.ToString(Formatting.None));
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static Dictionary<string, UnityEngine.Object> CollectAllAssetReferences(ExportReport report)
        {
            var result = new Dictionary<string, UnityEngine.Object>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in Directory.GetFiles(JsonizationEditorUtil.PresetsRoot, "*.json", SearchOption.AllDirectories))
            {
                if (file.Replace('\\','/').Contains("/Schemas/")) continue;
                try
                {
                    JObject root = JObject.Parse(File.ReadAllText(file));
                    foreach (JObject obj in root.DescendantsAndSelf().OfType<JObject>().Where(o => o.Property("$asset") != null))
                    {
                        string key = obj.Value<string>("$asset");
                        string path = obj.Value<string>("editorPath");
                        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(path)) continue;
                        UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                        if (asset != null) result[key] = asset;
                    }
                }
                catch { }
            }
            return result;
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
