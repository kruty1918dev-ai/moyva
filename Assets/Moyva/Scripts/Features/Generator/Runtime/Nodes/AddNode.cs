using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    /// <summary>
    /// Universal typed Add/Merge node for the Moyva graph editor.
    /// This is intentionally separate from TileWorldCreator's native Add modifier,
    /// which adds Blueprint Layers. Graph Add works with graph data: masks, maps,
    /// numeric values, vectors and strings.
    /// </summary>
    [NodeInfo("Add", "Math", "Typed графова Add-нода. Визначає підтримуваний тип за підключенням, адаптує порти і повертає результат того самого типу.")]
    public sealed class AddNode : NodeBase, IPreviewableNode
    {
        public enum AddValueKind
        {
            Any = 0,
            Bool = 1,
            Int = 2,
            Float = 3,
            String = 4,
            BoolMask = 10,
            IntMap = 11,
            FloatMap = 12,
            StringMap = 13,
            Vector2 = 20,
            Vector3 = 21,
            Vector2Int = 22,
            Vector3Int = 23
        }

        [SerializeField, HideInInspector] private AddValueKind _valueKind = AddValueKind.Any;

        [NonSerialized] private object _lastResult;

        public AddValueKind ValueKind => _valueKind;
        public bool IsTypeResolved => _valueKind != AddValueKind.Any;
        public Type ResolvedValueType => ResolveType(_valueKind);

        public override string Title => _valueKind == AddValueKind.Any
            ? "Add"
            : $"Add ({FormatKind(_valueKind)})";

        public override string Category => "Math";

        public override PortDefinition[] Inputs
        {
            get
            {
                var type = ResolvedValueType ?? typeof(object);
                return new[]
                {
                    new PortDefinition(BuildInputName("A", type), type, PortDirection.Input),
                    new PortDefinition(BuildInputName("B", type), type, PortDirection.Input)
                };
            }
        }

        public override PortDefinition[] Outputs
        {
            get
            {
                var type = ResolvedValueType ?? typeof(object);
                return new[]
                {
                    new PortDefinition(BuildOutputName(type), type, PortDirection.Output)
                };
            }
        }

        /// <summary>
        /// Called by the editor when a connection to this node is created/removed.
        /// Returns true when the serialized node type changed and the graph view should be rebuilt.
        /// </summary>
        public bool TrySetValueType(Type valueType)
        {
            if (!TryGetSupportedKind(valueType, out var kind))
                return false;

            return SetValueKind(kind);
        }

        public bool SetValueKind(AddValueKind kind)
        {
            if (_valueKind == kind)
                return false;

            _valueKind = kind;
            return true;
        }

        public bool ResetValueType()
        {
            if (_valueKind == AddValueKind.Any)
                return false;

            _valueKind = AddValueKind.Any;
            return true;
        }

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            object a = inputs != null && inputs.Length > 0 ? inputs[0] : null;
            object b = inputs != null && inputs.Length > 1 ? inputs[1] : null;

            var kind = _valueKind;
            if (kind == AddValueKind.Any)
            {
                var runtimeType = ResolveRuntimeType(a, b);
                if (!TryGetSupportedKind(runtimeType, out kind))
                    return NodeOutput.Error($"Add node не підтримує тип '{runtimeType?.Name ?? "null"}'. Підтримуються bool, int, float, string, Vector2/3, Vector2Int/3Int, bool[,], int[,], float[,] і string[,].");
            }

            if (!ValidateRuntimeValues(kind, a, b, out var validationError))
                return NodeOutput.Error(validationError);

            object result;
            try
            {
                result = kind switch
                {
                    AddValueKind.Bool => (bool)a || (bool)b,
                    AddValueKind.Int => (int)a + (int)b,
                    AddValueKind.Float => (float)a + (float)b,
                    AddValueKind.String => string.Concat(a as string ?? string.Empty, b as string ?? string.Empty),
                    AddValueKind.BoolMask => AddBoolMasks((bool[,])a, (bool[,])b),
                    AddValueKind.IntMap => AddIntMaps((int[,])a, (int[,])b),
                    AddValueKind.FloatMap => AddFloatMaps((float[,])a, (float[,])b),
                    AddValueKind.StringMap => MergeStringMaps((string[,])a, (string[,])b),
                    AddValueKind.Vector2 => (Vector2)a + (Vector2)b,
                    AddValueKind.Vector3 => (Vector3)a + (Vector3)b,
                    AddValueKind.Vector2Int => (Vector2Int)a + (Vector2Int)b,
                    AddValueKind.Vector3Int => (Vector3Int)a + (Vector3Int)b,
                    _ => null
                };
            }
            catch (Exception ex)
            {
                return NodeOutput.Error($"Add node не зміг виконати тип '{FormatKind(kind)}': {ex.Message}");
            }

            _lastResult = result;
            return NodeOutput.Success(result);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            int tw = Mathf.Max(16, width);
            int th = Mathf.Max(16, height);
            var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            if (_lastResult is bool[,] boolMask)
            {
                DrawBoolMask(tex, boolMask, tw, th);
                return tex;
            }

            if (_lastResult is float[,] floatMap)
            {
                DrawFloatMap(tex, floatMap, tw, th);
                return tex;
            }

            if (_lastResult is int[,] intMap)
            {
                DrawIntMap(tex, intMap, tw, th);
                return tex;
            }

            if (_lastResult is string[,] stringMap)
            {
                DrawStringMap(tex, stringMap, tw, th);
                return tex;
            }

            DrawNoData(tex, tw, th);
            return tex;
        }

        public static bool TryGetSupportedKind(Type type, out AddValueKind kind)
        {
            kind = AddValueKind.Any;
            if (type == null || type == typeof(object))
                return false;

            if (type == typeof(bool)) kind = AddValueKind.Bool;
            else if (type == typeof(int)) kind = AddValueKind.Int;
            else if (type == typeof(float)) kind = AddValueKind.Float;
            else if (type == typeof(string)) kind = AddValueKind.String;
            else if (type == typeof(bool[,])) kind = AddValueKind.BoolMask;
            else if (type == typeof(int[,])) kind = AddValueKind.IntMap;
            else if (type == typeof(float[,])) kind = AddValueKind.FloatMap;
            else if (type == typeof(string[,])) kind = AddValueKind.StringMap;
            else if (type == typeof(Vector2)) kind = AddValueKind.Vector2;
            else if (type == typeof(Vector3)) kind = AddValueKind.Vector3;
            else if (type == typeof(Vector2Int)) kind = AddValueKind.Vector2Int;
            else if (type == typeof(Vector3Int)) kind = AddValueKind.Vector3Int;
            else return false;

            return true;
        }

        public static Type ResolveType(AddValueKind kind)
        {
            return kind switch
            {
                AddValueKind.Bool => typeof(bool),
                AddValueKind.Int => typeof(int),
                AddValueKind.Float => typeof(float),
                AddValueKind.String => typeof(string),
                AddValueKind.BoolMask => typeof(bool[,]),
                AddValueKind.IntMap => typeof(int[,]),
                AddValueKind.FloatMap => typeof(float[,]),
                AddValueKind.StringMap => typeof(string[,]),
                AddValueKind.Vector2 => typeof(Vector2),
                AddValueKind.Vector3 => typeof(Vector3),
                AddValueKind.Vector2Int => typeof(Vector2Int),
                AddValueKind.Vector3Int => typeof(Vector3Int),
                _ => null
            };
        }

        private static string BuildInputName(string prefix, Type type)
        {
            return type == typeof(object)
                ? $"{prefix} (any)"
                : $"{prefix} ({FormatType(type)})";
        }

        private static string BuildOutputName(Type type)
        {
            return type == typeof(object)
                ? "Result (any)"
                : $"Result ({FormatType(type)})";
        }

        private static string FormatKind(AddValueKind kind)
        {
            return kind switch
            {
                AddValueKind.Bool => "bool",
                AddValueKind.Int => "int",
                AddValueKind.Float => "float",
                AddValueKind.String => "string",
                AddValueKind.BoolMask => "bool[,] mask",
                AddValueKind.IntMap => "int[,] map",
                AddValueKind.FloatMap => "float[,] map",
                AddValueKind.StringMap => "string[,] map",
                AddValueKind.Vector2 => "Vector2",
                AddValueKind.Vector3 => "Vector3",
                AddValueKind.Vector2Int => "Vector2Int",
                AddValueKind.Vector3Int => "Vector3Int",
                _ => "any"
            };
        }

        private static string FormatType(Type type)
        {
            if (type == typeof(bool[,])) return "bool[,]";
            if (type == typeof(int[,])) return "int[,]";
            if (type == typeof(float[,])) return "float[,]";
            if (type == typeof(string[,])) return "string[,]";
            return type?.Name ?? "any";
        }

        private static Type ResolveRuntimeType(object a, object b)
        {
            if (a != null)
                return a.GetType();
            if (b != null)
                return b.GetType();
            return null;
        }

        private static bool ValidateRuntimeValues(AddValueKind kind, object a, object b, out string error)
        {
            error = null;
            var type = ResolveType(kind);
            if (type == null)
            {
                error = "Add node не має визначеного підтримуваного типу.";
                return false;
            }

            if (a == null || b == null)
            {
                error = $"Add node очікує два підключені значення типу '{FormatKind(kind)}'. Один із входів порожній.";
                return false;
            }

            if (!type.IsInstanceOfType(a) || !type.IsInstanceOfType(b))
            {
                error = $"Add node типізована як '{FormatKind(kind)}', але отримала '{a.GetType().Name}' і '{b.GetType().Name}'. Перепідключи входи одного типу.";
                return false;
            }

            if (a is Array arrayA && b is Array arrayB)
            {
                if (arrayA.Rank != 2 || arrayB.Rank != 2)
                {
                    error = "Add node підтримує тільки двовимірні карти/маски для array-типів.";
                    return false;
                }

                if (arrayA.GetLength(0) != arrayB.GetLength(0) || arrayA.GetLength(1) != arrayB.GetLength(1))
                {
                    error = $"Add node отримала карти різного розміру: {arrayA.GetLength(0)}x{arrayA.GetLength(1)} і {arrayB.GetLength(0)}x{arrayB.GetLength(1)}.";
                    return false;
                }
            }

            return true;
        }

        private static bool[,] AddBoolMasks(bool[,] a, bool[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new bool[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = a[x, y] || b[x, y];
            return result;
        }

        private static int[,] AddIntMaps(int[,] a, int[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = a[x, y] + b[x, y];
            return result;
        }

        private static float[,] AddFloatMaps(float[,] a, float[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = a[x, y] + b[x, y];
            return result;
        }

        private static string[,] MergeStringMaps(string[,] a, string[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new string[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = string.IsNullOrEmpty(b[x, y]) ? a[x, y] : b[x, y];
            return result;
        }

        private static void DrawBoolMask(Texture2D tex, bool[,] mask, int width, int height)
        {
            int sw = mask.GetLength(0);
            int sh = mask.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                int sy = y * sh / height;
                for (int x = 0; x < width; x++)
                {
                    int sx = x * sw / width;
                    tex.SetPixel(x, y, mask[sx, sy] ? Color.white : Color.black);
                }
            }
            tex.Apply(false, false);
        }

        private static void DrawFloatMap(Texture2D tex, float[,] map, int width, int height)
        {
            int sw = map.GetLength(0);
            int sh = map.GetLength(1);
            FindFloatRange(map, out float min, out float max);
            float range = Mathf.Max(0.0001f, max - min);

            for (int y = 0; y < height; y++)
            {
                int sy = y * sh / height;
                for (int x = 0; x < width; x++)
                {
                    int sx = x * sw / width;
                    float t = Mathf.Clamp01((map[sx, sy] - min) / range);
                    tex.SetPixel(x, y, new Color(t, t, t, 1f));
                }
            }
            tex.Apply(false, false);
        }

        private static void DrawIntMap(Texture2D tex, int[,] map, int width, int height)
        {
            int sw = map.GetLength(0);
            int sh = map.GetLength(1);
            FindIntRange(map, out int min, out int max);
            float range = Mathf.Max(1f, max - min);

            for (int y = 0; y < height; y++)
            {
                int sy = y * sh / height;
                for (int x = 0; x < width; x++)
                {
                    int sx = x * sw / width;
                    float t = Mathf.Clamp01((map[sx, sy] - min) / range);
                    tex.SetPixel(x, y, new Color(t, t, t, 1f));
                }
            }
            tex.Apply(false, false);
        }

        private static void DrawStringMap(Texture2D tex, string[,] map, int width, int height)
        {
            int sw = map.GetLength(0);
            int sh = map.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                int sy = y * sh / height;
                for (int x = 0; x < width; x++)
                {
                    int sx = x * sw / width;
                    string value = map[sx, sy];
                    tex.SetPixel(x, y, string.IsNullOrEmpty(value)
                        ? Color.black
                        : Color.HSVToRGB(Mathf.Abs(value.GetHashCode() % 1024) / 1024f, 0.72f, 0.92f));
                }
            }
            tex.Apply(false, false);
        }

        private static void DrawNoData(Texture2D tex, int width, int height)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                bool stripe = ((x / 8) + (y / 8)) % 2 == 0;
                tex.SetPixel(x, y, stripe
                    ? new Color(0.12f, 0.14f, 0.20f, 1f)
                    : new Color(0.22f, 0.18f, 0.12f, 1f));
            }
            tex.Apply(false, false);
        }

        private static void FindFloatRange(float[,] map, out float min, out float max)
        {
            min = float.PositiveInfinity;
            max = float.NegativeInfinity;
            for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                float v = map[x, y];
                if (v < min) min = v;
                if (v > max) max = v;
            }

            if (float.IsInfinity(min) || float.IsInfinity(max))
            {
                min = 0f;
                max = 1f;
            }
        }

        private static void FindIntRange(int[,] map, out int min, out int max)
        {
            min = int.MaxValue;
            max = int.MinValue;
            for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                int v = map[x, y];
                if (v < min) min = v;
                if (v > max) max = v;
            }

            if (min == int.MaxValue || max == int.MinValue)
            {
                min = 0;
                max = 1;
            }
        }
    }
}
