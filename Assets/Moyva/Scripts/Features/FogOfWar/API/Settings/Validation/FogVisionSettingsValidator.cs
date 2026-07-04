using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation
{
    internal static class FogVisionSettingsValidator
    {
        public static void Normalize(FogOfWarSettings settings)
        {
            settings.DefaultVisionRange = Mathf.Max(1, settings.DefaultVisionRange);
            settings.MinVisionRange = Mathf.Max(1, settings.MinVisionRange);
            settings.MaxVisionRange = Mathf.Max(settings.MinVisionRange, settings.MaxVisionRange);
            settings.DefaultVisionRange = Mathf.Clamp(settings.DefaultVisionRange, settings.MinVisionRange, settings.MaxVisionRange);
        }
    }
}
