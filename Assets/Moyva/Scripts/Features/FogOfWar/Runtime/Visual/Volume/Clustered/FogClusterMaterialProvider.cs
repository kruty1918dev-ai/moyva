using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogClusterMaterialProvider : IFogClusterMaterialProvider
    {
        private readonly FogOfWarSettings _settings;

        public FogClusterMaterialProvider([InjectOptional] FogOfWarSettings settings = null)
        {
            _settings = settings;
        }

        public bool ShouldRenderState(FogStateType state)
        {
            if (state == FogStateType.Visible)
                return false;

            var stateSettings = ResolveStateSettings(state);
            return stateSettings == null || stateSettings.Enabled;
        }

        public FogVolumeStateTileSettings ResolveStateSettings(FogStateType state)
        {
            var volume = _settings?.Volume;
            return state == FogStateType.Explored ? volume?.Explored : volume?.Unexplored;
        }

        public Material ResolveMaterial(FogStateType state)
            => ResolveMaterial(ResolveStateSettings(state));

        private static Material ResolveMaterial(FogVolumeStateTileSettings stateSettings)
        {
            if (stateSettings?.TileVariants == null)
                return null;

            for (int i = 0; i < stateSettings.TileVariants.Count; i++)
            {
                var preset = stateSettings.TileVariants[i]?.Preset;
                if (preset == null)
                    continue;

                Material material = preset.GetMaterialOverride();
                if (material != null)
                    return material;

                var prefab = preset.DUALGRD_fillTile
                    ?? preset.DUALGRD_edgeTile
                    ?? preset.DUALGRD_cornerTile
                    ?? preset.DUALGRD_invertedCornerTile
                    ?? preset.DUALGRD_doubleInteriorCornerTile;
                var renderer = prefab != null ? prefab.GetComponentInChildren<MeshRenderer>() : null;
                if (renderer != null && renderer.sharedMaterial != null)
                    return renderer.sharedMaterial;
            }

            return null;
        }
    }
}
