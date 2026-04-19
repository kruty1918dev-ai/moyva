using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Grid.API;
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
        private const int NodeTooltipDelayMs = 450;

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
        private bool _inlinePreviewsVisible = true;
        private readonly Dictionary<GeneratorNodeView, NodeBorderSnapshot> _edgeHoverNodeSnapshots = new();
        private readonly Dictionary<string, string> _edgeTooltipTexts = new();
        private Label _floatingTooltip;
        public event Action GraphChanged;

        private struct NodeBorderSnapshot
        {
            public Color LeftColor;
            public Color RightColor;
            public Color TopColor;
            public Color BottomColor;
            public float LeftWidth;
            public float RightWidth;
            public float TopWidth;
            public float BottomWidth;
            public int ActiveHoverCount;
        }

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

            _floatingTooltip = new Label
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    display = DisplayStyle.None,
                    backgroundColor = new Color(0.09f, 0.09f, 0.09f, 0.96f),
                    color = Color.white,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 5,
                    paddingBottom = 5,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftColor = new Color(0.35f, 0.35f, 0.35f),
                    borderRightColor = new Color(0.35f, 0.35f, 0.35f),
                    borderTopColor = new Color(0.35f, 0.35f, 0.35f),
                    borderBottomColor = new Color(0.35f, 0.35f, 0.35f),
                    unityTextAlign = TextAnchor.MiddleLeft,
                    maxWidth = 340,
                    whiteSpace = WhiteSpace.Normal,
                    unityFontStyleAndWeight = FontStyle.Normal
                }
            };
            Add(_floatingTooltip);

            // Keyboard shortcuts
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        public void SetMinimapVisible(bool visible) => _miniMap.visible = visible;

        private void BringAllNodesToFront()
        {
            foreach (var node in nodes.OfType<GeneratorNodeView>())
                node.BringToFront();
        }

        private static Color GetEdgeColorForType(Type type)
        {
            if (type == typeof(float[,])) return new Color(0.35f, 0.9f, 0.5f);
            if (type == typeof(string[,])) return new Color(0.45f, 0.75f, 1f);
            if (type == typeof(bool[,])) return new Color(1f, 0.86f, 0.32f);
            if (type == typeof(int[,])) return new Color(1f, 0.62f, 0.3f);
            if (type == typeof(object)) return new Color(0.9f, 0.9f, 0.9f);
            return new Color(0.75f, 0.75f, 0.75f);
        }

        private static string FormatTypeName(Type type)
        {
            if (type == typeof(float[,])) return "float[,] (HeightMap)";
            if (type == typeof(string[,])) return "string[,] (Tile/Biome/Object map)";
            if (type == typeof(bool[,])) return "bool[,] (Mask)";
            if (type == typeof(int[,])) return "int[,]";
            if (type == typeof(object)) return "Any";
            return type?.Name ?? "Unknown";
        }

        private Edge CreateStyledEdge(GeneratorPort outputPort, GeneratorPort inputPort)
        {
            var edge = outputPort.ConnectTo(inputPort);
            SetupEdgeVisuals(edge, outputPort, inputPort);
            return edge;
        }

        private void SetupEdgeVisuals(Edge edge, GeneratorPort outputPort, GeneratorPort inputPort)
        {
            if (edge == null || outputPort == null || inputPort == null)
                return;

            var dataType = outputPort.PortValueType ?? inputPort.PortValueType;
            var baseColor = GetEdgeColorForType(dataType);

            SetEdgeColor(edge, baseColor);
            SetEdgeWidth(edge, 2.2f);

            var srcView = outputPort.node as GeneratorNodeView;
            var dstView = inputPort.node as GeneratorNodeView;
            string srcNodeTitle = srcView?.NodeData?.Title ?? "Source";
            string dstNodeTitle = dstView?.NodeData?.Title ?? "Target";

            string edgeTooltip =
                $"{srcNodeTitle}.{outputPort.portName} -> {dstNodeTitle}.{inputPort.portName}\n" +
                $"Data: {FormatTypeName(dataType)}";
            edge.tooltip = string.Empty;
            _edgeTooltipTexts[GetEdgeKey(edge)] = edgeTooltip;

            edge.RegisterCallback<MouseEnterEvent>(evt =>
            {
                SetEdgeColor(edge, Color.white);
                SetEdgeWidth(edge, 4f);
                ShowFloatingTooltip(edgeTooltip, edge.ChangeCoordinatesTo(this, evt.localMousePosition));

                if (srcView != null) HighlightNodeForEdgeHover(srcView, true);
                if (dstView != null) HighlightNodeForEdgeHover(dstView, true);
            });

            edge.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (_floatingTooltip.style.display == DisplayStyle.Flex)
                    MoveFloatingTooltip(edge.ChangeCoordinatesTo(this, evt.localMousePosition));
            });

            edge.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                SetEdgeColor(edge, baseColor);
                SetEdgeWidth(edge, 2.2f);
                HideFloatingTooltip();

                if (srcView != null) HighlightNodeForEdgeHover(srcView, false);
                if (dstView != null) HighlightNodeForEdgeHover(dstView, false);
            });
        }

        private static string GetEdgeKey(Edge edge)
        {
            var sourceView = edge.output?.node as GeneratorNodeView;
            var targetView = edge.input?.node as GeneratorNodeView;
            var sourcePort = edge.output as GeneratorPort;
            var targetPort = edge.input as GeneratorPort;
            if (sourceView == null || targetView == null || sourcePort == null || targetPort == null)
                return string.Empty;

            return $"{sourceView.NodeData.NodeId}:{sourcePort.PortIndex}>{targetView.NodeData.NodeId}:{targetPort.PortIndex}";
        }

        private void ShowFloatingTooltip(string text, Vector2 graphPosition)
        {
            if (_floatingTooltip == null || string.IsNullOrEmpty(text))
                return;

            _floatingTooltip.text = text;
            _floatingTooltip.style.display = DisplayStyle.Flex;
            MoveFloatingTooltip(graphPosition);
        }

        private void MoveFloatingTooltip(Vector2 graphPosition)
        {
            if (_floatingTooltip == null)
                return;

            _floatingTooltip.style.left = graphPosition.x + 14f;
            _floatingTooltip.style.top = graphPosition.y + 16f;
        }

        private void HideFloatingTooltip()
        {
            if (_floatingTooltip == null)
                return;

            _floatingTooltip.style.display = DisplayStyle.None;
        }

        private void SetupNodeVisuals(GeneratorNodeView nodeView)
        {
            if (nodeView == null)
                return;

            bool isHovering = false;
            Vector2 lastGraphMousePos = Vector2.zero;
            IVisualElementScheduledItem delayedTooltipTask = null;

            nodeView.RegisterCallback<MouseEnterEvent>(evt =>
            {
                isHovering = true;
                lastGraphMousePos = nodeView.ChangeCoordinatesTo(this, evt.localMousePosition);

                delayedTooltipTask?.Pause();
                delayedTooltipTask = schedule.Execute(() =>
                {
                    string currentTooltip = nodeView.HoverTooltipText;
                    if (string.IsNullOrWhiteSpace(currentTooltip)) return;
                    if (!isHovering) return;
                    ShowFloatingTooltip(currentTooltip, lastGraphMousePos);
                }).StartingIn(NodeTooltipDelayMs);
            });

            nodeView.RegisterCallback<MouseMoveEvent>(evt =>
            {
                lastGraphMousePos = nodeView.ChangeCoordinatesTo(this, evt.localMousePosition);
                if (_floatingTooltip.style.display == DisplayStyle.Flex)
                {
                    MoveFloatingTooltip(lastGraphMousePos);
                }
            });

            nodeView.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                isHovering = false;
                delayedTooltipTask?.Pause();
                HideFloatingTooltip();
            });
        }

        private static void SetEdgeColor(Edge edge, Color color)
        {
            var edgeType = edge.GetType();
            var defaultColorProp = edgeType.GetProperty("defaultColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            TrySetColorMember(edge, defaultColorProp, "m_DefaultColor", "defaultColor", color);

            var selectedColorProp = edgeType.GetProperty("selectedColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            TrySetColorMember(edge, selectedColorProp, "m_SelectedColor", "selectedColor", Color.Lerp(color, Color.white, 0.35f));

            if (TryGetEdgeControl(edge, out var edgeControl))
            {
                TrySetColorMember(edgeControl, edgeControl.GetType().GetProperty("inputColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), "m_InputColor", "inputColor", color);
                TrySetColorMember(edgeControl, edgeControl.GetType().GetProperty("outputColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), "m_OutputColor", "outputColor", color);
            }

            edge.MarkDirtyRepaint();
        }

        private static void SetEdgeWidth(Edge edge, float width)
        {
            if (TryGetEdgeControl(edge, out var edgeControl))
            {
                var widthProp = edgeControl.GetType().GetProperty("edgeWidth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (widthProp?.CanWrite == true)
                {
                    widthProp.SetValue(edgeControl, ConvertNumericWidth(width, widthProp.PropertyType));
                }
                else
                {
                    var widthField = edgeControl.GetType().GetField("m_EdgeWidth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?? edgeControl.GetType().GetField("edgeWidth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (widthField != null)
                        widthField.SetValue(edgeControl, ConvertNumericWidth(width, widthField.FieldType));
                }
            }

            edge.MarkDirtyRepaint();
        }

        private static object ConvertNumericWidth(float width, Type targetType)
        {
            if (targetType == typeof(float))
                return width;

            if (targetType == typeof(double))
                return (double)width;

            if (targetType == typeof(int))
                return Mathf.RoundToInt(width);

            if (targetType == typeof(long))
                return (long)Mathf.RoundToInt(width);

            if (targetType == typeof(short))
                return (short)Mathf.RoundToInt(width);

            if (targetType == typeof(byte))
                return (byte)Mathf.Clamp(Mathf.RoundToInt(width), byte.MinValue, byte.MaxValue);

            return Convert.ChangeType(width, targetType, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static bool TryGetEdgeControl(Edge edge, out object edgeControl)
        {
            var edgeControlProp = edge.GetType().GetProperty("edgeControl", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            edgeControl = edgeControlProp?.GetValue(edge);
            return edgeControl != null;
        }

        private static void TrySetColorMember(object target, PropertyInfo property, string preferredFieldName, string fallbackFieldName, Color value)
        {
            if (target == null)
                return;

            if (property?.CanWrite == true)
            {
                property.SetValue(target, value);
                return;
            }

            var targetType = target.GetType();
            var field = targetType.GetField(preferredFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? targetType.GetField(fallbackFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field != null && field.FieldType == typeof(Color))
                field.SetValue(target, value);
        }

        private void HighlightNodeForEdgeHover(GeneratorNodeView node, bool active)
        {
            if (node == null)
                return;

            if (!_edgeHoverNodeSnapshots.TryGetValue(node, out var snapshot))
            {
                snapshot = new NodeBorderSnapshot
                {
                    LeftColor = node.style.borderLeftColor.value,
                    RightColor = node.style.borderRightColor.value,
                    TopColor = node.style.borderTopColor.value,
                    BottomColor = node.style.borderBottomColor.value,
                    LeftWidth = node.style.borderLeftWidth.value,
                    RightWidth = node.style.borderRightWidth.value,
                    TopWidth = node.style.borderTopWidth.value,
                    BottomWidth = node.style.borderBottomWidth.value,
                    ActiveHoverCount = 0
                };
            }

            if (active)
            {
                snapshot.ActiveHoverCount++;

                node.style.borderLeftColor = Color.white;
                node.style.borderRightColor = Color.white;
                node.style.borderTopColor = Color.white;
                node.style.borderBottomColor = Color.white;

                node.style.borderLeftWidth = Mathf.Max(2f, snapshot.LeftWidth);
                node.style.borderRightWidth = Mathf.Max(2f, snapshot.RightWidth);
                node.style.borderTopWidth = Mathf.Max(2f, snapshot.TopWidth);
                node.style.borderBottomWidth = Mathf.Max(2f, snapshot.BottomWidth);

                _edgeHoverNodeSnapshots[node] = snapshot;
                return;
            }

            snapshot.ActiveHoverCount = Mathf.Max(0, snapshot.ActiveHoverCount - 1);
            if (snapshot.ActiveHoverCount == 0)
            {
                node.style.borderLeftColor = snapshot.LeftColor;
                node.style.borderRightColor = snapshot.RightColor;
                node.style.borderTopColor = snapshot.TopColor;
                node.style.borderBottomColor = snapshot.BottomColor;

                node.style.borderLeftWidth = snapshot.LeftWidth;
                node.style.borderRightWidth = snapshot.RightWidth;
                node.style.borderTopWidth = snapshot.TopWidth;
                node.style.borderBottomWidth = snapshot.BottomWidth;

                _edgeHoverNodeSnapshots.Remove(node);
            }
            else
            {
                _edgeHoverNodeSnapshots[node] = snapshot;
            }
        }

        internal void SetInlinePreviewsVisible(bool visible)
        {
            _inlinePreviewsVisible = visible;
            foreach (var view in nodes.OfType<GeneratorNodeView>())
                view.SetPreviewVisible(visible);
        }

        internal bool TryGetBestPreview(out Texture2D previewTexture, out string status)
        {
            // 1) Selected node preview
            var selectedNode = selection.OfType<GeneratorNodeView>().FirstOrDefault();
            if (selectedNode != null && selectedNode.PreviewTexture != null)
            {
                previewTexture = selectedNode.PreviewTexture;
                status = selectedNode.PreviewStatus;
                return true;
            }

            // 2) Output node preview
            var outputNode = nodes
                .OfType<GeneratorNodeView>()
                .FirstOrDefault(v => v.NodeData is OutputNode && v.PreviewTexture != null);
            if (outputNode != null)
            {
                previewTexture = outputNode.PreviewTexture;
                status = outputNode.PreviewStatus;
                return true;
            }

            // 3) First available preview
            var anyNode = nodes
                .OfType<GeneratorNodeView>()
                .FirstOrDefault(v => v.PreviewTexture != null);
            if (anyNode != null)
            {
                previewTexture = anyNode.PreviewTexture;
                status = anyNode.PreviewStatus;
                return true;
            }

            previewTexture = null;
            status = "No preview available";
            return false;
        }

        internal NodeBase GetPrimarySelectedNodeData()
        {
            return selection.OfType<GeneratorNodeView>().FirstOrDefault()?.NodeData;
        }

        internal void OpenPreviewWindowForNode(GeneratorNodeView nodeView)
        {
            if (nodeView != null)
            {
                ClearSelection();
                AddToSelection(nodeView);
                FrameSelection();
            }

            GraphPreviewWindow.Open(_window);
        }

        internal void UpdateNodePreviews(GraphExecutionResult result,
            EditorPreviewSettings settings = null,
            List<WorldLayerData> layerData = null,
            int previewSize = 128,
            bool heatmap = false)
        {
            var tileRegistry = settings?.TileRegistry;
            var views = nodes.OfType<GeneratorNodeView>().ToList();
            if (views.Count == 0)
                return;

            foreach (var view in views)
                view.ClearPreview();

            if (_graphAsset == null || result == null)
                return;

            var viewById = new Dictionary<string, GeneratorNodeView>(views.Count);
            foreach (var view in views)
            {
                if (view.NodeData != null && !string.IsNullOrEmpty(view.NodeData.NodeId))
                    viewById[view.NodeData.NodeId] = view;
            }

            foreach (var node in _graphAsset.Nodes)
            {
                if (node == null) continue;
                if (!viewById.TryGetValue(node.NodeId, out var nodeView)) continue;

                // ── OutputNode: composite preview ──
                if (node is OutputNode)
                {
                    var outputs = result.GetOutputs(node.NodeId);
                    string[,] biomeMap = null;
                    string[,] objectMap = null;
                    float[,] heightMap = null;
                    string[,] buildingMap = null;

                    if (outputs != null)
                    {
                        if (outputs.Length > 0) biomeMap = outputs[0] as string[,];
                        if (outputs.Length > 1) objectMap = outputs[1] as string[,];
                        if (outputs.Length > 2) heightMap = outputs[2] as float[,];
                        if (outputs.Length > 3) buildingMap = outputs[3] as string[,];
                    }

                    var composite = CompositePreviewBuilder.Build(
                        layerData, biomeMap, objectMap, heightMap, buildingMap,
                        tileRegistry,
                        settings?.MapObjectRegistry,
                        settings?.BuildingRegistry);

                    nodeView.SetPreview(composite, "Composite preview", ownsTexture: true);
                    nodeView.SetPreviewVisible(_inlinePreviewsVisible);
                    continue;
                }

                // ── Звичайні ноди ──
                Texture2D preview = null;
                bool ownsPreview = false;
                string status = null;

                if (node is IPreviewableNode previewable)
                {
                    preview = previewable.GeneratePreview(previewSize, previewSize);
                    if (preview != null)
                        status = "Node preview";
                }

                if (preview == null)
                {
                    var outputs = result.GetOutputs(node.NodeId);

                    // Для lake/water/mask-вузлів показуємо саме bool-mask, а не перший (часто BiomeMap) вихід.
                    outputs = SelectPreferredPreviewOutputs(node, outputs);

                    preview = NodePreviewTextureFactory.TryBuild(
                        outputs,
                        previewSize,
                        previewSize,
                        out ownsPreview,
                        out status,
                        tileRegistry,
                        heatmap);
                }

                nodeView.SetPreview(preview, status, ownsPreview);
                nodeView.SetPreviewVisible(_inlinePreviewsVisible);
            }
        }

        private static object[] SelectPreferredPreviewOutputs(NodeBase node, object[] outputs)
        {
            if (node == null || outputs == null || outputs.Length == 0)
                return outputs;

            // 1) LakeGenerationNode: завжди віддаємо пріоритет WaterMask (port #1).
            if (node is LakeGenerationNode && outputs.Length > 1 && outputs[1] is bool[,])
                return new object[] { outputs[1] };

            // 2) Загальне правило: якщо є bool[,] вихід з назвою "*Mask*", показуємо його.
            var defs = node.Outputs;
            if (defs != null)
            {
                int count = Mathf.Min(defs.Length, outputs.Length);
                for (int i = 0; i < count; i++)
                {
                    if (outputs[i] is not bool[,]) continue;

                    var name = defs[i].Name;
                    if (!string.IsNullOrEmpty(name) && name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0)
                        return new object[] { outputs[i] };
                }
            }

            return outputs;
        }

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

            var createGraphPos = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("Create Node...", _ =>
            {
                if (_isReadOnly)
                    return;

                var screenPos = GUIUtility.GUIToScreenPoint(evt.mousePosition);
                SearchWindow.Open(new SearchWindowContext(screenPos), _searchProvider);
            }, _ => _isReadOnly
                ? DropdownMenuAction.Status.Disabled
                : DropdownMenuAction.Status.Normal);

            evt.menu.AppendAction("Create Group", _ =>
            {
                CreateGroupAtPosition(createGraphPos);
            }, _ => _isReadOnly
                ? DropdownMenuAction.Status.Disabled
                : DropdownMenuAction.Status.Normal);

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
        private void CreateGroupAtPosition(Vector2 graphPosition)
        {
            if (_isReadOnly) return;

            var selectedNodes = selection.OfType<GeneratorNodeView>().ToList();
            if (selectedNodes.Count > 0)
            {
                GroupSelection();
                return;
            }

            var group = new Group { title = "Node Group" };
            group.SetPosition(new Rect(graphPosition.x, graphPosition.y, 260f, 140f));
            AddElement(group);
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
            if (a.PortValueType == typeof(object) || b.PortValueType == typeof(object))
                return true;

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
            _edgeTooltipTexts.Clear();
            HideFloatingTooltip();

            // Clear
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (asset == null) return;

            _graphAsset.RepairMissingNodeConnections();
            _graphAsset.RemoveNullNodes();
            FlattenLegacyRoutePoints();

            // Create node views
            var nodeViews = new Dictionary<string, GeneratorNodeView>();
            foreach (var node in asset.Nodes)
            {
                if (node == null) continue;
                var view = new GeneratorNodeView(node);
                AddElement(view);
                SetupNodeVisuals(view);
                view.SetPreviewVisible(_inlinePreviewsVisible);
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

                var edge = CreateStyledEdge(outputPort, inputPort);
                AddElement(edge);
            }

            BringAllNodesToFront();

            UpdateNodePreviews(null);
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
            SetupNodeVisuals(view);
            view.SetPreviewVisible(_inlinePreviewsVisible);

            EditorUtility.SetDirty(_graphAsset);
            GraphChanged?.Invoke();
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
                if (change.elementsToRemove.Count > 0)
                    GraphChanged?.Invoke();
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

                    SetupEdgeVisuals(edge, sourcePort, targetPort);
                }
                EditorUtility.SetDirty(_graphAsset);
                BringAllNodesToFront();
                if (change.edgesToCreate.Count > 0)
                    GraphChanged?.Invoke();
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
                if (change.movedElements.Count > 0)
                    GraphChanged?.Invoke();
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
                    _edgeTooltipTexts.Remove(GetEdgeKey(edge));
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
                SetupNodeVisuals(view);
                view.SetPreviewVisible(_inlinePreviewsVisible);
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
                SetupNodeVisuals(view);
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

                AddElement(CreateStyledEdge(outputPort, inputPort));
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
                SetupNodeVisuals(view);
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
                    AddElement(CreateStyledEdge(outPort, inPort));
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

            Undo.RecordObject(_graphAsset, "Auto-Layout");

            FlattenLegacyRoutePoints();

            // Refresh views from the current asset state before layout
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            // Rebuild views from asset
            var nodeViews = new Dictionary<string, GeneratorNodeView>();
            foreach (var node in _graphAsset.Nodes)
            {
                if (node == null) continue;
                var view = new GeneratorNodeView(node);
                AddElement(view);
                SetupNodeVisuals(view);
                view.SetPreviewVisible(_inlinePreviewsVisible);
                nodeViews[node.NodeId] = view;
            }

            // Recreate edges
            foreach (var conn in _graphAsset.Connections)
            {
                if (!nodeViews.TryGetValue(conn.SourceNodeId, out var sv)) continue;
                if (!nodeViews.TryGetValue(conn.TargetNodeId, out var tv)) continue;
                var oPort = sv.GetOutputPort(conn.SourcePortIndex);
                var iPort = tv.GetInputPort(conn.TargetPortIndex);
                if (oPort == null || iPort == null) continue;
                AddElement(CreateStyledEdge(oPort, iPort));
            }

            var nodeViewMap = nodeViews;
            if (nodeViewMap.Count == 0) return;

            // --- Build adjacency ---
            var nodeIds = new HashSet<string>(nodeViewMap.Keys);
            var outgoing = new Dictionary<string, List<string>>();
            var incoming = new Dictionary<string, List<string>>();

            foreach (var id in nodeIds)
            {
                outgoing[id] = new List<string>();
                incoming[id] = new List<string>();
            }

            foreach (var conn in _graphAsset.Connections)
            {
                if (!nodeIds.Contains(conn.SourceNodeId) || !nodeIds.Contains(conn.TargetNodeId))
                    continue;
                outgoing[conn.SourceNodeId].Add(conn.TargetNodeId);
                incoming[conn.TargetNodeId].Add(conn.SourceNodeId);
            }

            // --- Topological order ---
            var topoOrder = new List<string>();
            var inDeg = new Dictionary<string, int>();
            foreach (var id in nodeIds) inDeg[id] = 0;
            foreach (var id in nodeIds)
                foreach (var next in outgoing[id])
                    inDeg[next]++;

            var topoQ = new Queue<string>();
            foreach (var kvp in inDeg)
                if (kvp.Value == 0) topoQ.Enqueue(kvp.Key);

            while (topoQ.Count > 0)
            {
                string id = topoQ.Dequeue();
                topoOrder.Add(id);
                foreach (var next in outgoing[id])
                    if (--inDeg[next] == 0) topoQ.Enqueue(next);
            }

            foreach (var id in nodeIds)
                if (!topoOrder.Contains(id)) topoOrder.Add(id);

            // --- Layer assignment: longest-path ---
            var layerOf = new Dictionary<string, int>();
            foreach (var id in topoOrder)
            {
                int best = -1;
                foreach (var pred in incoming[id])
                    if (layerOf.TryGetValue(pred, out int pl) && pl > best) best = pl;
                layerOf[id] = best + 1;
            }

            // Pull-right: push source nodes closer to consumers
            for (int i = topoOrder.Count - 1; i >= 0; i--)
            {
                string id = topoOrder[i];
                if (outgoing[id].Count == 0) continue;
                int minSucc = int.MaxValue;
                foreach (var succ in outgoing[id])
                    if (layerOf.TryGetValue(succ, out int sl) && sl < minSucc) minSucc = sl;
                if (minSucc != int.MaxValue && minSucc - 1 > layerOf[id])
                    layerOf[id] = minSucc - 1;
            }

            // --- Group into layers ---
            int maxLayer = 0;
            foreach (var kvp in layerOf)
                if (kvp.Value > maxLayer) maxLayer = kvp.Value;

            var layers = new List<List<string>>();
            for (int i = 0; i <= maxLayer; i++)
                layers.Add(new List<string>());
            foreach (var kvp in layerOf)
                layers[kvp.Value].Add(kvp.Key);

            // Barycenter ordering
            ImproveLayerOrdering(layers, incoming, outgoing);

            // ================================================================
            //  Y-coordinate assignment: initial stack → median refinement
            // ================================================================

            const float xGap = 80f;
            const float yGap = 30f;

            // Node width/height cache
            var nodeW = new Dictionary<string, float>();
            var nodeH = new Dictionary<string, float>();
            foreach (var id in nodeIds)
            {
                if (nodeViewMap.TryGetValue(id, out var v))
                {
                    nodeW[id] = 220f;
                    nodeH[id] = EstimateNodeHeight(v);
                }
                else
                {
                    nodeW[id] = 220f;
                    nodeH[id] = 160f;
                }
            }

            // Step 1: initial Y — stack nodes vertically per layer, centered at Y=0
            var posY = new Dictionary<string, float>();
            foreach (var layer in layers)
            {
                float totalH = 0f;
                for (int i = 0; i < layer.Count; i++)
                {
                    totalH += nodeH[layer[i]];
                    if (i < layer.Count - 1) totalH += yGap;
                }

                float y = -totalH * 0.5f;
                for (int i = 0; i < layer.Count; i++)
                {
                    posY[layer[i]] = y;
                    y += nodeH[layer[i]] + yGap;
                }
            }

            // Step 2: median refinement (several passes)
            for (int pass = 0; pass < 6; pass++)
            {
                for (int l = 1; l < layers.Count; l++)
                {
                    var layer = layers[l];
                    AssignMedianY(layer, incoming, posY, nodeH, yGap);
                    PreventOverlap(layer, posY, nodeH, yGap);
                }
                for (int l = layers.Count - 2; l >= 0; l--)
                {
                    var layer = layers[l];
                    AssignMedianY(layer, outgoing, posY, nodeH, yGap);
                    PreventOverlap(layer, posY, nodeH, yGap);
                }
            }

            // ================================================================
            //  Apply positions
            // ================================================================

            float cursorX = 50f;
            for (int col = 0; col < layers.Count; col++)
            {
                var layer = layers[col];
                if (layer.Count == 0) continue;

                // Find max node width in this column
                float maxW = 0f;
                for (int row = 0; row < layer.Count; row++)
                {
                    float w = nodeW.TryGetValue(layer[row], out float wv) ? wv : 220f;
                    if (w > maxW) maxW = w;
                }

                for (int row = 0; row < layer.Count; row++)
                {
                    if (!nodeViewMap.TryGetValue(layer[row], out var view)) continue;

                    float w = nodeW.TryGetValue(layer[row], out float ww) ? ww : 220f;
                    var pos = new Vector2(cursorX, posY[layer[row]]);
                    view.SetPosition(new Rect(pos.x, pos.y, w, 0));
                    view.NodeData.EditorPosition = pos;
                }

                cursorX += maxW + xGap;
            }

            BringAllNodesToFront();
            EditorUtility.SetDirty(_graphAsset);
            GraphChanged?.Invoke();
        }

        /// <summary>
        /// Set each node's Y to the median center-Y of its connected neighbors.
        /// </summary>
        private static void AssignMedianY(
            List<string> layer,
            Dictionary<string, List<string>> adjacency,
            Dictionary<string, float> posY,
            Dictionary<string, float> nodeH,
            float yGap)
        {
            for (int i = 0; i < layer.Count; i++)
            {
                string id = layer[i];
                if (!adjacency.TryGetValue(id, out var neighbors) || neighbors.Count == 0)
                    continue;

                // Collect center-Y of all neighbors
                var centers = new List<float>(neighbors.Count);
                foreach (var n in neighbors)
                {
                    if (posY.TryGetValue(n, out float ny) && nodeH.TryGetValue(n, out float nh))
                        centers.Add(ny + nh * 0.5f);
                }

                if (centers.Count == 0) continue;

                centers.Sort();
                float median = centers[centers.Count / 2];
                float h = nodeH.ContainsKey(id) ? nodeH[id] : 160f;

                // Place node so its center aligns with the median
                posY[id] = median - h * 0.5f;
            }
        }

        /// <summary>
        /// Push apart overlapping nodes within a layer (maintaining order).
        /// </summary>
        private static void PreventOverlap(
            List<string> layer,
            Dictionary<string, float> posY,
            Dictionary<string, float> nodeH,
            float yGap)
        {
            if (layer.Count <= 1) return;

            // Sort layer by current Y position (preserve visual order)
            layer.Sort((a, b) => posY[a].CompareTo(posY[b]));

            // Push downward
            for (int i = 1; i < layer.Count; i++)
            {
                float prevBottom = posY[layer[i - 1]] + nodeH[layer[i - 1]] + yGap;
                if (posY[layer[i]] < prevBottom)
                    posY[layer[i]] = prevBottom;
            }

            // Re-center the column around original center
            float top = posY[layer[0]];
            float bottom = posY[layer[^1]] + nodeH[layer[^1]];
            float center = (top + bottom) * 0.5f;
            float shift = -center;
            for (int i = 0; i < layer.Count; i++)
                posY[layer[i]] += shift;
        }

        /// <summary>
        /// Re-reads all node positions from the asset and refreshes the graph.
        /// Used by Undo/Redo to synchronize the visual state.
        /// </summary>
        public void RefreshFromAsset()
        {
            if (_graphAsset == null) return;
            PopulateGraph(_graphAsset, _isReadOnly);
        }

        /// <summary>
        /// Estimates the visual height of a node view.
        /// Uses resolvedStyle.height when available, otherwise calculates from node data:
        /// title bar + category badge + port rows + preview + custom-editor button.
        /// </summary>
        private float EstimateNodeHeight(GeneratorNodeView view)
        {
            // Try to use the actual measured height first
            float resolved = view.resolvedStyle.height;
            if (resolved > 20f && !float.IsNaN(resolved))
                return resolved;

            return EstimateNodeHeightFromData(view.NodeData);
        }

        private float EstimateNodeHeightFromData(NodeBase node)
        {
            float h = 34f;  // title bar
            h += 20f;       // category badge

            int portRows = Mathf.Max(node.Inputs?.Length ?? 0, node.Outputs?.Length ?? 0);
            h += portRows * 22f;

            // Preview (only when inline previews are visible)
            if (_inlinePreviewsVisible)
            {
                h += 26f; // container overhead
                h += (node is OutputNode) ? 256f : 108f;
            }

            if (node is ICustomEditorNode) h += 34f;

            h += 14f; // bottom padding
            return h;
        }

        private void ImproveLayerOrdering(List<List<string>> layers,
            Dictionary<string, List<string>> incoming,
            Dictionary<string, List<string>> outgoing)
        {
            if (layers.Count == 0)
                return;

            var rank = new Dictionary<string, float>();
            for (int i = 0; i < layers[0].Count; i++)
                rank[layers[0][i]] = i;

            for (int sweep = 0; sweep < 3; sweep++)
            {
                for (int l = 1; l < layers.Count; l++)
                {
                    var layer = layers[l];
                    layer.Sort((a, b) => CompareLayoutNodes(a, b, incoming, rank));
                    for (int i = 0; i < layer.Count; i++)
                        rank[layer[i]] = i;
                }

                for (int l = layers.Count - 2; l >= 0; l--)
                {
                    var layer = layers[l];
                    layer.Sort((a, b) => CompareLayoutNodes(a, b, outgoing, rank));
                    for (int i = 0; i < layer.Count; i++)
                        rank[layer[i]] = i;
                }
            }
        }

        private int CompareLayoutNodes(string a, string b,
            Dictionary<string, List<string>> adjacency,
            Dictionary<string, float> rank)
        {
            float ax = GetBarycenter(a, adjacency, rank);
            float bx = GetBarycenter(b, adjacency, rank);

            if (!Mathf.Approximately(ax, bx))
                return ax.CompareTo(bx);

            return string.CompareOrdinal(a, b);
        }

        private void FlattenLegacyRoutePoints()
        {
            if (_graphAsset == null)
                return;

            var routeNodes = _graphAsset.Nodes
                .OfType<RoutePointNode>()
                .Where(node => node != null)
                .ToList();

            if (routeNodes.Count == 0)
                return;

            foreach (var routeNode in routeNodes)
            {
                var incomingConnections = _graphAsset.Connections
                    .Where(conn => conn.TargetNodeId == routeNode.NodeId)
                    .ToList();

                var outgoingConnections = _graphAsset.Connections
                    .Where(conn => conn.SourceNodeId == routeNode.NodeId)
                    .ToList();

                foreach (var incomingConn in incomingConnections)
                {
                    foreach (var outgoingConn in outgoingConnections)
                    {
                        _graphAsset.AddConnection(
                            incomingConn.SourceNodeId,
                            incomingConn.SourcePortIndex,
                            outgoingConn.TargetNodeId,
                            outgoingConn.TargetPortIndex);
                    }
                }

                _graphAsset.RemoveNode(routeNode);
            }

            EditorUtility.SetDirty(_graphAsset);
        }


        private static float GetBarycenter(string nodeId,
            Dictionary<string, List<string>> incoming,
            Dictionary<string, float> rank)
        {
            if (!incoming.TryGetValue(nodeId, out var parents) || parents.Count == 0)
                return float.MaxValue;

            float sum = 0f;
            int count = 0;
            for (int i = 0; i < parents.Count; i++)
            {
                if (!rank.TryGetValue(parents[i], out var r)) continue;
                sum += r;
                count++;
            }

            return count > 0 ? sum / count : float.MaxValue;
        }

        #endregion
    }
}
