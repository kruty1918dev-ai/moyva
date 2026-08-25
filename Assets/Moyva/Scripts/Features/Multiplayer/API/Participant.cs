namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Представляє мережевого учасника сесії.
    /// </summary>
    public sealed class Participant
    {
        public ParticipantIdentity Identity { get; }
        public bool IsHost { get; }

        /// <summary>Створює учасника з незмінною ідентичністю та роллю хоста.</summary>
        public Participant(ParticipantIdentity identity, bool isHost)
        {
            Identity = identity;
            IsHost = isHost;
        }

        /// <summary>Повертає копію учасника з роллю хоста.</summary>
        public Participant AsHost() =>
            new Participant(Identity, isHost: true);
    }
}
