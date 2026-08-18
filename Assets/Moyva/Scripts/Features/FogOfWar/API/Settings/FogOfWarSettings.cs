using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Root MoyvaJsonConfigObject config for FogOfWar. Serialized fields live in partial files
    /// so Unity keeps existing field names while feature sections stay isolated.
    /// </summary>
public partial class FogOfWarSettings : MoyvaJsonConfigObject
    {
        /// <summary>
        /// Backward-compatible wrapper for older call sites.
        /// Prefer <see cref="FogTilePresetUtility.HasUsableDualGridPreset"/> in new code.
        /// </summary>
        public static bool HasUsableDualGridPreset(TilePreset preset)
            => FogTilePresetUtility.HasUsableDualGridPreset(preset);

        private void OnValidate()
        {
            FogOfWarSettingsValidator.Normalize(this);
        }
    }
}
