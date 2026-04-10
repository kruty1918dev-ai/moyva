using Kruty1918.Moyva.Calendar.Config;
using UnityEngine;

namespace Kruty1918.Moyva.Calendar.Runtime
{
    /// <summary>
    /// ScriptableObject — конфігурація початку гри для календаря.
    /// Дозволяє обирати початковий рік, місяць, день та годину при налаштуванні сесії.
    ///
    /// За замовчуванням встановлено рік <see cref="CalendarConfig.PeakUkraineYear"/> (1054) —
    /// пасхалка: рік смерті Ярослава Мудрого і кінець золотої доби Київської Русі.
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/Calendar/Session Config", fileName = "CalendarSessionConfig")]
    public sealed class CalendarSessionConfigSO : ScriptableObject
    {
        [Header("Початкова дата гри")]
        [Tooltip("Рік початку гри. За замовчуванням — розквіт Київської Русі (1054).")]
        [SerializeField] private int _startYear  = CalendarConfig.PeakUkraineYear;

        [Tooltip("Місяць початку гри (1–12).")]
        [SerializeField] private int _startMonth = 1;

        [Tooltip("День початку гри (1–30).")]
        [SerializeField] private int _startDay   = 1;

        [Tooltip("Година початку гри (0–23).")]
        [SerializeField] private int _startHour  = 6;

        /// <summary>
        /// Будує <see cref="CalendarConfig"/> з поточними налаштуваннями початкової дати,
        /// використовуючи дефолтні значення решти параметрів.
        /// </summary>
        public CalendarConfig BuildConfig()
        {
            CalendarConfig def = CalendarConfig.Default();
            return new CalendarConfig(
                schemaVersion:     def.SchemaVersion,
                startYear:         _startYear,
                startMonth:        _startMonth,
                startDay:          _startDay,
                startHour:         _startHour,
                monthsInYear:      def.MonthsInYear,
                daysInMonth:       def.DaysInMonth,
                hoursInDay:        def.HoursInDay,
                dayStartHour:      def.DayStartHour,
                nightStartHour:    def.NightStartHour,
                dawnDurationHours: def.DawnDurationHours,
                duskDurationHours: def.DuskDurationHours,
                hoursPerTurn:      def.HoursPerTurn);
        }
    }
}
