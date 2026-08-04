using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal static class ChunkFirstHeightAudit
    {
        private static readonly Dictionary<string, Record> Records =
            new Dictionary<string, Record>(System.StringComparer.Ordinal);
        private static readonly HashSet<string> LogicalRecorded =
            new HashSet<string>(System.StringComparer.Ordinal);
        private static readonly HashSet<string> PlacementRecorded =
            new HashSet<string>(System.StringComparer.Ordinal);
        private static readonly HashSet<string> Logged =
            new HashSet<string>(System.StringComparer.Ordinal);

        public static void Reset()
        {
            Records.Clear();
            LogicalRecorded.Clear();
            PlacementRecorded.Clear();
            Logged.Clear();
        }

        public static void RecordLogicalLayer(
            string graphLayerId,
            string layerName,
            float graphDefaultHeight,
            float blueprintHeight,
            float projectedSurfaceHeight,
            float sampleHeight,
            float sampleSurfaceHeight)
        {
            if (string.IsNullOrWhiteSpace(graphLayerId)
                || !LogicalRecorded.Add(graphLayerId))
            {
                return;
            }

            Record record = GetOrCreate(graphLayerId);
            record.Layer = layerName ?? graphLayerId;
            record.GraphDefaultHeight = graphDefaultHeight;
            record.BlueprintHeight = blueprintHeight;
            record.ProjectedSurfaceHeight = projectedSurfaceHeight;
            record.SampleHeight = sampleHeight;
            record.SampleSurfaceHeight = sampleSurfaceHeight;
            Records[graphLayerId] = record;
        }

        public static void RecordPlacement(
            string graphLayerId,
            string layerName,
            Vector2Int cell,
            string resolvedWinnerLayer,
            float resolvedWinnerSurface,
            float supportHeight,
            float prefabTopOffset,
            float prefabBottomOffset,
            float rootMatrixY,
            float actualTopWorldY,
            float actualBottomWorldY)
        {
            if (string.IsNullOrWhiteSpace(graphLayerId)
                || !PlacementRecorded.Add(graphLayerId))
            {
                return;
            }

            Record record = GetOrCreate(graphLayerId);
            record.Layer = layerName ?? graphLayerId;
            record.Cell = cell;
            record.ResolvedWinnerLayer = resolvedWinnerLayer;
            record.ResolvedWinnerSurface = resolvedWinnerSurface;
            record.SupportHeight = supportHeight;
            record.PrefabTopOffset = prefabTopOffset;
            record.PrefabBottomOffset = prefabBottomOffset;
            record.RootMatrixY = rootMatrixY;
            record.ActualTopWorldY = actualTopWorldY;
            record.ActualBottomWorldY = actualBottomWorldY;
            Records[graphLayerId] = record;
        }

        public static void RecordChunkBounds(
            string graphLayerId,
            Bounds bounds)
        {
            if (string.IsNullOrWhiteSpace(graphLayerId)
                || !Records.TryGetValue(graphLayerId, out Record record))
            {
                return;
            }

            record.FinalChunkBoundsMinY = bounds.min.y;
            record.FinalChunkBoundsMaxY = bounds.max.y;
            Records[graphLayerId] = record;

            if (Logged.Add(graphLayerId))
                Debug.Log(Format(record));
        }

        public static IReadOnlyList<string> SnapshotLines()
        {
            var lines = new List<string>(Records.Count);
            foreach (Record record in Records.Values)
                lines.Add(Format(record));
            lines.Sort(System.StringComparer.Ordinal);
            return lines;
        }

        public static bool TryGetRecord(
            string layerName,
            out string formatted)
        {
            foreach (Record record in Records.Values)
            {
                if (!string.Equals(record.Layer, layerName, System.StringComparison.Ordinal))
                    continue;

                formatted = Format(record);
                return true;
            }

            formatted = null;
            return false;
        }

        private static Record GetOrCreate(string graphLayerId)
        {
            return Records.TryGetValue(graphLayerId, out Record record)
                ? record
                : default;
        }

        private static string Format(Record record)
        {
            var builder = new StringBuilder(384);
            builder.AppendLine("[MoyvaHeightAudit]");
            builder.Append("layer=").AppendLine(record.Layer ?? string.Empty);
            builder.Append("cell=").Append(record.Cell.x).Append(',').Append(record.Cell.y).AppendLine();
            builder.Append("graphDefaultHeight=").AppendLine(FormatFloat(record.GraphDefaultHeight));
            builder.Append("blueprintHeight=").AppendLine(FormatFloat(record.BlueprintHeight));
            builder.Append("projectedSurfaceHeight=").AppendLine(FormatFloat(record.ProjectedSurfaceHeight));
            builder.Append("sampleHeight=").AppendLine(FormatFloat(record.SampleHeight));
            builder.Append("sampleSurfaceHeight=").AppendLine(FormatFloat(record.SampleSurfaceHeight));
            builder.Append("resolvedWinnerLayer=").AppendLine(record.ResolvedWinnerLayer ?? string.Empty);
            builder.Append("resolvedWinnerSurface=").AppendLine(FormatFloat(record.ResolvedWinnerSurface));
            builder.Append("supportHeight=").AppendLine(FormatFloat(record.SupportHeight));
            builder.Append("prefabTopOffset=").AppendLine(FormatFloat(record.PrefabTopOffset));
            builder.Append("prefabBottomOffset=").AppendLine(FormatFloat(record.PrefabBottomOffset));
            builder.Append("rootMatrixY=").AppendLine(FormatFloat(record.RootMatrixY));
            builder.Append("actualTopWorldY=").AppendLine(FormatFloat(record.ActualTopWorldY));
            builder.Append("actualBottomWorldY=").AppendLine(FormatFloat(record.ActualBottomWorldY));
            builder.Append("finalChunkBoundsMinY=").AppendLine(FormatFloat(record.FinalChunkBoundsMinY));
            builder.Append("finalChunkBoundsMaxY=").Append(FormatFloat(record.FinalChunkBoundsMaxY));
            return builder.ToString();
        }

        private static string FormatFloat(float value)
            => float.IsNaN(value) || float.IsInfinity(value)
                ? value.ToString(CultureInfo.InvariantCulture)
                : value.ToString("0.###", CultureInfo.InvariantCulture);

        private struct Record
        {
            public string Layer;
            public Vector2Int Cell;
            public float GraphDefaultHeight;
            public float BlueprintHeight;
            public float ProjectedSurfaceHeight;
            public float SampleHeight;
            public float SampleSurfaceHeight;
            public string ResolvedWinnerLayer;
            public float ResolvedWinnerSurface;
            public float SupportHeight;
            public float PrefabTopOffset;
            public float PrefabBottomOffset;
            public float RootMatrixY;
            public float ActualTopWorldY;
            public float ActualBottomWorldY;
            public float FinalChunkBoundsMinY;
            public float FinalChunkBoundsMaxY;
        }
    }
}
