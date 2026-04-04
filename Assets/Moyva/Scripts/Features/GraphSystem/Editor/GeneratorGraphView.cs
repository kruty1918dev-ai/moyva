using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    public sealed class GeneratorGraphView : GraphView
    {
        private GraphAsset _graphAsset;
        private readonly GraphEditorWindow _window;
        private NodeSearchProvider _searchProvider;
        private bool _isReadOnly;

        public EditorWindow EditorWindow => _window;

        public GeneratorGraphView(GraphEditorWindow window)
        {
            _window = window;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            _searchProvider = ScriptableObject.CreateInstance<NodeSearchProvider>();
            _searchProvider.Initialize(this);

            nodeCreationRequest = ctx =>
            {
                SearchWindow.Open(
                    new SearchWindowContext(ctx.screenMousePosition),
                    _searchProvider);
            };

            graphViewChanged += OnGraphViewChanged;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.Where(p =>
                p.direction != startPort.direction &&
                p.node != startPort.node &&
                AreTypesCompatible(startPort as GeneratorPort, p as GeneratorPort))
                .ToList();
        }

        private bool AreTypesCompatible(GeneratorPort a, GeneratorPort b)
        {
            if (a == null || b == null) return false;
            return a.PortValueType.IsAssignableFrom(b.PortValueType)
                || b.PortValueType.IsAssignableFrom(a.PortValueType);
        }

        public void SetReadOnly(bool readOnly)
        {
            _isReadOnly = readOnly;

            // Disable node creation in read-only mode
            nodeCreationRequest = readOnly ? null : (Action<NodeCreationContext>)(ctx =>
            {
                SearchWindow.Open(
                    new SearchWindowContext(ctx.screenMousePosition),
                    _searchProvider);
            });
        }

        public void PopulateGraph(GraphAsset asset, bool readOnly = false)
        {
            _graphAsset = asset;
            _isReadOnly = readOnly;

            // Clear
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (asset == null) return;

            // Create node views
            var nodeViews = new Dictionary<string, GeneratorNodeView>();
            foreach (var node in asset.Nodes)
            {
                if (node == null) continue;
                var view = new GeneratorNodeView(node);
                AddElement(view);
                nodeViews[node.NodeId] = view;
            }

            // Create edges
            foreach (var conn in asset.Connections)
            {
                if (!nodeViews.TryGetValue(conn.SourceNodeId, out var sourceView))
                    continue;
                if (!nodeViews.TryGetValue(conn.TargetNodeId, out var targetView))
                    continue;

                var outputPort = sourceView.GetOutputPort(conn.SourcePortIndex);
                var inputPort = targetView.GetInputPort(conn.TargetPortIndex);
                if (outputPort == null || inputPort == null) continue;

                var edge = outputPort.ConnectTo(inputPort);
                AddElement(edge);
            }
        }

        public void CreateNode(Type nodeType, Vector2 position)
        {
            if (_graphAsset == null || _isReadOnly) return;

            Undo.RecordObject(_graphAsset, "Add Node");
            var node = _graphAsset.AddNode(nodeType);
            if (node == null) return;

            node.EditorPosition = position;

            var view = new GeneratorNodeView(node);
            AddElement(view);

            EditorUtility.SetDirty(_graphAsset);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (_graphAsset == null) return change;

            if (_isReadOnly)
            {
                change.edgesToCreate?.Clear();
                change.elementsToRemove?.Clear();
                change.movedElements?.Clear();
                return change;
            }

            // Handle removed elements
            if (change.elementsToRemove != null)
            {
                Undo.RecordObject(_graphAsset, "Remove Graph Elements");
                foreach (var element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        RemoveEdgeFromAsset(edge);
                    }
                    else if (element is GeneratorNodeView nodeView)
                    {
                        _graphAsset.RemoveNode(nodeView.NodeData);
                    }
                }
                EditorUtility.SetDirty(_graphAsset);
            }

            // Handle new edges
            if (change.edgesToCreate != null)
            {
                Undo.RecordObject(_graphAsset, "Connect Nodes");
                foreach (var edge in change.edgesToCreate)
                {
                    var sourceView = edge.output.node as GeneratorNodeView;
                    var targetView = edge.input.node as GeneratorNodeView;
                    if (sourceView == null || targetView == null) continue;

                    var sourcePort = edge.output as GeneratorPort;
                    var targetPort = edge.input as GeneratorPort;
                    if (sourcePort == null || targetPort == null) continue;

                    _graphAsset.AddConnection(
                        sourceView.NodeData.NodeId, sourcePort.PortIndex,
                        targetView.NodeData.NodeId, targetPort.PortIndex);
                }
                EditorUtility.SetDirty(_graphAsset);
            }

            // Handle moved elements
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is GeneratorNodeView nodeView)
                    {
                        var rect = nodeView.GetPosition();
                        nodeView.NodeData.EditorPosition =
                            new Vector2(rect.x, rect.y);
                    }
                }
                EditorUtility.SetDirty(_graphAsset);
            }

            return change;
        }

        private void RemoveEdgeFromAsset(Edge edge)
        {
            var sourceView = edge.output.node as GeneratorNodeView;
            var targetView = edge.input.node as GeneratorNodeView;
            if (sourceView == null || targetView == null) return;

            var sourcePort = edge.output as GeneratorPort;
            var targetPort = edge.input as GeneratorPort;
            if (sourcePort == null || targetPort == null) return;

            var connections = _graphAsset.Connections;
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                var conn = connections[i];
                if (conn.SourceNodeId == sourceView.NodeData.NodeId
                    && conn.SourcePortIndex == sourcePort.PortIndex
                    && conn.TargetNodeId == targetView.NodeData.NodeId
                    && conn.TargetPortIndex == targetPort.PortIndex)
                {
                    _graphAsset.RemoveConnection(conn);
                    break;
                }
            }
        }
    }
}
