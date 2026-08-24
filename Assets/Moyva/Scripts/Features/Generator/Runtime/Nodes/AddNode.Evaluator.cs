using System;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public sealed partial class AddNode
    {
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

        private static object Min(AddValueKind kind, object a, object b) =>
            kind switch
            {
                AddValueKind.Int => Mathf.Min((int)a, (int)b),
                AddValueKind.Float => Mathf.Min((float)a, (float)b),
                AddValueKind.IntMap => MinIntMaps((int[,])a, (int[,])b),
                AddValueKind.FloatMap => MinFloatMaps((float[,])a, (float[,])b),
                _ => throw new NotSupportedException($"Min supports only numeric types. Got '{FormatKind(kind)}'.")
            };

        private static object Max(AddValueKind kind, object a, object b) =>
            kind switch
            {
                AddValueKind.Int => Mathf.Max((int)a, (int)b),
                AddValueKind.Float => Mathf.Max((float)a, (float)b),
                AddValueKind.IntMap => MaxIntMaps((int[,])a, (int[,])b),
                AddValueKind.FloatMap => MaxFloatMaps((float[,])a, (float[,])b),
                _ => throw new NotSupportedException($"Max supports only numeric types. Got '{FormatKind(kind)}'.")
            };

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

            if (mode is AddMode.Min or AddMode.Max or AddMode.Subtract or AddMode.Multiply or AddMode.Divide or AddMode.Power or AddMode.Modulo && !IsNumericKind(kind))
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

        private static object Subtract(AddValueKind kind, object a, object b)
        {
            return kind switch
            {
                AddValueKind.Int => (int)a - (int)b,
                AddValueKind.Float => (float)a - (float)b,
                AddValueKind.IntMap => SubtractIntMaps((int[,])a, (int[,])b),
                AddValueKind.FloatMap => SubtractFloatMaps((float[,])a, (float[,])b),
                _ => throw new NotSupportedException($"Subtract supports only numeric types. Got '{FormatKind(kind)}'.")
            };
        }

        private static object Multiply(AddValueKind kind, object a, object b)
        {
            return kind switch
            {
                AddValueKind.Int => (int)a * (int)b,
                AddValueKind.Float => (float)a * (float)b,
                AddValueKind.IntMap => MultiplyIntMaps((int[,])a, (int[,])b),
                AddValueKind.FloatMap => MultiplyFloatMaps((float[,])a, (float[,])b),
                _ => throw new NotSupportedException($"Multiply supports only numeric types. Got '{FormatKind(kind)}'.")
            };
        }

        private static object Divide(AddValueKind kind, object a, object b)
        {
            return kind switch
            {
                AddValueKind.Int => (int)b != 0 ? (int)a / (int)b : 0,
                AddValueKind.Float => !Mathf.Approximately((float)b, 0f)
                    ? (float)a / (float)b
                    : float.NaN,
                AddValueKind.IntMap => DivideIntMaps((int[,])a, (int[,])b),
                AddValueKind.FloatMap => DivideFloatMaps((float[,])a, (float[,])b),
                _ => throw new NotSupportedException($"Divide supports only numeric types. Got '{FormatKind(kind)}'.")
            };
        }

        private static object Power(AddValueKind kind, object a, object b)
        {
            return kind switch
            {
                AddValueKind.Int => (int)Mathf.Pow((int)a, (int)b),
                AddValueKind.Float => Mathf.Pow((float)a, (float)b),
                AddValueKind.IntMap => PowerIntMaps((int[,])a, (int[,])b),
                AddValueKind.FloatMap => PowerFloatMaps((float[,])a, (float[,])b),
                _ => throw new NotSupportedException($"Power supports only numeric types. Got '{FormatKind(kind)}'.")
            };
        }

        private static object Modulo(AddValueKind kind, object a, object b)
        {
            return kind switch
            {
                AddValueKind.Int => (int)b != 0 ? (int)a % (int)b : 0,
                AddValueKind.Float => !Mathf.Approximately((float)b, 0f)
                    ? Mathf.Repeat((float)a, (float)b)
                    : float.NaN,
                AddValueKind.IntMap => ModuloIntMaps((int[,])a, (int[,])b),
                AddValueKind.FloatMap => ModuloFloatMaps((float[,])a, (float[,])b),
                _ => throw new NotSupportedException($"Modulo supports only numeric types. Got '{FormatKind(kind)}'.")
            };
        }

        private static int[,] SubtractIntMaps(int[,] a, int[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = a[x, y] - b[x, y];
            return result;
        }

        private static float[,] SubtractFloatMaps(float[,] a, float[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float av = IsFinite(a[x, y]) ? a[x, y] : 0f;
                float bv = IsFinite(b[x, y]) ? b[x, y] : 0f;
                result[x, y] = av - bv;
            }
            return result;
        }

        private static int[,] MultiplyIntMaps(int[,] a, int[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = a[x, y] * b[x, y];
            return result;
        }

        private static float[,] MultiplyFloatMaps(float[,] a, float[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float av = IsFinite(a[x, y]) ? a[x, y] : 0f;
                float bv = IsFinite(b[x, y]) ? b[x, y] : 0f;
                result[x, y] = av * bv;
            }
            return result;
        }

        private static int[,] DivideIntMaps(int[,] a, int[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = b[x, y] != 0 ? a[x, y] / b[x, y] : 0;
            return result;
        }

        private static float[,] DivideFloatMaps(float[,] a, float[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float av = IsFinite(a[x, y]) ? a[x, y] : 0f;
                float bv = IsFinite(b[x, y]) && b[x, y] != 0f ? b[x, y] : 1f;
                result[x, y] = av / bv;
            }
            return result;
        }

        private static int[,] PowerIntMaps(int[,] a, int[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = (int)Mathf.Pow(a[x, y], b[x, y]);
            return result;
        }

        private static float[,] PowerFloatMaps(float[,] a, float[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float av = IsFinite(a[x, y]) ? a[x, y] : 0f;
                float bv = IsFinite(b[x, y]) ? b[x, y] : 0f;
                result[x, y] = Mathf.Pow(av, bv);
            }
            return result;
        }

        private static int[,] ModuloIntMaps(int[,] a, int[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = b[x, y] != 0 ? a[x, y] % b[x, y] : 0;
            return result;
        }

        private static float[,] ModuloFloatMaps(float[,] a, float[,] b)
        {
            int w = a.GetLength(0);
            int h = a.GetLength(1);
            var result = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float av = IsFinite(a[x, y]) ? a[x, y] : 0f;
                float bv = IsFinite(b[x, y]) ? b[x, y] : 1f;
                result[x, y] = bv != 0f ? Mathf.Repeat(av, bv) : 0f;
            }
            return result;
        }

    }
}
