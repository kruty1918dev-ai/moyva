using System;

namespace Kruty1918.Moyva.Calendar.Domain
{
    /// <summary>
    /// Immutable value object representing a point in game time.
    /// Hours are the finest granularity (no minutes or seconds).
    /// </summary>
    public readonly struct GameDateTime : IEquatable<GameDateTime>
    {
        public int Year  { get; }
        public int Month { get; }
        public int Day   { get; }
        public int Hour  { get; }

        public GameDateTime(int year, int month, int day, int hour)
        {
            Year  = year;
            Month = month;
            Day   = day;
            Hour  = hour;
        }

        public bool Equals(GameDateTime other) =>
            Year == other.Year && Month == other.Month && Day == other.Day && Hour == other.Hour;

        public override bool Equals(object obj) => obj is GameDateTime dt && Equals(dt);
        public override int GetHashCode() => HashCode.Combine(Year, Month, Day, Hour);

        public static bool operator ==(GameDateTime a, GameDateTime b) => a.Equals(b);
        public static bool operator !=(GameDateTime a, GameDateTime b) => !a.Equals(b);

        public override string ToString() =>
            $"Year {Year}, Month {Month:D2}, Day {Day:D2}, {Hour:D2}:00";
    }
}
