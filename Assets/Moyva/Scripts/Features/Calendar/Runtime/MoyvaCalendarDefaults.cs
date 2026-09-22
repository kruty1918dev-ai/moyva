using Kruty1918.Calendar.Config;

namespace Kruty1918.Calendar.Runtime
{
    /// <summary>
    /// Moyva session defaults for the reusable calendar package.
    /// </summary>
    public static class MoyvaCalendarDefaults
    {
        /// <summary>
        /// Easter egg: рік смерті Ярослава Мудрого і кінець золотої доби Київської Русі —
        /// найпотужнішого державного утворення в історії України (1054 р.).
        /// </summary>
        public const int PeakUkraineYear = 1054;

        public static CalendarConfig Create() =>
            new CalendarConfig(
                schemaVersion:     CalendarConfig.CurrentSchemaVersion,
                startYear:         PeakUkraineYear,
                startMonth:        1,
                startDay:          1,
                startHour:         6,
                monthsInYear:      12,
                daysInMonth:       30,
                hoursInDay:        24,
                dayStartHour:      6,
                nightStartHour:    20,
                dawnDurationHours: 1,
                duskDurationHours: 1,
                hoursPerTurn:      1);
    }
}
