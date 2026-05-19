namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Значення цілі приєднання до кімнати: lobby id або join code.
    /// Залежності: формується JoinRoomResolver і споживається join flow сервісами.
    /// </summary>
    public struct JoinRoomTarget
    {
        /// <summary>Тип цілі приєднання.</summary>
        public JoinRoomTargetKind Kind { get; }

        /// <summary>Сире значення цілі приєднання.</summary>
        public string Value { get; }

        /// <summary>True, якщо значення цілі є заповненим і тип не дорівнює None.</summary>
        public bool IsValid => Kind != JoinRoomTargetKind.None && !string.IsNullOrWhiteSpace(Value);

        /// <summary>Створити нову ціль приєднання.</summary>
        public JoinRoomTarget(JoinRoomTargetKind kind, string value)
        {
            Kind = kind;
            Value = value ?? string.Empty;
        }
    }
}
