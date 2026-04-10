namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Represents a participant (human or bot) in a session.
    /// </summary>
    public sealed class Participant
    {
        public ParticipantIdentity Identity { get; }
        public bool IsBot { get; }
        public bool IsHost { get; }

        public Participant(ParticipantIdentity identity, bool isBot, bool isHost)
        {
            Identity = identity;
            IsBot = isBot;
            IsHost = isHost;
        }

        public Participant AsHost() =>
            new Participant(Identity, IsBot, isHost: true);
    }
}
