using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogClusterMeshBuilder : IFogClusterMeshBuilder
    {
        private const string ClusterDiagTag = "[MoyvaFogClusterDiag]";
        private const float HeightEpsilon = 0.001f;
        private const float BoundarySurfaceOverlap = 0.02f;
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
            int sides = 0;
            var heightSampler = new FogVolumeHeightSampler(context, _settings);
            float cellSize = Mathf.Max(0.0001f, context.CellSize);
            Vector3 origin = context.HasMapWorldBounds ? context.MapWorldBounds.min : Vector3.zero;

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    var cell = new Vector2Int(x, y);
                    cellsChecked++;
                    if (!TryResolveRenderedSample(
                            cell,
                            context,
                            fogService,
                            heightSampler,
                            out FogCellRenderSample sample))
                    {
                        continue;
                    }

                    _geometryBuilder.AddCellQuad(
                        cell,
                        sample.Height,
                        cellSize,
                        origin,
                        sample.SubMeshIndex);
                    quads++;
                    sides += AddOpenSides(sample, context, fogService, heightSampler, cellSize, origin);
                }
            }

            _geometryBuilder.ApplyTo(mesh);

            if (ShouldLogClusterUpdates())
                Debug.Log($"{ClusterDiagTag} RebuildCluster key={key} cellsChecked={cellsChecked}, quads={quads}, sides={sides}.");
        }

        private static int ResolveSubMeshIndex(FogStateType state)
            => state == FogStateType.Explored ? 1 : 0;

        private int AddOpenSides(
            FogCellRenderSample sample,
            FogWorldVisualContext context,
            IFogStateReader fogService,
            FogVolumeHeightSampler heightSampler,
            float cellSize,
            Vector3 origin)
        {
            int sides = 0;
            float x0 = origin.x + sample.Cell.x * cellSize;
            float z0 = origin.z + sample.Cell.y * cellSize;
            float x1 = x0 + cellSize;
            float z1 = z0 + cellSize;

            sides += AddSideIfNeeded(
                sample,
                new Vector2Int(sample.Cell.x, sample.Cell.y + 1),
                context,
                fogService,
                heightSampler,
                new Vector3(x0, sample.Height, z1),
                new Vector3(x1, sample.Height, z1),
                cellSize);
            sides += AddSideIfNeeded(
                sample,
                new Vector2Int(sample.Cell.x + 1, sample.Cell.y),
                context,
                fogService,
                heightSampler,
                new Vector3(x1, sample.Height, z1),
                new Vector3(x1, sample.Height, z0),
                cellSize);
            sides += AddSideIfNeeded(
                sample,
                new Vector2Int(sample.Cell.x, sample.Cell.y - 1),
                context,
                fogService,
                heightSampler,
                new Vector3(x1, sample.Height, z0),
                new Vector3(x0, sample.Height, z0),
                cellSize);
            sides += AddSideIfNeeded(
                sample,
                new Vector2Int(sample.Cell.x - 1, sample.Cell.y),
                context,
                fogService,
                heightSampler,
                new Vector3(x0, sample.Height, z0),
                new Vector3(x0, sample.Height, z1),
                cellSize);

            return sides;
        }

        private int AddSideIfNeeded(
            FogCellRenderSample sample,
            Vector2Int neighborCell,
            FogWorldVisualContext context,
            IFogStateReader fogService,
            FogVolumeHeightSampler heightSampler,
            Vector3 topStart,
            Vector3 topEnd,
            float cellSize)
        {
            float bottomHeight;
            if (TryResolveRenderedSample(neighborCell, context, fogService, heightSampler, out FogCellRenderSample neighbor))
            {
                if (sample.Height <= neighbor.Height + HeightEpsilon)
                    return 0;

                bottomHeight = neighbor.Height;
            }
            else
            {
                bottomHeight = ResolveBoundaryBottomHeight(sample, neighborCell, context, heightSampler, cellSize);
            }

            if (sample.Height <= bottomHeight + HeightEpsilon)
                return 0;

            _geometryBuilder.AddCellSide(
                topStart,
                topEnd,
                new Vector3(topEnd.x, bottomHeight, topEnd.z),
                new Vector3(topStart.x, bottomHeight, topStart.z),
                sample.SubMeshIndex);
            return 1;
        }

        private static float ResolveBoundaryBottomHeight(
            FogCellRenderSample sample,
            Vector2Int neighborCell,
            FogWorldVisualContext context,
            FogVolumeHeightSampler heightSampler,
            float cellSize)
        {
            float surfaceHeight = sample.SurfaceHeight;
            if (IsInBounds(neighborCell, context))
                surfaceHeight = Mathf.Min(surfaceHeight, heightSampler.ResolveGeneratedSurfaceHeight(neighborCell));

            float bottomHeight = surfaceHeight - BoundarySurfaceOverlap;
            float fallbackDepth = Mathf.Max(BoundarySurfaceOverlap, cellSize * 0.05f);
            return Mathf.Min(bottomHeight, sample.Height - fallbackDepth);
        }

        private bool TryResolveRenderedSample(
            Vector2Int cell,
            FogWorldVisualContext context,
            IFogStateReader fogService,
            FogVolumeHeightSampler heightSampler,
            out FogCellRenderSample sample)
        {
            sample = default;
            if (!IsInBounds(cell, context))
                return false;

            FogStateType state = fogService.GetFogState(cell);
            if (!_materialProvider.ShouldRenderState(state))
                return false;

            var stateSettings = _materialProvider.ResolveStateSettings(state);
            sample = new FogCellRenderSample(
                cell,
                heightSampler.ResolveWorldHeight(heightSampler.ResolveGeneratedSurfaceHeight(cell), stateSettings),
                heightSampler.ResolveGeneratedSurfaceHeight(cell),
                ResolveSubMeshIndex(state));
            return true;
        }

        private static bool IsInBounds(Vector2Int cell, FogWorldVisualContext context)
            => cell.x >= 0 && cell.y >= 0 && cell.x < context.Width && cell.y < context.Height;

        private bool ShouldLogClusterUpdates()
            => _settings == null || _settings.Volume.LogClusterUpdates;

        private readonly struct FogCellRenderSample
        {
            public FogCellRenderSample(
                Vector2Int cell,
                float height,
                float surfaceHeight,
                int subMeshIndex)
            {
                Cell = cell;
                Height = height;
                SurfaceHeight = surfaceHeight;
                SubMeshIndex = subMeshIndex;
            }

            public Vector2Int Cell { get; }
            public float Height { get; }
            public float SurfaceHeight { get; }
            public int SubMeshIndex { get; }
        }
    }
}
