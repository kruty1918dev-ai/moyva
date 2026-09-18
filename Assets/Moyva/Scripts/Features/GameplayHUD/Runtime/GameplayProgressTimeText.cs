using System;
using System.Globalization;
using Kruty1918.Moyva.Shared.Localization;
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

        public static string BuildDuration(int turns, IGameplayProgressClock clock, ILocalizationService loca = null)
            => BuildDuration(turns, clock?.IsRealtime == true, clock?.SandboxRoundSeconds ?? 10f, loca);

        public static string BuildDuration(int turns, bool realtime, float roundSeconds, ILocalizationService loca = null)
        {
            if (turns <= 0)
                return loca?.T("Instant") ?? "Instant";
            return realtime
                ? (loca?.TF("up to {0}", Duration(turns * (double)roundSeconds)) ?? "up to " + Duration(turns * (double)roundSeconds))
                : Turns(turns, loca);
        }

        public static string Remaining(int turns, IGameplayProgressClock clock, ILocalizationService loca = null)
        {
            int remaining = Math.Max(0, turns);
            if (clock?.IsRealtime != true)
                return Turns(remaining, loca);

            double seconds = remaining == 0 ? 0d
                : (remaining - 1d) * clock.SandboxRoundSeconds + clock.SecondsUntilNextProgress;
            return Duration(seconds);
        }

        private static string Turns(int turns, ILocalizationService loca = null)
            => loca != null
                ? loca.TF("{0} {1}", turns, loca.TN("turn", "turns", turns))
                : $"{turns} {(turns == 1 ? "turn" : "turns")}";
    }
}
