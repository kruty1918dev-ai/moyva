using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization
{
    /// <summary>
    /// Load -> Validate -> Resolve -> Freeze runtime repository.
    /// Reads generated TextAssets once; gameplay performs dictionary lookups only.
    /// </summary>
    public static class MoyvaJsonRuntime
    {
        private static readonly object Sync = new();
        private static bool _loaded;
        private static Dictionary<string, JObject> _rawByTypeAndId;
        private static Dictionary<string, JObject> _rawBySchemaAndId;
        private static Dictionary<string, object> _frozen;
        private static MoyvaJsonAssetCatalog _catalog;
        private static JsonSerializer _serializer;
        private static string _fingerprint = string.Empty;

        public static bool IsLoaded => _loaded;
        public static string ConfigFingerprint
        {
            get
            {
                EnsureLoaded();
                return _fingerprint;
            }
        }

        public static void EnsureLoaded()
        {
            if (_loaded)
                return;

            lock (Sync)
            {
                if (_loaded)
                    return;

                LoadAll();
                _loaded = true;
            }
        }

        public static T Get<T>(string id) where T : class
        {
            return Get(typeof(T), id) as T;
        }

        public static object Get(Type type, string id)
        {
            EnsureLoaded();

            if (type == null || string.IsNullOrWhiteSpace(id))
                return null;

            string key = MakeKey(type.FullName, id);

            if (_frozen.TryGetValue(key, out var cached))
                return cached;

            if (!_rawByTypeAndId.TryGetValue(key, out var raw))
            {
                // Fallback to schema when a migrated type was renamed but its
                // JSON schema/id remained stable.
                var byId = _rawByTypeAndId
                    .FirstOrDefault(pair =>
                        pair.Key.EndsWith(
                            "|" + id,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            pair.Value.Value<string>("sourceType"),
                            type.FullName,
                            StringComparison.Ordinal));
                raw = byId.Value;
            }

            if (raw == null)
                return null;

            return DeserializeAndFreeze(type, id, raw);
        }

        public static object GetByTypeName(string fullTypeName, string id)
        {
            EnsureLoaded();

            if (string.IsNullOrWhiteSpace(fullTypeName))
                return null;

            Type type = ResolveType(fullTypeName);
            if (type == null)
                return null;

            return Get(type, id);
        }

        public static IReadOnlyList<T> GetAll<T>() where T : class
        {
            EnsureLoaded();

            Type type = typeof(T);
            string prefix = type.FullName + "|";
            var result = new List<T>();

            foreach (var pair in _rawByTypeAndId)
            {
                if (!pair.Key.StartsWith(
                        prefix,
                        StringComparison.Ordinal))
                    continue;

                string id = pair.Value.Value<string>("id");
                if (Get(type, id) is T value)
                    result.Add(value);
            }

            return result;
        }

        /// <summary>
        /// Migration compatibility for old Resources.Load&lt;T&gt; config calls.
        /// It maps the legacy resource basename to the same slug used by the
        /// exporter, then falls back only when exactly one config of T exists.
        /// This is not an authoring path; JSON remains the source of truth.
        /// </summary>
        public static T GetLegacyResource<T>(string legacyResourcePath)
            where T : class
        {
            EnsureLoaded();

            string id = LegacySlug(
                System.IO.Path.GetFileName(
                    (legacyResourcePath ?? string.Empty)
                    .Replace('\\', '/')));

            if (!string.IsNullOrWhiteSpace(id))
            {
                T exact = Get<T>(id);
                if (exact != null)
                    return exact;
            }

            IReadOnlyList<T> all = GetAll<T>();
            if (all.Count == 1)
                return all[0];

            return null;
        }

        public static T[] GetAllLegacyResources<T>(string legacyResourcePath)
            where T : class
        {
            return GetAll<T>().ToArray();
        }

        private static string LegacySlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

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

            while (chars.Count > 0 && chars[chars.Count - 1] == '-')
                chars.RemoveAt(chars.Count - 1);

            return new string(chars.ToArray());
        }

        public static IReadOnlyList<object> GetAll(Type type)
        {
            EnsureLoaded();
            if (type == null)
                return Array.Empty<object>();

            string prefix = type.FullName + "|";
            var result = new List<object>();

            foreach (var pair in _rawByTypeAndId)
            {
                if (!pair.Key.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                string id = pair.Value.Value<string>("id");
                object value = Get(type, id);
                if (value != null)
                    result.Add(value);
            }

            return result;
        }

        public static UnityEngine.Object ResolveAsset(string key)
        {
            EnsureLoaded();
            return _catalog != null ? _catalog.Resolve(key) : null;
        }

        public static void ResetForExplicitReload()
        {
            lock (Sync)
            {
                _loaded = false;
                _rawByTypeAndId = null;
                _rawBySchemaAndId = null;
                _frozen = null;
                _catalog = null;
                _serializer = null;
                _fingerprint = string.Empty;
                MoyvaJsonTypeRegistry.ClearCache();
            }
        }

        private static void LoadAll()
        {
            _rawByTypeAndId = new Dictionary<string, JObject>(
                StringComparer.OrdinalIgnoreCase);
            _rawBySchemaAndId = new Dictionary<string, JObject>(
                StringComparer.OrdinalIgnoreCase);
            _frozen = new Dictionary<string, object>(
                StringComparer.OrdinalIgnoreCase);

            var catalogPrefab =
                Resources.Load<GameObject>("MoyvaRuntimeAssetCatalog");
            _catalog = catalogPrefab != null
                ? catalogPrefab.GetComponent<MoyvaJsonAssetCatalog>()
                : null;

            TextAsset[] files =
                Resources.LoadAll<TextAsset>("MoyvaConfigGenerated");

            if (files == null || files.Length == 0)
            {
                throw new InvalidOperationException(
                    "[MoyvaJson] No generated runtime JSON TextAssets found. " +
                    "Run the Jsonization build sync.");
            }

            var canonicalForHash = new List<string>();

            foreach (TextAsset file in files.OrderBy(
                         value => value.name,
                         StringComparer.Ordinal))
            {
                if (file == null || string.IsNullOrWhiteSpace(file.text))
                    continue;

                JObject root;
                try
                {
                    root = JObject.Parse(file.text);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"[MoyvaJson] {file.name}: invalid JSON. {ex.Message}",
                        ex);
                }

                string schema = root.Value<string>("schema");
                string id = root.Value<string>("id");
                string model = root.Value<string>("model");

                if (string.IsNullOrWhiteSpace(schema) ||
                    string.IsNullOrWhiteSpace(id) ||
                    string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidOperationException(
                        $"[MoyvaJson] {file.name}: root metadata " +
                        "schema/version/id/model is required.");
                }

                // Pass82 exported two Construction editor-authoring families into the generated
                // Resources folder. They were never runtime MoyvaJsonConfigObject models, so
                // they must not participate in the runtime registry. Keep the exception below
                // fail-closed for every other unresolved model/schema pair.
                if (IsLegacyEditorOnlyConstructionDocument(root, model, schema))
                    continue;

                Type resolvedType = MoyvaJsonTypeRegistry.ResolveConfigModel(model, schema);
                if (resolvedType == null)
                    throw new InvalidOperationException(
                        $"[MoyvaJson] {file.name}: no allow-listed runtime config type " +
                        $"for model='{model}', schema='{schema}'.");

                string sourceType = resolvedType.FullName;
                string typeKey = MakeKey(sourceType, id);
                if (_rawByTypeAndId.ContainsKey(typeKey))
                {
                    throw new InvalidOperationException(
                        $"[MoyvaJson] Duplicate config ID '{id}' " +
                        $"for sourceType '{sourceType}'.");
                }

                string schemaKey = MakeKey(schema, id);
                if (_rawBySchemaAndId.ContainsKey(schemaKey))
                {
                    throw new InvalidOperationException(
                        $"[MoyvaJson] Duplicate schema/id '{schema}/{id}'.");
                }

                _rawByTypeAndId[typeKey] = root;
                _rawBySchemaAndId[schemaKey] = root;

                canonicalForHash.Add(
                    root.ToString(Formatting.None));
            }

            using var sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(
                string.Join("\n", canonicalForHash));
            _fingerprint = BitConverter
                .ToString(sha.ComputeHash(bytes))
                .Replace("-", string.Empty)
                .ToLowerInvariant();

            _serializer = CreateSerializer();
        }

        private static bool IsLegacyEditorOnlyConstructionDocument(
            JObject root,
            string model,
            string schema)
        {
            if (root == null || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(schema))
                return false;

            var migration = root["migration"] as JObject;
            string sourceAssetPath = migration?.Value<string>("sourceAssetPath");
            if (string.IsNullOrWhiteSpace(sourceAssetPath))
                return false;

            string normalizedPath = sourceAssetPath.Replace('\\', '/');
            const string legacyTemplateRoot =
                "Assets/Moyva/Data/ScriptableObjects/Construction/Templates/";
            if (!normalizedPath.StartsWith(legacyTemplateRoot, StringComparison.OrdinalIgnoreCase))
                return false;

            return
                (string.Equals(model, "building-archetype", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(schema, "moyva.building-archetype", StringComparison.OrdinalIgnoreCase)) ||
                (string.Equals(model, "building-template-library", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(schema, "moyva.building-template-library", StringComparison.OrdinalIgnoreCase));
        }

        private static object DeserializeAndFreeze(
            Type type,
            string id,
            JObject root)
        {
            string key = MakeKey(type.FullName, id);
            if (_frozen.TryGetValue(key, out var cached))
                return cached;

            object instance;
            try
            {
                instance = MoyvaJsonObjectFactory.Create(type);
                if (instance == null) throw new InvalidOperationException("factory returned null");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"[MoyvaJson] Cannot create {type.FullName} for '{id}'.",
                    ex);
            }

            // Cache before Populate so cyclic config references can resolve.
            _frozen[key] = instance;

            JObject data = (JObject)root.DeepClone();
            data.Remove("$schema");
            data.Remove("schema");
            data.Remove("version");
            data.Remove("id");
            data.Remove("model");
            data.Remove("sourceType");
            data.Remove("sourceAssetGuid");
            data.Remove("sourceAssetPath");
            data.Remove("migration");

            try
            {
                using JsonReader reader = data.CreateReader();
                _serializer.Populate(reader, instance);
            }
            catch (Exception ex)
            {
                _frozen.Remove(key);
                throw new InvalidOperationException(
                    $"[MoyvaJson] Failed to deserialize '{id}' " +
                    $"as {type.FullName}: {ex.Message}",
                    ex);
            }

            ValidateObjectGraph(instance, id);

            if (instance is MoyvaJsonConfigObject config)
            {
                config.JsonId = id;
                config.JsonSchema = root.Value<string>("schema") ?? string.Empty;
                config.JsonVersion = root.Value<int?>("version") ?? 1;
                config.JsonSourcePath =
                    root.Value<string>("sourceAssetPath") ?? string.Empty;
                config.name = id;
            }

            return instance;
        }

        private static JsonSerializer CreateSerializer()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                MissingMemberHandling = MissingMemberHandling.Error,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                NullValueHandling = NullValueHandling.Include,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new MoyvaJsonContractResolver(),
            };

            settings.Converters.Add(new UnityValueConverter());
            settings.Converters.Add(new UnityObjectReferenceConverter());
            settings.Converters.Add(new ConfigReferenceConverter());
            settings.Converters.Add(new SafePolymorphicConverter());

            return JsonSerializer.Create(settings);
        }

        private static void ValidateObjectGraph(object root, string sourceId)
        {
            if (root == null)
                return;

            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            ValidateRecursive(root, "$", sourceId, visited, 0);
        }

        private static void ValidateRecursive(
            object value,
            string path,
            string sourceId,
            HashSet<object> visited,
            int depth)
        {
            if (value == null || depth > 32)
                return;

            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) ||
                type == typeof(decimal) || typeof(UnityEngine.Object).IsAssignableFrom(type))
                return;

            if (!type.IsValueType && !visited.Add(value))
                return;

            if (value is IEnumerable enumerable && !(value is string))
            {
                int index = 0;
                foreach (object item in enumerable)
                    ValidateRecursive(item, path + "[" + index++ + "]", sourceId, visited, depth + 1);
                return;
            }

            FieldInfo[] fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic || field.IsNotSerialized)
                    continue;

                bool serialized = field.IsPublic ||
                    field.GetCustomAttribute<SerializeField>() != null ||
                    field.GetCustomAttribute<SerializeReference>() != null;
                if (!serialized)
                    continue;

                object fieldValue = field.GetValue(value);
                string fieldPath = path + "." + field.Name.TrimStart('_');

                if (fieldValue is IConvertible convertible)
                {
                    var min = field.GetCustomAttribute<MinAttribute>();
                    if (min != null)
                    {
                        double numeric = Convert.ToDouble(convertible, System.Globalization.CultureInfo.InvariantCulture);
                        if (numeric < min.min)
                            throw new InvalidOperationException(
                                $"[MoyvaJson] {sourceId} {fieldPath}: expected >= {min.min}, got {numeric}.");
                    }

                    var range = field.GetCustomAttribute<RangeAttribute>();
                    if (range != null)
                    {
                        double numeric = Convert.ToDouble(convertible, System.Globalization.CultureInfo.InvariantCulture);
                        if (numeric < range.min || numeric > range.max)
                            throw new InvalidOperationException(
                                $"[MoyvaJson] {sourceId} {fieldPath}: expected {range.min}..{range.max}, got {numeric}.");
                    }
                }

                ValidateRecursive(fieldValue, fieldPath, sourceId, visited, depth + 1);
            }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        private static string MakeKey(string a, string b)
        {
            return (a ?? string.Empty) + "|" + (b ?? string.Empty);
        }

        private static Type ResolveType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false, false);
                if (type != null)
                    return type;
            }

            return null;
        }

        private sealed class UnityValueConverter : JsonConverter
        {
            public override bool CanWrite => false;
            public override bool CanConvert(Type t)
            {
                Type targetType = Nullable.GetUnderlyingType(t) ?? t;
                return targetType == typeof(Vector2) || targetType == typeof(Vector2Int) ||
                       targetType == typeof(Vector3) || targetType == typeof(Vector3Int) ||
                       targetType == typeof(Vector4) || targetType == typeof(Quaternion) ||
                       targetType == typeof(Color) || targetType == typeof(Color32) || targetType == typeof(LayerMask) ||
                       targetType == typeof(Rect) || targetType == typeof(RectInt) || targetType == typeof(Bounds) || targetType == typeof(BoundsInt) ||
                       targetType == typeof(Gradient) || targetType == typeof(AnimationCurve);
            }

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                Type targetType = Nullable.GetUnderlyingType(t) ?? t;
                JToken token = JToken.Load(reader);

                if ((targetType == typeof(Color) || targetType == typeof(Color32))
                    && token.Type == JTokenType.String)
                {
                    string colorText = token.Value<string>();
                    if (!string.IsNullOrWhiteSpace(colorText))
                    {
                        string htmlColor = colorText.Trim();
                        if (!htmlColor.StartsWith("#", StringComparison.Ordinal))
                            htmlColor = "#" + htmlColor;

                        if (ColorUtility.TryParseHtmlString(htmlColor, out Color parsed))
                        {
                            if (targetType == typeof(Color32))
                                return (Color32)parsed;

                            return parsed;
                        }
                    }

                    throw new JsonSerializationException("Invalid Unity color value " + colorText);
                }

                if (targetType == typeof(LayerMask))
                {
                    int v = Convert.ToInt32(token, System.Globalization.CultureInfo.InvariantCulture);
                    LayerMask m = new LayerMask(); m.value = v; return m;
                }

                if (token is not JObject o)
                    throw new JsonSerializationException("Expected object for Unity value type " + targetType.FullName);

                float F(string n) => o.Value<float?>(n) ?? 0f;
                int I(string n) => o.Value<int?>(n) ?? 0;
                if (targetType == typeof(Vector2)) return new Vector2(F("x"), F("y"));
                if (targetType == typeof(Vector2Int)) return new Vector2Int(I("x"), I("y"));
                if (targetType == typeof(Vector3)) return new Vector3(F("x"), F("y"), F("z"));
                if (targetType == typeof(Vector3Int)) return new Vector3Int(I("x"), I("y"), I("z"));
                if (targetType == typeof(Vector4)) return new Vector4(F("x"), F("y"), F("z"), F("w"));
                if (targetType == typeof(Quaternion)) return new Quaternion(F("x"), F("y"), F("z"), F("w"));
                if (targetType == typeof(Color)) return new Color(F("r"), F("g"), F("b"), o.Value<float?>("a") ?? 1f);
                if (targetType == typeof(Color32)) return new Color32((byte)I("r"), (byte)I("g"), (byte)I("b"), (byte)(o.Value<int?>("a") ?? 255));
                if (targetType == typeof(Rect)) return new Rect(F("x"), F("y"), F("width"), F("height"));
                if (targetType == typeof(RectInt)) return new RectInt(I("x"), I("y"), I("width"), I("height"));
                if (targetType == typeof(Bounds)) return new Bounds(
                    new Vector3(o["center"]?.Value<float?>("x") ?? 0f, o["center"]?.Value<float?>("y") ?? 0f, o["center"]?.Value<float?>("z") ?? 0f),
                    new Vector3(o["size"]?.Value<float?>("x") ?? 0f, o["size"]?.Value<float?>("y") ?? 0f, o["size"]?.Value<float?>("z") ?? 0f));
                if (targetType == typeof(BoundsInt)) return new BoundsInt(
                    new Vector3Int(o["position"]?.Value<int?>("x") ?? 0, o["position"]?.Value<int?>("y") ?? 0, o["position"]?.Value<int?>("z") ?? 0),
                    new Vector3Int(o["size"]?.Value<int?>("x") ?? 0, o["size"]?.Value<int?>("y") ?? 0, o["size"]?.Value<int?>("z") ?? 0));
                if (targetType == typeof(Gradient))
                {
                    var gradient = new Gradient();
                    var colors = new List<GradientColorKey>();
                    var alphas = new List<GradientAlphaKey>();
                    if (o["colorKeys"] is JArray colorKeys)
                        foreach (JObject k in colorKeys.OfType<JObject>())
                            colors.Add(new GradientColorKey(
                                new Color(k["color"]?.Value<float?>("r") ?? 0f, k["color"]?.Value<float?>("g") ?? 0f, k["color"]?.Value<float?>("b") ?? 0f, k["color"]?.Value<float?>("a") ?? 1f),
                                k.Value<float?>("time") ?? 0f));
                    if (o["alphaKeys"] is JArray alphaKeys)
                        foreach (JObject k in alphaKeys.OfType<JObject>())
                            alphas.Add(new GradientAlphaKey(k.Value<float?>("alpha") ?? 1f, k.Value<float?>("time") ?? 0f));
                    gradient.SetKeys(colors.ToArray(), alphas.ToArray());
                    if (Enum.TryParse(o.Value<string>("mode"), true, out GradientMode mode)) gradient.mode = mode;
                    return gradient;
                }
                if (targetType == typeof(AnimationCurve))
                {
                    var list = new List<Keyframe>();
                    if (o["keys"] is JArray keys)
                    {
                        foreach (JObject k in keys.OfType<JObject>())
                        {
                            var frame = new Keyframe(
                                k.Value<float?>("time") ?? 0f,
                                k.Value<float?>("value") ?? 0f,
                                k.Value<float?>("inTangent") ?? 0f,
                                k.Value<float?>("outTangent") ?? 0f,
                                k.Value<float?>("inWeight") ?? 0f,
                                k.Value<float?>("outWeight") ?? 0f);
                            if (Enum.TryParse(k.Value<string>("weightedMode"), true, out WeightedMode weighted))
                                frame.weightedMode = weighted;
                            list.Add(frame);
                        }
                    }
                    var curve = new AnimationCurve(list.ToArray());
                    if (Enum.TryParse(o.Value<string>("preWrapMode"), true, out WrapMode pre)) curve.preWrapMode = pre;
                    if (Enum.TryParse(o.Value<string>("postWrapMode"), true, out WrapMode post)) curve.postWrapMode = post;
                    return curve;
                }
                throw new JsonSerializationException("Unsupported Unity value type " + targetType.FullName);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                => throw new NotSupportedException();
        }

        private sealed class UnityObjectReferenceConverter : JsonConverter
        {
            public override bool CanWrite => false;

            public override bool CanConvert(Type objectType)
            {
                return typeof(UnityEngine.Object).IsAssignableFrom(objectType);
            }

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                JObject token = JObject.Load(reader);
                string key = token.Value<string>("$asset");
                string inlineTypeName = token.Value<string>("$type");
                if (!string.IsNullOrWhiteSpace(inlineTypeName))
                {
                    Type inlineType = ResolveType(inlineTypeName);
                    if (!IsAllowedInlineUnityObjectType(inlineType, objectType))
                    {
                        throw new JsonSerializationException(
                            $"Inline Unity object type '{inlineTypeName}' is not allow-listed for {objectType.FullName}.");
                    }

                    object instance = MoyvaJsonObjectFactory.Create(inlineType);
                    if (instance == null)
                        throw new JsonSerializationException($"Could not create inline Unity object '{inlineTypeName}'.");

                    token.Remove("$type");
                    using JsonReader tokenReader = token.CreateReader();
                    serializer.Populate(tokenReader, instance);
                    return instance;
                }

                UnityEngine.Object asset = ResolveAsset(key);

                if (asset == null)
                {
                    bool required = token.Value<bool?>("required") ?? false;
                    if (required)
                    {
                        throw new JsonSerializationException(
                            $"Unknown required Unity asset key '{key}'.");
                    }

                    return null;
                }

                if (!objectType.IsInstanceOfType(asset))
                {
                    throw new JsonSerializationException(
                        $"Asset '{key}' is {asset.GetType().FullName}, " +
                        $"expected {objectType.FullName}.");
                }

                return asset;
            }

            private static bool IsAllowedInlineUnityObjectType(Type type, Type expectedType)
            {
                if (type == null || expectedType == null || type.IsAbstract ||
                    !expectedType.IsAssignableFrom(type) ||
                    !typeof(ScriptableObject).IsAssignableFrom(type))
                    return false;

                string ns = type.Namespace ?? string.Empty;
                return ns.StartsWith("Kruty1918.Moyva", StringComparison.Ordinal) ||
                       ns.StartsWith("GiantGrey.TileWorldCreator", StringComparison.Ordinal);
            }

            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class ConfigReferenceConverter : JsonConverter
        {
            public override bool CanWrite => false;

            public override bool CanConvert(Type objectType)
            {
                return typeof(MoyvaJsonConfigObject).IsAssignableFrom(objectType);
            }

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                JObject token = JObject.Load(reader);
                JObject reference = token["$config"] as JObject;

                if (reference != null)
                {
                    string id = reference.Value<string>("id");
                    string typeName = reference.Value<string>("sourceType");
                    string model = reference.Value<string>("model");

                    Type actual = !string.IsNullOrWhiteSpace(typeName)
                        ? ResolveType(typeName)
                        : MoyvaJsonTypeRegistry.ResolveConfigModel(
                            model,
                            MoyvaJsonTypeRegistry.SchemaForConfigType(objectType));
                    actual ??= objectType;

                    if (actual == null ||
                        !objectType.IsAssignableFrom(actual))
                    {
                        actual = objectType;
                    }

                    object resolved = Get(actual, id);
                    if (resolved == null)
                        throw new JsonSerializationException(
                            $"Unknown config reference '{actual.FullName}/{id}'.");
                    return resolved;
                }

                Type inlineType = objectType;
                if (objectType.IsAbstract || objectType.IsInterface)
                {
                    string typeId = token.Value<string>("$type") ?? token.Value<string>("type");
                    inlineType = MoyvaJsonTypeRegistry.Resolve(objectType, typeId);
                    if (inlineType == null)
                        throw new JsonSerializationException(
                            $"Unknown allow-listed inline config type '{typeId}' for {objectType.FullName}.");
                    token.Remove("$type");
                }

                object instance = existingValue ?? MoyvaJsonObjectFactory.Create(inlineType);
                using JsonReader nested = token.CreateReader();
                serializer.Populate(nested, instance);
                return instance;
            }

            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class SafePolymorphicConverter : JsonConverter
        {
            public override bool CanWrite => false;

            public override bool CanConvert(Type objectType)
            {
                if (objectType == typeof(string) ||
                    objectType.IsPrimitive ||
                    objectType.IsEnum ||
                    typeof(UnityEngine.Object).IsAssignableFrom(objectType) ||
                    typeof(MoyvaJsonConfigObject).IsAssignableFrom(objectType))
                {
                    return false;
                }

                return objectType.IsAbstract || objectType.IsInterface;
            }

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                JObject token = JObject.Load(reader);
                string typeId =
                    token.Value<string>("$type") ??
                    token.Value<string>("type");

                Type actual =
                    MoyvaJsonTypeRegistry.Resolve(objectType, typeId);

                if (actual == null)
                {
                    throw new JsonSerializationException(
                        $"Unknown allow-listed type '{typeId}' " +
                        $"for {objectType.FullName}.");
                }

                token.Remove("$type");
                object instance = MoyvaJsonObjectFactory.Create(actual);

                using JsonReader nested = token.CreateReader();
                serializer.Populate(nested, instance);
                return instance;
            }

            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }
    }
}
