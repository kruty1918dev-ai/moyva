using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogClusterMeshBuilder : IFogClusterMeshBuilder
    {
        private const string ClusterDiagTag = "[MoyvaFogClusterDiag]";
        private readonly FogOfWarSettings _settings;
        private readonly IFogClusterGeometryBuilder _geometryBuilder;
        private readonly IFogClusterMaterialProvider _materialProvider;

        public FogClusterMeshBuilder(
            [InjectOptional] FogOfWarSettings settings = null,
            [InjectOptional] IFogClusterGeometryBuilder geometryBuilder = null,
            [InjectOptional] IFogClusterMaterialProvider materialProvider = null)
        {
            _settings = settings;
            _geometryBuilder = geometryBuilder ?? new FogClusterGeometryBuilder();
            _materialProvider = materialProvider ?? new FogClusterMaterialProvider(settings);
        }

        public void RebuildCluster(
            FogClusterKey key,
            Mesh mesh,
            FogWorldVisualContext context,
            IFogStateReader fogService)
        {
            if (mesh == null)
                return;

            _geometryBuilder.Clear();
            mesh.Clear();

            if (fogService == null || !context.IsValid)
                return;

            int clusterSize = Mathf.Max(1, _settings?.Volume.ClusterSize ?? 16);
            int startX = Mathf.Clamp(key.ClusterX * clusterSize, 0, context.Width);
            int startY = Mathf.Clamp(key.ClusterY * clusterSize, 0, context.Height);
            int endX = Mathf.Min(startX + clusterSize, context.Width);
            int endY = Mathf.Min(startY + clusterSize, context.Height);
            int cellsChecked = 0;
            int quads = 0;
            var heightSampler = new FogVolumeHeightSampler(context, _settings);
            float cellSize = Mathf.Max(0.0001f, context.CellSize);
            Vector3 origin = context.HasMapWorldBounds ? context.MapWorldBounds.min : Vector3.zero;

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    var cell = new Vector2Int(x, y);
                    cellsChecked++;
                    FogStateType state = fogService.GetFogState(cell);
                    if (!_materialProvider.ShouldRenderState(state))
                        continue;

                    int heightKey = heightSampler.ResolveHeightKey(cell);
                    var stateSettings = _materialProvider.ResolveStateSettings(state);
                    _geometryBuilder.AddCellQuad(
                        cell,
                        heightSampler.ResolveWorldHeight(heightKey, stateSettings),
                        cellSize,
                        origin,
                        ResolveSubMeshIndex(state));
                    quads++;
                }
            }

            _geometryBuilder.ApplyTo(mesh);

            if (ShouldLogClusterUpdates())
                Debug.Log($"{ClusterDiagTag} RebuildCluster key={key} cellsChecked={cellsChecked}, quads={quads}.");
        }

        private static int ResolveSubMeshIndex(FogStateType state)
            => state == FogStateType.Explored ? 1 : 0;

        private bool ShouldLogClusterUpdates()
            => _settings == null || _settings.Volume.LogClusterUpdates;
    }
}
