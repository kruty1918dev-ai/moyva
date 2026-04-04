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
        private MiniMap _miniMap;

        // Copy/Paste buffer
        private static List<CopiedNodeData> _copyBuffer = new();

        public GraphAsset GraphAsset => _graphAsset;
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

            // Minimap
            _miniMap = new MiniMap { anchored = true };
            _miniMap.SetPosition(new Rect(10, 30, 200, 140));
            Add(_miniMap);

            // Keyboard shortcuts
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        public void SetMinimapVisible(bool visible) => _miniMap.visible = visible;

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

        #region Keyboard Shortcuts

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (_isReadOnly) return;

            bool ctrl = evt.ctrlKey || evt.commandKey;

            if (ctrl && evt.keyCode == KeyCode.C)
            {
                CopySelection();
                evt.StopPropagation();
            }
            else if (ctrl && evt.keyCode == KeyCode.V)
            {
                PasteNodes();
                evt.StopPropagation();
            }
            else if (ctrl && evt.keyCode == KeyCode.D)
            {
                DuplicateSelection();
                evt.StopPropagation();
            }
            else if (ctrl && evt.keyCode == KeyCode.G)
            {
                GroupSelection();
                evt.StopPropagation();
            }
            else if (ctrl && evt.keyCode == KeyCode.T)
            {
                AddStickyNote();
                evt.StopPropagation();
            }
        }

        #endregion

        #region Copy / Paste

        private struct CopiedNodeData
        {
            public Type NodeType;
            public Vector2 Position;
            public string JsonData;
        }

        private void CopySelection()
        {
            _copyBuffer.Clear();
            foreach (var element in selection)
            {
                if (element is GeneratorNodeView nodeView)
                {
                    var rect = nodeView.GetPosition();
                    _copyBuffer.Add(new CopiedNodeData
                    {
                        NodeType = nodeView.NodeData.GetType(),
                        Position = new Vector2(rect.x, rect.y),
                        JsonData = EditorJsonUtility.ToJson(nodeView.NodeData)
                    });
                }
            }
        }

        private void PasteNodes()
        {
            if (_graphAsset == null || _copyBuffer.Count == 0) return;

            Undo.RecordObject(_graphAsset, "Paste Nodes");
            ClearSelection();

            Vector2 offset = new Vector2(40, 40);
            var newViews = new List<GeneratorNodeView>();

            foreach (var data in _copyBuffer)
            {
                var node = _graphAsset.AddNode(data.NodeType);
                if (node == null) continue;

                // Apply serialized data (preserving field values)
                EditorJsonUtility.FromJsonOverwrite(data.JsonData, node);
                // Reset ID so it's unique
                node.NodeId = Guid.NewGuid().ToString();
                node.EditorPosition = data.Position + offset;

                var view = new GeneratorNodeView(node);
                AddElement(view);
                AddToSelection(view);
                newViews.Add(view);
            }

            EditorUtility.SetDirty(_graphAsset);
        }

        private void DuplicateSelection()
        {
            CopySelection();
            PasteNodes();
        }

        #endregion

        #region Groups & Sticky Notes

        public void GroupSelection()
        {
            if (_isReadOnly) return;

            var selectedNodes = selection.OfType<GeneratorNodeView>().ToList();
            if (selectedNodes.Count == 0) return;

            var group = new Group { title = "Node Group" };

            // Calculate group bounds
            foreach (var node in selectedNodes)
                group.AddElement(node);

            AddElement(group);
        }

        public void AddStickyNote()
        {
            if (_isReadOnly) return;

            var note = new StickyNote
            {
                title = "Note",
                contents = "..."
            };
            // Place near current view center
            var center = contentViewContainer.WorldToLocal(
                new Vector2(layout.width / 2, layout.height / 2));
            note.SetPosition(new Rect(center.x, center.y, 200, 160));
            AddElement(note);
        }

        #endregion

        #region Export / Import

        public void ExportNodesToFile()
        {
            if (_graphAsset == null) return;

            var selectedNodes = selection.OfType<GeneratorNodeView>().ToList();
            var nodesToExport = selectedNodes.Count > 0
                ? selectedNodes.Select(v => v.NodeData)
                : _graphAsset.Nodes.Where(n => n != null);

            var nodeIdSet = new HashSet<string>(
                nodesToExport.Select(n => n.NodeId));

            var preset = new GraphPreset();
            foreach (var node in nodesToExport)
            {
                preset.nodes.Add(new NodePresetEntry
                {
                    nodeTypeAssemblyQualifiedName = node.GetType().AssemblyQualifiedName,
                    originalNodeId = node.NodeId,
                    position = node.EditorPosition,
                    jsonData = EditorJsonUtility.ToJson(node)
                });
            }

            foreach (var conn in _graphAsset.Connections)
            {
                if (nodeIdSet.Contains(conn.SourceNodeId) && nodeIdSet.Contains(conn.TargetNodeId))
                {
                    preset.connections.Add(new ConnectionEntry
                    {
                        sourceNodeId = conn.SourceNodeId,
                        sourcePortIndex = conn.SourcePortIndex,
                        targetNodeId = conn.TargetNodeId,
                        targetPortIndex = conn.TargetPortIndex
                    });
                }
            }

            var path = GraphPresetIO.ShowExportPanel(_graphAsset.name);
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                GraphPresetIO.WriteToFile(preset, path);
                Debug.Log($"[GraphPreset] Exported {preset.nodes.Count} node(s) and " +
                          $"{preset.connections.Count} connection(s) to {path}");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Export Failed", ex.Message, "OK");
            }
        }

        public void ImportNodesFromFile()
        {
            if (_graphAsset == null || _isReadOnly) return;

            var path = GraphPresetIO.ShowImportPanel();
            if (string.IsNullOrEmpty(path)) return;

            GraphPreset preset;
            try
            {
                preset = GraphPresetIO.ReadFromFile(path);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Import Failed", ex.Message, "OK");
                return;
            }

            var idMap = new Dictionary<string, string>();
            var newViews = new Dictionary<string, GeneratorNodeView>();

            // Place imported nodes to the right of the existing graph content
            float maxX = 100f;
            foreach (var element in graphElements)
            {
                if (element is GeneratorNodeView nv)
                {
                    float right = nv.GetPosition().xMax;
                    if (right > maxX) maxX = right;
                }
            }
            Vector2 importOffset = new Vector2(maxX + 100f, 0f);

            Undo.RecordObject(_graphAsset, "Import Graph Preset");

            int created = 0;
            int skipped = 0;

            foreach (var entry in preset.nodes)
            {
                var nodeType = Type.GetType(entry.nodeTypeAssemblyQualifiedName);
                if (nodeType == null)
                {
                    Debug.LogWarning($"[GraphPreset] Type not found, skipping: {entry.nodeTypeAssemblyQualifiedName}");
                    skipped++;
                    continue;
                }

                var node = _graphAsset.AddNode(nodeType);
                if (node == null) { skipped++; continue; }

                // Assign fresh identity before restoring data so FromJsonOverwrite cannot
                // clobber the new ID (NodeId setter writes to the backing field)
                string newId = Guid.NewGuid().ToString();
                idMap[entry.originalNodeId] = newId;
                node.NodeId = newId;
                node.EditorPosition = entry.position + importOffset;

                // Restore serialized field values (overwrites position / id, so re-apply below)
                EditorJsonUtility.FromJsonOverwrite(entry.jsonData, node);
                node.NodeId = newId;
                node.EditorPosition = entry.position + importOffset;

                var view = new GeneratorNodeView(node);
                AddElement(view);
                newViews[newId] = view;
                created++;
            }

            int imported = 0;
            foreach (var connEntry in preset.connections)
            {
                if (!idMap.TryGetValue(connEntry.sourceNodeId, out var newSource)) continue;
                if (!idMap.TryGetValue(connEntry.targetNodeId, out var newTarget)) continue;

                _graphAsset.AddConnection(
                    newSource, connEntry.sourcePortIndex,
                    newTarget, connEntry.targetPortIndex);

                if (!newViews.TryGetValue(newSource, out var sourceView)) continue;
                if (!newViews.TryGetValue(newTarget, out var targetView)) continue;

                var outputPort = sourceView.GetOutputPort(connEntry.sourcePortIndex);
                var inputPort = targetView.GetInputPort(connEntry.targetPortIndex);
                if (outputPort == null || inputPort == null)
                {
                    Debug.LogWarning($"[GraphPreset] Could not find port(s) for connection " +
                                     $"{newSource}:{connEntry.sourcePortIndex} → {newTarget}:{connEntry.targetPortIndex}. " +
                                     "Connection skipped.");
                    continue;
                }

                AddElement(outputPort.ConnectTo(inputPort));
                imported++;
            }

            EditorUtility.SetDirty(_graphAsset);

            Debug.Log($"[GraphPreset] Imported {created} node(s) and {imported} connection(s)" +
                      (skipped > 0 ? $"; skipped {skipped} unknown type(s)." : "."));

            if (skipped > 0)
                EditorUtility.DisplayDialog("Import Warning",
                    $"Imported {created} node(s) and {imported} connection(s).\n\n" +
                    $"Skipped {skipped} node(s) — their types were not found in this project.\n" +
                    "Check the Console for details.",
                    "OK");
        }

        #endregion

        #region Auto-Layout

        public void AutoLayout()
        {
            if (_graphAsset == null) return;

            var nodeViewMap = new Dictionary<string, GeneratorNodeView>();
            foreach (var element in graphElements)
            {
                if (element is GeneratorNodeView nv)
                    nodeViewMap[nv.NodeData.NodeId] = nv;
            }

            if (nodeViewMap.Count == 0) return;

            // Build adjacency for topological layers
            var inDegree = new Dictionary<string, int>();
            var adj = new Dictionary<string, List<string>>();
            foreach (var id in nodeViewMap.Keys)
            {
                inDegree[id] = 0;
                adj[id] = new List<string>();
            }

            foreach (var conn in _graphAsset.Connections)
            {
                if (!nodeViewMap.ContainsKey(conn.SourceNodeId)) continue;
                if (!nodeViewMap.ContainsKey(conn.TargetNodeId)) continue;
                adj[conn.SourceNodeId].Add(conn.TargetNodeId);
                inDegree[conn.TargetNodeId]++;
            }

            // Assign layers via Kahn's
            var layers = new List<List<string>>();
            var queue = new Queue<string>();
            foreach (var kvp in inDegree)
                if (kvp.Value == 0) queue.Enqueue(kvp.Key);

            while (queue.Count > 0)
            {
                var layer = new List<string>();
                int count = queue.Count;
                for (int i = 0; i < count; i++)
                {
                    var id = queue.Dequeue();
                    layer.Add(id);
                    foreach (var next in adj[id])
                    {
                        inDegree[next]--;
                        if (inDegree[next] == 0) queue.Enqueue(next);
                    }
                }
                layers.Add(layer);
            }

            // Position nodes
            float xSpacing = 300f;
            float ySpacing = 120f;

            Undo.RecordObject(_graphAsset, "Auto-Layout");

            for (int col = 0; col < layers.Count; col++)
            {
                var layer = layers[col];
                float totalHeight = (layer.Count - 1) * ySpacing;
                float startY = -totalHeight / 2f;

                for (int row = 0; row < layer.Count; row++)
                {
                    var id = layer[row];
                    if (!nodeViewMap.TryGetValue(id, out var view)) continue;

                    var pos = new Vector2(col * xSpacing + 50, startY + row * ySpacing + 50);
                    view.SetPosition(new Rect(pos.x, pos.y, 220, 0));
                    view.NodeData.EditorPosition = pos;
                }
            }

            EditorUtility.SetDirty(_graphAsset);
        }

        #endregion
    }
}
