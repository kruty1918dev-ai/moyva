using System;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Immutable identity of a participant. Used for strict 4-player world lock comparisons.
    /// </summary>
    public sealed class ParticipantIdentity : IEquatable<ParticipantIdentity>
    {
        public const string BotIdPrefix = "BOT_";

        public string PlayerId { get; }
        public string Nickname { get; }

        public ParticipantIdentity(string playerId, string nickname)
        {
            PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
            Nickname = nickname ?? throw new ArgumentNullException(nameof(nickname));
        }

        public bool Equals(ParticipantIdentity other)
        {
            if (other is null) return false;
            return string.Equals(PlayerId, other.PlayerId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => Equals(obj as ParticipantIdentity);

        public override int GetHashCode() => PlayerId.GetHashCode();

        public override string ToString() => $"[{PlayerId}] {Nickname}";
    }
}
