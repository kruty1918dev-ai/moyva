using System;
using System.Collections;
using System.Reflection;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public enum PortDirection
    {
        Input,
        Output
    }

    public enum PortMapSizePolicy
    {
        None,
        MatchContext,
        Variable
    }

    public sealed class PortDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public Type ValueType { get; }
        public PortDirection Direction { get; }
        public bool IsRequired { get; }
        public bool AllowNull { get; }
        public bool AcceptsAnyValue { get; }
        public PortMapSizePolicy MapSizePolicy { get; }

        public PortDefinition(string name, Type valueType, PortDirection direction)
            : this(
                name,
                valueType,
                direction,
                stableId: null,
                isRequired: direction == PortDirection.Input,
                allowNull: false,
                acceptsAnyValue: valueType == typeof(object),
                mapSizePolicy: InferMapSizePolicy(valueType))
        {
        }

        public PortDefinition(
            string name,
            Type valueType,
            PortDirection direction,
            string stableId,
            bool isRequired,
            bool allowNull,
            bool acceptsAnyValue,
            PortMapSizePolicy mapSizePolicy)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Port" : name;
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
            Direction = direction;
            Id = string.IsNullOrWhiteSpace(stableId)
                ? BuildStableId(Name, direction)
                : stableId;
            IsRequired = direction == PortDirection.Input && isRequired;
            AllowNull = allowNull;
            AcceptsAnyValue = acceptsAnyValue;
            MapSizePolicy = mapSizePolicy == PortMapSizePolicy.None
                ? InferMapSizePolicy(valueType)
                : mapSizePolicy;
        }

        public static PortDefinition Input<T>(
            string name,
            string stableId = null,
            PortMapSizePolicy mapSizePolicy = PortMapSizePolicy.None) =>
            new(
                name,
                typeof(T),
                PortDirection.Input,
                stableId,
                isRequired: true,
                allowNull: false,
                acceptsAnyValue: false,
                mapSizePolicy: mapSizePolicy);

        public static PortDefinition OptionalInput<T>(
            string name,
            string stableId = null,
            PortMapSizePolicy mapSizePolicy = PortMapSizePolicy.None) =>
            new(
                name,
                typeof(T),
                PortDirection.Input,
                stableId,
                isRequired: false,
                allowNull: true,
                acceptsAnyValue: false,
                mapSizePolicy: mapSizePolicy);

        public static PortDefinition AnyInput(
            string name,
            bool required = false,
            string stableId = null) =>
            new(
                name,
                typeof(object),
                PortDirection.Input,
                stableId,
                required,
                allowNull: !required,
                acceptsAnyValue: true,
                mapSizePolicy: PortMapSizePolicy.Variable);

        public static PortDefinition Output<T>(
            string name,
            string stableId = null,
            bool allowNull = false,
            PortMapSizePolicy mapSizePolicy = PortMapSizePolicy.None) =>
            new(
                name,
                typeof(T),
                PortDirection.Output,
                stableId,
                isRequired: false,
                allowNull: allowNull,
                acceptsAnyValue: false,
                mapSizePolicy: mapSizePolicy);

        public bool IsCompatibleWith(PortDefinition other)
        {
            if (other == null || Direction == other.Direction)
                return false;

            var source = Direction == PortDirection.Output ? this : other;
            var target = Direction == PortDirection.Input ? this : other;

            if (source.AcceptsAnyValue || target.AcceptsAnyValue)
                return true;

            return AreValueTypesCompatible(source.ValueType, target.ValueType);
        }

        public static bool AreValueTypesCompatible(Type sourceType, Type targetType)
        {
            if (sourceType == null || targetType == null)
                return false;

            // Legacy compatibility for call sites that only have Type information.
            // New code should prefer IsCompatibleWith so wildcard intent is explicit.
            if (sourceType == typeof(object) || targetType == typeof(object))
                return true;

            if (targetType.IsAssignableFrom(sourceType))
                return true;

            var sourceElementType = GetIndexableElementType(sourceType);
            return sourceElementType != null && targetType.IsAssignableFrom(sourceElementType);
        }

        public static bool RequiresElementIndexing(Type sourceType, Type targetType)
        {
            if (sourceType == null || targetType == null)
                return false;
            if (sourceType == typeof(object) || targetType == typeof(object))
                return false;
            if (targetType.IsAssignableFrom(sourceType))
                return false;

            var sourceElementType = GetIndexableElementType(sourceType);
            return sourceElementType != null && targetType.IsAssignableFrom(sourceElementType);
        }

        public static Type GetIndexableElementType(Type type)
        {
            if (type == null || type == typeof(string))
                return null;

            if (type.IsArray)
                return type.GetArrayRank() == 1 ? type.GetElementType() : null;

            if (TryGetGenericIndexableElementType(type, out var elementType))
                return elementType;

            var interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (TryGetGenericIndexableElementType(interfaces[i], out elementType))
                    return elementType;
            }

            return null;
        }

        public static bool TryGetIndexableCount(object value, out int count)
        {
            count = 0;
            if (value == null || value is string)
                return false;

            if (value is Array array && array.Rank == 1)
            {
                count = array.Length;
                return true;
            }

            if (value is ICollection collection)
            {
                count = collection.Count;
                return true;
            }

            var countProperty = value.GetType().GetProperty("Count", BindingFlags.Instance | BindingFlags.Public);
            if (countProperty != null && countProperty.PropertyType == typeof(int))
            {
                count = (int)countProperty.GetValue(value);
                return true;
            }

            return false;
        }

        public static bool TryGetIndexableValue(object value, int index, out object element)
        {
            element = null;
            if (!TryGetIndexableCount(value, out int count) || count <= 0)
                return false;

            if (index < 0 || index >= count)
                return false;

            if (value is Array array && array.Rank == 1)
            {
                element = array.GetValue(index);
                return true;
            }

            if (value is IList list)
            {
                element = list[index];
                return true;
            }

            var itemProperty = value.GetType().GetProperty("Item", BindingFlags.Instance | BindingFlags.Public,
                null, null, new[] { typeof(int) }, null);
            if (itemProperty != null)
            {
                element = itemProperty.GetValue(value, new object[] { index });
                return true;
            }

            return false;
        }

        private static bool TryGetGenericIndexableElementType(Type type, out Type elementType)
        {
            elementType = null;
            if (type == null || !type.IsGenericType)
                return false;

            var definition = type.GetGenericTypeDefinition();
            if (definition != typeof(System.Collections.Generic.IList<>)
                && definition != typeof(System.Collections.Generic.IReadOnlyList<>)
                && definition != typeof(System.Collections.Generic.List<>))
                return false;

            elementType = type.GetGenericArguments()[0];
            return true;
        }

        private static PortMapSizePolicy InferMapSizePolicy(Type type)
        {
            return type != null && type.IsArray && type.GetArrayRank() == 2
                ? PortMapSizePolicy.MatchContext
                : PortMapSizePolicy.Variable;
        }

        private static string BuildStableId(string name, PortDirection direction)
        {
            var chars = new char[name.Length];
            int length = 0;
            for (int i = 0; i < name.Length; i++)
            {
                char c = char.ToLowerInvariant(name[i]);
                if (char.IsLetterOrDigit(c))
                    chars[length++] = c;
                else if (length > 0 && chars[length - 1] != '_')
                    chars[length++] = '_';
            }

            string normalized = new string(chars, 0, length).Trim('_');
            if (string.IsNullOrEmpty(normalized))
                normalized = "port";
            return (direction == PortDirection.Input ? "in." : "out.") + normalized;
        }
    }
}
