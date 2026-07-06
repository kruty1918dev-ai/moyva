using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerMaskUtility
    {
        bool[,] ExtractOutputMask(object[] outputs, LayerOutputKind outputKind, int width, int height);
        bool[,] ExtractConnectedMask(GraphAsset graph, OutputNode outputNode, GraphExecutionResult result, int width, int height);
        bool[,] ExtractTileSettingsMask(GraphAsset graph, string layerId, GraphExecutionResult result, int width, int height);
        bool[,] Normalize(bool[,] source, int width, int height);
        IEnumerable<Vector2> EnumeratePositions(bool[,] mask);
    }

    internal sealed class GraphCompilerMaskUtility : IGraphCompilerMaskUtility
    {
        public bool[,] ExtractOutputMask(object[] outputs, LayerOutputKind outputKind, int width, int height)
        {
            if (outputs == null)
                return null;
            if (outputs.Length > OutputNode.MaskInputIndex && outputs[OutputNode.MaskInputIndex] is bool[,] directMask)
                return Normalize(directMask, width, height);
            if (outputKind == LayerOutputKind.Masks)
                return null;
            if (outputs.Length > OutputNode.BiomeMapInputIndex && outputs[OutputNode.BiomeMapInputIndex] is string[,] biomeMap)
                return BuildMaskFromStringMap(biomeMap, width, height);
            if (outputs.Length > OutputNode.ObjectMapInputIndex && outputs[OutputNode.ObjectMapInputIndex] is string[,] objectMap)
                return BuildMaskFromStringMap(objectMap, width, height);
            if (outputs.Length > OutputNode.BuildingMapInputIndex && outputs[OutputNode.BuildingMapInputIndex] is string[,] buildingMap)
                return BuildMaskFromStringMap(buildingMap, width, height);
            return null;
        }

        public bool[,] ExtractConnectedMask(GraphAsset graph, OutputNode outputNode, GraphExecutionResult result, int width, int height)
        {
            var mask = ExtractConnectedMaskForTargetPort(graph, outputNode?.NodeId, OutputNode.MaskInputIndex, result, width, height);
            if (mask != null || outputNode == null || outputNode.OutputKind != LayerOutputKind.Masks || graph?.Connections == null)
                return mask;

            foreach (var connection in graph.Connections)
            {
                if (connection == null || connection.TargetNodeId != outputNode.NodeId)
                    continue;
                var outputs = result.GetOutputs(connection.SourceNodeId);
                if (outputs != null && connection.SourcePortIndex >= 0 && connection.SourcePortIndex < outputs.Length
                    && outputs[connection.SourcePortIndex] is bool[,] sourceMask)
                    return Normalize(sourceMask, width, height);
            }

            return null;
        }

        public bool[,] ExtractTileSettingsMask(GraphAsset graph, string layerId, GraphExecutionResult result, int width, int height)
        {
            bool[,] merged = null;
            foreach (var node in TileSettingsNode.GetNodesForLayer(graph, layerId))
            {
                if (node == null || !node.HasRenderableTileOutput)
                    continue;
                var outputs = result.GetOutputs(node.NodeId);
                if (outputs != null && outputs.Length > 0 && outputs[0] is bool[,] nodeMask)
                    merged = Merge(merged, Normalize(nodeMask, width, height));
            }

            return merged;
        }

        public bool[,] Normalize(bool[,] source, int width, int height)
        {
            if (source == null)
                return null;
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            var result = new bool[safeWidth, safeHeight];
            for (int x = 0; x < Mathf.Min(safeWidth, source.GetLength(0)); x++)
            for (int y = 0; y < Mathf.Min(safeHeight, source.GetLength(1)); y++)
                result[x, y] = source[x, y];
            return result;
        }

        public IEnumerable<Vector2> EnumeratePositions(bool[,] mask)
        {
            if (mask == null)
                yield break;
            for (int x = 0; x < mask.GetLength(0); x++)
            for (int y = 0; y < mask.GetLength(1); y++)
                if (mask[x, y])
                    yield return new Vector2(x, y);
        }

        private bool[,] ExtractConnectedMaskForTargetPort(GraphAsset graph, string targetNodeId,
            int targetPortIndex, GraphExecutionResult result, int width, int height)
        {
            if (graph?.Connections == null || string.IsNullOrEmpty(targetNodeId) || result == null)
                return null;
            foreach (var connection in graph.Connections)
            {
                if (connection == null || connection.TargetNodeId != targetNodeId || connection.TargetPortIndex != targetPortIndex)
                    continue;
                var outputs = result.GetOutputs(connection.SourceNodeId);
                if (outputs != null && connection.SourcePortIndex >= 0 && connection.SourcePortIndex < outputs.Length
                    && outputs[connection.SourcePortIndex] is bool[,] sourceMask)
                    return Normalize(sourceMask, width, height);
            }

            return null;
        }

        private static bool[,] Merge(bool[,] target, bool[,] source)
        {
            if (source == null)
                return target;
            if (target == null)
                return source;
            for (int x = 0; x < Mathf.Min(target.GetLength(0), source.GetLength(0)); x++)
            for (int y = 0; y < Mathf.Min(target.GetLength(1), source.GetLength(1)); y++)
                target[x, y] |= source[x, y];
            return target;
        }

        private bool[,] BuildMaskFromStringMap(string[,] source, int width, int height)
        {
            if (source == null)
                return null;
            var result = new bool[Mathf.Max(1, width), Mathf.Max(1, height)];
            for (int x = 0; x < Mathf.Min(result.GetLength(0), source.GetLength(0)); x++)
            for (int y = 0; y < Mathf.Min(result.GetLength(1), source.GetLength(1)); y++)
                result[x, y] = !string.IsNullOrEmpty(source[x, y]);
            return result;
        }
    }
}
