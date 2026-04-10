using System;

namespace Kruty1918.Moyva.Faction.API
{
    /// <summary>
    /// Унікальний ідентифікатор фракції.
    /// Використовується замість рядка щоб уникнути помилок при порівнянні та
    /// забезпечити чіткий тип у сигналах і сервісах.
    /// </summary>
    public readonly struct FactionId : IEquatable<FactionId>
    {
        public static readonly FactionId Empty = new FactionId(string.Empty);

        public string Value { get; }

        public FactionId(string value) => Value = value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(Value);

        public bool Equals(FactionId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is FactionId other && Equals(other);
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public override string ToString() => Value;

        public static bool operator ==(FactionId a, FactionId b) => a.Equals(b);
        public static bool operator !=(FactionId a, FactionId b) => !a.Equals(b);

        public static implicit operator string(FactionId id) => id.Value;
        public static explicit operator FactionId(string value) => new FactionId(value);
    }
}
