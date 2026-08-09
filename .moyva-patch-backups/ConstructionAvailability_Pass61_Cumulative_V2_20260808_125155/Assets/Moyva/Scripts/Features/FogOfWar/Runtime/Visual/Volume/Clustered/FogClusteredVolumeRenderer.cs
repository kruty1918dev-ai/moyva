using System.Collections.Generic;
using System.Diagnostics;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogClusteredVolumeRenderer : IFogClusteredVolumeRenderer
    {
        private const string ClusterDiagTag = "[MoyvaFogClusterDiag]";
        private readonly IFogClusterMeshRegistry _registry;
        private readonly IFogClusterMeshBuilder _meshBuilder;
        private readonly IFogClusterMaterialProvider _materialProvider;
        private readonly IFogClusterMeshPresenter _meshPresenter;
        private readonly FogOfWarSettings _settings;
        private const string PerfLogTag =
            "[MoyvaConstructionPerf]";
        private const double SlowClusterRebuildThresholdMs = 1.0d;

        private readonly HashSet<FogClusterKey> _fullRebuildKeys =
            new HashSet<FogClusterKey>();
        private readonly List<FogClusterKey> _fullRebuildBuffer =
            new List<FogClusterKey>();

        public FogClusteredVolumeRenderer(
            IFogClusterMeshRegistry registry,
            IFogClusterMeshBuilder meshBuilder,
            [InjectOptional] IFogClusterMaterialProvider materialProvider = null,
            [InjectOptional] IFogClusterMeshPresenter meshPresenter = null,
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
                Debug.LogError($"{ClusterDiagTag} ERROR missingMeshRegistry");
                return;
            }

            _registry.ClearAll();
            _fullRebuildKeys.Clear();

            if (!context.IsValid || fogService == null)
            {
                Debug.LogError($"{ClusterDiagTag} ERROR missingContext contextValid={context.IsValid}, hasFogService={fogService != null}");
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

            if (ShouldLogClusterUpdates())
            {
                Debug.Log(
                    $"{ClusterDiagTag} FullClusterRebuild START " +
                    $"clusters={_fullRebuildBuffer.Count}, " +
                    $"context={context.Width}x{context.Height}.");
            }

            RebuildClusters(
                _fullRebuildBuffer,
                context,
                fogService);
        }

        public void RebuildClusters(IReadOnlyList<FogClusterKey> dirtyClusters, FogWorldVisualContext context, IFogStateReader fogService)
        {
            if (_registry == null || _meshBuilder == null)
            {
                Debug.LogError($"{ClusterDiagTag} ERROR missingMeshRegistry");
                return;
            }

            if (dirtyClusters == null || dirtyClusters.Count == 0)
                return;

            if (!context.IsValid || fogService == null)
            {
                Debug.LogError($"{ClusterDiagTag} ERROR missingContext contextValid={context.IsValid}, hasFogService={fogService != null}");
                return;
            }

            long startedAt = Stopwatch.GetTimestamp();
            if (ShouldLogClusterUpdates())
            {
                Debug.Log(
                    $"{ClusterDiagTag} PartialClusterRebuild START " +
                    $"clusters={dirtyClusters.Count}.");
            }

            for (int i = 0; i < dirtyClusters.Count; i++)
            {
                var key = dirtyClusters[i];
                var handle = _registry.GetOrCreate(key);
                _meshBuilder.RebuildCluster(key, handle.Mesh, context, fogService);

                bool hasGeometry = handle.Mesh != null && handle.Mesh.vertexCount > 0;
                _meshPresenter.Apply(handle, hasGeometry);
            }

            double elapsedMs =
                (Stopwatch.GetTimestamp() - startedAt)
                * 1000d
                / Stopwatch.Frequency;

            if (ShouldLogClusterUpdates())
            {
                Debug.Log(
                    $"{ClusterDiagTag} PartialClusterRebuild DONE " +
                    $"clusters={dirtyClusters.Count}, " +
                    $"elapsedMs={elapsedMs:0.###}.");
            }

            if (Debug.isDebugBuild
                && elapsedMs >= SlowClusterRebuildThresholdMs)
            {
                Debug.Log(
                    $"{PerfLogTag} fog-clusters " +
                    $"clusters={dirtyClusters.Count} " +
                    $"elapsedMs={elapsedMs:0.###}");
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

        private bool ShouldLogClusterUpdates()
            => Debug.isDebugBuild
                && _settings != null
                && _settings.Volume.LogClusterUpdates;
    }
}
