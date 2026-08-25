using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphLayerCoverageAudit
    {
        private const string Prefix = "[MOYVA_LAYER_COVERAGE]";

        // Детальний список нод виводиться лише для розріджених шарів.
        private const float SparseLayerThreshold = 0.15f;

        private static bool Enabled =>
            Application.isEditor || Debug.isDebugBuild;

        [System.Diagnostics.Conditional("MOYVA_DEEP_GENERATION_DIAGNOSTICS")]
        public static void LogEvaluation(
            GraphAsset graph,
            GraphEvaluationSnapshot snapshot)
        {
            if (!Enabled || graph == null || snapshot == null)
                return;

            var builder = new StringBuilder(2048);

            builder.Append(Prefix)
                .Append(" stage=EVALUATION")
                .Append(" success=")
                .Append(snapshot.Success)
                .Append(" seed=")
                .Append(snapshot.Seed)
                .Append(" map=")
                .Append(snapshot.MapSize.x)
                .Append('x')
                .Append(snapshot.MapSize.y)
                .AppendLine();

            foreach (GeneratorLayerDefinition layer in EnumerateLayers(graph))
            {
                snapshot.CompiledLayerMatrices.TryGetValue(
                    layer.Id,
                    out bool[,] mask);

                MaskStats stats = AnalyzeMask(mask);

                OutputNode outputNode =
                    GraphLayerRuntimeSemantics.GetLayerOutputNode(
                        graph,
                        layer.Id);

                builder.Append("layer=")
                    .Append(Safe(layer.Name))
                    .Append(" id=")
                    .Append(Safe(layer.Id))
                    .Append(" height=")
                    .Append(Format(layer.DefaultHeight))
                    .Append(" outputKind=")
                    .Append(outputNode != null
                        ? outputNode.OutputKind.ToString()
                        : "<missing>")
                    .Append(' ');

                AppendMaskStats(builder, stats);
                builder.AppendLine();

                if (ShouldAppendNodeDetails(layer, stats))
                {
                    AppendNodeDetails(
                        builder,
                        graph,
                        layer,
                        snapshot);
                }
            }
        }

        [System.Diagnostics.Conditional("MOYVA_DEEP_GENERATION_DIAGNOSTICS")]
        public static void LogCompilerMasks(
    GraphAsset graph,
    IReadOnlyDictionary<string, bool[,]> masks)
        {
            if (!Enabled || graph == null)
                return;

            var builder =
                new StringBuilder(1024);

            builder.Append(Prefix)
                .Append(" stage=COMPILER_MASK")
                .AppendLine();

            foreach (GeneratorLayerDefinition layer
                     in EnumerateLayers(graph))
            {
                bool[,] mask = null;

                if (masks != null)
                {
                    masks.TryGetValue(
                        layer.Id,
                        out mask);
                }

                GraphExecutionScope scope =
                    graph.CreateExecutionScope(
                        layer.Id);

                int nativeTwcModifiers = 0;
                int layerReferences = 0;

                if (scope?.Nodes != null)
                {
                    for (int i = 0;
                         i < scope.Nodes.Count;
                         i++)
                    {
                        NodeBase node =
                            scope.Nodes[i];

                        if (node is TwcModifierNode)
                            nativeTwcModifiers++;

                        if (node is LayerMaskReferenceNode)
                            layerReferences++;
                    }
                }

                string mode;

                if (mask != null)
                {
                    mode =
                        "precomputed-mask-authoritative";
                }
                else if (nativeTwcModifiers > 0
                         || layerReferences > 0)
                {
                    mode =
                        "native-twc-fallback";
                }
                else
                {
                    mode =
                        "no-mask";
                }

                builder.Append("layer=")
                    .Append(Safe(layer.Name))
                    .Append(" id=")
                    .Append(Safe(layer.Id))
                    .Append(" mode=")
                    .Append(mode)
                    .Append(" nativeModifiers=")
                    .Append(nativeTwcModifiers)
                    .Append(" layerReferences=")
                    .Append(layerReferences)
                    .Append(' ');

                AppendMaskStats(
                    builder,
                    AnalyzeMask(mask));

                builder.AppendLine();
            }
        }

        [System.Diagnostics.Conditional("MOYVA_DEEP_GENERATION_DIAGNOSTICS")]
        public static void LogBlueprintPositions(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            Vector2Int mapSize)
        {
            if (!Enabled
                || graph == null
                || manager == null
                || compiled == null)
            {
                return;
            }

            var builder = new StringBuilder(1024);

            builder.Append(Prefix)
                .Append(" stage=BLUEPRINT")
                .Append(" map=")
                .Append(mapSize.x)
                .Append('x')
                .Append(mapSize.y)
                .AppendLine();

            for (int i = 0; i < compiled.Count; i++)
            {
                CompiledLayerMap layerMap = compiled[i];

                if (layerMap == null)
                    continue;

                GeneratorLayerDefinition graphLayer =
                    graph.GetLayerById(layerMap.GraphLayerId);

                BlueprintLayer blueprint =
                    manager.GetBlueprintLayerByGuid(
                        layerMap.BlueprintLayerGuid);

                int rawPositions =
                    blueprint?.allPositions?.Count ?? 0;

                int inBoundsPositions = 0;
                int duplicatePositions = 0;
                int outOfBoundsPositions = 0;

                var uniquePositions =
                    new HashSet<Vector2Int>();

                if (blueprint?.allPositions != null)
                {
                    foreach (Vector2 position in blueprint.allPositions)
                    {
                        int x = Mathf.RoundToInt(position.x);
                        int y = Mathf.RoundToInt(position.y);

                        if (x < 0
                            || x >= mapSize.x
                            || y < 0
                            || y >= mapSize.y)
                        {
                            outOfBoundsPositions++;
                            continue;
                        }

                        inBoundsPositions++;

                        if (!uniquePositions.Add(
                                new Vector2Int(x, y)))
                        {
                            duplicatePositions++;
                        }
                    }
                }

                builder.Append("layer=")
                    .Append(Safe(
                        graphLayer?.Name
                        ?? layerMap.LayerName
                        ?? layerMap.GraphLayerId))
                    .Append(" id=")
                    .Append(Safe(layerMap.GraphLayerId))
                    .Append(" blueprintGuid=")
                    .Append(Safe(layerMap.BlueprintLayerGuid))
                    .Append(" raw=")
                    .Append(rawPositions)
                    .Append(" inBounds=")
                    .Append(inBoundsPositions)
                    .Append(" unique=")
                    .Append(uniquePositions.Count)
                    .Append(" duplicates=")
                    .Append(duplicatePositions)
                    .Append(" outOfBounds=")
                    .Append(outOfBoundsPositions)
                    .Append(" bounds=")
                    .Append(DescribePointBounds(uniquePositions))
                    .AppendLine();
            }
        }

        [System.Diagnostics.Conditional("MOYVA_DEEP_GENERATION_DIAGNOSTICS")]
        public static void LogLogicalMap(
            GraphAsset graph,
            GraphLogicalTileMap map)
        {
            if (!Enabled || graph == null || map == null)
                return;

            var sampleCounts =
                new Dictionary<string, int>(
                    StringComparer.Ordinal);

            var cellsByLayer =
                new Dictionary<string, HashSet<Vector2Int>>(
                    StringComparer.Ordinal);

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                {
                    TileStackCell stack =
                        map.GetCellStack(x, y);

                    if (stack == null)
                        continue;

                    for (int i = 0; i < stack.Samples.Count; i++)
                    {
                        GraphTileLayerSample sample =
                            stack.Samples[i];

                        string layerId =
                            sample.GraphLayerId
                            ?? string.Empty;

                        sampleCounts.TryGetValue(
                            layerId,
                            out int currentCount);

                        sampleCounts[layerId] =
                            currentCount + 1;

                        if (!cellsByLayer.TryGetValue(
                                layerId,
                                out HashSet<Vector2Int> cells))
                        {
                            cells = new HashSet<Vector2Int>();
                            cellsByLayer[layerId] = cells;
                        }

                        cells.Add(new Vector2Int(x, y));
                    }
                }

            var builder = new StringBuilder(1024);

            builder.Append(Prefix)
                .Append(" stage=LOGICAL_MAP")
                .Append(" map=")
                .Append(map.Width)
                .Append('x')
                .Append(map.Height)
                .AppendLine();

            foreach (GeneratorLayerDefinition layer in EnumerateLayers(graph))
            {
                sampleCounts.TryGetValue(
                    layer.Id,
                    out int sampleCount);

                int uniqueCells =
                    cellsByLayer.TryGetValue(
                        layer.Id,
                        out HashSet<Vector2Int> cells)
                        ? cells.Count
                        : 0;

                builder.Append("layer=")
                    .Append(Safe(layer.Name))
                    .Append(" id=")
                    .Append(Safe(layer.Id))
                    .Append(" samples=")
                    .Append(sampleCount)
                    .Append(" uniqueCells=")
                    .Append(uniqueCells)
                    .Append(" coverage=")
                    .Append(FormatPercent(
                        uniqueCells,
                        map.Width * map.Height))
                    .AppendLine();
            }
        }

        [System.Diagnostics.Conditional("MOYVA_DEEP_GENERATION_DIAGNOSTICS")]
        public static void LogResolvedWinners(
            GraphLogicalTileMap map,
            IReadOnlyDictionary<
                Vector2Int,
                ResolvedTileComposition> resolved)
        {
            if (!Enabled || map == null || resolved == null)
                return;

            var layerNames =
                new Dictionary<string, string>(
                    StringComparer.Ordinal);

            var presentCells =
                new Dictionary<string, HashSet<Vector2Int>>(
                    StringComparer.Ordinal);

            var terrainPresentCells =
                new Dictionary<string, HashSet<Vector2Int>>(
                    StringComparer.Ordinal);

            var winnerCells =
                new Dictionary<string, HashSet<Vector2Int>>(
                    StringComparer.Ordinal);

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                {
                    Vector2Int cell =
                        new Vector2Int(x, y);

                    TileStackCell stack =
                        map.GetCellStack(x, y);

                    if (stack == null)
                        continue;

                    for (int i = 0; i < stack.Samples.Count; i++)
                    {
                        GraphTileLayerSample sample =
                            stack.Samples[i];

                        string layerId =
                            sample.GraphLayerId
                            ?? string.Empty;

                        if (!layerNames.ContainsKey(layerId))
                        {
                            layerNames[layerId] =
                                sample.GraphLayerName
                                ?? layerId;
                        }

                        AddCell(
                            presentCells,
                            layerId,
                            cell);

                        if (sample.IsTerrainLike
                            && sample.LayerKind
                            != LayerKind.OverlayTerrain)
                        {
                            AddCell(
                                terrainPresentCells,
                                layerId,
                                cell);
                        }
                    }
                }

            foreach (KeyValuePair<
                         Vector2Int,
                         ResolvedTileComposition> pair in resolved)
            {
                ResolvedTileComposition composition =
                    pair.Value;

                if (!composition.HasMainTerrain)
                    continue;

                string layerId =
                    composition.MainTerrain.GraphLayerId
                    ?? string.Empty;

                if (!layerNames.ContainsKey(layerId))
                {
                    layerNames[layerId] =
                        composition.MainTerrain.GraphLayerName
                        ?? layerId;
                }

                AddCell(
                    winnerCells,
                    layerId,
                    pair.Key);
            }

            var allLayerIds =
                new HashSet<string>(
                    presentCells.Keys,
                    StringComparer.Ordinal);

            allLayerIds.UnionWith(
                winnerCells.Keys);

            var builder = new StringBuilder(1024);

            builder.Append(Prefix)
                .Append(" stage=RESOLVED")
                .Append(" map=")
                .Append(map.Width)
                .Append('x')
                .Append(map.Height)
                .AppendLine();

            foreach (string layerId in allLayerIds
                         .OrderBy(id =>
                             layerNames.TryGetValue(
                                 id,
                                 out string name)
                                 ? name
                                 : id,
                             StringComparer.Ordinal))
            {
                int present =
                    presentCells.TryGetValue(
                        layerId,
                        out HashSet<Vector2Int> allCells)
                        ? allCells.Count
                        : 0;

                int terrainPresent =
                    terrainPresentCells.TryGetValue(
                        layerId,
                        out HashSet<Vector2Int> terrainCells)
                        ? terrainCells.Count
                        : 0;

                int winners =
                    winnerCells.TryGetValue(
                        layerId,
                        out HashSet<Vector2Int> wonCells)
                        ? wonCells.Count
                        : 0;

                int lost =
                    Mathf.Max(
                        0,
                        terrainPresent - winners);

                builder.Append("layer=")
                    .Append(Safe(
                        layerNames.TryGetValue(
                            layerId,
                            out string layerName)
                            ? layerName
                            : layerId))
                    .Append(" id=")
                    .Append(Safe(layerId))
                    .Append(" present=")
                    .Append(present)
                    .Append(" terrainPresent=")
                    .Append(terrainPresent)
                    .Append(" winners=")
                    .Append(winners)
                    .Append(" lostToHigherOrOther=")
                    .Append(lost)
                    .Append(" winnerCoverage=")
                    .Append(FormatPercent(
                        winners,
                        map.Width * map.Height))
                    .AppendLine();
            }
        }

        private static void AppendNodeDetails(
            StringBuilder builder,
            GraphAsset graph,
            GeneratorLayerDefinition layer,
            GraphEvaluationSnapshot snapshot)
        {
            GraphExecutionScope scope =
                graph.CreateExecutionScope(layer.Id);

            if (scope?.Nodes == null)
                return;

            var nodes =
                new List<NodeBase>();

            for (int i = 0; i < scope.Nodes.Count; i++)
            {
                NodeBase node = scope.Nodes[i];

                if (node == null)
                    continue;

                if (!snapshot.NodeRecords.TryGetValue(
                        node.NodeId,
                        out NodeEvaluationRecord record))
                {
                    continue;
                }

                if (!record.IsAuthoritative)
                    continue;

                nodes.Add(node);
            }

            nodes.Sort(
                (left, right) =>
                    GetNodeOrder(snapshot, left)
                        .CompareTo(
                            GetNodeOrder(snapshot, right)));

            builder.Append("  nodeBreakdown layer=")
                .Append(Safe(layer.Name))
                .AppendLine();

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeBase node = nodes[i];

                snapshot.NodeRecords.TryGetValue(
                    node.NodeId,
                    out NodeEvaluationRecord record);

                builder.Append("    order=")
                    .Append(record?.Log?.OrderIndex ?? -1)
                    .Append(" node=")
                    .Append(Safe(node.Title))
                    .Append(" type=")
                    .Append(node.GetType().Name)
                    .Append(" status=")
                    .Append(record?.Log?.Status.ToString()
                            ?? "<unknown>")
                    .Append(" participation=")
                    .Append(record?.Participation.ToString()
                            ?? "<unknown>")
                    .Append(" data=")
                    .Append(DescribeRecord(record))
                    .AppendLine();
            }
        }

        private static string DescribeRecord(
            NodeEvaluationRecord record)
        {
            if (record == null)
                return "<missing-record>";

            var parts = new List<string>();

            object[] outputs = record.Outputs;

            if (outputs != null)
            {
                for (int i = 0; i < outputs.Length; i++)
                {
                    string description =
                        DescribeValue(outputs[i]);

                    if (!string.IsNullOrEmpty(description))
                    {
                        parts.Add(
                            $"out{i}:{description}");
                    }
                }
            }

            string artifactDescription =
                DescribeValue(record.Artifact);

            if (!string.IsNullOrEmpty(
                    artifactDescription))
            {
                parts.Add(
                    $"artifact:{artifactDescription}");
            }

            if (parts.Count == 0)
                return "<no-output-data>";

            return string.Join(" | ", parts);
        }

        private static string DescribeValue(object value)
        {
            switch (value)
            {
                case null:
                    return null;

                case LayerOutputSnapshot snapshot:
                    return DescribeLayerOutputSnapshot(
                        snapshot);

                case bool[,] mask:
                    return DescribeBooleanMask(mask);

                case string[,] stringMap:
                    return DescribeStringMap(stringMap);

                case float[,] floatMap:
                    return DescribeFloatMap(floatMap);

                case ILayerMaskArtifact maskArtifact:
                    return "layerMask="
                           + DescribeBooleanMask(
                               maskArtifact.LayerMask);

                default:
                    return value.GetType().Name;
            }
        }

        private static string DescribeLayerOutputSnapshot(
            LayerOutputSnapshot snapshot)
        {
            return "kind="
                   + snapshot.OutputKind
                   + ",layerMask="
                   + DescribeBooleanMask(snapshot.LayerMask)
                   + ",explicitMask="
                   + DescribeBooleanMask(snapshot.Mask)
                   + ",biome="
                   + DescribeStringMap(snapshot.BiomeMap)
                   + ",height="
                   + DescribeFloatMap(snapshot.HeightMap);
        }

        private static string DescribeBooleanMask(
            bool[,] mask)
        {
            MaskStats stats = AnalyzeMask(mask);

            if (!stats.HasData)
                return "<null>";

            return stats.TrueCount
                   + "/"
                   + stats.Total
                   + "("
                   + FormatPercent(
                       stats.TrueCount,
                       stats.Total)
                   + ")";
        }

        private static string DescribeStringMap(
            string[,] map)
        {
            if (map == null)
                return "<null>";

            int populated = 0;

            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (!string.IsNullOrEmpty(map[x, y]))
                        populated++;
                }

            return populated
                   + "/"
                   + map.Length;
        }

        private static string DescribeFloatMap(
            float[,] map)
        {
            if (map == null)
                return "<null>";

            int finite = 0;
            int nonZero = 0;
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;

            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    float value = map[x, y];

                    if (float.IsNaN(value)
                        || float.IsInfinity(value))
                    {
                        continue;
                    }

                    finite++;

                    if (Mathf.Abs(value) > 0.0001f)
                        nonZero++;

                    minimum = Mathf.Min(minimum, value);
                    maximum = Mathf.Max(maximum, value);
                }

            if (finite == 0)
                return "finite=0";

            return "nonZero="
                   + nonZero
                   + "/"
                   + map.Length
                   + ",min="
                   + Format(minimum)
                   + ",max="
                   + Format(maximum);
        }

        private static MaskStats AnalyzeMask(
            bool[,] mask)
        {
            if (mask == null)
                return default;

            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            int trueCount = 0;
            int edgeTouches = 0;

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            var visited =
                new bool[width, height];

            int componentCount = 0;
            int largestComponent = 0;

            var queue =
                new Queue<Vector2Int>();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (!mask[x, y])
                        continue;

                    trueCount++;

                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);

                    if (x == 0
                        || y == 0
                        || x == width - 1
                        || y == height - 1)
                    {
                        edgeTouches++;
                    }
                }

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (!mask[x, y] || visited[x, y])
                        continue;

                    componentCount++;
                    int componentSize = 0;

                    visited[x, y] = true;
                    queue.Enqueue(new Vector2Int(x, y));

                    while (queue.Count > 0)
                    {
                        Vector2Int current =
                            queue.Dequeue();

                        componentSize++;

                        TryEnqueue(
                            current.x - 1,
                            current.y,
                            mask,
                            visited,
                            queue);

                        TryEnqueue(
                            current.x + 1,
                            current.y,
                            mask,
                            visited,
                            queue);

                        TryEnqueue(
                            current.x,
                            current.y - 1,
                            mask,
                            visited,
                            queue);

                        TryEnqueue(
                            current.x,
                            current.y + 1,
                            mask,
                            visited,
                            queue);
                    }

                    largestComponent = Mathf.Max(
                        largestComponent,
                        componentSize);
                }

            return new MaskStats(
                width,
                height,
                trueCount,
                minX,
                minY,
                maxX,
                maxY,
                componentCount,
                largestComponent,
                edgeTouches);
        }

        private static void TryEnqueue(
            int x,
            int y,
            bool[,] mask,
            bool[,] visited,
            Queue<Vector2Int> queue)
        {
            if (x < 0
                || y < 0
                || x >= mask.GetLength(0)
                || y >= mask.GetLength(1)
                || !mask[x, y]
                || visited[x, y])
            {
                return;
            }

            visited[x, y] = true;
            queue.Enqueue(new Vector2Int(x, y));
        }

        private static void AppendMaskStats(
            StringBuilder builder,
            MaskStats stats)
        {
            if (!stats.HasData)
            {
                builder.Append("mask=<null>");
                return;
            }

            builder.Append("mask=")
                .Append(stats.TrueCount)
                .Append('/')
                .Append(stats.Total)
                .Append(" coverage=")
                .Append(FormatPercent(
                    stats.TrueCount,
                    stats.Total))
                .Append(" bounds=")
                .Append(stats.DescribeBounds())
                .Append(" components=")
                .Append(stats.ComponentCount)
                .Append(" largestComponent=")
                .Append(stats.LargestComponent)
                .Append(" edgeTouches=")
                .Append(stats.EdgeTouches);
        }

        private static bool ShouldAppendNodeDetails(
            GeneratorLayerDefinition layer,
            MaskStats stats)
        {
            if (!stats.HasData || stats.Total <= 0)
                return true;

            float coverage =
                (float)stats.TrueCount / stats.Total;

            return coverage <= SparseLayerThreshold
                   || layer.DefaultHeight >= 1f;
        }

        private static int GetNodeOrder(
            GraphEvaluationSnapshot snapshot,
            NodeBase node)
        {
            if (snapshot.NodeRecords.TryGetValue(
                    node.NodeId,
                    out NodeEvaluationRecord record))
            {
                return record.Log?.OrderIndex
                       ?? int.MaxValue;
            }

            return int.MaxValue;
        }

        private static IEnumerable<
            GeneratorLayerDefinition> EnumerateLayers(
            GraphAsset graph)
        {
            if (graph?.Layers == null)
                yield break;

            foreach (GeneratorLayerDefinition layer
                     in graph.Layers
                         .Where(layer =>
                             layer != null
                             && layer.Enabled)
                         .OrderBy(layer =>
                             layer.SortingOrder)
                         .ThenBy(layer =>
                             layer.Name,
                             StringComparer.Ordinal))
            {
                yield return layer;
            }
        }

        private static void AddCell(
            Dictionary<string, HashSet<Vector2Int>> dictionary,
            string layerId,
            Vector2Int cell)
        {
            if (!dictionary.TryGetValue(
                    layerId,
                    out HashSet<Vector2Int> cells))
            {
                cells = new HashSet<Vector2Int>();
                dictionary[layerId] = cells;
            }

            cells.Add(cell);
        }

        private static string DescribePointBounds(
            HashSet<Vector2Int> points)
        {
            if (points == null || points.Count == 0)
                return "<empty>";

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (Vector2Int point in points)
            {
                minX = Mathf.Min(minX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxX = Mathf.Max(maxX, point.x);
                maxY = Mathf.Max(maxY, point.y);
            }

            return $"({minX},{minY})-({maxX},{maxY})";
        }

        private static string FormatPercent(
            int value,
            int total)
        {
            if (total <= 0)
                return "0%";

            return (
                    (double)value
                    / total
                    * 100d)
                .ToString(
                    "0.###",
                    CultureInfo.InvariantCulture)
                + "%";
        }

        private static string Format(float value)
        {
            return value.ToString(
                "0.###",
                CultureInfo.InvariantCulture);
        }

        private static string Safe(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "<empty>"
                : value.Replace(
                    '\n',
                    ' ');
        }

        private readonly struct MaskStats
        {
            public MaskStats(
                int width,
                int height,
                int trueCount,
                int minX,
                int minY,
                int maxX,
                int maxY,
                int componentCount,
                int largestComponent,
                int edgeTouches)
            {
                Width = width;
                Height = height;
                TrueCount = trueCount;
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
                ComponentCount = componentCount;
                LargestComponent = largestComponent;
                EdgeTouches = edgeTouches;
                HasData = true;
            }

            public bool HasData { get; }
            public int Width { get; }
            public int Height { get; }
            public int TrueCount { get; }
            public int MinX { get; }
            public int MinY { get; }
            public int MaxX { get; }
            public int MaxY { get; }
            public int ComponentCount { get; }
            public int LargestComponent { get; }
            public int EdgeTouches { get; }
            public int Total => Width * Height;

            public string DescribeBounds()
            {
                if (!HasData || TrueCount == 0)
                    return "<empty>";

                return $"({MinX},{MinY})-({MaxX},{MaxY})";
            }
        }
    }
}