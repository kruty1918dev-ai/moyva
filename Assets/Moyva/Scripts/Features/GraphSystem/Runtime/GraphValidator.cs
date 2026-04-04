using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    public sealed class GraphValidator
    {
        public List<ValidationError> Validate(GraphAsset graph)
        {
            var errors = new List<ValidationError>();

            ValidateNullNodes(graph, errors);
            ValidateCycles(graph, errors);
            ValidateConnections(graph, errors);
            ValidateRequiredInputs(graph, errors);

            return errors;
        }

        private void ValidateNullNodes(GraphAsset graph, List<ValidationError> errors)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (graph.Nodes[i] == null)
                    errors.Add(new ValidationError(null,
                        $"Node at index {i} is null (missing script?).",
                        ValidationSeverity.Error));
            }
        }

        private void ValidateCycles(GraphAsset graph, List<ValidationError> errors)
        {
            var nodeIds = new HashSet<string>();
            var inDegree = new Dictionary<string, int>();
            var adjacency = new Dictionary<string, List<string>>();

            foreach (var node in graph.Nodes)
            {
                if (node == null) continue;
                nodeIds.Add(node.NodeId);
                inDegree[node.NodeId] = 0;
                adjacency[node.NodeId] = new List<string>();
            }

            foreach (var conn in graph.Connections)
            {
                if (!nodeIds.Contains(conn.SourceNodeId)
                    || !nodeIds.Contains(conn.TargetNodeId))
                    continue;

                adjacency[conn.SourceNodeId].Add(conn.TargetNodeId);
                inDegree[conn.TargetNodeId]++;
            }

            var queue = new Queue<string>();
            foreach (var kvp in inDegree)
                if (kvp.Value == 0) queue.Enqueue(kvp.Key);

            int visited = 0;
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                visited++;
                foreach (var neighbor in adjacency[current])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0) queue.Enqueue(neighbor);
                }
            }

            if (visited != nodeIds.Count)
                errors.Add(new ValidationError(null,
                    "Graph contains one or more cycles.",
                    ValidationSeverity.Error));
        }

        private void ValidateConnections(GraphAsset graph, List<ValidationError> errors)
        {
            foreach (var conn in graph.Connections)
            {
                var source = graph.GetNodeById(conn.SourceNodeId);
                var target = graph.GetNodeById(conn.TargetNodeId);

                if (source == null)
                {
                    errors.Add(new ValidationError(conn.SourceNodeId,
                        $"Connection references missing source node '{conn.SourceNodeId}'."));
                    continue;
                }
                if (target == null)
                {
                    errors.Add(new ValidationError(conn.TargetNodeId,
                        $"Connection references missing target node '{conn.TargetNodeId}'."));
                    continue;
                }

                var sourceOutputs = source.Outputs;
                var targetInputs = target.Inputs;

                if (conn.SourcePortIndex >= sourceOutputs.Length)
                {
                    errors.Add(new ValidationError(source.NodeId,
                        $"Output port index {conn.SourcePortIndex} out of range on '{source.Title}'."));
                    continue;
                }
                if (conn.TargetPortIndex >= targetInputs.Length)
                {
                    errors.Add(new ValidationError(target.NodeId,
                        $"Input port index {conn.TargetPortIndex} out of range on '{target.Title}'."));
                    continue;
                }

                var sourcePort = sourceOutputs[conn.SourcePortIndex];
                var targetPort = targetInputs[conn.TargetPortIndex];

                if (!targetPort.ValueType.IsAssignableFrom(sourcePort.ValueType))
                {
                    errors.Add(new ValidationError(target.NodeId,
                        $"Type mismatch: '{source.Title}'.{sourcePort.Name} ({sourcePort.ValueType.Name}) → '{target.Title}'.{targetPort.Name} ({targetPort.ValueType.Name})."));
                }
            }
        }

        private void ValidateRequiredInputs(GraphAsset graph, List<ValidationError> errors)
        {
            var connectedInputs = new HashSet<string>();
            foreach (var conn in graph.Connections)
                connectedInputs.Add($"{conn.TargetNodeId}:{conn.TargetPortIndex}");

            foreach (var node in graph.Nodes)
            {
                if (node == null) continue;
                var inputs = node.Inputs;
                for (int i = 0; i < inputs.Length; i++)
                {
                    string key = $"{node.NodeId}:{i}";
                    if (!connectedInputs.Contains(key))
                    {
                        errors.Add(new ValidationError(node.NodeId,
                            $"Input port '{inputs[i].Name}' on '{node.Title}' is not connected.",
                            ValidationSeverity.Warning));
                    }
                }
            }
        }
    }
}
