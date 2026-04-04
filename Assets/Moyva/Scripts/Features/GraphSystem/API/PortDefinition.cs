using System;

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
            return ValueType.IsAssignableFrom(other.ValueType)
                || other.ValueType.IsAssignableFrom(ValueType);
        }
    }
}
