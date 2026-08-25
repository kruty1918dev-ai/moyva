using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogClusterMeshPresenter
    {
        private readonly FogClusterMaterialProvider _materialProvider;
        private readonly Material[] _materials = new Material[2];

        public FogClusterMeshPresenter(FogClusterMaterialProvider materialProvider)
        {
            _materialProvider = materialProvider;
        }

        public void Apply(FogClusterMeshHandle handle, bool hasGeometry)
        {
            if (handle == null || handle.MeshRenderer == null)
                return;

            ConfigureObjectLayer(handle);
            handle.MeshRenderer.shadowCastingMode = ResolveShadowCastingMode();
            handle.MeshRenderer.receiveShadows = false;
            handle.MeshRenderer.enabled = hasGeometry;

            _materials[0] = _materialProvider?.ResolveMaterial(FogStateType.Unexplored);
            _materials[1] = _materialProvider?.ResolveMaterial(FogStateType.Explored);
            handle.MeshRenderer.sharedMaterials = _materials;
        }

        private void ConfigureObjectLayer(FogClusterMeshHandle handle)
        {
            var settings = ResolvePresentationSettings();
            if (handle.GameObject != null && settings != null)
                handle.GameObject.layer = ResolveLayer(settings.ObjectLayer);
        }

        private ShadowCastingMode ResolveShadowCastingMode()
        {
            var settings = ResolvePresentationSettings();
            return settings != null ? settings.ShadowCastingMode : ShadowCastingMode.Off;
        }

        private FogVolumeStateTileSettings ResolvePresentationSettings()
        {
            var unexplored = _materialProvider?.ResolveStateSettings(FogStateType.Unexplored);
            if (unexplored != null && unexplored.Enabled)
                return unexplored;

            var explored = _materialProvider?.ResolveStateSettings(FogStateType.Explored);
            return explored ?? unexplored;
        }

        private static int ResolveLayer(LayerMask mask)
        {
            int value = mask.value;
            if (value <= 0)
                return 0;

            for (int i = 0; i < 32; i++)
            {
                if ((value & (1 << i)) != 0)
                    return i;
            }

            return 0;
        }
    }
}
