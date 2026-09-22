using System;

namespace Kruty1918.Telemetry.Core
{
    /// <summary>
    /// Pseudonymous identity + session scope attached to every batch.
    /// InstallationId persists across runs (stable per install, regenerated on purge).
    /// ApplicationSessionId is new per process. GameplaySessionId scopes a match.
    /// No PII: all values are random GUIDs unless the product layer supplies PlayerId.
    /// </summary>
    public sealed class TelemetrySessionContext
    {
        public string InstallationId { get; }
        public string ApplicationSessionId { get; }
        public string PlayerId { get; private set; }
        public string GameplaySessionId { get; private set; }
        public string MatchId { get; private set; }

        /// <summary>Monotonic per-session event sequence. Used for sequence validation.</summary>
        public long NextSequence() => System.Threading.Interlocked.Increment(ref _sequence);
        private long _sequence;

        public TelemetrySessionContext(string installationId)
        {
            InstallationId = string.IsNullOrEmpty(installationId)
                ? throw new ArgumentNullException(nameof(installationId))
                : installationId;
            ApplicationSessionId = NewId();
        }

        public static string NewId() => Guid.NewGuid().ToString("N");

        /// <summary>Product layer may attach a pseudonymous player id; null clears it.</summary>
        public void SetPlayer(string playerId) => PlayerId = string.IsNullOrEmpty(playerId) ? null : playerId;

        /// <summary>Begin a gameplay scope; returns the new gameplay session id.</summary>
        public string BeginGameplaySession(string matchId = null)
        {
            GameplaySessionId = NewId();
            MatchId = matchId;
            return GameplaySessionId;
        }

        public void EndGameplaySession()
        {
            GameplaySessionId = null;
            MatchId = null;
        }
    }
}
