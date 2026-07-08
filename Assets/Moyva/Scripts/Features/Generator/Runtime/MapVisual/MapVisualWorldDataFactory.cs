using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualWorldDataFactory : IMapVisualWorldDataFactory
    {
        private readonly IGridService _gridService;
        private readonly IGridProjection _projection;
        private readonly IMapDataGenerator _generator;
        private readonly IGraphTwcMapDataDiagnostics _graphDiagnostics;
        private readonly IWorldGenerationDiagnostics _worldDiagnostics;

        public MapVisualWorldDataFactory(
            IGridService gridService,
            IGridProjection projection,
            IMapDataGenerator generator,
            [InjectOptional] IGraphTwcMapDataDiagnostics graphDiagnostics = null,
            [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null)
        {
            _gridService = gridService;
            _projection = projection;
            _generator = generator;
            _graphDiagnostics = graphDiagnostics;
            _worldDiagnostics = worldDiagnostics;
        }

        public GeneratedWorldData Generate()
        {
            int requestedWidth = _gridService.GridWidth;
            int requestedHeight = _gridService.GridHeight;
            string[,] biomeMap = null;
            string[,] objectMap = null;
            float[,] heightMap = null;
            string[,] buildingMap = null;
            _generator.GenerateMapData(requestedWidth, requestedHeight, (biomes, objects, heights, buildings) =>
            {
                biomeMap = biomes;
                objectMap = objects;
                heightMap = heights;
                buildingMap = buildings;
            });

            var data = new GeneratedWorldData
            {
                Width = ResolveWidth(biomeMap, objectMap, heightMap, buildingMap),
                Height = ResolveHeight(biomeMap, objectMap, heightMap, buildingMap),
                GridTopology = _projection.Topology,
                ProjectionMode = _projection.ProjectionMode,
                RenderMode = _projection.ProjectionMode == GridProjectionMode.Isometric3DPreview ? GridRenderMode.Mesh3DPreview : GridRenderMode.Mesh3D,
                NeighborhoodMode = ResolveNeighborhoodMode(_projection),
                BiomeMap = biomeMap,
                ObjectMap = objectMap,
                HeightMap = heightMap,
                BuildingMap = buildingMap,
                LogicalTileMap = _graphDiagnostics?.LastLogicalMap,
                CompiledLayers = _graphDiagnostics?.LastCompiledLayers,
                CellSize = _graphDiagnostics?.LastCellSize ?? 1f
            };
            if (_graphDiagnostics != null && _graphDiagnostics.TryGetLastBaseMapWorldBounds(out var bounds))
            {
                data.HasBaseMapWorldBounds = true;
                data.BaseMapWorldBounds = bounds;
            }
            ApplyLaunchMetadata(data);
            _worldDiagnostics?.GraphMapDataGenerated($"graph={_graphDiagnostics?.DiagnosticGraphName ?? "null"}, map={data.Width}x{data.Height}, seed={_graphDiagnostics?.DiagnosticSeed ?? 0}");
            return data;
        }

        private static int ResolveWidth(string[,] biomeMap, string[,] objectMap, float[,] heightMap, string[,] buildingMap)
            => biomeMap?.GetLength(0) ?? objectMap?.GetLength(0) ?? heightMap?.GetLength(0) ?? buildingMap?.GetLength(0) ?? 0;

        private static int ResolveHeight(string[,] biomeMap, string[,] objectMap, float[,] heightMap, string[,] buildingMap)
            => biomeMap?.GetLength(1) ?? objectMap?.GetLength(1) ?? heightMap?.GetLength(1) ?? buildingMap?.GetLength(1) ?? 0;

        private static GridNeighborhoodMode ResolveNeighborhoodMode(IGridProjection projection)
        {
            if (projection.Topology == GridTopology.HexAxial)
                return GridNeighborhoodMode.HexAxial6;
            return projection.ProjectionMode == GridProjectionMode.Isometric3DPreview ? GridNeighborhoodMode.VonNeumann4 : GridNeighborhoodMode.Moore8;
        }

        private static void ApplyLaunchMetadata(GeneratedWorldData data)
        {
            if (data == null || !GameLaunchContext.HasWorldSettings)
                return;
            data.WorldName = GameLaunchContext.WorldName;
            data.Seed = GameLaunchContext.Seed;
            data.Size = GameLaunchContext.Size;
            data.MapType = GameLaunchContext.MapType;
            data.Difficulty = GameLaunchContext.Difficulty;
        }
    }
}
