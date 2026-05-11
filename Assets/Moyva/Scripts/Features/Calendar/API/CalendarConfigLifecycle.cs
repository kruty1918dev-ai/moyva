using System;

namespace Kruty1918.Moyva.Calendar.Config
{
    /// <summary>
    /// Unified runtime lifecycle for calendar config:
    /// Load -> Validate -> Freeze.
    /// </summary>
    public static class CalendarConfigLifecycle
    {
        public static CalendarConfig LoadValidateFreeze(ICalendarConfigStore store, Action<string> warn = null)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            CalendarConfig loaded = store.Exists() ? store.Load() : CalendarConfig.Default();
            return ValidateAndFreeze(loaded, warn);
        }

        public static CalendarConfig ValidateAndFreeze(CalendarConfig config, Action<string> warn = null)
        {
            bool corrected = false;

            if (config == null)
            {
                corrected = true;
                config = CalendarConfig.Default();
            }

            int schemaVersion = config.SchemaVersion > 0 ? config.SchemaVersion : CalendarConfig.CurrentSchemaVersion;
            if (schemaVersion != config.SchemaVersion)
                corrected = true;

            int monthsInYear = config.MonthsInYear >= 1 ? config.MonthsInYear : 12;
            if (monthsInYear != config.MonthsInYear)
                corrected = true;

            int daysInMonth = config.DaysInMonth >= 1 ? config.DaysInMonth : 30;
            if (daysInMonth != config.DaysInMonth)
                corrected = true;

            int hoursInDay = config.HoursInDay >= 1 ? config.HoursInDay : 24;
            if (hoursInDay != config.HoursInDay)
                corrected = true;

            int startMonth = Clamp(config.StartMonth, 1, monthsInYear);
            if (startMonth != config.StartMonth)
                corrected = true;

            int startDay = Clamp(config.StartDay, 1, daysInMonth);
            if (startDay != config.StartDay)
                corrected = true;

            int startHour = Clamp(config.StartHour, 0, Math.Max(0, hoursInDay - 1));
            if (startHour != config.StartHour)
                corrected = true;

            int dayStartHour = Clamp(config.DayStartHour, 0, Math.Max(0, hoursInDay - 1));
            if (dayStartHour != config.DayStartHour)
                corrected = true;

            int nightStartHour = Clamp(config.NightStartHour, 0, Math.Max(0, hoursInDay - 1));
            if (nightStartHour != config.NightStartHour)
                corrected = true;

            int dawnDurationHours = Clamp(config.DawnDurationHours, 0, hoursInDay);
            if (dawnDurationHours != config.DawnDurationHours)
                corrected = true;

            int duskDurationHours = Clamp(config.DuskDurationHours, 0, hoursInDay);
            if (duskDurationHours != config.DuskDurationHours)
                corrected = true;

            int hoursPerTurn = config.HoursPerTurn >= 1 ? config.HoursPerTurn : 1;
            if (hoursPerTurn != config.HoursPerTurn)
                corrected = true;

            CalendarConfig frozen = new CalendarConfig(
                schemaVersion,
                config.StartYear,
                startMonth,
                startDay,
                startHour,
                monthsInYear,
                daysInMonth,
                hoursInDay,
                dayStartHour,
                nightStartHour,
                dawnDurationHours,
                duskDurationHours,
                hoursPerTurn);

            if (corrected)
                warn?.Invoke("Calendar config normalized during runtime lifecycle (Load/Validate/Freeze).");

            return frozen;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}