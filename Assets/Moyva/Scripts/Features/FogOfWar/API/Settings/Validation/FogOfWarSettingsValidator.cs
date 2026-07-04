using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation
{
    internal static class FogOfWarSettingsValidator
    {
        public static void Normalize(FogOfWarSettings settings)
        {
            if (settings == null)
                return;

            FogVisionSettingsValidator.Normalize(settings);
            FogTerrainLosSettingsValidator.Normalize(settings);
            FogVolumeSettingsValidator.Normalize(settings);
            FogLegacyOverlaySettingsValidator.Normalize(settings);
            FogStartupRevealSettingsValidator.Normalize(settings);
            FogRendererCullingSettingsValidator.Normalize(settings);
        }
    }
}
