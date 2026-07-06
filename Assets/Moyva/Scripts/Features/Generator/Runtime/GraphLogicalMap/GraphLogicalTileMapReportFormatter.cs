using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMapReportFormatter : IGraphLogicalTileMapReportFormatter
    {
        public string BuildSummary(GraphLogicalTileMapSnapshot snapshot)
        {
            var builder = new StringBuilder(2048);
            builder.AppendLine($"{GraphLogicalTileMapText.Tag} {snapshot.Source}");
            builder.AppendLine($"Graph: {snapshot.GraphName}");
            builder.AppendLine($"Seed: {snapshot.Seed}");
            builder.AppendLine($"MapSize: {snapshot.Width}x{snapshot.Height}");
            builder.AppendLine($"OccupiedTiles: {snapshot.OccupiedCount}");
            builder.AppendLine($"EmptyTiles: {snapshot.EmptyCount}");
            builder.AppendLine($"LayerHash: {snapshot.LayerHash:X16}");
            builder.AppendLine($"TileHash: {snapshot.TileHash:X16}");
            builder.AppendLine($"SameValueRegions: {snapshot.SameValueRegions}");
            builder.AppendLine($"EmptyRegions: {snapshot.EmptyRegions}");
            builder.AppendLine($"EdgeTransitions: {snapshot.EdgeTransitions}");
            builder.AppendLine("LayerCounts: " + FormatCounts(snapshot.LayerCounts));
            builder.AppendLine("TileCounts: " + FormatCounts(snapshot.TileCounts));
            return builder.ToString();
        }

        public string BuildComparison(GraphLogicalTileMapSnapshot current,
            GraphLogicalTileMapSnapshot other, out int mismatchCount)
        {
            var builder = new StringBuilder(2048);
            builder.AppendLine($"{GraphLogicalTileMapText.Tag} Preview/Scene comparison");
            builder.AppendLine($"Current: {current.Source} graph='{current.GraphName}' seed={current.Seed} size={current.Width}x{current.Height}");
            builder.AppendLine($"Other: {other.Source} graph='{other.GraphName}' seed={other.Seed} size={other.Width}x{other.Height}");
            if (!CanCompare(current, other))
            {
                mismatchCount = -1;
                builder.AppendLine("Result: NOT_COMPARABLE (graph/seed/size differs).");
                return builder.ToString();
            }

            var examples = CollectMismatches(current, other, out mismatchCount);
            builder.AppendLine($"LayerHash: current={current.LayerHash:X16}, other={other.LayerHash:X16}");
            builder.AppendLine($"TileHash: current={current.TileHash:X16}, other={other.TileHash:X16}");
            builder.AppendLine($"MismatchCells: {mismatchCount}");
            if (examples.Count > 0)
            {
                builder.AppendLine("First mismatches:");
                foreach (var example in examples)
                    builder.AppendLine(example);
            }

            return builder.ToString();
        }

        private static bool CanCompare(GraphLogicalTileMapSnapshot current, GraphLogicalTileMapSnapshot other)
        {
            return current.GraphName == other.GraphName
                   && current.Seed == other.Seed
                   && current.Width == other.Width
                   && current.Height == other.Height;
        }

        private static List<string> CollectMismatches(GraphLogicalTileMapSnapshot current,
            GraphLogicalTileMapSnapshot other, out int mismatchCount)
        {
            mismatchCount = 0;
            var examples = new List<string>(20);
            for (int x = 0; x < current.Width; x++)
            for (int y = 0; y < current.Height; y++)
            {
                string a = GraphLogicalTileMapText.Normalize(current.LayerGrid[x, y]);
                string b = GraphLogicalTileMapText.Normalize(other.LayerGrid[x, y]);
                if (string.Equals(a, b, StringComparison.Ordinal))
                    continue;
                mismatchCount++;
                if (examples.Count < 20)
                    examples.Add($"  - [{x}, {y}] {current.Source}='{a}' {other.Source}='{b}'");
            }

            return examples;
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
