using System;
using Kruty1918.Moyva.Calendar.Domain;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Спільна перевірка "чи звучить це правило у поточній фазі доби".
    /// Порожній список dayPhases — без обмежень; інакше назва фази (night/dawn/day/dusk).
    /// </summary>
    internal static class DayPhaseAudioGate
    {
        public static bool Allows(string[] dayPhases, DayPhase current)
        {
            if (dayPhases == null || dayPhases.Length == 0)
                return true;

            string name = current.ToString();
            for (int i = 0; i < dayPhases.Length; i++)
            {
                if (string.Equals(dayPhases[i], name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
