using System;
using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Domain;

namespace Kruty1918.Moyva.Calendar.Runtime
{
    /// <summary>
    /// Client-side read-only calendar proxy.
    /// Receives authoritative snapshots from the host via
    /// <see cref="ApplySnapshot"/> and exposes the same <see cref="ICalendarService"/> API.
    /// Calling <see cref="AdvanceTurn"/> or <see cref="SetByTotalHours"/> on the client
    /// side is a programming error and will throw <see cref="InvalidOperationException"/>.
    /// </summary>
    public sealed class ClientCalendarProxy : ICalendarService
    {
        private readonly CalendarConfig _config;
        private long _totalHours;
        private GameDateTime _current;
        private DayPhase _dayPhase;

        public GameDateTime   Current              => _current;
        public long           TotalHoursSinceEpoch => _totalHours;
        public DayPhase       CurrentDayPhase      => _dayPhase;
        public CalendarConfig Config               => _config;

        public event Action            OnHourChanged;
        public event Action            OnDayChanged;
        public event Action            OnMonthChanged;
        public event Action            OnYearChanged;
        public event Action<DayPhase>  OnDayPhaseChanged;

        public ClientCalendarProxy(CalendarConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _totalHours = 0;
            _current    = GameCalendarService.ComputeDateTime(_config, _totalHours);
            _dayPhase   = GameCalendarService.ComputeDayPhase(_config, _current.Hour);
        }

        /// <summary>
        /// Applies an authoritative snapshot received from the server.
        /// Fires change events for any values that have changed.
        /// </summary>
        public void ApplySnapshot(long totalHoursSinceEpoch)
        {
            if (totalHoursSinceEpoch == _totalHours) return;

            GameDateTime prevDt    = _current;
            DayPhase     prevPhase = _dayPhase;

            _totalHours = totalHoursSinceEpoch;
            _current    = GameCalendarService.ComputeDateTime(_config, _totalHours);
            _dayPhase   = GameCalendarService.ComputeDayPhase(_config, _current.Hour);

            OnHourChanged?.Invoke();

            if (_current.Day   != prevDt.Day)   OnDayChanged?.Invoke();
            if (_current.Month != prevDt.Month) OnMonthChanged?.Invoke();
            if (_current.Year  != prevDt.Year)  OnYearChanged?.Invoke();

            if (_dayPhase != prevPhase)
                OnDayPhaseChanged?.Invoke(_dayPhase);
        }

        public void AdvanceTurn() =>
            throw new InvalidOperationException(
                "ClientCalendarProxy is read-only. Only the authoritative server may advance the calendar.");

        public void SetByTotalHours(long totalHours) =>
            throw new InvalidOperationException(
                "ClientCalendarProxy is read-only. Use ApplySnapshot to apply server data.");
    }
}
