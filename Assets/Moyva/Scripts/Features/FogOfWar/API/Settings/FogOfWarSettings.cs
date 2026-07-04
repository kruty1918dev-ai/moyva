using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Root ScriptableObject config for FogOfWar. Serialized fields live in partial files
    /// so Unity keeps existing field names while feature sections stay isolated.
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/FogOfWarSettings", fileName = "FogOfWarSettings")]
    public partial class FogOfWarSettings : ScriptableObject
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
