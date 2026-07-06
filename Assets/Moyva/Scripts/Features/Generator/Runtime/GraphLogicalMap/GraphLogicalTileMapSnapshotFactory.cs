using System;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMapSnapshotFactory : IGraphLogicalTileMapSnapshotFactory
    {
        private readonly IGraphLogicalTileMapMetricsService _metrics;

        public GraphLogicalTileMapSnapshotFactory(IGraphLogicalTileMapMetricsService metrics)
        {
            _metrics = metrics;
        }

        public GraphLogicalTileMapSnapshot Create(string source, GraphAsset graph, int seed, GraphLogicalTileMap map)
        {
            var snapshot = new GraphLogicalTileMapSnapshot
            {
                Source = string.IsNullOrWhiteSpace(source) ? "Unknown" : source,
                GraphName = graph != null ? graph.name : "<null>",
                Seed = seed,
                Width = map.Width,
                Height = map.Height,
                LayerGrid = Clone(map.LayerNames),
                TileGrid = Clone(map.TileIds)
            };

            snapshot.LayerCounts = _metrics.CountValues(snapshot.LayerGrid, out snapshot.EmptyCount, out snapshot.OccupiedCount);
            snapshot.TileCounts = _metrics.CountValues(snapshot.TileGrid, out _, out _);
            snapshot.LayerHash = _metrics.ComputeHash(snapshot.LayerGrid);
            snapshot.TileHash = _metrics.ComputeHash(snapshot.TileGrid);
            snapshot.SameValueRegions = _metrics.CountRegions(snapshot.LayerGrid, _ => true);
            snapshot.EmptyRegions = _metrics.CountRegions(snapshot.LayerGrid, string.IsNullOrEmpty);
            snapshot.EdgeTransitions = _metrics.CountEdgeTransitions(snapshot.LayerGrid);
            return snapshot;
        }

        private static string[,] Clone(string[,] source)
        {
            var result = new string[source.GetLength(0), source.GetLength(1)];
            Array.Copy(source, result, source.Length);
            return result;
        }
    }
}
