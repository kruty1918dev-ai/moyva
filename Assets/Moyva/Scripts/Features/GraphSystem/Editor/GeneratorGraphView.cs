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
        private const string NodeClipboardPrefix = "MOYVA_NODE_V1:";

        [Serializable]
        private sealed class ClipboardNodePayload
        {
            public string nodeType;
            public string jsonData;
        }

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

        public bool CopyNodeAsText(GeneratorNodeView nodeView)
        {
            if (nodeView == null || nodeView.NodeData == null)
                return false;

            var payload = new ClipboardNodePayload
            {
                nodeType = nodeView.NodeData.GetType().AssemblyQualifiedName,
                jsonData = EditorJsonUtility.ToJson(nodeView.NodeData)
            };

            string json = JsonUtility.ToJson(payload);
            EditorGUIUtility.systemCopyBuffer = NodeClipboardPrefix + json;
            return true;
        }

        public bool PasteNodeFromText(Vector2 graphPosition, out string error)
        {
            error = null;

            if (_isReadOnly)
            {
                error = "Граф у режимі тільки для читання.";
                return false;
            }

            if (_graphAsset == null)
            {
                error = "GraphAsset не призначено.";
                return false;
            }

            string raw = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith(NodeClipboardPrefix, StringComparison.Ordinal))
            {
                error = "У буфері немає валідного тексту ноди Moyva.";
                return false;
            }

            string payloadJson = raw.Substring(NodeClipboardPrefix.Length);
            ClipboardNodePayload payload;
            try
            {
                payload = JsonUtility.FromJson<ClipboardNodePayload>(payloadJson);
            }
            catch (Exception ex)
            {
                error = $"Помилка читання тексту ноди: {ex.Message}";
                return false;
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.nodeType))
            {
                error = "Текст ноди пошкоджений або неповний.";
                return false;
            }

            Type nodeType = Type.GetType(payload.nodeType);
            if (nodeType == null)
            {
                error = $"Тип ноди не знайдено: {payload.nodeType}";
                return false;
            }

            Undo.RecordObject(_graphAsset, "Paste Node From Text");

            var node = _graphAsset.AddNode(nodeType);
            if (node == null)
            {
                error = "Не вдалося створити ноду цього типу.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(payload.jsonData))
                EditorJsonUtility.FromJsonOverwrite(payload.jsonData, node);

            node.NodeId = Guid.NewGuid().ToString();
            node.EditorPosition = graphPosition;

            var view = new GeneratorNodeView(node);
            AddElement(view);

            ClearSelection();
            AddToSelection(view);

            EditorUtility.SetDirty(_graphAsset);
            return true;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Вставити ноду з тексту", _ =>
            {
                Vector2 graphPos = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                if (!PasteNodeFromText(graphPos, out var error) && !string.IsNullOrEmpty(error))
                {
                    EditorUtility.DisplayDialog("Вставка ноди", error, "OK");
                }
            }, _ => _isReadOnly
                ? DropdownMenuAction.Status.Disabled
                : DropdownMenuAction.Status.Normal);
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

            // Collect referenced ScriptableObjects from all nodes
            var collectedSOs = new Dictionary<string, ScriptableObject>(); // guid → SO

            foreach (var node in nodesToExport)
            {
                preset.nodes.Add(new NodePresetEntry
                {
                    nodeTypeAssemblyQualifiedName = node.GetType().AssemblyQualifiedName,
                    originalNodeId = node.NodeId,
                    position = node.EditorPosition,
                    jsonData = EditorJsonUtility.ToJson(node)
                });

                CollectSOReferences(node, collectedSOs);
            }

            foreach (var kvp in collectedSOs)
            {
                preset.scriptableObjects.Add(new ScriptableObjectEntry
                {
                    originalGuid = kvp.Key,
                    typeAssemblyQualifiedName = kvp.Value.GetType().AssemblyQualifiedName,
                    assetName = kvp.Value.name,
                    jsonData = EditorJsonUtility.ToJson(kvp.Value)
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
                Debug.Log($"[GraphPreset] Exported {preset.nodes.Count} node(s), " +
                          $"{preset.connections.Count} connection(s), " +
                          $"{preset.scriptableObjects.Count} SO asset(s) to {path}");
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

            // Перший вибір: режим імпорту
            int modeChoice = EditorUtility.DisplayDialogComplex(
                "Import Preset",
                "Оберіть режим імпорту:\n\n" +
                "• «Замінити» — очистити граф, створити все наново\n" +
                "• «Модифікувати» — оновити існуючі вузли, додати відсутні",
                "Замінити",
                "Скасувати",
                "Модифікувати");

            if (modeChoice == 1) return; // Cancel

            if (modeChoice == 2)
            {
                MergePresetIntoGraph(preset);
                return;
            }

            // --- Оригінальний «Замінити» потік ---
            bool hasEmbeddedSOs = preset.scriptableObjects != null && preset.scriptableObjects.Count > 0;
            bool createFromPreset = false;

            if (hasEmbeddedSOs)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Import Preset — SO",
                    $"Цей пресет містить {preset.scriptableObjects.Count} вбудованих налаштувань " +
                    "(ScriptableObject): шум, висоти, біоми тощо.\n\n" +
                    "• «Створити нові» — створити нові SO-ассети з даних пресету\n" +
                    "• «Використати існуючі» — прив'язати до наявних ассетів проєкту",
                    "Створити нові",
                    "Скасувати",
                    "Використати існуючі");

                if (choice == 1) return;
                createFromPreset = choice == 0;
            }
            else
            {
                if (!EditorUtility.DisplayDialog("Import Preset",
                        "Поточний граф буде очищений і замінений на імпортований пресет.\n\n" +
                        "SO-посилання будуть прив'язані до існуючих ассетів проєкту.",
                        "Імпортувати", "Скасувати"))
                    return;
            }

            Undo.RecordObject(_graphAsset, "Import Graph Preset");

            // --- Clear existing graph (view + asset) ---
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            _graphAsset.ClearAll();
            graphViewChanged += OnGraphViewChanged;

            // --- Build SO lookup: originalGuid → loaded/created SO ---
            var soMap = new Dictionary<string, ScriptableObject>();
            int soCreated = 0;

            if (hasEmbeddedSOs && createFromPreset)
            {
                soCreated = CreateSOsFromPreset(preset.scriptableObjects, soMap);
            }

            var idMap = new Dictionary<string, string>();
            var newViews = new Dictionary<string, GeneratorNodeView>();

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

                string newId = Guid.NewGuid().ToString();
                idMap[entry.originalNodeId] = newId;
                node.NodeId = newId;
                node.EditorPosition = entry.position;

                // Restore serialized field values
                EditorJsonUtility.FromJsonOverwrite(entry.jsonData, node);
                node.NodeId = newId;
                node.EditorPosition = entry.position;

                // Assign SO references: either from newly created or from existing project assets
                if (createFromPreset)
                    AssignSOsFromMap(node, soMap);
                else
                    ResolveExistingSOReferences(node);

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

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(_graphAsset);

            var msg = $"Імпортовано {created} вузл(ів), {imported} з'єднань";
            if (soCreated > 0) msg += $", створено {soCreated} SO-ассет(ів)";
            if (skipped > 0) msg += $"\nПропущено {skipped} вузл(ів) — типи не знайдені";
            Debug.Log($"[GraphPreset] {msg}.");

            if (skipped > 0)
                EditorUtility.DisplayDialog("Попередження імпорту", msg, "OK");
        }

        /// <summary>
        /// Merge preset into existing graph: match nodes by type, update data, add missing, rebuild connections.
        /// </summary>
        private void MergePresetIntoGraph(GraphPreset preset)
        {
            Undo.RecordObject(_graphAsset, "Merge Graph Preset");

            // Build a pool of existing nodes grouped by type (for 1:1 matching)
            var existingByType = new Dictionary<string, List<NodeBase>>();
            foreach (var node in _graphAsset.Nodes)
            {
                if (node == null) continue;
                string typeName = node.GetType().AssemblyQualifiedName;
                if (!existingByType.TryGetValue(typeName, out var list))
                {
                    list = new List<NodeBase>();
                    existingByType[typeName] = list;
                }
                list.Add(node);
            }

            // Map: preset originalNodeId → actual nodeId in graph
            var idMap = new Dictionary<string, string>();
            var allViews = new Dictionary<string, GeneratorNodeView>();

            int updated = 0, added = 0, skipped = 0;

            foreach (var entry in preset.nodes)
            {
                var nodeType = Type.GetType(entry.nodeTypeAssemblyQualifiedName);
                if (nodeType == null)
                {
                    Debug.LogWarning($"[GraphPreset Merge] Type not found: {entry.nodeTypeAssemblyQualifiedName}");
                    skipped++;
                    continue;
                }

                string typeName = entry.nodeTypeAssemblyQualifiedName;
                NodeBase matched = null;

                // Try to match an existing node of the same type
                if (existingByType.TryGetValue(typeName, out var candidates) && candidates.Count > 0)
                {
                    matched = candidates[0];
                    candidates.RemoveAt(0);
                }

                if (matched != null)
                {
                    // Update existing node — restore field values, keep NodeId
                    string keepId = matched.NodeId;
                    EditorJsonUtility.FromJsonOverwrite(entry.jsonData, matched);
                    matched.NodeId = keepId;
                    matched.EditorPosition = entry.position;
                    ResolveExistingSOReferences(matched);

                    idMap[entry.originalNodeId] = keepId;
                    updated++;
                }
                else
                {
                    // Add new node
                    var node = _graphAsset.AddNode(nodeType);
                    if (node == null) { skipped++; continue; }

                    string newId = Guid.NewGuid().ToString();
                    node.NodeId = newId;
                    node.EditorPosition = entry.position;

                    EditorJsonUtility.FromJsonOverwrite(entry.jsonData, node);
                    node.NodeId = newId;
                    node.EditorPosition = entry.position;
                    ResolveExistingSOReferences(node);

                    idMap[entry.originalNodeId] = newId;
                    added++;
                }
            }

            // Rebuild connections: remove old, add from preset
            graphViewChanged -= OnGraphViewChanged;

            // Remove all existing edge views
            var existingEdges = edges.ToList();
            DeleteElements(existingEdges);

            // Clear asset connections
            var oldConnections = new List<Connection>(_graphAsset.Connections);
            foreach (var c in oldConnections)
                _graphAsset.RemoveConnection(c);

            // Rebuild node views
            var existingNodeViews = nodes.ToList();
            DeleteElements(existingNodeViews);

            foreach (var node in _graphAsset.Nodes)
            {
                if (node == null) continue;
                var view = new GeneratorNodeView(node);
                AddElement(view);
                allViews[node.NodeId] = view;
            }

            // Restore connections from preset
            int imported = 0;
            foreach (var connEntry in preset.connections)
            {
                if (!idMap.TryGetValue(connEntry.sourceNodeId, out var srcId)) continue;
                if (!idMap.TryGetValue(connEntry.targetNodeId, out var tgtId)) continue;

                _graphAsset.AddConnection(srcId, connEntry.sourcePortIndex,
                    tgtId, connEntry.targetPortIndex);

                if (!allViews.TryGetValue(srcId, out var srcView)) continue;
                if (!allViews.TryGetValue(tgtId, out var tgtView)) continue;

                var outPort = srcView.GetOutputPort(connEntry.sourcePortIndex);
                var inPort = tgtView.GetInputPort(connEntry.targetPortIndex);
                if (outPort != null && inPort != null)
                {
                    AddElement(outPort.ConnectTo(inPort));
                    imported++;
                }
            }

            graphViewChanged += OnGraphViewChanged;

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(_graphAsset);

            var msg = $"Модифіковано: оновлено {updated}, додано {added} вузл(ів), {imported} з'єднань";
            if (skipped > 0) msg += $"\nПропущено {skipped}";
            Debug.Log($"[GraphPreset Merge] {msg}");

            if (skipped > 0)
                EditorUtility.DisplayDialog("Попередження", msg, "OK");
        }

        #region SO Helpers

        private static readonly System.Reflection.BindingFlags SOFieldFlags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public;

        /// <summary>Collect all ScriptableObject references from a node into the dictionary (keyed by asset GUID).</summary>
        private static void CollectSOReferences(NodeBase node, Dictionary<string, ScriptableObject> collected)
        {
            foreach (var field in node.GetType().GetFields(SOFieldFlags))
            {
                if (!IsSerializedSOField(field)) continue;

                var so = field.GetValue(node) as ScriptableObject;
                if (so == null) continue;

                var assetPath = AssetDatabase.GetAssetPath(so);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (!string.IsNullOrEmpty(guid))
                    collected.TryAdd(guid, so);
            }
        }

        /// <summary>Create new SO assets from embedded preset data. Returns number created.</summary>
        private static int CreateSOsFromPreset(List<ScriptableObjectEntry> entries,
            Dictionary<string, ScriptableObject> soMap)
        {
            const string soFolder = "Assets/Moyva/SO/Generation";
            if (!AssetDatabase.IsValidFolder(soFolder))
                AssetDatabase.CreateFolder("Assets/Moyva/SO", "Generation");

            int count = 0;
            foreach (var entry in entries)
            {
                var soType = Type.GetType(entry.typeAssemblyQualifiedName);
                if (soType == null)
                {
                    Debug.LogWarning($"[GraphPreset] SO type not found: {entry.typeAssemblyQualifiedName}");
                    continue;
                }

                var newSO = ScriptableObject.CreateInstance(soType);
                newSO.name = entry.assetName;

                // Restore all field values from embedded JSON
                EditorJsonUtility.FromJsonOverwrite(entry.jsonData, newSO);
                newSO.name = entry.assetName;

                var assetPath = AssetDatabase.GenerateUniqueAssetPath(
                    $"{soFolder}/{entry.assetName}.asset");
                AssetDatabase.CreateAsset(newSO, assetPath);

                soMap[entry.originalGuid] = newSO;
                count++;
                Debug.Log($"[GraphPreset] Created SO from preset: {assetPath} (type: {soType.Name})");
            }

            return count;
        }

        /// <summary>Assign SO fields on a node using the guid→SO map (for "create new" mode).</summary>
        private static void AssignSOsFromMap(NodeBase node, Dictionary<string, ScriptableObject> soMap)
        {
            foreach (var field in node.GetType().GetFields(SOFieldFlags))
            {
                if (!IsSerializedSOField(field)) continue;

                var currentValue = field.GetValue(node) as ScriptableObject;
                if (currentValue != null) continue;

                // The JSON had a GUID reference that didn't resolve — find it in soMap by type
                foreach (var kvp in soMap)
                {
                    if (field.FieldType.IsInstanceOfType(kvp.Value))
                    {
                        field.SetValue(node, kvp.Value);
                        break;
                    }
                }
            }
        }

        /// <summary>Resolve null SO fields by finding existing assets of matching type in the project.</summary>
        private static void ResolveExistingSOReferences(NodeBase node)
        {
            var nodeType = node.GetType();

            foreach (var field in nodeType.GetFields(SOFieldFlags))
            {
                if (!IsSerializedSOField(field)) continue;

                var currentValue = field.GetValue(node) as ScriptableObject;
                if (currentValue != null) continue;

                var guids = AssetDatabase.FindAssets($"t:{field.FieldType.Name}");
                if (guids.Length > 0)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var existing = AssetDatabase.LoadAssetAtPath(assetPath, field.FieldType);
                    if (existing != null)
                    {
                        field.SetValue(node, existing);
                        Debug.Log($"[GraphPreset] Resolved {nodeType.Name}.{field.Name} → {assetPath}");
                    }
                }
            }
        }

        private static bool IsSerializedSOField(System.Reflection.FieldInfo field)
        {
            if (!field.IsDefined(typeof(SerializeField), true) && !field.IsPublic)
                return false;
            return typeof(ScriptableObject).IsAssignableFrom(field.FieldType) &&
                   field.FieldType != typeof(ScriptableObject);
        }

        #endregion

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
