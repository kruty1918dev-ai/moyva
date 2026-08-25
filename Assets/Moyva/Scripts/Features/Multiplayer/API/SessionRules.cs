using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Config
{
    /// <summary>
    /// Immutable value object describing rules for a session.
    /// </summary>
    public sealed class SessionRules
    {
        public SessionMode Mode { get; }
        public int MaxParticipants { get; }
        public bool AllowMatchSaveForAnalysis { get; }
        public bool StrictParticipantLock { get; }

        /// <summary>Створює незмінні правила сесії для мережевих учасників.</summary>
        public SessionRules(
            SessionMode mode,
            int maxParticipants,
            bool allowMatchSaveForAnalysis,
            bool strictParticipantLock)
        {
            Mode = mode;
            MaxParticipants = maxParticipants;
            AllowMatchSaveForAnalysis = allowMatchSaveForAnalysis;
            StrictParticipantLock = strictParticipantLock;
        }

        /// <summary>Повертає стандартні правила multiplayer-сесії.</summary>
        public static SessionRules Default() =>
            new SessionRules(
                mode: SessionMode.Multiplayer,
                maxParticipants: 4,
                allowMatchSaveForAnalysis: false,
                strictParticipantLock: false);
    }
}
