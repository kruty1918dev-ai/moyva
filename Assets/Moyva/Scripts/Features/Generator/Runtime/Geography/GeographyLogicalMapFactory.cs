using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Projects a <see cref="WorldGeographyResult"/> into the canonical
    /// <see cref="GraphLogicalTileMap"/> so the chunk-first builder, grid
    /// writer and object spawner consume geography output through the same
    /// contract as graph output. Every cell gets exactly one terrain sample;
    /// gameplay objects become <see cref="LayerKind.ObjectSpawn"/> samples on
    /// top of the terrain sample.
    /// </summary>
    internal sealed class GeographyLogicalMapFactory
    {
        private const string SourceNodeId = "world-geography";

        public GraphLogicalTileMap Build(
            WorldGeographyResult result,
            GeographyVisualSet visuals)
        {
            int width = result.Width;
            int height = result.Height;
            var map = new GraphLogicalTileMap(width, height);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                string tileId = result.TileMap[x, y];
                float surfaceHeight = result.HeightMap[x, y];
                int level = result.TerrainLevelMap[x, y];

                map.AddSample(x, y, TerrainSample(
                    tileId, visuals, surfaceHeight, level));

                string objectId = result.ObjectMap?[x, y];
                if (!string.IsNullOrWhiteSpace(objectId))
                    map.AddSample(x, y, ObjectSample(objectId, surfaceHeight));
            }

            return map;
        }

        /// <summary>
        /// Compiled-layer descriptors for diagnostics/save parity with the
        /// graph path. There is no real graph node behind geography layers;
        /// <c>SourceNodeId</c> makes that explicit.
        /// </summary>
        public IReadOnlyList<CompiledLayerMap> BuildCompiledLayers(GeographyVisualSet visuals)
        {
            var layers = new List<CompiledLayerMap>();
            if (visuals == null)
                return layers;

            foreach (var visual in visuals.Layers)
            {
                layers.Add(new CompiledLayerMap
                {
                    GraphLayerId = visual.GraphLayerId,
                    GridTileId = visual.TileId,
                    BlueprintLayerGuid = visual.BlueprintLayerGuid,
                    LayerName = visual.TileId,
                    SortingOrder = visual.SortOrder,
                    GraphLayerOrder = visual.SortOrder,
                    TerrainPriority = 0,
                    BuildLayerGuid = visual.BuildLayerGuid,
                    PresetId = visual.PresetId,
                    SourceNodeId = SourceNodeId,
                    HasRenderableTileOutput = !string.IsNullOrWhiteSpace(visual.PresetId),
                });
            }

            layers.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));
            return layers;
        }

        private static GraphTileLayerSample TerrainSample(
            string tileId,
            GeographyVisualSet visuals,
            float surfaceHeight,
            int level)
        {
            GeographyVisualLayer visual = null;
            visuals?.TryGet(tileId, out visual);
            return new GraphTileLayerSample(
                graphLayerId: visual?.GraphLayerId ?? ("geography-" + tileId),
                graphLayerName: tileId,
                blueprintLayerGuid: visual?.BlueprintLayerGuid ?? string.Empty,
                buildLayerGuid: visual?.BuildLayerGuid ?? string.Empty,
                tileId: tileId,
                presetId: visual?.PresetId ?? string.Empty,
                layerKind: LayerKind.BaseTerrain,
                sortingOrder: visual?.SortOrder ?? 0,
                graphLayerOrder: visual?.SortOrder ?? 0,
                terrainPriority: level,
                height: surfaceHeight,
                surfaceHeight: surfaceHeight,
                sourceNodeId: SourceNodeId);
        }

        private static GraphTileLayerSample ObjectSample(string objectId, float surfaceHeight)
        {
            return new GraphTileLayerSample(
                graphLayerId: "geography-objects",
                graphLayerName: "objects",
                blueprintLayerGuid: string.Empty,
                buildLayerGuid: string.Empty,
                tileId: objectId,
                presetId: string.Empty,
                layerKind: LayerKind.ObjectSpawn,
                sortingOrder: 100,
                graphLayerOrder: 100,
                terrainPriority: 0,
                height: surfaceHeight,
                surfaceHeight: surfaceHeight,
                sourceNodeId: SourceNodeId);
        }
    }
}
