using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    /// <summary>
    /// Валідатор структури графа генерації.
    /// Перевіряє: null-вузли, наявність циклів, коректність з'єднань (порти, типи),
    /// та наявність непідключених обов'язкових входів.
    /// </summary>
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
            var sorted = TopologicalSorter.Sort(graph);
            if (sorted == null)
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

                bool isWildcard = sourcePort.ValueType == typeof(object) || targetPort.ValueType == typeof(object);
                if (!isWildcard && !targetPort.ValueType.IsAssignableFrom(sourcePort.ValueType))
                {
                    errors.Add(new ValidationError(target.NodeId,
                        $"Type mismatch: '{source.Title}'.{sourcePort.Name} ({sourcePort.ValueType.Name}) → '{target.Title}'.{targetPort.Name} ({targetPort.ValueType.Name})."));
                }
            }
        }

        private void ValidateRequiredInputs(GraphAsset graph, List<ValidationError> errors)
        {
            var connectedInputs = new HashSet<string>();
            var connectedNodeIds = new HashSet<string>();
            foreach (var conn in graph.Connections)
            {
                connectedInputs.Add($"{conn.TargetNodeId}:{conn.TargetPortIndex}");
                connectedNodeIds.Add(conn.SourceNodeId);
                connectedNodeIds.Add(conn.TargetNodeId);
            }

            foreach (var node in graph.Nodes)
            {
                if (node == null) continue;

                // Isolated node (no incoming and no outgoing connections) is treated as unused,
                // not as an invalid graph state.
                if (!connectedNodeIds.Contains(node.NodeId))
                    continue;

                var inputs = node.Inputs;
                for (int i = 0; i < inputs.Length; i++)
                {
                    if (IsOptionalInput(inputs[i].Name))
                        continue;

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

        private static bool IsOptionalInput(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return false;

            return portName.IndexOf("optional", System.StringComparison.OrdinalIgnoreCase) >= 0
                || portName.IndexOf("опцій", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
