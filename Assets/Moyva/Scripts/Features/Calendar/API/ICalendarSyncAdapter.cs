using System;
using Kruty1918.Moyva.Calendar.Core;

namespace Kruty1918.Moyva.Calendar.Multiplayer
{
    /// <summary>
    /// Multiplayer integration point for the calendar.
    /// The host calls <see cref="NotifyTurnCompleted"/> after each resolved game turn.
    /// On the client side, <see cref="ApplyRemoteSnapshot"/> pushes a server snapshot into
    /// the local <see cref="ClientCalendarProxy"/>.
    /// </summary>
    public interface ICalendarSyncAdapter
    {
        /// <summary>
        /// Fired by the host after each turn is resolved.
        /// The argument is the new <see cref="ICalendarService.TotalHoursSinceEpoch"/> that
        /// the session/network layer must broadcast to all clients.
        /// </summary>
        event Action<long> OnHostAdvanced;

        /// <summary>
        /// Called by the session layer (host/server) after a global turn has been resolved.
        /// Internally calls <see cref="ICalendarService.AdvanceTurn"/>.
        /// </summary>
        void NotifyTurnCompleted();

        /// <summary>
        /// Called by the session layer (client) when a world snapshot is received.
        /// Pushes the authoritative <paramref name="totalHoursSinceEpoch"/> to the local proxy.
        /// </summary>
        void ApplyRemoteSnapshot(long totalHoursSinceEpoch);
    }
}
