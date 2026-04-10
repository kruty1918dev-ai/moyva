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
        public int MaxHumans { get; }
        public int MaxBots { get; }
        public bool AllowBotsFallbackOnLeave { get; }
        public bool AllowMatchSaveForAnalysis { get; }
        public bool StrictParticipantLock { get; }

        public SessionRules(
            SessionMode mode,
            int maxParticipants,
            int maxHumans,
            int maxBots,
            bool allowBotsFallbackOnLeave,
            bool allowMatchSaveForAnalysis,
            bool strictParticipantLock)
        {
            Mode = mode;
            MaxParticipants = maxParticipants;
            MaxHumans = maxHumans;
            MaxBots = maxBots;
            AllowBotsFallbackOnLeave = allowBotsFallbackOnLeave;
            AllowMatchSaveForAnalysis = allowMatchSaveForAnalysis;
            StrictParticipantLock = strictParticipantLock;
        }

        public static SessionRules Default() =>
            new SessionRules(
                mode: SessionMode.MultiplayerHumans,
                maxParticipants: 4,
                maxHumans: 4,
                maxBots: 0,
                allowBotsFallbackOnLeave: false,
                allowMatchSaveForAnalysis: false,
                strictParticipantLock: false);
    }
}
