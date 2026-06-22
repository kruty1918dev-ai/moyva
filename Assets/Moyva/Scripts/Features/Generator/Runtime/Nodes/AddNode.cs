using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    /// <summary>
    /// Universal typed Add/Merge node for the Moyva graph editor.
    /// Intentionally separate from TileWorldCreator's native Add modifier.
    ///
    /// Main workflow:
    /// A = base data
    /// B = data to merge OR bool[,] mask, depending on Mode
    ///
    /// Important mode:
    /// ApplyMask = output A only where B mask is true.
    /// </summary>
    [NodeInfo("Add", "Math", "Typed Add/Merge node with modes: add/merge, apply mask, subtract mask, overlay, min, max.")]
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

        public enum AddMode
        {
            /// <summary>
            /// Old behavior:
            /// bool -> OR
            /// bool[,] -> OR
            /// numbers/maps/vectors -> +
            /// string -> concat
            /// string[,] -> B overrides non-empty cells over A
            /// </summary>
            AddOrMerge = 0,

            /// <summary>
            /// A is base data, B is bool[,] mask.
            /// Result keeps A only where B is true.
            /// bool[,] result = A && B.
            /// int[,] outside mask = 0.
            /// float[,] outside mask = NaN.
            /// string[,] outside mask = null.
            /// </summary>
            ApplyMask = 1,

            /// <summary>
            /// A is base data, B is bool[,] mask.
            /// Result keeps A only where B is false.
            /// bool[,] result = A && !B.
            /// </summary>
            SubtractMask = 2,

            /// <summary>
            /// Same-typed map overlay.
            /// B overwrites A only on occupied B cells.
            /// bool[,] occupied = true
            /// int[,] occupied = non-zero
            /// float[,] occupied = finite number
            /// string[,] occupied = non-empty string
            /// </summary>
            OverlayBOnA = 3,

            /// <summary>
            /// Numeric min. Supports int, float, int[,], float[,].
            /// </summary>
            Min = 4,

            /// <summary>
            /// Numeric max. Supports int, float, int[,], float[,].
            /// </summary>
            Max = 5
        }

        [SerializeField]
        private AddMode _mode = AddMode.AddOrMerge;

        [SerializeField, HideInInspector]
        private AddValueKind _valueKind = AddValueKind.Any;

        [NonSerialized]
        private object _lastResult;

        public AddMode Mode => _mode;
        public AddValueKind ValueKind => _valueKind;
        public bool IsTypeResolved => _valueKind != AddValueKind.Any;
        public bool UsesMaskOnPortB => _mode is AddMode.ApplyMask or AddMode.SubtractMask;
        public Type ResolvedValueType => ResolveType(_valueKind);

        public override string Title
        {
            get
            {
                string type = _valueKind == AddValueKind.Any ? "any" : FormatKind(_valueKind);
                return _mode switch
                {
                    AddMode.ApplyMask => $"Add Masked ({type})",
                    AddMode.SubtractMask => $"Add Subtract Mask ({type})",
                    AddMode.OverlayBOnA => $"Add Overlay ({type})",
                    AddMode.Min => $"Add Min ({type})",
                    AddMode.Max => $"Add Max ({type})",
                    _ => _valueKind == AddValueKind.Any ? "Add" : $"Add ({type})"
                };
            }
        }

        public override string Category => "Math";

        public override PortDefinition[] Inputs
        {
            get
            {
                Type baseType = ResolvedValueType ?? typeof(object);
                Type bType = UsesMaskOnPortB ? typeof(bool[,]) : baseType;

                return new[]
                {
                    new PortDefinition(BuildInputName("A Base", baseType), baseType, PortDirection.Input),
                    new PortDefinition(UsesMaskOnPortB ? "B Mask (bool[,])" : BuildInputName("B", bType), bType, PortDirection.Input)
                };
            }
        }

        public override PortDefinition[] Outputs
        {
            get
            {
                Type type = ResolvedValueType ?? typeof(object);
                return new[]
                {
                    new PortDefinition(BuildOutputName(type), type, PortDirection.Output)
                };
            }
        }

        public bool SetMode(AddMode mode)
        {
            if (_mode == mode)
                return false;

            _mode = mode;

            // Mask modes require A to be a 2D map/mask, because B is bool[,] mask.
            if (UsesMaskOnPortB && _valueKind != AddValueKind.Any && !IsMapKind(_valueKind))
                _valueKind = AddValueKind.Any;

            // Min/Max are numeric only.
            if ((_mode is AddMode.Min or AddMode.Max) && _valueKind != AddValueKind.Any && !IsNumericKind(_valueKind))
                _valueKind = AddValueKind.Any;

            return true;
        }

        public bool TrySetValueType(Type valueType)
        {
            if (!TryGetSupportedKind(valueType, out var kind))
                return false;

            if (UsesMaskOnPortB && !IsMapKind(kind))
                return false;

            if ((_mode is AddMode.Min or AddMode.Max) && !IsNumericKind(kind))
                return false;

            return SetValueKind(kind);
        }

        public bool TrySetValueTypeFromPort(int targetPortIndex, Type sourceType)
        {
            if (!TryGetSupportedKind(sourceType, out var kind))
                return false;

            if (UsesMaskOnPortB && targetPortIndex == 1)
            {
                // B is mask, not the resolved output/base type.
                return false;
            }

            if (UsesMaskOnPortB && !IsMapKind(kind))
                return false;

            if ((_mode is AddMode.Min or AddMode.Max) && !IsNumericKind(kind))
                return false;

            return SetValueKind(kind);
        }

        public bool SetValueKind(AddValueKind kind)
        {
            if (UsesMaskOnPortB && kind != AddValueKind.Any && !IsMapKind(kind))
                return false;

            if ((_mode is AddMode.Min or AddMode.Max) && kind != AddValueKind.Any && !IsNumericKind(kind))
                return false;

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
                Type runtimeType = UsesMaskOnPortB
                    ? a?.GetType()
                    : ResolveRuntimeType(a, b);

                if (!TryGetSupportedKind(runtimeType, out kind))
                    return NodeOutput.Error($"Add node cannot resolve supported type from runtime value '{runtimeType?.Name ?? "null"}'.");

                if (UsesMaskOnPortB && !IsMapKind(kind))
                    return NodeOutput.Error($"Mode '{_mode}' supports only map/mask base data on A, but got '{FormatKind(kind)}'.");

                if ((_mode is AddMode.Min or AddMode.Max) && !IsNumericKind(kind))
                    return NodeOutput.Error($"Mode '{_mode}' supports only numeric values/maps, but got '{FormatKind(kind)}'.");
            }

            if (!ValidateRuntimeValues(_mode, kind, a, b, out string validationError))
                return NodeOutput.Error(validationError);

            object result;
            try
            {
                result = _mode switch
                {
                    AddMode.ApplyMask => ApplyMask(kind, a, (bool[,])b, invertMask: false),
                    AddMode.SubtractMask => ApplyMask(kind, a, (bool[,])b, invertMask: true),
                    AddMode.OverlayBOnA => Overlay(kind, a, b),
                    AddMode.Min => Min(kind, a, b),
                    AddMode.Max => Max(kind, a, b),
                    _ => AddOrMerge(kind, a, b)
                };
            }
            catch (Exception ex)
            {
                return NodeOutput.Error($"Add node failed. Mode='{_mode}', Type='{FormatKind(kind)}': {ex.Message}");
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

        public static bool IsMapKind(AddValueKind kind)
        {
            return kind is AddValueKind.BoolMask or AddValueKind.IntMap or AddValueKind.FloatMap or AddValueKind.StringMap;
        }

        public static bool IsNumericKind(AddValueKind kind)
        {
            return kind is AddValueKind.Int or AddValueKind.Float or AddValueKind.IntMap or AddValueKind.FloatMap;
        }

        private static object AddOrMerge(AddValueKind kind, object a, object b)
        {
            return kind switch
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

        private static object ApplyMask(AddValueKind kind, object baseValue, bool[,] mask, bool invertMask)
        {
            int w = mask.GetLength(0);
            int h = mask.GetLength(1);

            switch (kind)
            {
                case AddValueKind.BoolMask:
                {
                    var source = (bool[,])baseValue;
                    var result = new bool[w, h];
                    for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                    {
                        bool pass = invertMask ? !mask[x, y] : mask[x, y];
                        result[x, y] = source[x, y] && pass;
                    }
                    return result;
                }

                case AddValueKind.IntMap:
                {
                    var source = (int[,])baseValue;
                    var result = new int[w, h];
                    for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                    {
                        bool pass = invertMask ? !mask[x, y] : mask[x, y];
                        result[x, y] = pass ? source[x, y] : 0;
                    }
                    return result;
                }

                case AddValueKind.FloatMap:
                {
                    var source = (float[,])baseValue;
                    var result = new float[w, h];
                    for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                    {
                        bool pass = invertMask ? !mask[x, y] : mask[x, y];
                        result[x, y] = pass ? source[x, y] : float.NaN;
                    }
                    return result;
                }

                case AddValueKind.StringMap:
                {
                    var source = (string[,])baseValue;
                    var result = new string[w, h];
                    for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                    {
                        bool pass = invertMask ? !mask[x, y] : mask[x, y];
                        result[x, y] = pass ? source[x, y] : null;
                    }
                    return result;
                }

                default:
                    throw new NotSupportedException($"ApplyMask supports only map/mask base data. Got '{FormatKind(kind)}'.");
            }
        }

        private static object Overlay(AddValueKind kind, object a, object b)
        {
            switch (kind)
            {
                case AddValueKind.BoolMask:
                {
                    var aa = (bool[,])a;
                    var bb = (bool[,])b;
                    int w = aa.GetLength(0);
                    int h = aa.GetLength(1);
                    var result = new bool[w, h];
                    for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                        result[x, y] = bb[x, y] || aa[x, y];
                    return result;
                }

                case AddValueKind.IntMap:
                {
                    var aa = (int[,])a;
                    var bb = (int[,])b;
                    int w = aa.GetLength(0);
                    int h = aa.GetLength(1);
                    var result = new int[w, h];
                    for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                        result[x, y] = bb[x, y] != 0 ? bb[x, y] : aa[x, y];
                    return result;
                }

                case AddValueKind.FloatMap:
                {
                    var aa = (float[,])a;
                    var bb = (float[,])b;
                    int w = aa.GetLength(0);
                    int h = aa.GetLength(1);
                    var result = new float[w, h];
                    for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                        result[x, y] = IsFinite(bb[x, y]) ? bb[x, y] : aa[x, y];
                    return result;
                }

                case AddValueKind.StringMap:
                {
                    var aa = (string[,])a;
                    var bb = (string[,])b;
                    int w = aa.GetLength(0);
                    int h = aa.GetLength(1);
                    var result = new string[w, h];
                    for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                        result[x, y] = string.IsNullOrEmpty(bb[x, y]) ? aa[x, y] : bb[x, y];
                    return result;
                }

                default:
                    throw new NotSupportedException($"Overlay supports only map/mask types. Got '{FormatKind(kind)}'.");
            }
        }

        private static object Min(AddValueKind kind, object a, object b)
        {
            return kind switch
            {
                AddValueKind.Int => Mathf.Min((int)a, (int)b),
                AddValueKind.Float => Mathf.Min((float)a, (float)b),
                AddValueKind.IntMap => MinIntMaps((int[,])a, (int[,])b),
                AddValueKind.FloatMap => MinFloatMaps((float[,])a, (float[,])b),
                _ => throw new NotSupportedException($"Min supports only numeric types. Got '{FormatKind(kind)}'.")
            };
        }

        private static object Max(AddValueKind kind, object a, object b)
        {
            return kind switch
            {
                AddValueKind.Int => Mathf.Max((int)a, (int)b),
                AddValueKind.Float => Mathf.Max((float)a, (float)b),
                AddValueKind.IntMap => MaxIntMaps((int[,])a, (int[,])b),
                AddValueKind.FloatMap => MaxFloatMaps((float[,])a, (float[,])b),
                _ => throw new NotSupportedException($"Max supports only numeric types. Got '{FormatKind(kind)}'.")
            };
        }

        private static bool ValidateRuntimeValues(AddMode mode, AddValueKind kind, object a, object b, out string error)
        {
            error = null;
            Type baseType = ResolveType(kind);
            if (baseType == null)
            {
                error = "Add node has no resolved supported type.";
                return false;
            }

            if (a == null || b == null)
            {
                error = $"Add node mode '{mode}' expects two connected inputs. A or B is null.";
                return false;
            }

            if (mode is AddMode.ApplyMask or AddMode.SubtractMask)
            {
                if (!IsMapKind(kind))
                {
                    error = $"Mode '{mode}' supports only map/mask A input. Got '{FormatKind(kind)}'.";
                    return false;
                }

                if (!baseType.IsInstanceOfType(a))
                {
                    error = $"Add node A input must be '{FormatKind(kind)}', but got '{a.GetType().Name}'.";
                    return false;
                }

                if (b is not bool[,] mask)
                {
                    error = $"Mode '{mode}' expects B input to be bool[,] mask, but got '{b.GetType().Name}'.";
                    return false;
                }

                var baseArray = (Array)a;
                return ValidateArraySize(baseArray, mask, "A Base", "B Mask", out error);
            }

            if (!baseType.IsInstanceOfType(a) || !baseType.IsInstanceOfType(b))
            {
                error = $"Add node is typed as '{FormatKind(kind)}', but got '{a.GetType().Name}' and '{b.GetType().Name}'.";
                return false;
            }

            if (mode is AddMode.Min or AddMode.Max && !IsNumericKind(kind))
            {
                error = $"Mode '{mode}' supports only numeric values/maps. Got '{FormatKind(kind)}'.";
                return false;
            }

            if (mode == AddMode.OverlayBOnA && !IsMapKind(kind))
            {
                error = $"Mode '{mode}' supports only map/mask inputs. Got '{FormatKind(kind)}'.";
                return false;
            }

            if (a is Array arrayA && b is Array arrayB)
                return ValidateArraySize(arrayA, arrayB, "A", "B", out error);

            return true;
        }

        private static bool ValidateArraySize(Array a, Array b, string aName, string bName, out string error)
        {
            error = null;

            if (a.Rank != 2 || b.Rank != 2)
            {
                error = "Add node supports only two-dimensional map/mask arrays.";
                return false;
            }

            if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
            {
                error = $"Add node received different map sizes: {aName}={a.GetLength(0)}x{a.GetLength(1)}, {bName}={b.GetLength(0)}x{b.GetLength(1)}.";
                return false;
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
            {
                float av = IsFinite(a[x, y]) ? a[x, y] : 0f;
                float bv = IsFinite(b[x, y]) ? b[x, y] : 0f;
                result[x, y] = av + bv;
            }
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

        private static int[,] MinIntMaps(int[,] a, int[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = Mathf.Min(a[x, y], b[x, y]);
            return result;
        }

        private static int[,] MaxIntMaps(int[,] a, int[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = Mathf.Max(a[x, y], b[x, y]);
            return result;
        }

        private static float[,] MinFloatMaps(float[,] a, float[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float av = a[x, y];
                float bv = b[x, y];
                result[x, y] = !IsFinite(av) ? bv : !IsFinite(bv) ? av : Mathf.Min(av, bv);
            }
            return result;
        }

        private static float[,] MaxFloatMaps(float[,] a, float[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float av = a[x, y];
                float bv = b[x, y];
                result[x, y] = !IsFinite(av) ? bv : !IsFinite(bv) ? av : Mathf.Max(av, bv);
            }
            return result;
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

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
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
                    float value = map[sx, sy];
                    if (!IsFinite(value))
                    {
                        tex.SetPixel(x, y, Color.black);
                        continue;
                    }

                    float t = Mathf.Clamp01((value - min) / range);
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
                if (!IsFinite(v))
                    continue;

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