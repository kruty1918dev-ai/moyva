using System;
using System.Globalization;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static class GameplayProgressTimeText
    {
        public static string Elapsed(double seconds)
        {
            long total = (long)Math.Floor(Math.Max(0d, seconds));
            return total >= 3600
                ? string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}", total / 3600, total / 60 % 60, total % 60)
                : string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", total / 60, total % 60);
        }

        public static string Duration(double seconds)
            => Elapsed(Math.Ceiling(Math.Max(0d, seconds)));

        public static string BuildDuration(int turns, IGameplayProgressClock clock)
            => BuildDuration(turns, clock?.IsRealtime == true, clock?.SandboxRoundSeconds ?? 10f);

        public static string BuildDuration(int turns, bool realtime, float roundSeconds)
        {
            if (turns <= 0)
                return "Instant";
            return realtime ? "up to " + Duration(turns * (double)roundSeconds) : Turns(turns);
        }

        public static string Remaining(int turns, IGameplayProgressClock clock)
        {
            int remaining = Math.Max(0, turns);
            if (clock?.IsRealtime != true)
                return Turns(remaining);

            double seconds = remaining == 0 ? 0d
                : (remaining - 1d) * clock.SandboxRoundSeconds + clock.SecondsUntilNextProgress;
            return Duration(seconds);
        }

        private static string Turns(int turns)
            => $"{turns} {(turns == 1 ? "turn" : "turns")}";
    }
}
