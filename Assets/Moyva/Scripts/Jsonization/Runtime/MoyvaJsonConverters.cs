using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization
{
    /// <summary>
    /// Newtonsoft converter-и runtime JSON. Усі працюють тільки на читання:
    /// JSON є source of truth, а runtime не записує його назад.
    /// </summary>
    internal sealed class UnityValueConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type type)
        {
            Type target = Nullable.GetUnderlyingType(type) ?? type;
            return target == typeof(Vector2) ||
                   target == typeof(Vector2Int) ||
                   target == typeof(Vector3) ||
                   target == typeof(Vector3Int) ||
                   target == typeof(Vector4) ||
                   target == typeof(Quaternion) ||
                   target == typeof(Color) ||
                   target == typeof(Color32) ||
                   target == typeof(LayerMask) ||
                   target == typeof(Rect) ||
                   target == typeof(RectInt) ||
                   target == typeof(Bounds) ||
                   target == typeof(BoundsInt) ||
                   target == typeof(Gradient) ||
                   target == typeof(AnimationCurve);
        }

        public override object ReadJson(
            JsonReader reader,
            Type type,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            Type target = Nullable.GetUnderlyingType(type) ?? type;
            JToken token = JToken.Load(reader);
            if ((target == typeof(Color) || target == typeof(Color32)) &&
                token.Type == JTokenType.String)
            {
                return ReadHtmlColor(target, token.Value<string>());
            }

            if (target == typeof(LayerMask))
            {
                int value = Convert.ToInt32(token, CultureInfo.InvariantCulture);
                return new LayerMask { value = value };
            }

            if (token is not JObject valueObject)
            {
                throw new JsonSerializationException(
                    "Expected object for Unity value type " + target.FullName);
            }

            return ReadUnityObjectValue(target, valueObject);
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
            => throw new NotSupportedException();

        private static object ReadHtmlColor(Type target, string value)
        {
            string text = (value ?? string.Empty).Trim();
            if (!text.StartsWith("#", StringComparison.Ordinal))
                text = "#" + text;

            if (!ColorUtility.TryParseHtmlString(text, out Color color))
                throw new JsonSerializationException("Invalid Unity color value " + value);

            return target == typeof(Color32) ? (Color32)color : color;
        }

        private static object ReadUnityObjectValue(Type target, JObject value)
        {
            if (target == typeof(Vector2)) return new Vector2(F(value, "x"), F(value, "y"));
            if (target == typeof(Vector2Int)) return new Vector2Int(I(value, "x"), I(value, "y"));
            if (target == typeof(Vector3)) return new Vector3(F(value, "x"), F(value, "y"), F(value, "z"));
            if (target == typeof(Vector3Int)) return new Vector3Int(I(value, "x"), I(value, "y"), I(value, "z"));
            if (target == typeof(Vector4)) return new Vector4(F(value, "x"), F(value, "y"), F(value, "z"), F(value, "w"));
            if (target == typeof(Quaternion)) return new Quaternion(F(value, "x"), F(value, "y"), F(value, "z"), F(value, "w"));
            if (target == typeof(Color)) return new Color(F(value, "r"), F(value, "g"), F(value, "b"), value.Value<float?>("a") ?? 1f);
            if (target == typeof(Color32)) return new Color32((byte)I(value, "r"), (byte)I(value, "g"), (byte)I(value, "b"), (byte)(value.Value<int?>("a") ?? 255));
            if (target == typeof(Rect)) return new Rect(F(value, "x"), F(value, "y"), F(value, "width"), F(value, "height"));
            if (target == typeof(RectInt)) return new RectInt(I(value, "x"), I(value, "y"), I(value, "width"), I(value, "height"));
            if (target == typeof(Bounds)) return ReadBounds(value);
            if (target == typeof(BoundsInt)) return ReadBoundsInt(value);
            if (target == typeof(Gradient)) return ReadGradient(value);
            if (target == typeof(AnimationCurve)) return ReadAnimationCurve(value);

            throw new JsonSerializationException("Unsupported Unity value type " + target.FullName);
        }

        private static Bounds ReadBounds(JObject value)
            => new(
                new Vector3(
                    value["center"]?.Value<float?>("x") ?? 0f,
                    value["center"]?.Value<float?>("y") ?? 0f,
                    value["center"]?.Value<float?>("z") ?? 0f),
                new Vector3(
                    value["size"]?.Value<float?>("x") ?? 0f,
                    value["size"]?.Value<float?>("y") ?? 0f,
                    value["size"]?.Value<float?>("z") ?? 0f));

        private static BoundsInt ReadBoundsInt(JObject value)
            => new(
                new Vector3Int(
                    value["position"]?.Value<int?>("x") ?? 0,
                    value["position"]?.Value<int?>("y") ?? 0,
                    value["position"]?.Value<int?>("z") ?? 0),
                new Vector3Int(
                    value["size"]?.Value<int?>("x") ?? 0,
                    value["size"]?.Value<int?>("y") ?? 0,
                    value["size"]?.Value<int?>("z") ?? 0));

        private static Gradient ReadGradient(JObject value)
        {
            var gradient = new Gradient();
            var colors = new List<GradientColorKey>();
            var alphas = new List<GradientAlphaKey>();
            if (value["colorKeys"] is JArray colorKeys)
            {
                foreach (JObject key in colorKeys.OfType<JObject>())
                {
                    colors.Add(new GradientColorKey(
                        new Color(
                            key["color"]?.Value<float?>("r") ?? 0f,
                            key["color"]?.Value<float?>("g") ?? 0f,
                            key["color"]?.Value<float?>("b") ?? 0f,
                            key["color"]?.Value<float?>("a") ?? 1f),
                        key.Value<float?>("time") ?? 0f));
                }
            }

            if (value["alphaKeys"] is JArray alphaKeys)
            {
                foreach (JObject key in alphaKeys.OfType<JObject>())
                    alphas.Add(new GradientAlphaKey(
                        key.Value<float?>("alpha") ?? 1f,
                        key.Value<float?>("time") ?? 0f));
            }

            gradient.SetKeys(colors.ToArray(), alphas.ToArray());
            if (Enum.TryParse(value.Value<string>("mode"), true, out GradientMode mode))
                gradient.mode = mode;
            return gradient;
        }

        private static AnimationCurve ReadAnimationCurve(JObject value)
        {
            var keys = new List<Keyframe>();
            if (value["keys"] is JArray jsonKeys)
            {
                foreach (JObject key in jsonKeys.OfType<JObject>())
                {
                    var frame = new Keyframe(
                        key.Value<float?>("time") ?? 0f,
                        key.Value<float?>("value") ?? 0f,
                        key.Value<float?>("inTangent") ?? 0f,
                        key.Value<float?>("outTangent") ?? 0f,
                        key.Value<float?>("inWeight") ?? 0f,
                        key.Value<float?>("outWeight") ?? 0f);
                    if (Enum.TryParse(key.Value<string>("weightedMode"), true, out WeightedMode weighted))
                        frame.weightedMode = weighted;
                    keys.Add(frame);
                }
            }

            var curve = new AnimationCurve(keys.ToArray());
            if (Enum.TryParse(value.Value<string>("preWrapMode"), true, out WrapMode pre))
                curve.preWrapMode = pre;
            if (Enum.TryParse(value.Value<string>("postWrapMode"), true, out WrapMode post))
                curve.postWrapMode = post;
            return curve;
        }

        private static float F(JObject value, string name)
            => value.Value<float?>(name) ?? 0f;

        private static int I(JObject value, string name)
            => value.Value<int?>(name) ?? 0;
    }

    /// <summary>
    /// Резолвить UnityEngine.Object refs із runtime asset catalog або дозволені inline SO.
    /// </summary>
    internal sealed class UnityObjectReferenceConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
            => typeof(UnityEngine.Object).IsAssignableFrom(objectType);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject token = JObject.Load(reader);
            string inlineTypeName = token.Value<string>("$type");
            if (!string.IsNullOrWhiteSpace(inlineTypeName))
                return ReadInlineUnityObject(token, inlineTypeName, objectType, serializer);

            UnityEngine.Object asset = MoyvaJsonRuntime.ResolveAsset(token.Value<string>("$asset"));
            if (asset == null)
            {
                bool required = token.Value<bool?>("required") ?? false;
                if (required)
                    throw new JsonSerializationException(
                        $"Unknown required Unity asset key '{token.Value<string>("$asset")}'.");
                return null;
            }

            if (!objectType.IsInstanceOfType(asset))
            {
                throw new JsonSerializationException(
                    $"Asset '{token.Value<string>("$asset")}' is {asset.GetType().FullName}, expected {objectType.FullName}.");
            }

            return asset;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotSupportedException();

        private static object ReadInlineUnityObject(
            JObject token,
            string inlineTypeName,
            Type expectedType,
            JsonSerializer serializer)
        {
            Type inlineType = MoyvaJsonRuntimeTypeResolver.Resolve(inlineTypeName);
            if (!IsAllowedInlineUnityObjectType(inlineType, expectedType))
            {
                throw new JsonSerializationException(
                    $"Inline Unity object type '{inlineTypeName}' is not allow-listed for {expectedType.FullName}.");
            }

            object instance = MoyvaJsonObjectFactory.Create(inlineType);
            if (instance == null)
                throw new JsonSerializationException(
                    $"Could not create inline Unity object '{inlineTypeName}'.");

            token.Remove("$type");
            using JsonReader tokenReader = token.CreateReader();
            serializer.Populate(tokenReader, instance);
            return instance;
        }

        private static bool IsAllowedInlineUnityObjectType(Type type, Type expectedType)
        {
            if (type == null ||
                expectedType == null ||
                type.IsAbstract ||
                !expectedType.IsAssignableFrom(type) ||
                !typeof(ScriptableObject).IsAssignableFrom(type))
            {
                return false;
            }

            string ns = type.Namespace ?? string.Empty;
            return ns.StartsWith("Kruty1918.Moyva", StringComparison.Ordinal) ||
                   ns.StartsWith("GiantGrey.TileWorldCreator", StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Резолвить `$config` між Moyva JSON-документами і безпечно читає inline config values.
    /// </summary>
    internal sealed class ConfigReferenceConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
            => typeof(MoyvaJsonConfigObject).IsAssignableFrom(objectType);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject token = JObject.Load(reader);
            if (token["$config"] is JObject reference)
                return ReadConfigReference(reference, objectType);

            return ReadInlineConfig(token, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotSupportedException();

        private static object ReadConfigReference(JObject reference, Type expectedType)
        {
            string id = reference.Value<string>("id");
            string typeName = reference.Value<string>("sourceType");
            string model = reference.Value<string>("model");
            Type actual = !string.IsNullOrWhiteSpace(typeName)
                ? MoyvaJsonRuntimeTypeResolver.Resolve(typeName)
                : MoyvaJsonTypeRegistry.ResolveConfigModel(
                    model,
                    MoyvaJsonTypeRegistry.SchemaForConfigType(expectedType));
            actual ??= expectedType;

            if (actual == null || !expectedType.IsAssignableFrom(actual))
                actual = expectedType;

            object resolved = MoyvaJsonRuntime.Get(actual, id);
            if (resolved == null)
                throw new JsonSerializationException(
                    $"Unknown config reference '{actual.FullName}/{id}'.");
            return resolved;
        }

        private static object ReadInlineConfig(
            JObject token,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            Type inlineType = objectType;
            if (objectType.IsAbstract || objectType.IsInterface)
            {
                string typeId = token.Value<string>("$type") ?? token.Value<string>("type");
                inlineType = MoyvaJsonTypeRegistry.Resolve(objectType, typeId);
                if (inlineType == null)
                {
                    throw new JsonSerializationException(
                        $"Unknown allow-listed inline config type '{typeId}' for {objectType.FullName}.");
                }
                token.Remove("$type");
            }

            object instance = existingValue ?? MoyvaJsonObjectFactory.Create(inlineType);
            using JsonReader nested = token.CreateReader();
            serializer.Populate(nested, instance);
            return instance;
        }
    }

    /// <summary>
    /// Безпечний polymorphic converter: створює тільки типи, дозволені MoyvaJsonTypeRegistry.
    /// </summary>
    internal sealed class SafePolymorphicConverter : JsonConverter
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
            string typeId = token.Value<string>("$type") ?? token.Value<string>("type");
            Type actual = MoyvaJsonTypeRegistry.Resolve(objectType, typeId);
            if (actual == null)
            {
                throw new JsonSerializationException(
                    $"Unknown allow-listed type '{typeId}' for {objectType.FullName}.");
            }

            token.Remove("$type");
            object instance = MoyvaJsonObjectFactory.Create(actual);
            using JsonReader nested = token.CreateReader();
            serializer.Populate(nested, instance);
            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotSupportedException();
    }

    internal static class MoyvaJsonRuntimeTypeResolver
    {
        public static Type Resolve(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false, false);
                if (type != null)
                    return type;
            }

            return null;
        }
    }
}
