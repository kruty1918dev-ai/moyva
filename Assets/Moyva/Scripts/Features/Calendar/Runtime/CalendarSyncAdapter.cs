using System;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Multiplayer;

namespace Kruty1918.Moyva.Calendar.Runtime
{
    /// <summary>
    /// Default implementation of <see cref="ICalendarSyncAdapter"/>.
    /// Wire this into the session/turn layer to connect calendar with multiplayer.
    /// </summary>
    public sealed class CalendarSyncAdapter : ICalendarSyncAdapter
    {
        private readonly ICalendarService _authoritative;
        private readonly ClientCalendarProxy _proxy;

        /// <param name="authoritative">Server-side <see cref="GameCalendarService"/>.</param>
        /// <param name="proxy">Client-side proxy — may be null when running as a dedicated server.</param>
        public CalendarSyncAdapter(ICalendarService authoritative, ClientCalendarProxy proxy = null)
        {
            _authoritative = authoritative ?? throw new ArgumentNullException(nameof(authoritative));
            _proxy = proxy;
        }

        public void NotifyTurnCompleted()
        {
            _authoritative.AdvanceTurn();
        }

        public void ApplyRemoteSnapshot(long totalHoursSinceEpoch)
        {
            _proxy?.ApplySnapshot(totalHoursSinceEpoch);
        }
    }
}
