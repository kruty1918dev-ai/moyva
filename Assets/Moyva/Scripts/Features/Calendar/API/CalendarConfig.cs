namespace Kruty1918.Moyva.Calendar.Config
{
    /// <summary>
    /// Immutable configuration for the in-game calendar system.
    /// Stored as a binary file via CalendarBinaryConfigStore.
    /// </summary>
    public sealed class CalendarConfig
    {
        public const int CurrentSchemaVersion = 1;

        /// <summary>
        /// Easter egg: рік смерті Ярослава Мудрого і кінець золотої доби Київської Русі —
        /// найпотужнішого державного утворення в історії України (1054 р.).
        /// </summary>
        public const int PeakUkraineYear = 1054;

        /// <summary>Binary format version for forward compatibility.</summary>
        public int SchemaVersion { get; }

        // --- Start date/time ---
        public int StartYear  { get; }
        public int StartMonth { get; }
        public int StartDay   { get; }
        public int StartHour  { get; }

        // --- Calendar structure ---
        public int MonthsInYear { get; }
        public int DaysInMonth  { get; }
        public int HoursInDay   { get; }

        // --- Day/night boundaries ---
        /// <summary>Hour at which Day phase begins.</summary>
        public int DayStartHour   { get; }
        /// <summary>Hour at which Night phase begins.</summary>
        public int NightStartHour { get; }
        /// <summary>Duration of Dawn transition in hours.</summary>
        public int DawnDurationHours { get; }
        /// <summary>Duration of Dusk transition in hours.</summary>
        public int DuskDurationHours { get; }

        // --- Multiplayer ---
        /// <summary>How many in-game hours pass per single turn. Default: 1.</summary>
        public int HoursPerTurn { get; }

        public CalendarConfig(
            int schemaVersion,
            int startYear,
            int startMonth,
            int startDay,
            int startHour,
            int monthsInYear,
            int daysInMonth,
            int hoursInDay,
            int dayStartHour,
            int nightStartHour,
            int dawnDurationHours,
            int duskDurationHours,
            int hoursPerTurn)
        {
            SchemaVersion     = schemaVersion;
            StartYear         = startYear;
            StartMonth        = startMonth;
            StartDay          = startDay;
            StartHour         = startHour;
            MonthsInYear      = monthsInYear;
            DaysInMonth       = daysInMonth;
            HoursInDay        = hoursInDay;
            DayStartHour      = dayStartHour;
            NightStartHour    = nightStartHour;
            DawnDurationHours = dawnDurationHours;
            DuskDurationHours = duskDurationHours;
            HoursPerTurn      = hoursPerTurn;
        }

        public static CalendarConfig Default() =>
            new CalendarConfig(
                schemaVersion:     CurrentSchemaVersion,
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
