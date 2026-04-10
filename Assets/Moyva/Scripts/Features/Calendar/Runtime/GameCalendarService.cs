using System;
using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Domain;

namespace Kruty1918.Moyva.Calendar.Runtime
{
    /// <summary>
    /// Authoritative (server-side) calendar service.
    /// All time advancement originates here; clients receive snapshots and use
    /// <see cref="ClientCalendarProxy"/> to reflect the authoritative state.
    /// </summary>
    public sealed class GameCalendarService : ICalendarService
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

        public GameCalendarService(CalendarConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _totalHours = 0;
            Recalculate(raiseEvents: false);
        }

        // ------------------------------------------------------------------ mutation

        public void AdvanceTurn()
        {
            SetByTotalHours(_totalHours + _config.HoursPerTurn);
        }

        public void SetByTotalHours(long totalHours)
        {
            if (totalHours < 0) throw new ArgumentOutOfRangeException(nameof(totalHours));

            GameDateTime prevDt     = _current;
            DayPhase     prevPhase  = _dayPhase;

            _totalHours = totalHours;
            Recalculate(raiseEvents: true);

            OnHourChanged?.Invoke();

            if (_current.Day   != prevDt.Day)   OnDayChanged?.Invoke();
            if (_current.Month != prevDt.Month) OnMonthChanged?.Invoke();
            if (_current.Year  != prevDt.Year)  OnYearChanged?.Invoke();

            if (_dayPhase != prevPhase)
                OnDayPhaseChanged?.Invoke(_dayPhase);
        }

        // ------------------------------------------------------------------ private

        private void Recalculate(bool raiseEvents)
        {
            _current  = ComputeDateTime(_config, _totalHours);
            _dayPhase = ComputeDayPhase(_config, _current.Hour);
        }

        // ------------------------------------------------------------------ internal helpers (visible to tests)

        internal static GameDateTime ComputeDateTime(CalendarConfig cfg, long totalHours)
        {
            long hoursInDay   = cfg.HoursInDay;
            long daysInMonth  = cfg.DaysInMonth;
            long monthsInYear = cfg.MonthsInYear;

            // Total hours relative to the start of epoch day
            long startOffset = cfg.StartHour;
            long absHours    = totalHours + startOffset;

            long hour        = absHours % hoursInDay;
            long totalDays   = absHours / hoursInDay;

            long day         = totalDays % daysInMonth;
            long totalMonths = totalDays / daysInMonth;

            long month       = totalMonths % monthsInYear;
            long years       = totalMonths / monthsInYear;

            return new GameDateTime(
                year:  (int)(cfg.StartYear  + years),
                month: (int)(cfg.StartMonth + month),
                day:   (int)(cfg.StartDay   + day),
                hour:  (int)hour);
        }

        internal static DayPhase ComputeDayPhase(CalendarConfig cfg, int hour)
        {
            int dawnStart = cfg.DayStartHour - cfg.DawnDurationHours;
            int duskStart = cfg.NightStartHour - cfg.DuskDurationHours;

            // Dawn transition: [dawnStart, DayStartHour)
            if (hour >= dawnStart && hour < cfg.DayStartHour)
                return DayPhase.Dawn;

            // Day: [DayStartHour, duskStart)
            if (hour >= cfg.DayStartHour && hour < duskStart)
                return DayPhase.Day;

            // Dusk transition: [duskStart, NightStartHour)
            if (hour >= duskStart && hour < cfg.NightStartHour)
                return DayPhase.Dusk;

            // Night: everything else
            return DayPhase.Night;
        }
    }
}
