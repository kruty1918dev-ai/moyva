using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogClusterMeshPresenter : IFogClusterMeshPresenter
    {
        private const string ClusterDiagTag = "[MoyvaFogClusterDiag]";
        private readonly IFogClusterMaterialProvider _materialProvider;
        private readonly Material[] _materials = new Material[2];
        private bool _loggedMissingMaterial;

        public FogClusterMeshPresenter(IFogClusterMaterialProvider materialProvider)
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

            if (!_loggedMissingMaterial && ShouldWarnMissingMaterial())
            {
                _loggedMissingMaterial = true;
                Debug.LogWarning($"{ClusterDiagTag} Clustered fog renderer could not resolve one or more materials from FogOfWarSettings tile presets. Cluster meshes may render with Unity fallback material.");
            }
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

        private bool ShouldWarnMissingMaterial()
        {
            bool missingUnexplored = (_materialProvider?.ShouldRenderState(FogStateType.Unexplored) ?? true)
                && _materials[0] == null;
            bool missingExplored = (_materialProvider?.ShouldRenderState(FogStateType.Explored) ?? true)
                && _materials[1] == null;
            return missingUnexplored || missingExplored;
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
