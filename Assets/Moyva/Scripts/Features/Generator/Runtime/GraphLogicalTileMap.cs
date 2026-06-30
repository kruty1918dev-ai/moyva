using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMap
    {
        public GraphLogicalTileMap(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            TileIds = new string[Width, Height];
            GraphLayerIds = new string[Width, Height];
            LayerNames = new string[Width, Height];
            LayerHeights = new float[Width, Height];
            SurfaceHeights = new float[Width, Height];
        }

        public int Width { get; }
        public int Height { get; }
        public string[,] TileIds { get; }
        public string[,] GraphLayerIds { get; }
        public string[,] LayerNames { get; }
        public float[,] LayerHeights { get; }
        public float[,] SurfaceHeights { get; }

        public Dictionary<string, bool[,]> BuildLayerMatrices()
        {
            var matrices = new Dictionary<string, bool[,]>(StringComparer.Ordinal);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    string layerId = GraphLayerIds[x, y];
                    if (string.IsNullOrEmpty(layerId))
                        continue;

                    if (!matrices.TryGetValue(layerId, out var matrix))
                    {
                        matrix = new bool[Width, Height];
                        matrices[layerId] = matrix;
                    }

                    matrix[x, y] = true;
                }
            }

            return matrices;
        }
    }

    internal static class GraphLogicalTileMapBuilder
    {
        public static GraphLogicalTileMap Build(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height)
        {
            var map = new GraphLogicalTileMap(width, height);
            if (graph == null || manager == null || compiled == null)
                return map;

            var ordered = new List<CompiledLayerMap>(compiled);
            ordered.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));

            for (int i = 0; i < ordered.Count; i++)
            {
                var layerMap = ordered[i];
                if (layerMap == null
                    || !layerMap.HasRenderableTileOutput
                    || string.IsNullOrEmpty(layerMap.GraphLayerId)
                    || string.IsNullOrEmpty(layerMap.BlueprintLayerGuid))
                    continue;

                var graphLayer = graph.GetLayerById(layerMap.GraphLayerId);
                if (graphLayer == null || !graphLayer.Enabled)
                    continue;

                var buildLayer = FindTilesBuildLayer(manager.configuration, layerMap.BlueprintLayerGuid);
                if (buildLayer == null || !buildLayer.isEnabled)
                    continue;

                var blueprint = manager.GetBlueprintLayerByGuid(layerMap.BlueprintLayerGuid);
                if (blueprint == null)
                    continue;

                float layerHeight = blueprint.defaultLayerHeight;
                float surfaceHeight = ResolveTwcLayerSurfaceHeight(blueprint, buildLayer);
                string tileId = !string.IsNullOrWhiteSpace(layerMap.GridTileId)
                    ? layerMap.GridTileId
                    : layerMap.GraphLayerId;
                string layerName = !string.IsNullOrWhiteSpace(layerMap.LayerName)
                    ? layerMap.LayerName
                    : graphLayer.Name;

                if (buildLayer.generateFlatSurface)
                {
                    FillLayerCells(map, layerMap.GraphLayerId, layerName, tileId, layerHeight, surfaceHeight);
                    continue;
                }

                if (blueprint.allPositions == null || blueprint.allPositions.Count == 0)
                    continue;

                foreach (var position in blueprint.allPositions)
                {
                    int x = Mathf.RoundToInt(position.x);
                    int y = Mathf.RoundToInt(position.y);
                    if (x < 0 || x >= map.Width || y < 0 || y >= map.Height)
                        continue;

                    SetCell(map, x, y, layerMap.GraphLayerId, layerName, tileId, layerHeight, surfaceHeight);
                }
            }

            return map;
        }

        private static void FillLayerCells(
            GraphLogicalTileMap map,
            string graphLayerId,
            string layerName,
            string tileId,
            float layerHeight,
            float surfaceHeight)
        {
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                    SetCell(map, x, y, graphLayerId, layerName, tileId, layerHeight, surfaceHeight);
            }
        }

        private static void SetCell(
            GraphLogicalTileMap map,
            int x,
            int y,
            string graphLayerId,
            string layerName,
            string tileId,
            float layerHeight,
            float surfaceHeight)
        {
            map.TileIds[x, y] = tileId;
            map.GraphLayerIds[x, y] = graphLayerId;
            map.LayerNames[x, y] = layerName;
            map.LayerHeights[x, y] = layerHeight;
            map.SurfaceHeights[x, y] = surfaceHeight;
        }

        internal static TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string blueprintLayerGuid)
        {
            if (configuration?.buildLayerFolders == null || string.IsNullOrWhiteSpace(blueprintLayerGuid))
                return null;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is not TilesBuildLayer buildLayer)
                        continue;

                    if (string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, StringComparison.Ordinal)
                        || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, StringComparison.Ordinal))
                    {
                        return buildLayer;
                    }
                }
            }

            return null;
        }

        internal static float ResolveTwcLayerSurfaceHeight(BlueprintLayer blueprint, TilesBuildLayer buildLayer)
        {
            float baseHeight = blueprint != null ? blueprint.defaultLayerHeight : 0f;
            if (buildLayer == null)
                return baseHeight;

            float layerBaseHeight = baseHeight + buildLayer.layerYOffset;
            if (buildLayer.tileLayers == null || buildLayer.tileLayers.Count == 0)
                return layerBaseHeight;

            bool hasTileLayer = false;
            float topHeight = layerBaseHeight;
            for (int i = 0; i < buildLayer.tileLayers.Count; i++)
            {
                var tileLayer = buildLayer.tileLayers[i];
                if (tileLayer == null)
                    continue;

                float candidate = layerBaseHeight + tileLayer.heightOffset;
                topHeight = hasTileLayer ? Mathf.Max(topHeight, candidate) : candidate;
                hasTileLayer = true;
            }

            return hasTileLayer ? topHeight : layerBaseHeight;
        }
    }

    internal static class GraphLogicalTileMapDiagnostics
    {
        private const string Tag = "[MoyvaGraphFinalMap]";
        private static Snapshot _lastPreview;
        private static Snapshot _lastScene;

        public static void EmitAndCompare(
            string source,
            GraphAsset graph,
            int seed,
            GraphLogicalTileMap map,
            UnityEngine.Object context = null)
        {
            if (map == null)
                return;

            var snapshot = Snapshot.Create(source, graph, seed, map);
            string summary = snapshot.BuildSummary();
            if (context != null)
                Debug.Log(summary, context);
            else
                Debug.Log(summary);

            bool isPreview = IsPreviewSource(source);
            var other = isPreview ? _lastScene : _lastPreview;
            if (other != null)
                LogComparison(snapshot, other, context);

            if (isPreview)
                _lastPreview = snapshot;
            else
                _lastScene = snapshot;
        }

        private static bool IsPreviewSource(string source)
        {
            return !string.IsNullOrEmpty(source)
                && source.IndexOf("preview", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void LogComparison(Snapshot current, Snapshot other, UnityEngine.Object context)
        {
            var builder = new StringBuilder(2048);
            builder.AppendLine($"{Tag} Preview/Scene comparison");
            builder.AppendLine($"Current: {current.Source} graph='{current.GraphName}' seed={current.Seed} size={current.Width}x{current.Height}");
            builder.AppendLine($"Other: {other.Source} graph='{other.GraphName}' seed={other.Seed} size={other.Width}x{other.Height}");

            if (!string.Equals(current.GraphName, other.GraphName, StringComparison.Ordinal)
                || current.Seed != other.Seed
                || current.Width != other.Width
                || current.Height != other.Height)
            {
                builder.AppendLine("Result: NOT_COMPARABLE (graph/seed/size differs).");
                Emit(builder.ToString(), context, warning: true);
                return;
            }

            int mismatchCount = 0;
            var examples = new List<string>(20);
            for (int x = 0; x < current.Width; x++)
            {
                for (int y = 0; y < current.Height; y++)
                {
                    string a = Normalize(current.LayerGrid[x, y]);
                    string b = Normalize(other.LayerGrid[x, y]);
                    if (string.Equals(a, b, StringComparison.Ordinal))
                        continue;

                    mismatchCount++;
                    if (examples.Count < 20)
                        examples.Add($"  - [{x}, {y}] {current.Source}='{a}' {other.Source}='{b}'");
                }
            }

            builder.AppendLine($"LayerHash: current={current.LayerHash:X16}, other={other.LayerHash:X16}");
            builder.AppendLine($"TileHash: current={current.TileHash:X16}, other={other.TileHash:X16}");
            builder.AppendLine($"MismatchCells: {mismatchCount}");
            if (examples.Count > 0)
            {
                builder.AppendLine("First mismatches:");
                for (int i = 0; i < examples.Count; i++)
                    builder.AppendLine(examples[i]);
            }

            Emit(builder.ToString(), context, mismatchCount > 0);
        }

        private static void Emit(string message, UnityEngine.Object context, bool warning)
        {
            if (warning)
            {
                if (context != null)
                    Debug.LogWarning(message, context);
                else
                    Debug.LogWarning(message);
                return;
            }

            if (context != null)
                Debug.Log(message, context);
            else
                Debug.Log(message);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrEmpty(value) ? "<empty>" : value;
        }

        private sealed class Snapshot
        {
            public string Source;
            public string GraphName;
            public int Seed;
            public int Width;
            public int Height;
            public ulong LayerHash;
            public ulong TileHash;
            public int EmptyCount;
            public int OccupiedCount;
            public int SameValueRegions;
            public int EmptyRegions;
            public int EdgeTransitions;
            public string[,] LayerGrid;
            public string[,] TileGrid;
            public Dictionary<string, int> LayerCounts;
            public Dictionary<string, int> TileCounts;

            public static Snapshot Create(string source, GraphAsset graph, int seed, GraphLogicalTileMap map)
            {
                var snapshot = new Snapshot
                {
                    Source = string.IsNullOrWhiteSpace(source) ? "Unknown" : source,
                    GraphName = graph != null ? graph.name : "<null>",
                    Seed = seed,
                    Width = map.Width,
                    Height = map.Height,
                    LayerGrid = Clone(map.LayerNames),
                    TileGrid = Clone(map.TileIds)
                };

                snapshot.LayerCounts = CountValues(snapshot.LayerGrid, out snapshot.EmptyCount, out snapshot.OccupiedCount);
                snapshot.TileCounts = CountValues(snapshot.TileGrid, out _, out _);
                snapshot.LayerHash = ComputeHash(snapshot.LayerGrid);
                snapshot.TileHash = ComputeHash(snapshot.TileGrid);
                snapshot.SameValueRegions = CountSameValueRegions(snapshot.LayerGrid);
                snapshot.EmptyRegions = CountEmptyRegions(snapshot.LayerGrid);
                snapshot.EdgeTransitions = CountEdgeTransitions(snapshot.LayerGrid);
                return snapshot;
            }

            public string BuildSummary()
            {
                var builder = new StringBuilder(2048);
                builder.AppendLine($"{Tag} {Source}");
                builder.AppendLine($"Graph: {GraphName}");
                builder.AppendLine($"Seed: {Seed}");
                builder.AppendLine($"MapSize: {Width}x{Height}");
                builder.AppendLine($"OccupiedTiles: {OccupiedCount}");
                builder.AppendLine($"EmptyTiles: {EmptyCount}");
                builder.AppendLine($"LayerHash: {LayerHash:X16}");
                builder.AppendLine($"TileHash: {TileHash:X16}");
                builder.AppendLine($"SameValueRegions: {SameValueRegions}");
                builder.AppendLine($"EmptyRegions: {EmptyRegions}");
                builder.AppendLine($"EdgeTransitions: {EdgeTransitions}");
                builder.AppendLine("LayerCounts: " + FormatCounts(LayerCounts));
                builder.AppendLine("TileCounts: " + FormatCounts(TileCounts));
                return builder.ToString();
            }
        }

        private static string[,] Clone(string[,] source)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var result = new string[width, height];
            Array.Copy(source, result, source.Length);
            return result;
        }

        private static Dictionary<string, int> CountValues(string[,] grid, out int emptyCount, out int occupiedCount)
        {
            emptyCount = 0;
            occupiedCount = 0;
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string value = Normalize(grid[x, y]);
                    if (value == "<empty>")
                        emptyCount++;
                    else
                        occupiedCount++;

                    counts.TryGetValue(value, out int count);
                    counts[value] = count + 1;
                }
            }

            return counts;
        }

        private static ulong ComputeHash(string[,] grid)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    string value = Normalize(grid[x, y]);
                    for (int i = 0; i < value.Length; i++)
                    {
                        hash ^= value[i];
                        hash *= prime;
                    }

                    hash ^= 31;
                    hash *= prime;
                }
            }

            return hash;
        }

        private static int CountSameValueRegions(string[,] grid)
        {
            return CountRegions(grid, value => true);
        }

        private static int CountEmptyRegions(string[,] grid)
        {
            return CountRegions(grid, string.IsNullOrEmpty);
        }

        private static int CountRegions(string[,] grid, Func<string, bool> include)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            var visited = new bool[width, height];
            var queue = new Queue<Vector2Int>();
            int regions = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (visited[x, y] || !include(grid[x, y]))
                        continue;

                    regions++;
                    string value = Normalize(grid[x, y]);
                    visited[x, y] = true;
                    queue.Enqueue(new Vector2Int(x, y));

                    while (queue.Count > 0)
                    {
                        var cell = queue.Dequeue();
                        TryVisit(cell.x + 1, cell.y);
                        TryVisit(cell.x - 1, cell.y);
                        TryVisit(cell.x, cell.y + 1);
                        TryVisit(cell.x, cell.y - 1);
                    }

                    void TryVisit(int nx, int ny)
                    {
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                            return;
                        if (visited[nx, ny] || !include(grid[nx, ny]))
                            return;
                        if (!string.Equals(Normalize(grid[nx, ny]), value, StringComparison.Ordinal))
                            return;

                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }

            return regions;
        }

        private static int CountEdgeTransitions(string[,] grid)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            int transitions = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string value = Normalize(grid[x, y]);
                    if (x + 1 < width && !string.Equals(value, Normalize(grid[x + 1, y]), StringComparison.Ordinal))
                        transitions++;
                    if (y + 1 < height && !string.Equals(value, Normalize(grid[x, y + 1]), StringComparison.Ordinal))
                        transitions++;
                }
            }

            return transitions;
        }

        private static string FormatCounts(Dictionary<string, int> counts)
        {
            if (counts == null || counts.Count == 0)
                return "<none>";

            return string.Join(", ", counts
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => $"{pair.Key}={pair.Value}"));
        }
    }
}
