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

        private VisualElement _elementIndexContainer;
        private IntegerField _elementIndexField;
        private Label _elementCountLabel;
        private Action<int> _elementIndexChanged;
        private bool _updatingElementIndex;

        private GeneratorPort(Orientation orientation, Direction direction,
            Capacity capacity, Type type)
            : base(orientation, direction, capacity, type)
        {
        }

        public static GeneratorPort Create(PortDefinition def, int index, Direction direction, Capacity capacity)
        {
            var listener = new DefaultEdgeConnectorListener();
            var port = new GeneratorPort(Orientation.Horizontal, direction, capacity, def.ValueType)
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

        public void SetElementIndexControl(int index, int elementCount, Action<int> onChanged)
        {
            EnsureElementIndexControl();

            _elementIndexChanged = onChanged;
            _elementIndexContainer.style.display = DisplayStyle.Flex;

            int clampedIndex = Mathf.Max(0, index);
            if (elementCount > 0)
                clampedIndex = Mathf.Min(clampedIndex, elementCount - 1);

            _updatingElementIndex = true;
            _elementIndexField.SetValueWithoutNotify(clampedIndex);
            _updatingElementIndex = false;

            _elementCountLabel.text = elementCount > 0 ? $"/{elementCount}" : "/?";
            _elementIndexContainer.tooltip = elementCount > 0
                ? $"Index {clampedIndex} з {elementCount} доступних елементів списку."
                : $"Index {clampedIndex}. Кількість елементів буде відома після виконання графа.";
        }

        public void ClearElementIndexControl()
        {
            if (_elementIndexContainer == null)
                return;

            _elementIndexChanged = null;
            _elementIndexContainer.style.display = DisplayStyle.None;
        }

        private void EnsureElementIndexControl()
        {
            if (_elementIndexContainer != null)
                return;

            _elementIndexContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginLeft = 6,
                    display = DisplayStyle.None
                }
            };

            var label = new Label("idx")
            {
                style =
                {
                    fontSize = 9,
                    color = new Color(0.72f, 0.72f, 0.72f),
                    marginRight = 2
                }
            };

            _elementIndexField = new IntegerField
            {
                isDelayed = true,
                style =
                {
                    width = 42,
                    height = 18,
                    fontSize = 10,
                    marginLeft = 0,
                    marginRight = 0
                }
            };
            _elementIndexField.RegisterValueChangedCallback(evt =>
            {
                if (_updatingElementIndex)
                    return;

                _elementIndexChanged?.Invoke(Mathf.Max(0, evt.newValue));
            });

            _elementCountLabel = new Label("/?")
            {
                style =
                {
                    fontSize = 9,
                    color = new Color(0.72f, 0.72f, 0.72f),
                    marginLeft = 2
                }
            };

            _elementIndexContainer.Add(label);
            _elementIndexContainer.Add(_elementIndexField);
            _elementIndexContainer.Add(_elementCountLabel);
            Add(_elementIndexContainer);
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
            if (type == typeof(bool))
                return new Color(1.0f, 0.85f, 0.2f);  // Amber — booleans
            if (type == typeof(string))
                return new Color(0.95f, 0.55f, 0.9f); // Pink — strings
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
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(Vector2Int)) return "Vector2Int";
            if (type == typeof(object)) return "Any";
            return type.Name;
        }
    }
}
