using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Options passed when creating or joining a session.
    /// </summary>
    public sealed class SessionConnectOptions
    {
        public ParticipantIdentity LocalIdentity { get; }
        public string RoomId { get; }
        public bool CreateIfNotExists { get; }
        public SessionRules Rules { get; }
        public uint ConfigChecksum { get; }

        public SessionConnectOptions(
            ParticipantIdentity localIdentity,
            string roomId,
            bool createIfNotExists,
            SessionRules rules,
            uint configChecksum)
        {
            LocalIdentity = localIdentity;
            RoomId = roomId;
            CreateIfNotExists = createIfNotExists;
            Rules = rules;
            ConfigChecksum = configChecksum;
        }
    }
}
