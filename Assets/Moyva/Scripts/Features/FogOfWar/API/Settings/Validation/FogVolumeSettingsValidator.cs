using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation
{
    internal static class FogVolumeSettingsValidator
    {
        public static void Normalize(FogOfWarSettings settings)
        {
            settings.Volume ??= new FogVolumeTileSettings();
            settings.Volume.EnsureDefaults();
        }
    }
}
