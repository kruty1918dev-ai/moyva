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

    public sealed class PortDefinition
    {
        public string Name { get; }
        public Type ValueType { get; }
        public PortDirection Direction { get; }

        public PortDefinition(string name, Type valueType, PortDirection direction)
        {
            Name = name;
            ValueType = valueType;
            Direction = direction;
        }

        public static PortDefinition Input<T>(string name) =>
            new(name, typeof(T), PortDirection.Input);

        public static PortDefinition Output<T>(string name) =>
            new(name, typeof(T), PortDirection.Output);

        public bool IsCompatibleWith(PortDefinition other)
        {
            if (Direction == other.Direction) return false;

            var source = Direction == PortDirection.Output ? this : other;
            var target = Direction == PortDirection.Input ? this : other;

            return AreValueTypesCompatible(source.ValueType, target.ValueType);
        }

        public static bool AreValueTypesCompatible(Type sourceType, Type targetType)
        {
            if (sourceType == null || targetType == null)
                return false;

            // object-port works as a wildcard passthrough for reroute/route-point nodes.
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

            index = Math.Max(0, Math.Min(index, count - 1));

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
    }
}
