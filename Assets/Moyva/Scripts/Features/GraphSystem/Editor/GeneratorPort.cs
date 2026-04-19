using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    public sealed class GeneratorPort : Port
    {
        public int PortIndex { get; private set; }
        public Type PortValueType { get; private set; }

        private GeneratorPort(Orientation orientation, Direction direction,
            Capacity capacity, Type type)
            : base(orientation, direction, capacity, type)
        {
        }

        public static GeneratorPort Create(PortDefinition def, int index,
            Direction direction, Capacity capacity)
        {
            var listener = new DefaultEdgeConnectorListener();
            var port = new GeneratorPort(Orientation.Horizontal, direction,
                capacity, def.ValueType)
            {
                PortIndex = index,
                PortValueType = def.ValueType,
                portName = def.Name,
                portColor = GetColorForType(def.ValueType),
                tooltip = $"{def.Name} ({GetTypeName(def.ValueType)})"
            };
            port.m_EdgeConnector = new EdgeConnector<Edge>(listener);
            VisualElementExtensions.AddManipulator(port, port.m_EdgeConnector);
            return port;
        }

        private class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            private GraphViewChange _graphViewChange;

            public void OnDropOutsidePort(Edge edge, Vector2 position) { }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                var edgesToCreate = new System.Collections.Generic.List<Edge> { edge };
                var edgesToDelete = new System.Collections.Generic.List<GraphElement>();

                if (edge.input.capacity == Capacity.Single)
                {
                    foreach (var conn in edge.input.connections)
                    {
                        if (conn != edge)
                            edgesToDelete.Add(conn);
                    }
                }
                if (edge.output.capacity == Capacity.Single)
                {
                    foreach (var conn in edge.output.connections)
                    {
                        if (conn != edge)
                            edgesToDelete.Add(conn);
                    }
                }

                if (edgesToDelete.Count > 0)
                    graphView.DeleteElements(edgesToDelete);

                _graphViewChange.edgesToCreate = edgesToCreate;
                graphView.graphViewChanged?.Invoke(_graphViewChange);

                graphView.AddElement(edge);
            }
        }

        private static Color GetColorForType(Type type)
        {
            if (type == typeof(float[,]))
                return new Color(0.4f, 0.8f, 0.4f);   // Green — height maps
            if (type == typeof(string[,]))
                return new Color(0.4f, 0.6f, 1.0f);   // Blue — tile maps
            if (type == typeof(bool[,]))
                return new Color(1.0f, 0.8f, 0.3f);   // Yellow — masks
            if (type == typeof(int[,]))
                return new Color(0.9f, 0.5f, 0.2f);   // Orange — int maps
            if (type == typeof(int))
                return new Color(0.7f, 0.7f, 0.7f);   // Gray — scalars
            if (type == typeof(float))
                return new Color(0.5f, 0.9f, 0.5f);   // Light green
            if (type == typeof(Vector2Int))
                return new Color(0.8f, 0.4f, 0.8f);   // Purple
            if (type == typeof(object))
                return new Color(0.95f, 0.95f, 0.95f); // Any/wildcard

            return new Color(0.8f, 0.8f, 0.8f);        // Default
        }

        private static string GetTypeName(Type type)
        {
            if (type == typeof(float[,])) return "HeightMap";
            if (type == typeof(string[,])) return "TileMap";
            if (type == typeof(bool[,])) return "Mask";
            if (type == typeof(int[,])) return "IntMap";
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(Vector2Int)) return "Vector2Int";
            if (type == typeof(object)) return "Any";
            return type.Name;
        }
    }
}
