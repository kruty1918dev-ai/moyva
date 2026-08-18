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
        GameDateTime Current { get; }
        long TotalHoursSinceEpoch { get; }
        DayPhase CurrentDayPhase { get; }
        CalendarConfig Config { get; }

        event Action OnHourChanged;
        event Action OnDayChanged;
        event Action OnMonthChanged;
        event Action OnYearChanged;
        event Action<DayPhase> OnDayPhaseChanged;

        /// <summary>
        /// Advances time by <see cref="CalendarConfig.HoursPerTurn"/> hours.
        /// The authoritative turn loop calls this once after a full gameplay round completes.
        /// </summary>
        void AdvanceTurn();

        /// <summary>
        /// Applies an authoritative live/sync value and publishes calendar change events.
        /// Re-applying the current value is idempotent and emits no duplicate events.
        /// </summary>
        void SetByTotalHours(long totalHours);
    }

    /// <summary>
    /// Persistence-only mutation boundary. Restores canonical calendar state without publishing
    /// gameplay change events; this prevents save/load from replaying economy or presentation ticks.
    /// </summary>
    public interface ICalendarStateRestorer
    {
        void RestoreByTotalHours(long totalHours);
    }
}
