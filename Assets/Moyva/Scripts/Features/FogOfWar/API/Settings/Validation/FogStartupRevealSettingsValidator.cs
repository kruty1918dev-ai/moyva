using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation
{
    internal static class FogStartupRevealSettingsValidator
    {
        public static void Normalize(FogOfWarSettings settings)
        {
            settings.StartupFallbackRevealRadius = Mathf.Max(1, settings.StartupFallbackRevealRadius);
            settings.StartupFallbackMinMarginFromBorder = Mathf.Max(0, settings.StartupFallbackMinMarginFromBorder);
            settings.StartupFallbackRelativeMarginFactor = Mathf.Clamp(settings.StartupFallbackRelativeMarginFactor, 0f, 0.45f);
        }
    }
}
