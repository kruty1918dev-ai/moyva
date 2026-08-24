using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    public sealed partial class GraphRunner
    {
        private static bool TryGatherInputs(
            NodeBase node,
            Dictionary<string, object[]> cache,
            IReadOnlyDictionary<string, IReadOnlyList<Connection>> connectionsByTarget,
            NodeContext context,
            out object[] inputs,
            out string error)
        {
            var portDefs = node?.Inputs ?? Array.Empty<PortDefinition>();
            inputs = new object[portDefs.Length];
            error = null;
            var assigned = new bool[portDefs.Length];

            if (node != null
                && connectionsByTarget != null
                && connectionsByTarget.TryGetValue(node.NodeId, out var incoming))
            {
                for (int c = 0; c < incoming.Count; c++)
                {
                    var connection = incoming[c];
                    if (connection == null)
                        continue;
                    if (connection.TargetPortIndex < 0
                        || connection.TargetPortIndex >= inputs.Length)
                    {
                        error = $"Connection '{connection.ConnectionId}' targets " +
                            $"missing input index {connection.TargetPortIndex}.";
                        return false;
                    }
                    if (assigned[connection.TargetPortIndex])
                    {
                        error = $"Input '{portDefs[connection.TargetPortIndex].Name}' " +
                            "has more than one connection.";
                        return false;
                    }
                    if (!cache.TryGetValue(
                            connection.SourceNodeId,
                            out var sourceOutputs))
                    {
                        error = $"Input '{portDefs[connection.TargetPortIndex].Name}' " +
                            $"cannot read source node '{connection.SourceNodeId}'.";
                        return false;
                    }
                    if (connection.SourcePortIndex < 0
                        || connection.SourcePortIndex >= sourceOutputs.Length)
                    {
                        error = $"Connection '{connection.ConnectionId}' reads " +
                            $"missing output index {connection.SourcePortIndex}.";
                        return false;
                    }

                    var targetPort = portDefs[connection.TargetPortIndex];
                    if (!TrySelectConnectionValue(
                            sourceOutputs[connection.SourcePortIndex],
                            targetPort,
                            connection.SourceElementIndex,
                            out var value,
                            out error))
                    {
                        error = $"Input '{targetPort.Name}': {error}";
                        return false;
                    }
                    if (!TryValidatePortValue(
                            targetPort,
                            value,
                            context,
                            out error))
                    {
                        error = $"Input '{targetPort.Name}': {error}";
                        return false;
                    }

                    inputs[connection.TargetPortIndex] = value;
                    assigned[connection.TargetPortIndex] = true;
                }
            }

            for (int i = 0; i < portDefs.Length; i++)
            {
                if (portDefs[i] == null)
                {
                    error = $"Input definition at index {i} is null.";
                    return false;
                }
                if (portDefs[i].IsRequired && !assigned[i])
                {
                    error = $"Required input '{portDefs[i].Name}' " +
                        $"({portDefs[i].Id}) is not connected.";
                    return false;
                }
            }

            return true;
        }

        private static bool TrySelectConnectionValue(
            object sourceValue,
            PortDefinition targetPort,
            int sourceElementIndex,
            out object value,
            out string error)
        {
            value = sourceValue;
            error = null;
            if (targetPort == null)
            {
                error = "Target port definition is null.";
                return false;
            }
            if (sourceValue == null
                || targetPort.AcceptsAnyValue
                || targetPort.ValueType.IsInstanceOfType(sourceValue))
            {
                return true;
            }

            if (!PortDefinition.TryGetIndexableValue(
                    sourceValue,
                    sourceElementIndex,
                    out var element))
            {
                error = $"Value '{sourceValue.GetType().Name}' is not assignable " +
                    $"to '{targetPort.ValueType.Name}' and element index " +
                    $"{sourceElementIndex} cannot be resolved.";
                return false;
            }
            if (element != null
                && !targetPort.ValueType.IsInstanceOfType(element))
            {
                error = $"Element {sourceElementIndex} has type " +
                    $"'{element.GetType().Name}', expected " +
                    $"'{targetPort.ValueType.Name}'.";
                return false;
            }

            value = element;
            return true;
        }

        private static bool TryValidateOutput(
            NodeBase node,
            NodeOutput output,
            NodeContext context,
            out string error)
        {
            error = null;
            if (node == null)
            {
                error = "Node is null.";
                return false;
            }
            if (output == null)
            {
                error = "Node returned null instead of NodeOutput.";
                return false;
            }

            var definitions = node.Outputs ?? Array.Empty<PortDefinition>();
            var values = output.Values ?? Array.Empty<object>();
            if (values.Length != definitions.Length)
            {
                error = $"Output contract mismatch: node declares " +
                    $"{definitions.Length} port(s) but returned " +
                    $"{values.Length} value(s).";
                return false;
            }

            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                {
                    error = $"Output definition at index {i} is null.";
                    return false;
                }
                if (!TryValidatePortValue(
                        definition,
                        values[i],
                        context,
                        out var portError))
                {
                    error = $"Output '{definition.Name}' " +
                        $"({definition.Id}): {portError}";
                    return false;
                }
            }

            if (output.Artifact is ILayerMaskArtifact layerArtifact
                && layerArtifact.LayerMask != null)
            {
                var mask = layerArtifact.LayerMask;
                int expectedWidth = Math.Max(1, context?.MapSize.x ?? 0);
                int expectedHeight = Math.Max(1, context?.MapSize.y ?? 0);
                if (mask.GetLength(0) != expectedWidth
                    || mask.GetLength(1) != expectedHeight)
                {
                    error = $"Layer output artifact map size is " +
                        $"{mask.GetLength(0)}x{mask.GetLength(1)}, expected " +
                        $"{expectedWidth}x{expectedHeight}.";
                    return false;
                }
            }

            return true;
        }

        private static bool TryValidatePortValue(
            PortDefinition port,
            object value,
            NodeContext context,
            out string error)
        {
            error = null;
            if (port == null)
            {
                error = "Port definition is null.";
                return false;
            }
            if (value == null)
            {
                if (port.AllowNull
                    || !port.IsRequired
                    && port.Direction == PortDirection.Input)
                {
                    return true;
                }

                error = "Value is null.";
                return false;
            }
            if (!port.AcceptsAnyValue
                && !port.ValueType.IsInstanceOfType(value))
            {
                error = $"Runtime type is '{value.GetType().Name}', " +
                    $"expected '{port.ValueType.Name}'.";
                return false;
            }

            if (port.MapSizePolicy == PortMapSizePolicy.MatchContext
                && value is Array map
                && map.Rank == 2)
            {
                int expectedWidth = Math.Max(1, context?.MapSize.x ?? 0);
                int expectedHeight = Math.Max(1, context?.MapSize.y ?? 0);
                if (map.GetLength(0) != expectedWidth
                    || map.GetLength(1) != expectedHeight)
                {
                    error = $"Map size is {map.GetLength(0)}x{map.GetLength(1)}, " +
                        $"expected {expectedWidth}x{expectedHeight}.";
                    return false;
                }
            }

            return true;
        }
    }
}
