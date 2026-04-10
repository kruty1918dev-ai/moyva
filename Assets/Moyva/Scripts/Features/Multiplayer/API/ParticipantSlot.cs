namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Represents a named/colored slot in a session (no gameplay data).
    /// </summary>
    public sealed class ParticipantSlot
    {
        public int SlotIndex { get; }
        public string ColorName { get; }
        public string DisplayName { get; }
        public bool IsOccupied { get; }

        public ParticipantSlot(int slotIndex, string colorName, string displayName, bool isOccupied)
        {
            SlotIndex = slotIndex;
            ColorName = colorName;
            DisplayName = displayName;
            IsOccupied = isOccupied;
        }

        public ParticipantSlot WithOccupied(bool occupied) =>
            new ParticipantSlot(SlotIndex, ColorName, DisplayName, occupied);
    }
}
