using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{

    /// <summary>
    /// Перевіряє базові умови для роботи volume fog path: settings, manager, installer і dual-grid presets.
    /// </summary>
    internal sealed class FogVolumeValidationService : IFogVolumeValidationService
    {
        /// <summary>
        /// Формує текстове повідомлення для Odin inspector validation panel.
        /// </summary>
        /// <param name="host">Host-компонент, який перевіряється.</param>
        /// <returns>Резюме валідності поточного scene/setup state.</returns>
        public string BuildValidationSummary(IFogVolumeValidationHost host)
        {
            if (host?.Settings == null)
                return "Missing FogOfWarSettings.";

            if (host.TileWorldCreatorManager == null)
                return "Missing TileWorldCreatorManager on the same GameObject.";

            if (Object.FindObjectsByType<FogOfWarInstaller>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0)
                return "Missing FogOfWarInstaller in scene. Zenject bindings are still required for fog service/save/visibility.";

            if (!HasConfiguredFogPreset(host.Settings.Volume.Unexplored) && !HasConfiguredFogPreset(host.Settings.Volume.Explored))
                return "No enabled fog state has a valid dual-grid TilePreset.";

            return "Ready: this component follows generated world grid/height data and builds fog as a TWC dual-grid volume.";
        }

        private static bool HasConfiguredFogPreset(FogVolumeStateTileSettings state)
        {
            if (state == null || !state.Enabled || state.TileVariants == null)
                return false;

            for (int i = 0; i < state.TileVariants.Count; i++)
            {
                var variant = state.TileVariants[i];
                if (variant != null
                    && variant.Preset != null
                    && FogOfWarSettings.HasUsableDualGridPreset(variant.Preset))
                    return true;
            }

            return false;
        }
    }
}
