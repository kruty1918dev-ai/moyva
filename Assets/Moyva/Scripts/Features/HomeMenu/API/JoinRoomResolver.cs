namespace Kruty1918.Moyva.HomeMenu.API
{
    public enum JoinRoomTargetKind
    {
        None,
        JoinCode,
        LobbyId
    }

    public struct JoinRoomTarget
    {
        public JoinRoomTargetKind Kind { get; }
        public string Value { get; }

        public bool IsValid => Kind != JoinRoomTargetKind.None && !string.IsNullOrWhiteSpace(Value);

        public JoinRoomTarget(JoinRoomTargetKind kind, string value)
        {
            Kind = kind;
            Value = value ?? string.Empty;
        }
    }

    public static class JoinRoomResolver
    {
        public static JoinRoomTarget FromManualInput(string input)
        {
            var value = input?.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);

            return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, value);
        }

        public static JoinRoomTarget FromRoom(RoomInfo room)
        {
            if (room.HasJoinCode)
                return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, room.JoinCode.Trim());

            if (room.HasLobbyId)
                return new JoinRoomTarget(JoinRoomTargetKind.LobbyId, room.LobbyId.Trim());

            return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);
        }
    }
}