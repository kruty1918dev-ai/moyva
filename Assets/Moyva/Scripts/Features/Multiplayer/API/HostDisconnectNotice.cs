using System.Threading;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Process-wide latch indicating that the local participant was forcefully
    /// removed from a running multiplayer session (e.g. host closed the lobby
    /// or the room was destroyed while the gameplay scene was active).
    /// The HomeMenu scene reads this on startup to surface a user-visible
    /// notification once the user has been returned to the main menu.
    /// </summary>
    public static class HostDisconnectNotice
    {
        private static int _hasPending;
        private static string _reason;

        public static bool HasPending => Interlocked.CompareExchange(ref _hasPending, 0, 0) == 1;

        public static string Reason => _reason;

        public static void Set(string reason)
        {
            _reason = reason ?? string.Empty;
            Interlocked.Exchange(ref _hasPending, 1);
        }

        /// <summary>
        /// Atomically reads and clears the pending notice.
        /// Returns <c>true</c> and outputs the reason if a notice was pending.
        /// </summary>
        public static bool TryConsume(out string reason)
        {
            if (Interlocked.Exchange(ref _hasPending, 0) == 1)
            {
                reason = _reason ?? string.Empty;
                _reason = null;
                return true;
            }

            reason = null;
            return false;
        }

        public static void Clear()
        {
            Interlocked.Exchange(ref _hasPending, 0);
            _reason = null;
        }
    }
}
