using System;
using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Domain;

namespace Kruty1918.Moyva.Calendar.Runtime
{
    /// <summary>
    /// Authoritative (server-side) calendar service.
    /// All live time advancement originates here; persistence restoration uses the explicit
    /// silent state-restorer boundary so loading never replays gameplay side effects.
    /// </summary>
    public sealed class GameCalendarService : ICalendarService, ICalendarStateRestorer
    {
        private readonly CalendarConfig _config;
        private long _totalHours;
        private GameDateTime _current;
        private DayPhase _dayPhase;

        public GameDateTime Current => _current;
        public long TotalHoursSinceEpoch => _totalHours;
        public DayPhase CurrentDayPhase => _dayPhase;
        public CalendarConfig Config => _config;

        public event Action OnHourChanged;
        public event Action OnDayChanged;
        public event Action OnMonthChanged;
        public event Action OnYearChanged;
        public event Action<DayPhase> OnDayPhaseChanged;

        public GameCalendarService(CalendarConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _totalHours = 0;
            Recalculate();
        }

        public void AdvanceTurn()
        {
            SetByTotalHours(checked(_totalHours + _config.HoursPerTurn));
        }

        public void SetByTotalHours(long totalHours)
        {
            SetByTotalHoursCore(totalHours, publishEvents: true);
        }

        public void RestoreByTotalHours(long totalHours)
        {
            SetByTotalHoursCore(totalHours, publishEvents: false);
        }

        private void SetByTotalHoursCore(long totalHours, bool publishEvents)
        {
            if (totalHours < 0)
                throw new ArgumentOutOfRangeException(nameof(totalHours));

            if (totalHours == _totalHours)
                return;

            GameDateTime previousDateTime = _current;
            DayPhase previousPhase = _dayPhase;

            _totalHours = totalHours;
            Recalculate();

            if (!publishEvents)
                return;

            OnHourChanged?.Invoke();

            if (_current.Day != previousDateTime.Day)
                OnDayChanged?.Invoke();
            if (_current.Month != previousDateTime.Month)
                OnMonthChanged?.Invoke();
            if (_current.Year != previousDateTime.Year)
                OnYearChanged?.Invoke();
            if (_dayPhase != previousPhase)
                OnDayPhaseChanged?.Invoke(_dayPhase);
        }

        private void Recalculate()
        {
            _current = ComputeDateTime(_config, _totalHours);
            _dayPhase = ComputeDayPhase(_config, _current.Hour);
        }

        public static GameDateTime ComputeDateTime(CalendarConfig cfg, long totalHours)
        {
            long hoursInDay = cfg.HoursInDay;
            long daysInMonth = cfg.DaysInMonth;
            long monthsInYear = cfg.MonthsInYear;
            long startOffset = cfg.StartHour;
            long absHours = totalHours + startOffset;
            long hour = absHours % hoursInDay;
            long totalDays = absHours / hoursInDay;
            long day = totalDays % daysInMonth;
            long totalMonths = totalDays / daysInMonth;
            long month = totalMonths % monthsInYear;
            long years = totalMonths / monthsInYear;

            return new GameDateTime(
                year: (int)(cfg.StartYear + years),
                month: (int)(cfg.StartMonth + month),
                day: (int)(cfg.StartDay + day),
                hour: (int)hour);
        }

        public static DayPhase ComputeDayPhase(CalendarConfig cfg, int hour)
        {
            int dawnStart = cfg.DayStartHour - cfg.DawnDurationHours;
            int duskStart = cfg.NightStartHour - cfg.DuskDurationHours;

            if (hour >= dawnStart && hour < cfg.DayStartHour)
                return DayPhase.Dawn;
            if (hour >= cfg.DayStartHour && hour < duskStart)
                return DayPhase.Day;
            if (hour >= duskStart && hour < cfg.NightStartHour)
                return DayPhase.Dusk;
            return DayPhase.Night;
        }
    }
}
