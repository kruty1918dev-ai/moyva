using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkFirstCellResolutionDebug
    {
        public string BuildReport(Vector2Int cell, TileNeighborhood neighborhood, ResolvedTileComposition resolved)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Cell: {cell.x},{cell.y}");
            builder.AppendLine("Stack:");
            AppendStack(builder, neighborhood.Center);
            builder.AppendLine("Neighbors:");
            AppendNeighbor(builder, "N", neighborhood.North);
            AppendNeighbor(builder, "E", neighborhood.East);
            AppendNeighbor(builder, "S", neighborhood.South);
            AppendNeighbor(builder, "W", neighborhood.West);
            AppendNeighbor(builder, "NE", neighborhood.NorthEast);
            AppendNeighbor(builder, "SE", neighborhood.SouthEast);
            AppendNeighbor(builder, "SW", neighborhood.SouthWest);
            AppendNeighbor(builder, "NW", neighborhood.NorthWest);
            builder.AppendLine("Resolved:");
            builder.AppendLine($"  MainTerrain = {(resolved.HasMainTerrain ? resolved.MainTerrain.GraphLayerName ?? resolved.MainTerrain.GraphLayerId : "<none>")}");
            builder.AppendLine($"  Overlay = {(resolved.HasOverlay ? resolved.Overlay.GraphLayerName ?? resolved.Overlay.GraphLayerId : "<none>")}");
            builder.AppendLine("Reason:");
            builder.AppendLine($"  {resolved.Reason}");
            return builder.ToString();
        }

        private static void AppendStack(StringBuilder builder, TileStackCell cell)
        {
            if (cell == null || cell.IsEmpty)
            {
                builder.AppendLine("  <empty>");
                return;
            }

            for (int i = 0; i < cell.Samples.Count; i++)
            {
                var sample = cell.Samples[i];
                builder.AppendLine($"  {i} {sample.LayerKind} {sample.GraphLayerName ?? sample.GraphLayerId} priority {sample.TerrainPriority} source {sample.SourceNodeId ?? sample.GraphLayerId}");
            }
        }

        private static void AppendNeighbor(StringBuilder builder, string label, TileStackCell cell)
        {
            if (cell == null || !cell.TryGetTopCompatibilitySample(out var sample))
            {
                builder.AppendLine($"  {label} <empty>");
                return;
            }

            builder.AppendLine($"  {label} {sample.GraphLayerName ?? sample.GraphLayerId}");
        }
    }
}
