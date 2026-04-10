using System;
using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Domain;

namespace Kruty1918.Moyva.Calendar.Core
{
    /// <summary>
    /// Public contract for the in-game calendar service.
    /// The calendar is a pure domain service — it does not render or send network messages.
    /// </summary>
    public interface ICalendarService
    {
        // --- State ---

        /// <summary>Current in-game date and time.</summary>
        GameDateTime Current { get; }

        /// <summary>
        /// Monotonically increasing counter of in-game hours elapsed since the epoch
        /// (the StartDate/StartHour defined in CalendarConfig).
        /// Used as the canonical sync value in multiplayer.
        /// </summary>
        long TotalHoursSinceEpoch { get; }

        /// <summary>Current phase of the day (Night / Dawn / Day / Dusk).</summary>
        DayPhase CurrentDayPhase { get; }

        /// <summary>The config used to initialise this service instance.</summary>
        CalendarConfig Config { get; }

        // --- Events ---

        event Action OnHourChanged;
        event Action OnDayChanged;
        event Action OnMonthChanged;
        event Action OnYearChanged;
        event Action<DayPhase> OnDayPhaseChanged;

        // --- Mutation ---

        /// <summary>
        /// Advances time by <see cref="CalendarConfig.HoursPerTurn"/> hours.
        /// Called by the authoritative server/session layer once per turn.
        /// </summary>
        void AdvanceTurn();

        /// <summary>
        /// Sets calendar state to match the given canonical hour index.
        /// Used by clients when receiving a world snapshot from the host.
        /// </summary>
        void SetByTotalHours(long totalHours);
    }
}
