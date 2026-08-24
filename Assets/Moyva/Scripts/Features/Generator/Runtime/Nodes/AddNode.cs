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
    /// Modes:
    /// AddOrMerge = додати/злити (OR для bool, + для чисел, concat для string)
    /// ApplyMask = застосувати маску (A лише там де B=true)
    /// SubtractMask = відняти маску (A лише там де B=false)
    /// OverlayBOnA = накладати B на A (B переписує A тільки на зайнятих клітинках)
    /// Min/Max = мінімум/максимум
    /// Subtract = віднімання
    /// Multiply = множення
    /// Divide = ділення
    /// Power = степінь (A^B)
    /// Modulo = остача від ділення
    /// </summary>
    [NodeInfo(
        "Add",
        "Math",
        "Універсальний типізований вузол додавання та злиття з режимами маски, накладання й числових операцій.",
        StableId = "moyva.math.add",
        Order = 10,
        PreviewOutput = "out.result")]
    public sealed partial class AddNode : NodeBase
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
            Max = 5,

            /// <summary>
            /// Numeric subtract. Supports int, float, int[,], float[,].
            /// </summary>
            Subtract = 6,

            /// <summary>
            /// Numeric multiply. Supports int, float, int[,], float[,].
            /// </summary>
            Multiply = 7,

            /// <summary>
            /// Numeric divide. Supports int, float, int[,], float[,].
            /// Division by zero returns 0 for int, NaN for float.
            /// </summary>
            Divide = 8,

            /// <summary>
            /// Numeric power (A^B). Supports int, float, int[,], float[,].
            /// </summary>
            Power = 9,

            /// <summary>
            /// Numeric modulo (A % B). Supports int, float, int[,], float[,].
            /// </summary>
            Modulo = 10
        }

        [SerializeField]
        private AddMode _mode = AddMode.AddOrMerge;

        [SerializeField, HideInInspector]
        private AddValueKind _valueKind = AddValueKind.Any;

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
                    AddMode.Subtract => $"Add Subtract ({type})",
                    AddMode.Multiply => $"Add Multiply ({type})",
                    AddMode.Divide => $"Add Divide ({type})",
                    AddMode.Power => $"Add Power ({type})",
                    AddMode.Modulo => $"Add Modulo ({type})",
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
                    CreateInputPort(BuildInputName("A Base", baseType), baseType, "in.a"),
                    CreateInputPort(
                        UsesMaskOnPortB ? "B Mask (bool[,])" : BuildInputName("B", bType),
                        bType,
                        "in.b")
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
                    CreateOutputPort(BuildOutputName(type), type, "out.result")
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

            // Min/Max/Subtract/Multiply/Divide/Power/Modulo are numeric only.
            if ((_mode is AddMode.Min or AddMode.Max or AddMode.Subtract or AddMode.Multiply or AddMode.Divide or AddMode.Power or AddMode.Modulo) && _valueKind != AddValueKind.Any && !IsNumericKind(_valueKind))
                _valueKind = AddValueKind.Any;

            return true;
        }

        public bool TrySetValueType(Type valueType)
        {
            if (!TryGetSupportedKind(valueType, out var kind))
                return false;

            if (UsesMaskOnPortB && !IsMapKind(kind))
                return false;

            if ((_mode is AddMode.Min or AddMode.Max or AddMode.Subtract or AddMode.Multiply or AddMode.Divide or AddMode.Power or AddMode.Modulo) && !IsNumericKind(kind))
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

            if ((_mode is AddMode.Min or AddMode.Max or AddMode.Subtract or AddMode.Multiply or AddMode.Divide or AddMode.Power or AddMode.Modulo) && !IsNumericKind(kind))
                return false;

            return SetValueKind(kind);
        }

        public bool SetValueKind(AddValueKind kind)
        {
            if (UsesMaskOnPortB && kind != AddValueKind.Any && !IsMapKind(kind))
                return false;

            if ((_mode is AddMode.Min or AddMode.Max or AddMode.Subtract or AddMode.Multiply or AddMode.Divide or AddMode.Power or AddMode.Modulo) && kind != AddValueKind.Any && !IsNumericKind(kind))
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

                if ((_mode is AddMode.Min or AddMode.Max or AddMode.Subtract or AddMode.Multiply or AddMode.Divide or AddMode.Power or AddMode.Modulo) && !IsNumericKind(kind))
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
                    AddMode.Subtract => Subtract(kind, a, b),
                    AddMode.Multiply => Multiply(kind, a, b),
                    AddMode.Divide => Divide(kind, a, b),
                    AddMode.Power => Power(kind, a, b),
                    AddMode.Modulo => Modulo(kind, a, b),
                    _ => AddOrMerge(kind, a, b)
                };
            }
            catch (Exception ex)
            {
                return NodeOutput.Error($"Add node failed. Mode='{_mode}', Type='{FormatKind(kind)}': {ex.Message}");
            }

            return NodeOutput.Success(result);
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

        private static PortDefinition CreateInputPort(string name, Type type, string stableId) =>
            new(
                name,
                type,
                PortDirection.Input,
                stableId,
                isRequired: true,
                allowNull: false,
                acceptsAnyValue: type == typeof(object),
                mapSizePolicy: PortMapSizePolicy.None);

        private static PortDefinition CreateOutputPort(string name, Type type, string stableId) =>
            new(
                name,
                type,
                PortDirection.Output,
                stableId,
                isRequired: false,
                allowNull: false,
                acceptsAnyValue: type == typeof(object),
                mapSizePolicy: PortMapSizePolicy.None);
    }
}
