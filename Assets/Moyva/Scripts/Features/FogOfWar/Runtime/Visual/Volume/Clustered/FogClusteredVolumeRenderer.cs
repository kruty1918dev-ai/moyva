using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogClusteredVolumeRenderer
    {
        private const string LogTag = "[FogVolume]";
        private readonly FogClusterMeshRegistry _registry;
        private readonly FogClusterMeshBuilder _meshBuilder;
        private readonly FogClusterMaterialProvider _materialProvider;
        private readonly FogClusterMeshPresenter _meshPresenter;
        private readonly FogOfWarSettings _settings;
        private readonly HashSet<FogClusterKey> _fullRebuildKeys =
            new HashSet<FogClusterKey>();
        private readonly List<FogClusterKey> _fullRebuildBuffer =
            new List<FogClusterKey>();

        public FogClusteredVolumeRenderer(
            FogClusterMeshRegistry registry,
            FogClusterMeshBuilder meshBuilder,
            [InjectOptional] FogClusterMaterialProvider materialProvider = null,
            [InjectOptional] FogClusterMeshPresenter meshPresenter = null,
            [InjectOptional] FogOfWarSettings settings = null)
        {
            _registry = registry;
            _meshBuilder = meshBuilder;
            _materialProvider = materialProvider ?? new FogClusterMaterialProvider(settings);
            _meshPresenter = meshPresenter ?? new FogClusterMeshPresenter(_materialProvider);
            _settings = settings;
        }

        public void RebuildFull(FogWorldVisualContext context, IFogStateReader fogService)
        {
            if (_registry == null || _meshBuilder == null)
            {
                Debug.LogError($"{LogTag} Cluster mesh registry or builder is missing.");
                return;
            }

            _registry.ClearAll();
            _fullRebuildKeys.Clear();

            if (!context.IsValid || fogService == null)
            {
                Debug.LogError($"{LogTag} Cannot rebuild clusters without a valid world context and fog state.");
                return;
            }

            int clusterSize = Mathf.Max(1, _settings?.Volume.ClusterSize ?? 16);
            for (int y = 0; y < context.Height; y++)
            {
                for (int x = 0; x < context.Width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    FogStateType state = fogService.GetFogState(cell);
                    if (!_materialProvider.ShouldRenderState(state))
                        continue;

                    _fullRebuildKeys.Add(new FogClusterKey(x / clusterSize, y / clusterSize));
                }
            }

            _fullRebuildBuffer.Clear();
            foreach (FogClusterKey key in _fullRebuildKeys)
                _fullRebuildBuffer.Add(key);

            RebuildClusters(
                _fullRebuildBuffer,
                context,
                fogService);
        }

        public void RebuildClusters(IReadOnlyList<FogClusterKey> dirtyClusters, FogWorldVisualContext context, IFogStateReader fogService)
        {
            if (_registry == null || _meshBuilder == null)
            {
                Debug.LogError($"{LogTag} Cluster mesh registry or builder is missing.");
                return;
            }

            if (dirtyClusters == null || dirtyClusters.Count == 0)
                return;

            if (!context.IsValid || fogService == null)
            {
                Debug.LogError($"{LogTag} Cannot rebuild clusters without a valid world context and fog state.");
                return;
            }

            for (int i = 0; i < dirtyClusters.Count; i++)
            {
                var key = dirtyClusters[i];
                var handle = _registry.GetOrCreate(key);
                _meshBuilder.RebuildCluster(key, handle.Mesh, context, fogService);

                bool hasGeometry = handle.Mesh != null && handle.Mesh.vertexCount > 0;
                _meshPresenter.Apply(handle, hasGeometry);
            }

        }

        public void Clear()
        {
            _registry?.ClearAll();
        }

        public void ConfigureRoot(Transform parent)
        {
            _registry?.SetRootParent(parent);
        }

    }
}
