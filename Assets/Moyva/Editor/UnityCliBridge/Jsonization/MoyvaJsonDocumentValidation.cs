using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    internal sealed class MoyvaJsonValidationIssue
    {
        public MoyvaJsonValidationIssue(
            string sourcePath,
            string documentId,
            string pointer,
            string graphNodeId,
            string message)
        {
            SourcePath = sourcePath;
            DocumentId = documentId;
            Pointer = string.IsNullOrWhiteSpace(pointer) ? "/" : pointer;
            GraphNodeId = graphNodeId;
            Message = message;
        }

        public string SourcePath { get; }
        public string DocumentId { get; }
        public string Pointer { get; }
        public string GraphNodeId { get; }
        public string Message { get; }

        public override string ToString()
        {
            string node = string.IsNullOrWhiteSpace(GraphNodeId)
                ? string.Empty
                : $", graph node '{GraphNodeId}'";
            return $"{SourcePath} [{DocumentId}{node}] {Pointer}: {Message}";
        }
    }

    /// <summary>
    /// Validator одного staged JSON-документа.
    /// Схема перевіряє форму, а цей клас додає Moyva-семантику: refs, asset-и,
    /// link policy, tile/movement правила і точні JSON Pointer diagnostics.
    /// </summary>
    internal static class MoyvaJsonDocumentValidation
    {
        internal static IReadOnlyList<MoyvaJsonValidationIssue> Validate(
            string sourcePath,
            JObject document)
        {
            var issues = new List<MoyvaJsonValidationIssue>();
            string documentId = document?.Value<string>("id") ?? "<unknown>";
            if (document == null)
            {
                AddIssue(issues, sourcePath, documentId, string.Empty, null, "document is null");
                return issues;
            }

            JObject schema = LoadSchema(sourcePath, document, out string schemaError);
            if (schema == null)
            {
                AddIssue(issues, sourcePath, documentId, "/$schema", document, schemaError);
                return issues;
            }

            ValidateToken(
                document,
                schema,
                schema,
                string.Empty,
                sourcePath,
                documentId,
                issues);

            MoyvaJsonDocumentIndex documentIndex = MoyvaJsonDocumentIndex.Build(
                sourcePath,
                document);
            ValidateReferences(
                document,
                schema,
                schema,
                string.Empty,
                sourcePath,
                documentId,
                documentIndex,
                issues);
            ValidateDuplicateIdentity(sourcePath, document, documentIndex, issues);
            ValidateDocumentLinkSemantics(sourcePath, document, documentId, documentIndex, issues);
            ValidateTileSemantics(sourcePath, document, documentId, issues);
            ValidateMovementSemantics(sourcePath, document, documentId, issues);
            return issues;
        }

        internal static JObject LoadSchema(
            string sourcePath,
            JObject document,
            out string error)
        {
            error = null;
            string relativeSchema = document?.Value<string>("$schema");
            if (string.IsNullOrWhiteSpace(relativeSchema))
            {
                error = "missing schema path";
                return null;
            }

            string sourceDirectory = Path.GetDirectoryName(sourcePath) ?? string.Empty;
            string schemaPath = Path.GetFullPath(Path.Combine(sourceDirectory, relativeSchema));
            if (!File.Exists(schemaPath))
            {
                error = $"schema file not found: {relativeSchema}";
                return null;
            }

            try
            {
                return JObject.Parse(File.ReadAllText(schemaPath));
            }
            catch (Exception exception)
            {
                error = $"invalid schema '{relativeSchema}': {exception.Message}";
                return null;
            }
        }

        internal static JObject ResolveSchema(JObject schema, JObject rootSchema)
        {
            if (schema == null)
                return new JObject();

            string reference = schema.Value<string>("$ref");
            if (string.IsNullOrWhiteSpace(reference) || !reference.StartsWith("#/", StringComparison.Ordinal))
                return schema;

            JToken current = rootSchema;
            string[] segments = reference.Substring(2).Split('/');
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i].Replace("~1", "/").Replace("~0", "~");
                current = current?[segment];
            }

            if (current is not JObject referenced)
                return schema;

            var merged = (JObject)referenced.DeepClone();
            foreach (JProperty property in schema.Properties())
            {
                if (property.Name == "$ref")
                    continue;
                merged[property.Name] = property.Value.DeepClone();
            }

            return merged;
        }

        internal static UnityEngine.Object ResolveAssetReference(JObject assetReference)
        {
            if (assetReference == null)
                return null;

            string key = assetReference.Value<string>("$asset");
            string editorPath = assetReference.Value<string>("editorPath");
            if (!string.IsNullOrWhiteSpace(editorPath))
            {
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(editorPath);
                for (int i = 0; i < assets.Length; i++)
                {
                    UnityEngine.Object asset = assets[i];
                    if (asset != null && string.Equals(
                            JsonizationEditorUtil.AssetKey(asset),
                            key,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return asset;
                    }
                }
            }

            GameObject catalogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                JsonizationEditorUtil.CatalogPath);
            MoyvaJsonAssetCatalog catalog = catalogPrefab != null
                ? catalogPrefab.GetComponent<MoyvaJsonAssetCatalog>()
                : null;
            return catalog?.Resolve(key);
        }

        internal static Type ResolveType(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return typeof(UnityEngine.Object);

            Type direct = Type.GetType(fullName, throwOnError: false);
            if (direct != null)
                return direct;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type candidate = assembly.GetType(fullName, throwOnError: false);
                if (candidate != null)
                    return candidate;
            }

            return typeof(UnityEngine.Object);
        }

        private static void ValidateToken(
            JToken token,
            JObject rawSchema,
            JObject rootSchema,
            string pointer,
            string sourcePath,
            string documentId,
            List<MoyvaJsonValidationIssue> issues)
        {
            JObject schema = ResolveSchema(rawSchema, rootSchema);
            if (!MatchesType(token, schema["type"]))
            {
                AddIssue(
                    issues,
                    sourcePath,
                    documentId,
                    pointer,
                    token,
                    $"expected type {FormatType(schema["type"])}, got {token.Type}");
                return;
            }

            if (schema.TryGetValue("const", out JToken constant)
                && !JToken.DeepEquals(token, constant))
            {
                AddIssue(issues, sourcePath, documentId, pointer, token,
                    $"expected constant value {constant.ToString(Newtonsoft.Json.Formatting.None)}");
            }

            if (schema["enum"] is JArray enumValues
                && !enumValues.Any(value => JToken.DeepEquals(value, token)))
            {
                AddIssue(issues, sourcePath, documentId, pointer, token,
                    "value is not one of the allowed enum values");
            }

            if (token is JObject objectToken)
                ValidateObject(objectToken, schema, rootSchema, pointer, sourcePath, documentId, issues);
            else if (token is JArray arrayToken)
                ValidateArray(arrayToken, schema, rootSchema, pointer, sourcePath, documentId, issues);
            else if (token.Type == JTokenType.String)
                ValidateString(token.Value<string>(), schema, pointer, sourcePath, documentId, token, issues);
            else if (token.Type is JTokenType.Integer or JTokenType.Float)
                ValidateNumber(token.Value<double>(), schema, pointer, sourcePath, documentId, token, issues);

            if (schema["allOf"] is JArray allOf)
            {
                foreach (JObject branch in allOf.OfType<JObject>())
                {
                    if (branch["if"] is JObject condition)
                    {
                        bool conditionMatches = MatchesSchema(token, condition, rootSchema);
                        JObject selected = conditionMatches
                            ? branch["then"] as JObject
                            : branch["else"] as JObject;
                        if (selected != null)
                            ValidateToken(token, selected, rootSchema, pointer, sourcePath, documentId, issues);
                    }
                    else
                    {
                        ValidateToken(token, branch, rootSchema, pointer, sourcePath, documentId, issues);
                    }
                }
            }
        }

        private static void ValidateObject(
            JObject token,
            JObject schema,
            JObject rootSchema,
            string pointer,
            string sourcePath,
            string documentId,
            List<MoyvaJsonValidationIssue> issues)
        {
            JObject properties = schema["properties"] as JObject;
            if (schema["required"] is JArray required)
            {
                foreach (string propertyName in required.Values<string>())
                {
                    if (token.Property(propertyName) == null)
                    {
                        AddIssue(issues, sourcePath, documentId,
                            JoinPointer(pointer, propertyName), token,
                            "required property is missing");
                    }
                }
            }

            if (schema.Value<bool?>("additionalProperties") == false && properties != null)
            {
                foreach (JProperty property in token.Properties())
                {
                    if (IsRootEditorMetadata(pointer, property.Name))
                        continue;

                    if (properties.Property(property.Name) == null)
                    {
                        AddIssue(issues, sourcePath, documentId,
                            JoinPointer(pointer, property.Name), property.Value,
                            "property is not declared by the schema");
                    }
                }
            }

            if (properties == null)
                return;

            foreach (JProperty property in token.Properties())
            {
                if (IsRootEditorMetadata(pointer, property.Name)
                    && properties[property.Name] is not JObject)
                {
                    continue;
                }

                if (properties[property.Name] is not JObject propertySchema)
                    continue;
                ValidateToken(
                    property.Value,
                    propertySchema,
                    rootSchema,
                    JoinPointer(pointer, property.Name),
                    sourcePath,
                    documentId,
                    issues);
            }
        }

        private static void ValidateArray(
            JArray token,
            JObject schema,
            JObject rootSchema,
            string pointer,
            string sourcePath,
            string documentId,
            List<MoyvaJsonValidationIssue> issues)
        {
            if (schema.Value<bool?>("uniqueItems") == true)
            {
                for (int i = 0; i < token.Count; i++)
                {
                    for (int j = i + 1; j < token.Count; j++)
                    {
                        if (JToken.DeepEquals(token[i], token[j]))
                        {
                            AddIssue(issues, sourcePath, documentId,
                                JoinPointer(pointer, j.ToString(CultureInfo.InvariantCulture)),
                                token[j], "duplicate array item");
                        }
                    }
                }
            }

            if (schema["items"] is not JObject itemSchema)
                return;

            for (int i = 0; i < token.Count; i++)
            {
                ValidateToken(
                    token[i],
                    itemSchema,
                    rootSchema,
                    JoinPointer(pointer, i.ToString(CultureInfo.InvariantCulture)),
                    sourcePath,
                    documentId,
                    issues);
            }
        }

        private static void ValidateString(
            string value,
            JObject schema,
            string pointer,
            string sourcePath,
            string documentId,
            JToken token,
            List<MoyvaJsonValidationIssue> issues)
        {
            value ??= string.Empty;
            int? minimumLength = schema.Value<int?>("minLength");
            if (minimumLength.HasValue && value.Length < minimumLength.Value)
            {
                AddIssue(issues, sourcePath, documentId, pointer, token,
                    $"minimum length is {minimumLength.Value}");
            }

            string pattern = schema.Value<string>("pattern");
            if (!string.IsNullOrWhiteSpace(pattern) && !Regex.IsMatch(value, pattern))
            {
                AddIssue(issues, sourcePath, documentId, pointer, token,
                    $"value does not match pattern {pattern}");
            }
        }

        private static void ValidateNumber(
            double value,
            JObject schema,
            string pointer,
            string sourcePath,
            string documentId,
            JToken token,
            List<MoyvaJsonValidationIssue> issues)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                AddIssue(issues, sourcePath, documentId, pointer, token,
                    "number must be finite");
                return;
            }

            double? minimum = schema.Value<double?>("minimum");
            double? maximum = schema.Value<double?>("maximum");
            double? exclusiveMinimum = schema.Value<double?>("exclusiveMinimum");
            double? exclusiveMaximum = schema.Value<double?>("exclusiveMaximum");
            if (minimum.HasValue && value < minimum.Value)
                AddIssue(issues, sourcePath, documentId, pointer, token, $"minimum is {minimum.Value}");
            if (maximum.HasValue && value > maximum.Value)
                AddIssue(issues, sourcePath, documentId, pointer, token, $"maximum is {maximum.Value}");
            if (exclusiveMinimum.HasValue && value <= exclusiveMinimum.Value)
                AddIssue(issues, sourcePath, documentId, pointer, token, $"value must be > {exclusiveMinimum.Value}");
            if (exclusiveMaximum.HasValue && value >= exclusiveMaximum.Value)
                AddIssue(issues, sourcePath, documentId, pointer, token, $"value must be < {exclusiveMaximum.Value}");
        }

        private static void ValidateReferences(
            JToken token,
            JObject rawSchema,
            JObject rootSchema,
            string pointer,
            string sourcePath,
            string documentId,
            MoyvaJsonDocumentIndex documentIndex,
            List<MoyvaJsonValidationIssue> issues)
        {
            JObject schema = ResolveSchema(rawSchema, rootSchema);
            if (token is JObject objectToken)
            {
                if (objectToken["$config"] is JObject configReference)
                {
                    string model = configReference.Value<string>("model");
                    string id = configReference.Value<string>("id");
                    if (string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(id))
                    {
                        AddIssue(issues, sourcePath, documentId,
                            JoinPointer(pointer, "$config"), objectToken,
                            "config reference requires non-empty model and id");
                    }
                    else if (!documentIndex.ContainsConfig(model, id))
                    {
                        AddIssue(issues, sourcePath, documentId,
                            JoinPointer(pointer, "$config"), objectToken,
                            $"unknown config reference '{model}/{id}'");
                    }
                }

                if (objectToken.Property("$asset") != null)
                    ValidateAssetReference(objectToken, schema, pointer, sourcePath, documentId, issues);

                JObject properties = schema["properties"] as JObject;
                foreach (JProperty property in objectToken.Properties())
                {
                    if (IsRootEditorMetadata(pointer, property.Name))
                        continue;

                    JObject childSchema = properties?[property.Name] as JObject ?? new JObject();
                    ValidateReferences(
                        property.Value,
                        childSchema,
                        rootSchema,
                        JoinPointer(pointer, property.Name),
                        sourcePath,
                        documentId,
                        documentIndex,
                        issues);
                }
            }
            else if (token is JArray arrayToken)
            {
                JObject itemSchema = schema["items"] as JObject ?? new JObject();
                for (int i = 0; i < arrayToken.Count; i++)
                {
                    ValidateReferences(
                        arrayToken[i],
                        itemSchema,
                        rootSchema,
                        JoinPointer(pointer, i.ToString(CultureInfo.InvariantCulture)),
                        sourcePath,
                        documentId,
                        documentIndex,
                        issues);
                }
            }
            else if (token.Type == JTokenType.String)
            {
                string configModel = schema.Value<string>("x-moyva-configModel");
                string id = token.Value<string>();
                if (!string.IsNullOrWhiteSpace(configModel)
                    && !string.IsNullOrWhiteSpace(id)
                    && !documentIndex.ContainsConfig(configModel, id))
                {
                    AddIssue(issues, sourcePath, documentId, pointer, token,
                        $"unknown {configModel} id '{id}'");
                }
            }
        }

        private static void ValidateAssetReference(
            JObject assetReference,
            JObject schema,
            string pointer,
            string sourcePath,
            string documentId,
            List<MoyvaJsonValidationIssue> issues)
        {
            string key = assetReference.Value<string>("$asset");
            bool required = assetReference.Value<bool?>("required") ?? true;
            if (string.IsNullOrWhiteSpace(key))
            {
                if (required)
                    AddIssue(issues, sourcePath, documentId, JoinPointer(pointer, "$asset"),
                        assetReference, "required asset ID is empty");
                return;
            }

            UnityEngine.Object asset = ResolveAssetReference(assetReference);
            if (asset == null)
            {
                if (required)
                    AddIssue(issues, sourcePath, documentId, pointer, assetReference,
                        $"required asset '{key}' cannot be resolved");
                return;
            }

            string actualPath = AssetDatabase.GetAssetPath(asset);
            string storedPath = assetReference.Value<string>("editorPath");
            if (!string.Equals(actualPath, storedPath, StringComparison.OrdinalIgnoreCase))
            {
                AddIssue(issues, sourcePath, documentId, JoinPointer(pointer, "editorPath"),
                    assetReference, $"stale path '{storedPath}', current path is '{actualPath}'");
            }

            string expectedTypeName = schema.Value<string>("x-moyva-assetType");
            Type expectedType = ResolveType(expectedTypeName);
            if (expectedType != typeof(UnityEngine.Object) && !expectedType.IsInstanceOfType(asset))
            {
                AddIssue(issues, sourcePath, documentId, pointer, assetReference,
                    $"asset type is {asset.GetType().FullName}, expected {expectedTypeName}");
            }
        }

        private static void ValidateDuplicateIdentity(
            string sourcePath,
            JObject document,
            MoyvaJsonDocumentIndex documentIndex,
            List<MoyvaJsonValidationIssue> issues)
        {
            if (documentIndex.TryGetPaths(
                    document.Value<string>("model"),
                    document.Value<string>("id"),
                    out List<string> paths) &&
                paths.Count > 1)
            {
                AddIssue(issues, sourcePath, document.Value<string>("id"), "/id", document["id"],
                    "duplicate model/id; also found in " + string.Join(", ", paths));
            }
        }

        private static void ValidateDocumentLinkSemantics(
            string sourcePath,
            JObject document,
            string documentId,
            MoyvaJsonDocumentIndex documentIndex,
            List<MoyvaJsonValidationIssue> issues)
        {
            bool hasEditorMetadata =
                document?[MoyvaJsonDocumentMetadata.Editor] != null;
            if (!hasEditorMetadata)
                return;

            if (!documentIndex.TryGetDocument(sourcePath, out MoyvaJsonDocumentRecord current)
                || !current.IsRuntimeGameplay)
            {
                AddIssue(issues, sourcePath, documentId, "/editor", document?["editor"],
                    "editor metadata is allowed only on runtime gameplay JSON documents");
                return;
            }

            if (!MoyvaJsonDocumentLinkPolicy.TryValidateTarget(
                    document,
                    sourcePath,
                    out string pointer,
                    out string message))
            {
                AddIssue(issues, sourcePath, documentId, pointer, document?["editor"], message);
                return;
            }

            MoyvaJsonDocumentLinkTarget currentTarget =
                MoyvaJsonDocumentLinkPolicy.Read(document, sourcePath);
            if (!currentTarget.Enabled)
                return;

            foreach (MoyvaJsonDocumentRecord other in documentIndex.RuntimeGameplayDocuments)
            {
                if (string.Equals(other.Path, sourcePath, StringComparison.OrdinalIgnoreCase))
                    continue;

                MoyvaJsonDocumentLinkTarget otherTarget =
                    MoyvaJsonDocumentLinkPolicy.Read(other.Document, other.Path);
                if (!otherTarget.Enabled)
                    continue;

                if (string.Equals(
                        currentTarget.Path,
                        otherTarget.Path,
                        StringComparison.OrdinalIgnoreCase))
                {
                    AddIssue(issues, sourcePath, documentId, "/editor/documentLink/path",
                        document["editor"], $"document link path is also used by {other.Path}");
                    return;
                }
            }
        }

        private static void ValidateTileSemantics(
            string sourcePath,
            JObject document,
            string documentId,
            List<MoyvaJsonValidationIssue> issues)
        {
            if (!string.Equals(document.Value<string>("schema"), "moyva.tile-type", StringComparison.Ordinal))
                return;

            JObject visual = document["visual"] as JObject;
            string mode = visual?.Value<string>("gridMode");
            JArray variants = visual?["variants"] as JArray;
            if (string.Equals(mode, "Flat", StringComparison.Ordinal))
            {
                if (variants is { Count: > 0 })
                {
                    AddIssue(issues, sourcePath, documentId, "/visual/variants", variants,
                        "Flat mode does not use TileWorldCreator presets");
                }
                return;
            }

            if (variants == null)
                return;

            for (int i = 0; i < variants.Count; i++)
            {
                JObject presetReference = variants[i]?["preset"] as JObject;
                UnityEngine.Object preset = ResolveAssetReference(presetReference);
                if (preset == null)
                    continue;

                FieldInfo gridTypeField = preset.GetType().GetField(
                    "gridtype",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                string presetMode = gridTypeField?.GetValue(preset)?.ToString();
                bool presetIsDual = string.Equals(presetMode, "dual", StringComparison.OrdinalIgnoreCase);
                bool jsonIsDual = string.Equals(mode, "Dual", StringComparison.Ordinal);
                if (presetIsDual != jsonIsDual)
                {
                    AddIssue(issues, sourcePath, documentId,
                        $"/visual/variants/{i}/preset", presetReference,
                        $"preset is {(presetIsDual ? "Dual" : "Normal")}, JSON declares {mode}");
                }
            }
        }

        private static void ValidateMovementSemantics(
            string sourcePath,
            JObject document,
            string documentId,
            List<MoyvaJsonValidationIssue> issues)
        {
            if (!string.Equals(document.Value<string>("schema"), "moyva.movement-profile", StringComparison.Ordinal))
                return;

            ValidateMovementRule(document["fallback"] as JObject, "/fallback");
            ValidateRuleArray(document["classRules"] as JArray, "/classRules");
            ValidateRuleArray(document["tileOverrides"] as JArray, "/tileOverrides");
            return;

            void ValidateRuleArray(JArray rules, string pointer)
            {
                if (rules == null)
                    return;
                for (int i = 0; i < rules.Count; i++)
                    ValidateMovementRule(rules[i] as JObject, $"{pointer}/{i}");
            }

            void ValidateMovementRule(JObject rule, string pointer)
            {
                if (rule?.Value<bool?>("passable") != true)
                    return;
                double cost = rule.Value<double?>("staminaCost") ?? 0d;
                if (double.IsNaN(cost) || double.IsInfinity(cost) || cost <= 0d)
                {
                    AddIssue(issues, sourcePath, documentId, pointer + "/staminaCost",
                        rule?["staminaCost"], "passable rules require a finite stamina cost > 0");
                }
            }
        }

        private static bool MatchesSchema(JToken token, JObject schema, JObject rootSchema)
        {
            var scratch = new List<MoyvaJsonValidationIssue>();
            ValidateToken(token, schema, rootSchema, string.Empty, string.Empty, string.Empty, scratch);
            return scratch.Count == 0;
        }

        private static bool MatchesType(JToken token, JToken typeToken)
        {
            if (typeToken == null)
                return true;
            if (typeToken is JArray types)
                return types.Values<string>().Any(type => MatchesType(token, type));
            return MatchesType(token, typeToken.Value<string>());
        }

        private static bool MatchesType(JToken token, string type)
        {
            return type switch
            {
                null or "" => true,
                "object" => token.Type == JTokenType.Object,
                "array" => token.Type == JTokenType.Array,
                "string" => token.Type == JTokenType.String,
                "boolean" => token.Type == JTokenType.Boolean,
                "integer" => token.Type == JTokenType.Integer,
                "number" => token.Type is JTokenType.Integer or JTokenType.Float,
                "null" => token.Type is JTokenType.Null or JTokenType.Undefined,
                _ => true,
            };
        }

        private static string FormatType(JToken type)
            => type?.ToString(Newtonsoft.Json.Formatting.None) ?? "any";

        private static bool IsRootEditorMetadata(string pointer, string propertyName)
            => string.IsNullOrEmpty(pointer)
               && propertyName == MoyvaJsonDocumentMetadata.Editor;

        private static string JoinPointer(string pointer, string segment)
        {
            string escaped = (segment ?? string.Empty).Replace("~", "~0").Replace("/", "~1");
            return string.IsNullOrEmpty(pointer) ? "/" + escaped : pointer + "/" + escaped;
        }

        private static void AddIssue(
            List<MoyvaJsonValidationIssue> issues,
            string sourcePath,
            string documentId,
            string pointer,
            JToken context,
            string message)
        {
            issues.Add(new MoyvaJsonValidationIssue(
                sourcePath,
                documentId,
                pointer,
                FindGraphNodeId(context),
                message));
        }

        private static string FindGraphNodeId(JToken token)
        {
            for (JToken current = token; current != null; current = current.Parent)
            {
                if (current is JObject obj)
                {
                    string nodeId = obj.Value<string>("nodeId");
                    if (!string.IsNullOrWhiteSpace(nodeId))
                        return nodeId;
                }
            }
            return null;
        }
    }
}
