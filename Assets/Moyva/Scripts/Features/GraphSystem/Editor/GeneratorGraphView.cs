using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
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
        private const string NodePropertiesClipboardPrefix = "MOYVA_NODE_PROPERTIES_V1:";
        private const int NodeTooltipDelayMs = 450;

        [Serializable]
        private sealed class ClipboardNodePayload
        {
            public string nodeType;
            public string jsonData;
        }

        [Serializable]
        private sealed class ClipboardNodePropertiesPayload
        {
            public string nodeType;
            public string nodeTitle;
            public string compatibilityKey;
            public string jsonData;
            public List<ClipboardOwnedScriptableObjectPayload> ownedScriptableObjects = new();
        }

        [Serializable]
        private sealed class ClipboardOwnedScriptableObjectPayload
        {
            public string fieldName;
            public string objectType;
            public string objectName;
            public string jsonData;
        }

        [Serializable]
        private sealed class UnitySerializedNodeWrapper
        {
            public UnitySerializedNodeData MonoBehaviour = default;
        }

        [Serializable]
        private sealed class UnitySerializedNodeData
        {
            public string m_Name = default;
        }

        private GraphAsset _graphAsset;
        private readonly GraphEditorWindow _window;
        private NodeSearchProvider _searchProvider;
        private bool _isReadOnly;
        private MiniMap _miniMap;
        private bool _inlinePreviewsVisible = true;
        private string _activeLayerId;
        private readonly Dictionary<GeneratorNodeView, NodeBorderSnapshot> _edgeHoverNodeSnapshots = new();
        private readonly Dictionary<string, string> _edgeTooltipTexts = new();
        private Label _floatingTooltip;
        public event Action GraphChanged;
        public event Action CanvasBackgroundClicked;
        public event Action<string> StatusMessage;

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
            _miniMap.visible = false;
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
            RegisterCallback<MouseDownEvent>(OnBackgroundMouseDown);
        }

        private void OnBackgroundMouseDown(MouseDownEvent evt)
        {
            if (evt == null || evt.button != 0)
                return;

            var target = evt.target as VisualElement;
            if (target == null)
                return;

            if (target.GetFirstAncestorOfType<GeneratorNodeView>() != null)
                return;
            if (target.GetFirstAncestorOfType<Edge>() != null)
                return;
            if (target is Port || target.GetFirstAncestorOfType<Port>() != null)
                return;

            CanvasBackgroundClicked?.Invoke();
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
            {
                if (!visible)
                    view.ClearPreview("Inline preview disabled");
                view.SetPreviewVisible(visible);
            }
        }

        internal bool TryGetBestRawMaps(out float[,] floatMap, out string[,] tileMap)
        {
            floatMap = null;
            tileMap  = null;

            GeneratorNodeView source =
                selection.OfType<GeneratorNodeView>().FirstOrDefault(HasRawPreviewMaps)
                ?? nodes.OfType<GeneratorNodeView>().FirstOrDefault(v => v.NodeData is OutputNode && HasRawPreviewMaps(v))
                ?? nodes.OfType<GeneratorNodeView>().FirstOrDefault(HasRawPreviewMaps);

            if (source == null) return false;

            floatMap = source.PreviewFloatMap;
            tileMap  = source.PreviewTileMap;
            return floatMap != null || tileMap != null;
        }

        private static bool HasRawPreviewMaps(GeneratorNodeView view) =>
            view != null && (view.PreviewFloatMap != null || view.PreviewTileMap != null);

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

        internal int GetSelectedNodeCount()
        {
            return selection.OfType<GeneratorNodeView>().Count();
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
            int previewWidth = 128,
            int previewHeight = 128,
            bool heatmap = false,
            bool buildTextures = true)
        {
            var tileRegistry = settings?.TileRegistry;
            var views = nodes.OfType<GeneratorNodeView>().ToList();
            if (views.Count == 0)
                return;

            foreach (var view in views)
                view.ClearPreview(buildTextures ? "No preview" : "Inline preview disabled");

            RefreshConnectionIndexControls(result);

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

                var outputsAll = result.GetOutputs(node.NodeId);
                bool hidePreviewForNode = Attribute.IsDefined(node.GetType(), typeof(Kruty1918.Moyva.GraphSystem.API.HidePreviewAttribute));

                // ── OutputNode: composite preview ──
                if (node is OutputNode outputNode)
                {
                    var outputs = outputsAll;
                    string[,] biomeMap = null;
                    string[,] objectMap = null;
                    float[,] heightMap = null;
                    string[,] buildingMap = null;
                    bool[,] maskMap = null;

                    if (outputs != null)
                    {
                        if (outputs.Length > 0) biomeMap = outputs[0] as string[,];
                        if (outputs.Length > 1) objectMap = outputs[1] as string[,];
                        if (outputs.Length > 2) heightMap = outputs[2] as float[,];
                        if (outputs.Length > 3) buildingMap = outputs[3] as string[,];
                        if (outputs.Length > OutputNode.MaskInputIndex) maskMap = outputs[OutputNode.MaskInputIndex] as bool[,];
                    }
                    maskMap ??= ResolveOutputMaskPreviewFallback(outputNode, result);

                    if (buildTextures && !hidePreviewForNode)
                    {
                        Texture2D outputPreview = null;
                        bool ownsOutputPreview = false;
                        string previewStatus = null;
                        if (ShouldPreferOutputMaskPreview(outputNode, maskMap, biomeMap, objectMap, buildingMap))
                        {
                            outputPreview = NodePreviewTextureFactory.TryBuild(
                                new object[] { maskMap },
                                previewWidth,
                                previewHeight,
                                out ownsOutputPreview,
                                out previewStatus,
                                tileRegistry,
                                heatmap,
                                _graphAsset.SharedSettings);
                        }

                        if (outputPreview == null)
                        {
                            outputPreview = CompositePreviewBuilder.Build(
                                layerData, biomeMap, objectMap, heightMap, buildingMap,
                                tileRegistry,
                                settings?.MapObjectRegistry,
                                settings?.BuildingRegistry,
                                sharedSettings: _graphAsset.SharedSettings);
                            ownsOutputPreview = true;
                            previewStatus = "Composite preview";
                        }

                        nodeView.SetPreview(outputPreview, previewStatus, ownsOutputPreview);
                        nodeView.SetPreviewVisible(_inlinePreviewsVisible);
                    }
                    else
                    {
                        nodeView.ClearPreview(buildTextures ? "No preview" : "Inline preview disabled");
                        nodeView.SetPreviewVisible(false);
                    }


                    // update output labels
                    for (int i = 0; i < nodeView.OutputCount; i++)
                    {
                        string text = "-";
                        if (outputs != null && i < outputs.Length)
                        {
                            var v = outputs[i];
                            if (v == null) text = "-";
                            else if (v is int || v is float || v is string) text = v.ToString();
                            else if (v is Array) text = $"<{v.GetType().GetElementType()?.Name}[]>";
                            else text = $"<{v.GetType().Name}>";
                        }
                        nodeView.SetOutputValueText(i, text);
                    }

                    // update compact values under node
                    nodeView.SetOutputValues(outputs);
                    nodeView.SetPreviewRawMaps(heightMap, biomeMap ?? objectMap ?? buildingMap);

                    continue;
                }

                // ── Звичайні ноди ──
                Texture2D preview = null;
                bool ownsPreview = false;
                string status = null;

                if (buildTextures && !hidePreviewForNode)
                {
                    if (node is IPreviewableNode previewable)
                    {
                        preview = previewable.GeneratePreview(previewWidth, previewHeight);
                        if (preview != null)
                        {
                            status = "Node preview";
                            ownsPreview = true;
                        }
                    }

                    if (preview == null)
                    {
                        var previewOutputs = SelectPreferredPreviewOutputs(node, outputsAll);

                        preview = NodePreviewTextureFactory.TryBuild(
                            previewOutputs,
                            previewWidth,
                            previewHeight,
                            out ownsPreview,
                            out status,
                            tileRegistry,
                            heatmap,
                            _graphAsset.SharedSettings);
                    }

                    nodeView.SetPreview(preview, status, ownsPreview);
                    nodeView.SetPreviewVisible(_inlinePreviewsVisible);

                }
                else
                {
                    nodeView.ClearPreview(buildTextures ? "No preview" : "Inline preview disabled");
                    nodeView.SetPreviewVisible(false);
                }

                // Зберігаємо raw-дані без побудови Texture2D: Preview Window може показати значення при hover.
                float[,] rawFloat = null;
                string[,] rawTile = null;
                if (outputsAll != null)
                {
                    foreach (var o in outputsAll)
                    {
                        if (rawFloat == null && o is float[,] fm) rawFloat = fm;
                        if (rawTile == null && o is string[,] tm) rawTile = tm;
                    }
                }
                nodeView.SetPreviewRawMaps(rawFloat, rawTile);

                // update output labels
                for (int i = 0; i < nodeView.OutputCount; i++)
                {
                    string text = "-";
                    if (outputsAll != null && i < outputsAll.Length)
                    {
                        var v = outputsAll[i];
                        if (v == null) text = "-";
                        else if (v is int || v is float || v is string) text = v.ToString();
                        else if (v is Array) text = $"<{v.GetType().GetElementType()?.Name}[]>";
                        else text = $"<{v.GetType().Name}>";
                    }
                    nodeView.SetOutputValueText(i, text);
                }

                // update compact values under node
                nodeView.SetOutputValues(outputsAll);
            }
        }

        private static object[] SelectPreferredPreviewOutputs(NodeBase node, object[] outputs)
        {
            if (node == null || outputs == null || outputs.Length == 0)
                return outputs;

            // 1) Загальне правило: якщо є bool[,] вихід з назвою "*Mask*", показуємо його.
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

        private bool[,] ResolveOutputMaskPreviewFallback(OutputNode outputNode, GraphExecutionResult result)
        {
            if (_graphAsset?.Connections == null || outputNode == null || result == null)
                return null;

            var mask = ResolveConnectedMask(outputNode.NodeId, OutputNode.MaskInputIndex, result, allowAnyTargetPort: false);
            if (mask != null)
                return mask;

            if (outputNode.OutputKind != LayerOutputKind.Masks)
                return null;

            // Legacy safety: older "move to mask layer" connected bool masks to Output input 0.
            return ResolveConnectedMask(outputNode.NodeId, -1, result, allowAnyTargetPort: true);
        }

        private bool[,] ResolveConnectedMask(
            string targetNodeId,
            int targetPortIndex,
            GraphExecutionResult result,
            bool allowAnyTargetPort)
        {
            if (string.IsNullOrEmpty(targetNodeId) || result == null)
                return null;

            for (int i = 0; i < _graphAsset.Connections.Count; i++)
            {
                var connection = _graphAsset.Connections[i];
                if (connection == null || connection.TargetNodeId != targetNodeId)
                    continue;
                if (!allowAnyTargetPort && connection.TargetPortIndex != targetPortIndex)
                    continue;

                var outputs = result.GetOutputs(connection.SourceNodeId);
                if (outputs == null
                    || connection.SourcePortIndex < 0
                    || connection.SourcePortIndex >= outputs.Length
                    || outputs[connection.SourcePortIndex] is not bool[,] mask)
                {
                    continue;
                }

                return mask;
            }

            return null;
        }

        private static bool ShouldPreferOutputMaskPreview(
            OutputNode outputNode,
            bool[,] maskMap,
            string[,] biomeMap,
            string[,] objectMap,
            string[,] buildingMap)
        {
            if (outputNode == null || maskMap == null)
                return false;

            if (outputNode.OutputKind == LayerOutputKind.Masks)
                return true;

            return !HasAnyStringValue(biomeMap)
                && !HasAnyStringValue(objectMap)
                && !HasAnyStringValue(buildingMap);
        }

        private static bool HasAnyStringValue(string[,] map)
        {
            if (map == null)
                return false;

            int width = map.GetLength(0);
            int height = map.GetLength(1);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!string.IsNullOrEmpty(map[x, y]))
                    return true;
            }

            return false;
        }

        public bool CopyNodeAsText(GeneratorNodeView nodeView)
        {
            if (nodeView == null || nodeView.NodeData == null)
                return false;

            var payload = new ClipboardNodePayload
            {
                nodeType = nodeView.NodeData.GetType().AssemblyQualifiedName,
                jsonData = SanitizeSerializedNodeJsonForClipboard(EditorJsonUtility.ToJson(nodeView.NodeData))
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

            var node = _graphAsset.AddNode(nodeType, false, ResolveActiveLayerId());
            if (node == null)
            {
                error = "Не вдалося створити ноду цього типу.";
                return false;
            }

            if (!TryOverwriteSerializedNodeData(payload.jsonData, node, out string pasteError))
            {
                _graphAsset.RemoveNode(node);
                error = $"Не вдалося вставити ноду з тексту: {pasteError}";
                return false;
            }

            node.NodeId = Guid.NewGuid().ToString();
            if (!GraphStaticNodeUtility.IsStaticGraphNode(node))
                node.LayerId = ResolveActiveLayerId();
            ClearPastedNodeLayerReferences(node);
            node.EditorPosition = graphPosition;

            var view = new GeneratorNodeView(node);
            AddElement(view);

            ClearSelection();
            AddToSelection(view);

            EditorUtility.SetDirty(_graphAsset);
            _graphAsset.NormalizeGraphIds();
            _graphAsset.EnsureLayerGraphStates();
            GraphChanged?.Invoke();
            return true;
        }

        private static string SanitizeSerializedNodeJsonForClipboard(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            // NodeId/LayerId are internal graph bookkeeping. They must never be trusted from text clipboard data.
            json = Regex.Replace(json, "\\\"_nodeId\\\"\\s*:\\s*\\\"[^\\\"]*\\\"", "\\\"_nodeId\\\":\\\"\\\"");
            json = Regex.Replace(json, "\\\"_layerId\\\"\\s*:\\s*\\\"[^\\\"]*\\\"", "\\\"_layerId\\\":\\\"\\\"");
            json = Regex.Replace(json, "\\\"_targetGraphLayerId\\\"\\s*:\\s*\\\"[^\\\"]*\\\"", "\\\"_targetGraphLayerId\\\":\\\"\\\"");
            return json;
        }

        public bool CopyNodeProperties(GeneratorNodeView nodeView)
        {
            if (nodeView == null || nodeView.NodeData == null)
                return false;

            var payload = new ClipboardNodePropertiesPayload
            {
                nodeType = nodeView.NodeData.GetType().AssemblyQualifiedName,
                nodeTitle = nodeView.NodeData.Title,
                compatibilityKey = BuildNodePropertiesCompatibilityKey(nodeView.NodeData),
                jsonData = EditorJsonUtility.ToJson(nodeView.NodeData),
                ownedScriptableObjects = CollectOwnedScriptableObjectPayloads(nodeView.NodeData)
            };

            string json = JsonUtility.ToJson(payload);
            EditorGUIUtility.systemCopyBuffer = NodePropertiesClipboardPrefix + json;
            StatusMessage?.Invoke($"Скопійовано властивості ноди '{nodeView.NodeData.Title}'.");
            return true;
        }

        public bool CanPasteNodeProperties(GeneratorNodeView targetView)
        {
            if (_isReadOnly || targetView == null || targetView.NodeData == null)
                return false;

            return TryReadNodePropertiesClipboard(out var payload, out _)
                && AreNodePropertiesCompatible(payload, targetView.NodeData, out _);
        }

        public bool PasteNodeProperties(GeneratorNodeView targetView, out string error)
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

            if (targetView == null || targetView.NodeData == null)
            {
                error = "Цільову ноду не знайдено.";
                return false;
            }

            if (!TryReadNodePropertiesClipboard(out var payload, out error))
                return false;

            var targetNode = targetView.NodeData;
            if (!AreNodePropertiesCompatible(payload, targetNode, out error))
                return false;

            EnsureOwnedScriptableObjectFieldsReady(targetNode);
            var preservedOwnedObjects = CaptureOwnedScriptableObjects(targetNode);

            string keepNodeId = targetNode.NodeId;
            string keepLayerId = targetNode.LayerId;
            Vector2 keepPosition = targetNode.EditorPosition;
            string keepName = targetNode.name;
            HideFlags keepHideFlags = targetNode.hideFlags;

            Undo.RecordObject(targetNode, "Paste Node Properties");
            foreach (var ownedObject in preservedOwnedObjects.Values)
            {
                if (ownedObject != null)
                    Undo.RecordObject(ownedObject, "Paste Node Properties");
            }

            EditorJsonUtility.FromJsonOverwrite(payload.jsonData, targetNode);
            AssignSerializedSOReferencesFromJson(targetNode, payload.jsonData);

            targetNode.NodeId = keepNodeId;
            targetNode.LayerId = keepLayerId;
            targetNode.EditorPosition = keepPosition;
            targetNode.name = keepName;
            targetNode.hideFlags = keepHideFlags;

            RestoreOwnedScriptableObjects(targetNode, payload.ownedScriptableObjects, preservedOwnedObjects);

            if (targetNode is TwcModifierNode twcNode)
                twcNode.TryRestoreModifierInEditor();

            targetView.MarkDirtyRepaint();
            EditorUtility.SetDirty(targetNode);
            EditorUtility.SetDirty(_graphAsset);
            _graphAsset.EnsureLayerGraphStates();

            StatusMessage?.Invoke($"Вставлено властивості в ноду '{targetNode.Title}'.");
            GraphChanged?.Invoke();
            return true;
        }

        private static bool TryReadNodePropertiesClipboard(out ClipboardNodePropertiesPayload payload, out string error)
        {
            payload = null;
            error = null;

            string raw = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith(NodePropertiesClipboardPrefix, StringComparison.Ordinal))
            {
                error = "У буфері немає властивостей ноди Moyva.";
                return false;
            }

            string payloadJson = raw.Substring(NodePropertiesClipboardPrefix.Length);
            try
            {
                payload = JsonUtility.FromJson<ClipboardNodePropertiesPayload>(payloadJson);
            }
            catch (Exception ex)
            {
                error = $"Помилка читання властивостей ноди: {ex.Message}";
                return false;
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.nodeType) || string.IsNullOrWhiteSpace(payload.jsonData))
            {
                error = "Дані властивостей ноди пошкоджені або неповні.";
                return false;
            }

            payload.ownedScriptableObjects ??= new List<ClipboardOwnedScriptableObjectPayload>();
            return true;
        }

        private static bool AreNodePropertiesCompatible(
            ClipboardNodePropertiesPayload payload,
            NodeBase targetNode,
            out string error)
        {
            error = null;

            if (payload == null || targetNode == null)
            {
                error = "Немає даних для вставки властивостей.";
                return false;
            }

            Type sourceType = Type.GetType(payload.nodeType);
            if (sourceType == null)
            {
                error = $"Тип ноди не знайдено: {payload.nodeType}";
                return false;
            }

            if (sourceType != targetNode.GetType())
            {
                error = $"Властивості скопійовано з '{FormatClipboardNodeTitle(payload)}', а цільова нода має тип '{targetNode.Title}'. Вставка дозволена лише між нодами одного типу.";
                return false;
            }

            string sourceCompatibilityKey = string.IsNullOrWhiteSpace(payload.compatibilityKey)
                ? payload.nodeType
                : payload.compatibilityKey;
            string targetCompatibilityKey = BuildNodePropertiesCompatibilityKey(targetNode);

            if (!string.Equals(sourceCompatibilityKey, targetCompatibilityKey, StringComparison.Ordinal))
            {
                error = $"Властивості скопійовано з '{FormatClipboardNodeTitle(payload)}', а цільова нода має тип '{targetNode.Title}'. Вставка дозволена лише між однаковими нодами.";
                return false;
            }

            return true;
        }

        private static string FormatClipboardNodeTitle(ClipboardNodePropertiesPayload payload)
        {
            if (!string.IsNullOrWhiteSpace(payload?.nodeTitle))
                return payload.nodeTitle;

            if (!string.IsNullOrWhiteSpace(payload?.nodeType))
                return payload.nodeType;

            return "невідомої ноди";
        }

        private static string BuildNodePropertiesCompatibilityKey(NodeBase node)
        {
            if (node == null)
                return string.Empty;

            string typeName = node.GetType().AssemblyQualifiedName;
            if (node is TwcModifierNode twcNode)
                return $"{typeName}|twc:{twcNode.ModifierTypeName}";

            return typeName;
        }

        private static List<ClipboardOwnedScriptableObjectPayload> CollectOwnedScriptableObjectPayloads(NodeBase node)
        {
            var payloads = new List<ClipboardOwnedScriptableObjectPayload>();
            if (node == null)
                return payloads;

            foreach (var field in node.GetType().GetFields(SOFieldFlags))
            {
                if (!IsSerializedSOField(field))
                    continue;

                var so = field.GetValue(node) as ScriptableObject;
                if (!IsOwnedNodeSubAsset(node, so))
                    continue;

                payloads.Add(new ClipboardOwnedScriptableObjectPayload
                {
                    fieldName = field.Name,
                    objectType = so.GetType().AssemblyQualifiedName,
                    objectName = so.name,
                    jsonData = EditorJsonUtility.ToJson(so)
                });
            }

            return payloads;
        }

        private static void EnsureOwnedScriptableObjectFieldsReady(NodeBase node)
        {
            if (node is TwcModifierNode twcNode)
                twcNode.TryRestoreModifierInEditor();
        }

        private static Dictionary<string, ScriptableObject> CaptureOwnedScriptableObjects(NodeBase node)
        {
            var result = new Dictionary<string, ScriptableObject>();
            if (node == null)
                return result;

            foreach (var field in node.GetType().GetFields(SOFieldFlags))
            {
                if (!IsSerializedSOField(field))
                    continue;

                var so = field.GetValue(node) as ScriptableObject;
                if (IsOwnedNodeSubAsset(node, so))
                    result[field.Name] = so;
            }

            return result;
        }

        private static void RestoreOwnedScriptableObjects(
            NodeBase targetNode,
            List<ClipboardOwnedScriptableObjectPayload> payloads,
            Dictionary<string, ScriptableObject> preservedOwnedObjects)
        {
            if (targetNode == null)
                return;

            payloads ??= new List<ClipboardOwnedScriptableObjectPayload>();
            preservedOwnedObjects ??= new Dictionary<string, ScriptableObject>();

            var appliedFields = new HashSet<string>();

            foreach (var payload in payloads)
            {
                if (payload == null || string.IsNullOrWhiteSpace(payload.fieldName) || string.IsNullOrWhiteSpace(payload.objectType))
                    continue;

                var field = targetNode.GetType().GetField(payload.fieldName, SOFieldFlags);
                if (field == null || !IsSerializedSOField(field))
                    continue;

                Type objectType = Type.GetType(payload.objectType);
                if (objectType == null || !typeof(ScriptableObject).IsAssignableFrom(objectType))
                    continue;
                if (!field.FieldType.IsAssignableFrom(objectType))
                    continue;

                if (!preservedOwnedObjects.TryGetValue(field.Name, out var targetObject)
                    || targetObject == null
                    || !objectType.IsInstanceOfType(targetObject))
                {
                    targetObject = CreateOwnedScriptableObject(targetNode, objectType, payload.objectName);
                    if (targetObject == null)
                        continue;
                }

                string keepName = string.IsNullOrWhiteSpace(targetObject.name) ? payload.objectName : targetObject.name;
                HideFlags keepHideFlags = targetObject.hideFlags;

                Undo.RecordObject(targetObject, "Paste Node Properties");
                EditorJsonUtility.FromJsonOverwrite(payload.jsonData, targetObject);
                if (!string.IsNullOrWhiteSpace(keepName))
                    targetObject.name = keepName;
                targetObject.hideFlags = keepHideFlags;

                field.SetValue(targetNode, targetObject);
                EditorUtility.SetDirty(targetObject);
                appliedFields.Add(field.Name);
            }

            foreach (var kvp in preservedOwnedObjects)
            {
                if (appliedFields.Contains(kvp.Key))
                    continue;

                var field = targetNode.GetType().GetField(kvp.Key, SOFieldFlags);
                if (field != null && IsSerializedSOField(field))
                    field.SetValue(targetNode, kvp.Value);
            }
        }

        private static ScriptableObject CreateOwnedScriptableObject(NodeBase owner, Type objectType, string objectName)
        {
            if (owner == null || objectType == null || !typeof(ScriptableObject).IsAssignableFrom(objectType))
                return null;

            var so = ScriptableObject.CreateInstance(objectType);
            if (so == null)
                return null;

            so.name = string.IsNullOrWhiteSpace(objectName) ? objectType.Name : objectName;
            so.hideFlags = HideFlags.HideInHierarchy;

            string ownerPath = AssetDatabase.GetAssetPath(owner);
            if (!string.IsNullOrEmpty(ownerPath))
            {
                AssetDatabase.AddObjectToAsset(so, owner);
                Undo.RegisterCreatedObjectUndo(so, "Paste Node Properties");
            }

            return so;
        }

        private static bool IsOwnedNodeSubAsset(NodeBase owner, ScriptableObject so)
        {
            if (owner == null || so == null)
                return false;

            string ownerPath = AssetDatabase.GetAssetPath(owner);
            string objectPath = AssetDatabase.GetAssetPath(so);
            return !string.IsNullOrEmpty(ownerPath)
                && string.Equals(ownerPath, objectPath, StringComparison.Ordinal)
                && AssetDatabase.IsSubAsset(so);
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

            var group = new UnityEditor.Experimental.GraphView.Group { title = "Node Group" };
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

            var outputPort = a.direction == Direction.Output ? a : b;
            var inputPort = a.direction == Direction.Input ? a : b;

            return PortDefinition.AreValueTypesCompatible(outputPort.PortValueType, inputPort.PortValueType);
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

            GraphStaticNodeUtility.EnsureStaticNodes(asset);
            _graphAsset.EnsureLayerGraphStates();
            _graphAsset.RepairMissingNodeConnections();
            _graphAsset.RemoveNullNodes();
            FlattenLegacyRoutePoints();

            if (string.IsNullOrEmpty(_activeLayerId)
                || _graphAsset.GetLayerById(_activeLayerId) == null)
                _activeLayerId = _graphAsset.EnsureDefaultLayer();

            var scope = _graphAsset.CreateExecutionScope(_activeLayerId);

            // Create node views
            var nodeViews = new Dictionary<string, GeneratorNodeView>();
            foreach (var node in scope.Nodes)
            {
                if (node == null) continue;
                var view = new GeneratorNodeView(node);
                AddElement(view);
                SetupNodeVisuals(view);
                view.SetPreviewVisible(_inlinePreviewsVisible);
                nodeViews[node.NodeId] = view;
            }

            // Create edges
            foreach (var conn in scope.Connections)
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

            RefreshConnectionIndexControls();

            BringAllNodesToFront();

            UpdateNodePreviews(null);
        }

        public NodeBase CreateNode(Type nodeType, Vector2 position)
        {
            if (_graphAsset == null || _isReadOnly) return null;

            Undo.RecordObject(_graphAsset, "Add Node");
            var node = _graphAsset.AddNode(nodeType, false, ResolveActiveLayerId());
            if (node == null) return null;

            node.EditorPosition = position;

            var view = new GeneratorNodeView(node);
            AddElement(view);
            SetupNodeVisuals(view);
            view.SetPreviewVisible(_inlinePreviewsVisible);

            EditorUtility.SetDirty(_graphAsset);
            GraphChanged?.Invoke();
            return node;
        }

        public bool FocusNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return false;

            var nodeView = nodes
                .OfType<GeneratorNodeView>()
                .FirstOrDefault(view => view.NodeData != null && view.NodeData.NodeId == nodeId);
            if (nodeView == null)
                return false;

            ClearSelection();
            AddToSelection(nodeView);
            FrameSelection();
            return true;
        }

        public bool SelectNodeById(string nodeId, bool frameSelection = false)
        {
            if (string.IsNullOrEmpty(nodeId))
                return false;

            var nodeView = nodes
                .OfType<GeneratorNodeView>()
                .FirstOrDefault(view => view.NodeData != null && view.NodeData.NodeId == nodeId);
            if (nodeView == null)
                return false;

            ClearSelection();
            AddToSelection(nodeView);
            if (frameSelection)
                FrameSelection();
            return true;
        }

        /// <summary>
        /// Активний шар графа. Нові вузли призначаються цьому шару.
        /// </summary>
        public string ActiveLayerId
        {
            get => _activeLayerId;
            set
            {
                if (_activeLayerId == value)
                    return;

                _activeLayerId = value;
                if (_graphAsset != null)
                {
                    if (string.IsNullOrEmpty(_activeLayerId)
                        || _graphAsset.GetLayerById(_activeLayerId) == null)
                        _activeLayerId = _graphAsset.EnsureDefaultLayer();
                    PopulateGraph(_graphAsset, _isReadOnly);
                }
            }
        }

        public void SetActiveLayerWithoutRefresh(string layerId)
        {
            _activeLayerId = layerId;
        }

        /// <summary>
        /// Показує лише вузли активного шару (та глобальні вузли без шару),
        /// решту приховує. Ребра між прихованими вузлами теж приховуються.
        /// </summary>
        public void SetVisibleLayer(string layerId)
        {
            ActiveLayerId = layerId;
        }

        private string ResolveActiveLayerId()
        {
            if (_graphAsset == null)
                return _activeLayerId;

            if (!string.IsNullOrEmpty(_activeLayerId)
                && _graphAsset.GetLayerById(_activeLayerId) != null)
                return _activeLayerId;

            _activeLayerId = _graphAsset.EnsureDefaultLayer();
            return _activeLayerId;
        }

        public IReadOnlyList<NodeBase> GetSelectedNodesForActiveLayer()
        {
            string layerId = ResolveActiveLayerId();
            return selection
                .OfType<GeneratorNodeView>()
                .Select(view => view.NodeData)
                .Where(node => IsNodeInLayerEditScope(node, layerId, false))
                .ToList();
        }

        private bool IsNodeInActiveEditScope(NodeBase node, bool includeGlobal = false) =>
            IsNodeInLayerEditScope(node, ResolveActiveLayerId(), includeGlobal);

        private static bool IsNodeInLayerEditScope(NodeBase node, string layerId, bool includeGlobal)
        {
            if (node == null)
                return false;

            if (GraphStaticNodeUtility.IsStaticGraphNode(node))
                return includeGlobal;

            return !string.IsNullOrEmpty(layerId) && node.LayerId == layerId;
        }

        private int CountForeignVisibleNodes()
        {
            string layerId = ResolveActiveLayerId();
            int count = 0;
            foreach (var view in nodes.OfType<GeneratorNodeView>())
            {
                var node = view.NodeData;
                if (node == null || GraphStaticNodeUtility.IsStaticGraphNode(node))
                    continue;
                if (node.LayerId != layerId)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Створює вузол-обгортку TileWorldCreator-модифікатора заданого типу
        /// та призначає його активному шару.
        /// </summary>
        public void CreateTwcModifierNode(Type modifierType, Vector2 position)
        {
            if (_graphAsset == null || _isReadOnly || modifierType == null) return;

            Undo.RecordObject(_graphAsset, "Add TWC Node");
            var node = _graphAsset.AddNode(typeof(TwcModifierNode), false, ResolveActiveLayerId()) as TwcModifierNode;
            if (node == null) return;

            node.ConfigureModifier(modifierType);
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

            change.elementsToRemove?.RemoveAll(IsProtectedStaticElement);
            change.movedElements?.RemoveAll(IsProtectedStaticElement);

            // Handle removed elements
            if (change.elementsToRemove != null)
            {
                Undo.RecordObject(_graphAsset, "Remove Graph Elements");

                var nodeViewsToRemove = change.elementsToRemove
                    .OfType<GeneratorNodeView>()
                    .Where(view => view?.NodeData != null && !GraphStaticNodeUtility.IsStaticGraphNode(view.NodeData))
                    .ToList();
                var nodeIdsToRemove = new HashSet<string>(
                    nodeViewsToRemove
                        .Select(view => view.NodeData.NodeId)
                        .Where(id => !string.IsNullOrEmpty(id)));

                if (nodeIdsToRemove.Count > 0)
                {
                    var connectedEdges = edges
                        .Where(edge => IsEdgeConnectedToAny(edge, nodeIdsToRemove))
                        .Cast<GraphElement>()
                        .ToList();
                    foreach (var edge in connectedEdges)
                    {
                        if (!change.elementsToRemove.Contains(edge))
                            change.elementsToRemove.Add(edge);
                    }
                }

                foreach (var edge in change.elementsToRemove.OfType<Edge>().ToList())
                {
                    if (!IsEdgeConnectedToAny(edge, nodeIdsToRemove))
                        RemoveEdgeFromAsset(edge);
                    _edgeTooltipTexts.Remove(GetEdgeKey(edge));
                }

                if (nodeViewsToRemove.Count > 0)
                    _graphAsset.RemoveNodesCascade(nodeViewsToRemove.Select(view => view.NodeData), true);

                EditorUtility.SetDirty(_graphAsset);
                if (change.elementsToRemove.Count > 0)
                    GraphChanged?.Invoke();
            }

            // Handle new edges
            if (change.edgesToCreate != null)
            {
                Undo.RecordObject(_graphAsset, "Connect Nodes");
                int ignoredEdges = 0;
                for (int i = change.edgesToCreate.Count - 1; i >= 0; i--)
                {
                    var edge = change.edgesToCreate[i];
                    var sourceView = edge.output?.node as GeneratorNodeView;
                    var targetView = edge.input?.node as GeneratorNodeView;
                    if (sourceView == null || targetView == null
                        || CanCreateEdgeInActiveScope(sourceView.NodeData, targetView.NodeData))
                        continue;

                    change.edgesToCreate.RemoveAt(i);
                    ignoredEdges++;
                }

                if (ignoredEdges > 0)
                {
                    string message = $"Ignored {ignoredEdges} connection(s) outside the active layer graph scope.";
                    Debug.LogWarning("[Moyva Graph] " + message);
                    StatusMessage?.Invoke(message);
                }

                foreach (var edge in change.edgesToCreate)
                {
                    var sourceView = edge.output?.node as GeneratorNodeView;
                    var targetView = edge.input?.node as GeneratorNodeView;
                    if (sourceView == null || targetView == null) continue;

                    var sourcePort = edge.output as GeneratorPort;
                    var targetPort = edge.input as GeneratorPort;
                    if (sourcePort == null || targetPort == null) continue;

                    var connection = _graphAsset.AddConnection(
                        sourceView.NodeData.NodeId, sourcePort.PortIndex,
                        targetView.NodeData.NodeId, targetPort.PortIndex);

                    SetupEdgeVisuals(edge, sourcePort, targetPort);
                    RefreshConnectionIndexControl(connection);
                }
                EditorUtility.SetDirty(_graphAsset);
                BringAllNodesToFront();
                RefreshConnectionIndexControls();
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
                        if (!IsNodeInActiveEditScope(nodeView.NodeData, false))
                            continue;

                        var rect = nodeView.GetPosition();
                        nodeView.NodeData.EditorPosition =
                            new Vector2(rect.x, rect.y);
                    }
                }
                EditorUtility.SetDirty(_graphAsset);
                if (change.movedElements.Count > 0)
                    StatusMessage?.Invoke("Graph layout updated.");
            }

            return change;
        }

        private static bool IsProtectedStaticElement(GraphElement element) =>
            element is GeneratorNodeView nodeView && GraphStaticNodeUtility.IsStaticGraphNode(nodeView.NodeData);

        private bool CanCreateEdgeInActiveScope(NodeBase source, NodeBase target)
        {
            string layerId = ResolveActiveLayerId();
            bool sourceOk = IsNodeInLayerEditScope(source, layerId, true);
            bool targetOk = IsNodeInLayerEditScope(target, layerId, true);
            if (!sourceOk || !targetOk)
                return false;

            if (GraphStaticNodeUtility.IsStaticGraphNode(source) || GraphStaticNodeUtility.IsStaticGraphNode(target))
                return true;

            return source.LayerId == target.LayerId;
        }

        private static bool IsEdgeConnectedToAny(Edge edge, HashSet<string> nodeIds)
        {
            if (edge == null || nodeIds == null || nodeIds.Count == 0)
                return false;

            var sourceView = edge.output?.node as GeneratorNodeView;
            var targetView = edge.input?.node as GeneratorNodeView;
            string sourceId = sourceView?.NodeData?.NodeId;
            string targetId = targetView?.NodeData?.NodeId;
            return !string.IsNullOrEmpty(sourceId) && nodeIds.Contains(sourceId)
                || !string.IsNullOrEmpty(targetId) && nodeIds.Contains(targetId);
        }

        private void RemoveEdgeFromAsset(Edge edge)
        {
            var sourceView = edge.output?.node as GeneratorNodeView;
            var targetView = edge.input?.node as GeneratorNodeView;
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

        private void RefreshConnectionIndexControls(GraphExecutionResult result = null)
        {
            foreach (var nodeView in nodes.OfType<GeneratorNodeView>())
                nodeView.ClearInputElementIndexControls();

            if (_graphAsset == null)
                return;

            foreach (var connection in _graphAsset.GetConnectionsForLayer(ResolveActiveLayerId()))
                RefreshConnectionIndexControl(connection, result);
        }

        private void RefreshConnectionIndexControl(Connection connection, GraphExecutionResult result = null)
        {
            if (connection == null || _graphAsset == null)
                return;

            var source = _graphAsset.GetNodeById(connection.SourceNodeId);
            var target = _graphAsset.GetNodeById(connection.TargetNodeId);
            if (source == null || target == null)
                return;
            if (connection.SourcePortIndex < 0 || connection.SourcePortIndex >= source.Outputs.Length)
                return;
            if (connection.TargetPortIndex < 0 || connection.TargetPortIndex >= target.Inputs.Length)
                return;

            var sourcePortDef = source.Outputs[connection.SourcePortIndex];
            var targetPortDef = target.Inputs[connection.TargetPortIndex];
            if (!PortDefinition.RequiresElementIndexing(sourcePortDef.ValueType, targetPortDef.ValueType))
                return;

            int elementCount = GetRuntimeElementCount(result, connection);
            var targetView = nodes.OfType<GeneratorNodeView>()
                .FirstOrDefault(view => view.NodeData != null && view.NodeData.NodeId == connection.TargetNodeId);
            var inputPort = targetView?.GetInputPort(connection.TargetPortIndex);
            if (inputPort == null)
                return;

            inputPort.SetElementIndexControl(connection.SourceElementIndex, elementCount, newIndex =>
            {
                Undo.RecordObject(_graphAsset, "Set Connection Element Index");
                connection.SetSourceElementIndex(newIndex);
                EditorUtility.SetDirty(_graphAsset);
                RefreshConnectionIndexControl(connection, result);
                GraphChanged?.Invoke();
            });
        }

        private static int GetRuntimeElementCount(GraphExecutionResult result, Connection connection)
        {
            var outputs = result?.GetOutputs(connection.SourceNodeId);
            if (outputs == null || connection.SourcePortIndex < 0 || connection.SourcePortIndex >= outputs.Length)
                return 0;

            return PortDefinition.TryGetIndexableCount(outputs[connection.SourcePortIndex], out int count)
                ? count
                : 0;
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
            public string OriginalNodeId;
            public Type NodeType;
            public Vector2 Position;
            public string JsonData;
        }

        private struct CopiedConnectionData
        {
            public string SourceNodeId;
            public int SourcePortIndex;
            public string TargetNodeId;
            public int TargetPortIndex;
            public int SourceElementIndex;
        }

        private static readonly List<CopiedConnectionData> _connectionCopyBuffer = new();

        private void CopySelection()
        {
            _copyBuffer.Clear();
            _connectionCopyBuffer.Clear();
            if (_graphAsset == null)
                return;

            string layerId = ResolveActiveLayerId();
            foreach (var element in selection)
            {
                if (element is GeneratorNodeView nodeView)
                {
                    if (GraphStaticNodeUtility.IsStaticGraphNode(nodeView.NodeData))
                        continue;
                    if (!IsNodeInLayerEditScope(nodeView.NodeData, layerId, false))
                        continue;

                    var rect = nodeView.GetPosition();
                    _copyBuffer.Add(new CopiedNodeData
                    {
                        OriginalNodeId = nodeView.NodeData.NodeId,
                        NodeType = nodeView.NodeData.GetType(),
                        Position = new Vector2(rect.x, rect.y),
                        JsonData = EditorJsonUtility.ToJson(nodeView.NodeData)
                    });
                }
            }

            var selectedIds = new HashSet<string>(_copyBuffer
                .Select(data => data.OriginalNodeId)
                .Where(id => !string.IsNullOrEmpty(id)));
            if (selectedIds.Count == 0)
                return;

            foreach (var connection in _graphAsset.Connections)
            {
                if (connection == null)
                    continue;
                if (!selectedIds.Contains(connection.SourceNodeId)
                    || !selectedIds.Contains(connection.TargetNodeId))
                    continue;

                _connectionCopyBuffer.Add(new CopiedConnectionData
                {
                    SourceNodeId = connection.SourceNodeId,
                    SourcePortIndex = connection.SourcePortIndex,
                    TargetNodeId = connection.TargetNodeId,
                    TargetPortIndex = connection.TargetPortIndex,
                    SourceElementIndex = connection.SourceElementIndex
                });
            }
        }

        private void PasteNodes()
        {
            if (_graphAsset == null || _copyBuffer.Count == 0) return;

            Undo.RecordObject(_graphAsset, "Paste Nodes");
            ClearSelection();

            Vector2 offset = new Vector2(40, 40);
            var newViews = new List<GeneratorNodeView>();
            var idMap = new Dictionary<string, string>();
            string targetLayerId = ResolveActiveLayerId();
            var existingOutput = _graphAsset.GetNodesForLayer(targetLayerId)
                .OfType<OutputNode>()
                .FirstOrDefault();
            int failedNodes = 0;
            string firstPasteError = null;

            foreach (var data in _copyBuffer)
            {
                string oldId = data.OriginalNodeId;
                if (data.NodeType == typeof(OutputNode) && existingOutput != null)
                {
                    if (!string.IsNullOrEmpty(oldId))
                        idMap[oldId] = existingOutput.NodeId;
                    continue;
                }

                var node = _graphAsset.AddNode(data.NodeType, false, targetLayerId);
                if (node == null) continue;

                // Apply serialized data (preserving field values)
                if (!TryOverwriteSerializedNodeData(data.JsonData, node, out string pasteError))
                {
                    failedNodes++;
                    firstPasteError ??= pasteError;
                    _graphAsset.RemoveNode(node);
                    continue;
                }

                // Reset ID so it's unique
                node.NodeId = Guid.NewGuid().ToString();
                node.LayerId = targetLayerId;
                ClearPastedNodeLayerReferences(node);
                node.EditorPosition = data.Position + offset;
                if (!string.IsNullOrEmpty(oldId))
                    idMap[oldId] = node.NodeId;

                var view = new GeneratorNodeView(node);
                AddElement(view);
                SetupNodeVisuals(view);
                view.SetPreviewVisible(_inlinePreviewsVisible);
                AddToSelection(view);
                newViews.Add(view);
            }

            if (newViews.Count == 0)
            {
                if (failedNodes > 0)
                {
                    string message = $"Не вдалося вставити ноди: {firstPasteError}";
                    Debug.LogWarning($"[GeneratorGraphView] {message}");
                    StatusMessage?.Invoke(message);
                }

                return;
            }

            var copiedIds = new HashSet<string>(idMap.Keys);
            foreach (var connection in _connectionCopyBuffer)
            {
                if (!copiedIds.Contains(connection.SourceNodeId) || !copiedIds.Contains(connection.TargetNodeId))
                    continue;
                if (!idMap.TryGetValue(connection.SourceNodeId, out string newSourceId))
                    continue;
                if (!idMap.TryGetValue(connection.TargetNodeId, out string newTargetId))
                    continue;

                var newConnection = _graphAsset.AddConnection(
                    newSourceId,
                    connection.SourcePortIndex,
                    newTargetId,
                    connection.TargetPortIndex,
                    connection.SourceElementIndex);

                var sourceView = FindNodeViewById(newSourceId);
                var targetView = FindNodeViewById(newTargetId);
                var outputPort = sourceView?.GetOutputPort(newConnection.SourcePortIndex);
                var inputPort = targetView?.GetInputPort(newConnection.TargetPortIndex);
                if (outputPort != null && inputPort != null)
                    AddElement(CreateStyledEdge(outputPort, inputPort));
            }

            EditorUtility.SetDirty(_graphAsset);
            _graphAsset.NormalizeGraphIds();
            _graphAsset.RemoveInvalidConnections();
            _graphAsset.EnsureLayerGraphStates();
            TryConnectPastedTerminalToLayerOutput(targetLayerId, newViews);

            if (failedNodes > 0)
            {
                string message = $"Вставлено {newViews.Count} нод, пропущено {failedNodes}: {firstPasteError}";
                Debug.LogWarning($"[GeneratorGraphView] {message}");
                StatusMessage?.Invoke(message);
            }
            else
            {
                StatusMessage?.Invoke($"Вставлено {newViews.Count} нод.");
            }

            GraphChanged?.Invoke();
        }

        private static bool TryOverwriteSerializedNodeData(string jsonData, NodeBase node, out string error)
        {
            error = null;
            if (node == null || string.IsNullOrWhiteSpace(jsonData))
                return true;

            if (TryOverwriteSerializedNodeDataRaw(jsonData, node, out error))
                return true;

            string sanitized = SanitizeSerializedNodeJsonForClipboard(jsonData);
            if (!string.Equals(sanitized, jsonData, StringComparison.Ordinal)
                && TryOverwriteSerializedNodeDataRaw(sanitized, node, out error))
            {
                return true;
            }

            error = string.IsNullOrWhiteSpace(error)
                ? "serialized node JSON is invalid."
                : error;
            return false;
        }

        private static bool TryOverwriteSerializedNodeDataRaw(string jsonData, NodeBase node, out string error)
        {
            error = null;
            try
            {
                EditorJsonUtility.FromJsonOverwrite(jsonData, node);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static void ClearPastedNodeLayerReferences(NodeBase node)
        {
            if (node == null)
                return;

            var serialized = new SerializedObject(node);
            var targetLayerProperty = serialized.FindProperty("_targetGraphLayerId");
            if (targetLayerProperty == null || targetLayerProperty.propertyType != SerializedPropertyType.String)
                return;

            targetLayerProperty.stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private void TryConnectPastedTerminalToLayerOutput(string targetLayerId, IReadOnlyList<GeneratorNodeView> newViews)
        {
            if (_graphAsset == null || string.IsNullOrEmpty(targetLayerId) || newViews == null || newViews.Count == 0)
                return;

            var output = _graphAsset.GetNodesForLayer(targetLayerId).OfType<OutputNode>().FirstOrDefault();
            var layerConnections = _graphAsset.GetConnectionsForLayer(targetLayerId, false);
            if (output != null && layerConnections.Any(connection => connection != null && connection.TargetNodeId == output.NodeId))
                return;

            var pastedNodes = newViews
                .Select(view => view?.NodeData)
                .Where(node => node != null && node is not OutputNode)
                .ToList();
            var pastedIds = new HashSet<string>(pastedNodes.Select(node => node.NodeId));

            NodeBase bestSource = null;
            int bestSourcePort = -1;
            int bestTargetPort = -1;
            LayerOutputKind bestOutputKind = LayerOutputKind.Other;
            foreach (var node in pastedNodes)
            {
                var outputs = node.Outputs;
                if (outputs == null)
                    continue;

                for (int sourcePort = 0; sourcePort < outputs.Length; sourcePort++)
                {
                    if (layerConnections.Any(connection =>
                            connection != null
                            && connection.SourceNodeId == node.NodeId
                            && connection.SourcePortIndex == sourcePort
                            && pastedIds.Contains(connection.TargetNodeId)))
                        continue;

                    if (!TryResolveOutputTarget(outputs[sourcePort], out int targetPort, out var outputKind))
                        continue;

                    if (bestSource != null)
                        return;

                    bestSource = node;
                    bestSourcePort = sourcePort;
                    bestTargetPort = targetPort;
                    bestOutputKind = outputKind;
                }
            }

            if (bestSource == null)
                return;

            if (output == null)
            {
                output = _graphAsset.AddNode(typeof(OutputNode), false, targetLayerId) as OutputNode;
                if (output == null)
                    return;

                output.EditorPosition = bestSource.EditorPosition + new Vector2(320f, 0f);
                var outputView = new GeneratorNodeView(output);
                AddElement(outputView);
                SetupNodeVisuals(outputView);
                outputView.SetPreviewVisible(_inlinePreviewsVisible);
            }
            output.OutputKind = bestOutputKind;
            EditorUtility.SetDirty(output);

            var connection = _graphAsset.AddConnection(bestSource.NodeId, bestSourcePort, output.NodeId, bestTargetPort);
            _graphAsset.EnsureLayerGraphStates();

            var sourceView = FindNodeViewById(bestSource.NodeId);
            var targetView = FindNodeViewById(output.NodeId);
            var outputPort = sourceView?.GetOutputPort(bestSourcePort);
            var inputPort = targetView?.GetInputPort(bestTargetPort);
            if (connection != null && outputPort != null && inputPort != null)
                AddElement(CreateStyledEdge(outputPort, inputPort));
        }

        private GeneratorNodeView FindNodeViewById(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return null;

            return nodes
                .OfType<GeneratorNodeView>()
                .FirstOrDefault(view => view.NodeData != null && view.NodeData.NodeId == nodeId);
        }

        private static bool TryResolveOutputTarget(
            PortDefinition sourcePort,
            out int targetPort,
            out LayerOutputKind outputKind)
        {
            targetPort = -1;
            outputKind = LayerOutputKind.Other;
            if (sourcePort?.ValueType == typeof(string[,]))
            {
                targetPort = OutputNode.BiomeMapInputIndex;
                outputKind = LayerOutputKind.Tiles;
                return true;
            }

            if (sourcePort?.ValueType == typeof(float[,]))
            {
                targetPort = OutputNode.HeightMapInputIndex;
                outputKind = LayerOutputKind.Tiles;
                return true;
            }

            if (sourcePort?.ValueType == typeof(bool[,]))
            {
                targetPort = OutputNode.MaskInputIndex;
                outputKind = LayerOutputKind.Masks;
                return true;
            }

            targetPort = OutputNode.DataInputIndex;
            outputKind = LayerOutputKind.InternalData;
            return sourcePort != null;
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

            var group = new UnityEditor.Experimental.GraphView.Group { title = "Node Group" };

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
                        targetPortIndex = conn.TargetPortIndex,
                        sourceElementIndex = conn.SourceElementIndex
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
            else if (hasEmbeddedSOs)
            {
                BuildExistingSOMapFromPreset(preset.scriptableObjects, soMap);
            }

            var idMap = new Dictionary<string, string>();
            var reservedImportedNodeIds = new HashSet<string>();
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

                var node = _graphAsset.AddNode(nodeType, false, ResolveActiveLayerId());
                if (node == null) { skipped++; continue; }

                string newId = ReserveUniqueImportedNodeId(entry.originalNodeId, reservedImportedNodeIds);
                string presetNodeId = string.IsNullOrWhiteSpace(entry.originalNodeId)
                    ? newId
                    : entry.originalNodeId;
                idMap[presetNodeId] = newId;
                node.NodeId = newId;
                node.EditorPosition = entry.position;

                // Restore serialized field values
                RestoreNodeFromPresetEntry(node, entry, soMap);
                node.NodeId = newId;
                if (!GraphStaticNodeUtility.IsStaticGraphNode(node))
                    node.LayerId = ResolveActiveLayerId();
                node.EditorPosition = entry.position;

                // Assign SO references: either from newly created or from existing project assets
                if (createFromPreset)
                    AssignSOsFromMap(node, soMap);
                else
                    ResolveExistingSOReferences(node);

                if (node is TwcModifierNode twcImportedNode && string.IsNullOrEmpty(twcImportedNode.LayerId))
                    twcImportedNode.LayerId = _graphAsset.EnsureDefaultLayer();

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
                    newTarget, connEntry.targetPortIndex,
                    connEntry.sourceElementIndex);

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

            _graphAsset.NormalizeGraphIds();
            _graphAsset.EnsureLayerGraphStates();
            RefreshConnectionIndexControls();
            BringAllNodesToFront();

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(_graphAsset);
            GraphChanged?.Invoke();

            var msg = $"Імпортовано {created} вузл(ів), {imported} з'єднань";
            if (soCreated > 0) msg += $", створено {soCreated} SO-ассет(ів)";
            if (skipped > 0) msg += $"\nПропущено {skipped} вузл(ів) — типи не знайдені";
            Debug.Log($"[GraphPreset] {msg}.");

            if (skipped > 0)
                EditorUtility.DisplayDialog("Попередження імпорту", msg, "OK");
        }

        private static string ReserveUniqueImportedNodeId(string preferredId, HashSet<string> reservedIds)
        {
            reservedIds ??= new HashSet<string>();

            string id = string.IsNullOrWhiteSpace(preferredId)
                ? Guid.NewGuid().ToString()
                : preferredId;

            while (string.IsNullOrWhiteSpace(id) || !reservedIds.Add(id))
                id = Guid.NewGuid().ToString();

            return id;
        }

        private static string GetPresetNodeObjectName(NodePresetEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.jsonData))
                return null;

            try
            {
                var wrapper = JsonUtility.FromJson<UnitySerializedNodeWrapper>(entry.jsonData);
                string objectName = wrapper?.MonoBehaviour?.m_Name;
                return string.IsNullOrWhiteSpace(objectName) ? null : objectName;
            }
            catch
            {
                return null;
            }
        }

        private static string BuildNodeNameKey(string typeName, string objectName) =>
            $"{typeName}\n{objectName}";

        private static void AddNodeLookup(Dictionary<string, List<NodeBase>> lookup, string key, NodeBase node)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (!lookup.TryGetValue(key, out var list))
            {
                list = new List<NodeBase>();
                lookup[key] = list;
            }

            list.Add(node);
        }

        private static NodeBase TakeUnusedCandidate(List<NodeBase> candidates, HashSet<NodeBase> usedNodes)
        {
            if (candidates == null)
                return null;

            for (int index = 0; index < candidates.Count; index++)
            {
                var candidate = candidates[index];
                if (candidate == null || usedNodes.Contains(candidate))
                    continue;

                candidates.RemoveAt(index);
                return candidate;
            }

            return null;
        }

        /// <summary>
        /// Merge preset into existing graph: match nodes, update data, remove stale nodes, rebuild connections.
        /// </summary>
        private void MergePresetIntoGraph(GraphPreset preset)
        {
            Undo.RecordObject(_graphAsset, "Merge Graph Preset");

            var existingById = new Dictionary<string, NodeBase>();
            var existingByType = new Dictionary<string, List<NodeBase>>();
            var existingByTypeAndName = new Dictionary<string, List<NodeBase>>();

            foreach (var node in _graphAsset.Nodes)
            {
                if (node == null) continue;
                string typeName = node.GetType().AssemblyQualifiedName;
                existingById[node.NodeId] = node;
                AddNodeLookup(existingByType, typeName, node);
                AddNodeLookup(existingByTypeAndName, BuildNodeNameKey(typeName, node.name), node);
            }

            // Map: preset originalNodeId → actual nodeId in graph
            var idMap = new Dictionary<string, string>();
            var allViews = new Dictionary<string, GeneratorNodeView>();
            var usedNodes = new HashSet<NodeBase>();
            var orderedNodes = new List<NodeBase>();

            var soMap = new Dictionary<string, ScriptableObject>();
            int soCreated = CreateMissingSOsFromPreset(preset.scriptableObjects, soMap);

            int updated = 0, added = 0, skipped = 0, removedStale = 0;

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

                if (!string.IsNullOrWhiteSpace(entry.originalNodeId)
                    && existingById.TryGetValue(entry.originalNodeId, out var exactMatch)
                    && exactMatch != null
                    && !usedNodes.Contains(exactMatch)
                    && exactMatch.GetType() == nodeType)
                {
                    matched = exactMatch;
                }

                string presetObjectName = GetPresetNodeObjectName(entry);
                if (matched == null && !string.IsNullOrWhiteSpace(presetObjectName)
                    && existingByTypeAndName.TryGetValue(BuildNodeNameKey(typeName, presetObjectName), out var namedCandidates))
                {
                    matched = TakeUnusedCandidate(namedCandidates, usedNodes);
                }

                if (matched == null && existingByType.TryGetValue(typeName, out var typeCandidates))
                {
                    matched = TakeUnusedCandidate(typeCandidates, usedNodes);
                }

                if (matched != null)
                {
                    usedNodes.Add(matched);

                    // Update existing node — restore field values, keep NodeId
                    string keepId = matched.NodeId;
                    RestoreNodeFromPresetEntry(matched, entry, soMap);
                    matched.NodeId = keepId;
                    matched.EditorPosition = entry.position;
                    ResolveExistingSOReferences(matched);

                    if (matched is TwcModifierNode twcMatchedNode && string.IsNullOrEmpty(twcMatchedNode.LayerId))
                        twcMatchedNode.LayerId = _graphAsset.EnsureDefaultLayer();

                    string presetNodeId = string.IsNullOrWhiteSpace(entry.originalNodeId)
                        ? keepId
                        : entry.originalNodeId;
                    idMap[presetNodeId] = keepId;
                    orderedNodes.Add(matched);
                    updated++;
                }
                else
                {
                    // Add new node
                    var node = _graphAsset.AddNode(nodeType, false, ResolveActiveLayerId());
                    if (node == null) { skipped++; continue; }

                    string newId = string.IsNullOrWhiteSpace(entry.originalNodeId) || existingById.ContainsKey(entry.originalNodeId)
                        ? Guid.NewGuid().ToString()
                        : entry.originalNodeId;
                    node.NodeId = newId;
                    node.EditorPosition = entry.position;

                    RestoreNodeFromPresetEntry(node, entry, soMap);
                    node.NodeId = newId;
                    if (!GraphStaticNodeUtility.IsStaticGraphNode(node))
                        node.LayerId = ResolveActiveLayerId();
                    node.EditorPosition = entry.position;
                    ResolveExistingSOReferences(node);

                    if (node is TwcModifierNode twcAddedNode && string.IsNullOrEmpty(twcAddedNode.LayerId))
                        twcAddedNode.LayerId = _graphAsset.EnsureDefaultLayer();

                    string presetNodeId = string.IsNullOrWhiteSpace(entry.originalNodeId)
                        ? newId
                        : entry.originalNodeId;
                    idMap[presetNodeId] = newId;
                    usedNodes.Add(node);
                    orderedNodes.Add(node);
                    added++;
                }
            }

            var importedNodeIds = new HashSet<string>(idMap.Values);
            var staleNodes = _graphAsset.Nodes
                .Where(node => node != null && !importedNodeIds.Contains(node.NodeId))
                .ToList();

            foreach (var staleNode in staleNodes)
            {
                _graphAsset.RemoveNode(staleNode);
                removedStale++;
            }

            _graphAsset.ReorderNodes(orderedNodes);

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
                    tgtId, connEntry.targetPortIndex,
                    connEntry.sourceElementIndex);

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

            _graphAsset.NormalizeGraphIds();
            _graphAsset.EnsureLayerGraphStates();
            RefreshConnectionIndexControls();
            BringAllNodesToFront();

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(_graphAsset);
            GraphChanged?.Invoke();

            var msg = $"Модифіковано: оновлено {updated}, додано {added} вузл(ів), {imported} з'єднань";
            if (removedStale > 0) msg += $", видалено {removedStale} зайвих вузл(ів)";
            if (soCreated > 0) msg += $", створено {soCreated} SO-ассет(ів)";
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

        private const string DefaultGenerationSOFolder = "Assets/Moyva/SO/Generation";
        private const string MainTileRegistryPath = "Assets/Moyva/SO/Tile/TileRegistry.asset";

        private static readonly System.Text.RegularExpressions.Regex UnityGuidRegex = new("^[0-9a-fA-F]{32}$", System.Text.RegularExpressions.RegexOptions.Compiled);

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
            EnsureGenerationSOFolder();

            int count = 0;
            foreach (var entry in entries)
            {
                var soType = Type.GetType(entry.typeAssemblyQualifiedName);
                if (soType == null)
                {
                    Debug.LogWarning($"[GraphPreset] SO type not found: {entry.typeAssemblyQualifiedName}");
                    continue;
                }

                var existing = FindExistingSOForPresetEntry(entry, soType, exactNameOnly: true);
                if (existing != null)
                {
                    soMap[entry.originalGuid] = existing;
                    continue;
                }

                var newSO = ScriptableObject.CreateInstance(soType);
                newSO.name = entry.assetName;

                // Restore all field values from embedded JSON
                EditorJsonUtility.FromJsonOverwrite(entry.jsonData, newSO);
                newSO.name = entry.assetName;

                var assetPath = AssetDatabase.GenerateUniqueAssetPath(
                    $"{DefaultGenerationSOFolder}/{entry.assetName}.asset");
                AssetDatabase.CreateAsset(newSO, assetPath);

                soMap[entry.originalGuid] = newSO;
                count++;
                Debug.Log($"[GraphPreset] Created SO from preset: {assetPath} (type: {soType.Name})");
            }

            return count;
        }

        private static void BuildExistingSOMapFromPreset(List<ScriptableObjectEntry> entries,
            Dictionary<string, ScriptableObject> soMap)
        {
            if (entries == null) return;

            foreach (var entry in entries)
            {
                var soType = Type.GetType(entry.typeAssemblyQualifiedName);
                if (soType == null) continue;

                var existing = FindExistingSOForPresetEntry(entry, soType, exactNameOnly: false);
                if (existing != null)
                    soMap[entry.originalGuid] = existing;
            }
        }

        private static int CreateMissingSOsFromPreset(List<ScriptableObjectEntry> entries,
            Dictionary<string, ScriptableObject> soMap)
        {
            if (entries == null || entries.Count == 0) return 0;

            EnsureGenerationSOFolder();

            int count = 0;
            foreach (var entry in entries)
            {
                var soType = Type.GetType(entry.typeAssemblyQualifiedName);
                if (soType == null)
                {
                    Debug.LogWarning($"[GraphPreset] SO type not found: {entry.typeAssemblyQualifiedName}");
                    continue;
                }

                var existing = FindExistingSOForPresetEntry(entry, soType, exactNameOnly: true);
                if (existing != null)
                {
                    soMap[entry.originalGuid] = existing;
                    continue;
                }

                var newSO = ScriptableObject.CreateInstance(soType);
                newSO.name = entry.assetName;
                EditorJsonUtility.FromJsonOverwrite(entry.jsonData, newSO);
                newSO.name = entry.assetName;

                var assetPath = AssetDatabase.GenerateUniqueAssetPath(
                    $"{DefaultGenerationSOFolder}/{entry.assetName}.asset");
                AssetDatabase.CreateAsset(newSO, assetPath);

                soMap[entry.originalGuid] = newSO;
                count++;
                Debug.Log($"[GraphPreset] Created SO from preset: {assetPath} (type: {soType.Name})");
            }

            return count;
        }

        private static void EnsureGenerationSOFolder()
        {
            if (!AssetDatabase.IsValidFolder(DefaultGenerationSOFolder))
                AssetDatabase.CreateFolder("Assets/Moyva/SO", "Generation");
        }

        private static ScriptableObject FindExistingSOForPresetEntry(ScriptableObjectEntry entry, Type soType,
            bool exactNameOnly)
        {
            if (UnityGuidRegex.IsMatch(entry.originalGuid ?? string.Empty))
            {
                var byGuid = LoadScriptableObjectByGuid(entry.originalGuid, soType);
                if (byGuid != null)
                    return byGuid;
            }

            if (soType == typeof(TileRegistrySO) && !exactNameOnly)
            {
                var mainRegistry = AssetDatabase.LoadAssetAtPath(MainTileRegistryPath, soType) as ScriptableObject;
                if (mainRegistry != null)
                    return mainRegistry;
            }

            var guids = AssetDatabase.FindAssets($"t:{soType.Name}");
            var candidates = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => !string.IsNullOrEmpty(path))
                .OrderBy(path => path.IndexOf("/Prototype/", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0)
                .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(path => AssetDatabase.LoadAssetAtPath(path, soType) as ScriptableObject)
                .Where(asset => asset != null)
                .ToList();

            var exactName = candidates.FirstOrDefault(asset =>
                string.Equals(asset.name, entry.assetName, StringComparison.Ordinal));
            if (exactName != null)
                return exactName;

            return exactNameOnly ? null : candidates.FirstOrDefault();
        }

        private static void RestoreNodeFromPresetEntry(NodeBase node, NodePresetEntry entry,
            Dictionary<string, ScriptableObject> soMap)
        {
            var jsonData = RemapScriptableObjectGuids(entry.jsonData, soMap);
            EditorJsonUtility.FromJsonOverwrite(jsonData, node);
            AssignSerializedSOReferencesFromJson(node, jsonData);

            // Ensure imported TWC nodes have a live modifier instance so
            // their parameters are editable in the node inspector immediately.
            if (node is TwcModifierNode twcNode)
                twcNode.TryRestoreModifierInEditor();
        }

        private static string RemapScriptableObjectGuids(string jsonData,
            Dictionary<string, ScriptableObject> soMap)
        {
            if (string.IsNullOrEmpty(jsonData) || soMap == null || soMap.Count == 0)
                return jsonData;

            foreach (var kvp in soMap)
            {
                var assetPath = AssetDatabase.GetAssetPath(kvp.Value);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid)) continue;

                jsonData = jsonData.Replace($"\"guid\":\"{kvp.Key}\"", $"\"guid\":\"{guid}\"");
            }

            return jsonData;
        }

        private static void AssignSerializedSOReferencesFromJson(NodeBase node, string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData)) return;

            foreach (var field in node.GetType().GetFields(SOFieldFlags))
            {
                if (!IsSerializedSOField(field)) continue;

                var pattern = $"\\\"{System.Text.RegularExpressions.Regex.Escape(field.Name)}\\\"\\s*:\\s*\\{{[^}}]*\\\"guid\\\"\\s*:\\s*\\\"(?<guid>[^\\\"]+)\\\"";
                var match = System.Text.RegularExpressions.Regex.Match(jsonData, pattern);
                if (!match.Success) continue;

                var guid = match.Groups["guid"].Value;
                var asset = LoadScriptableObjectByGuid(guid, field.FieldType);
                if (asset != null)
                    field.SetValue(node, asset);
            }
        }

        private static ScriptableObject LoadScriptableObjectByGuid(string guid, Type expectedType)
        {
            if (!UnityGuidRegex.IsMatch(guid ?? string.Empty))
                return null;

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
                return null;

            return AssetDatabase.LoadAssetAtPath(assetPath, expectedType) as ScriptableObject;
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

            FlattenLegacyRoutePoints();

            string layerId = ResolveActiveLayerId();
            int foreignVisibleCount = CountForeignVisibleNodes();
            var selectedViews = selection
                .OfType<GeneratorNodeView>()
                .Where(view => IsNodeInLayerEditScope(view.NodeData, layerId, false))
                .ToList();
            bool selectedOnly = selectedViews.Count > 0;

            var layoutViews = selectedOnly
                ? selectedViews
                : nodes
                    .OfType<GeneratorNodeView>()
                    .Where(view => IsNodeInLayerEditScope(view.NodeData, layerId, false))
                    .ToList();

            var nodeViewMap = layoutViews
                .Where(view => view?.NodeData != null && !string.IsNullOrEmpty(view.NodeData.NodeId))
                .GroupBy(view => view.NodeData.NodeId)
                .ToDictionary(group => group.Key, group => group.First());
            if (nodeViewMap.Count == 0) return;

            Undo.RecordObject(_graphAsset, "Auto-Layout");
            foreach (var view in nodeViewMap.Values)
                Undo.RecordObject(view.NodeData, "Auto-Layout");

            // --- Build adjacency ---
            var nodeIds = new HashSet<string>(nodeViewMap.Keys);
            var outgoing = new Dictionary<string, List<string>>();
            var incoming = new Dictionary<string, List<string>>();

            foreach (var id in nodeIds)
            {
                outgoing[id] = new List<string>();
                incoming[id] = new List<string>();
            }

            foreach (var conn in _graphAsset.GetConnectionsForLayer(layerId, false))
            {
                if (conn == null)
                    continue;
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
                    nodeW[id] = v.PreferredWidth;
                    nodeH[id] = EstimateNodeHeight(v);
                }
                else
                {
                    nodeW[id] = 280f;
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
            if (foreignVisibleCount > 0)
            {
                string warning = $"Graph view contains {foreignVisibleCount} foreign node(s); Auto Layout ignored them.";
                Debug.LogWarning($"[Moyva Graph] {warning}");
                StatusMessage?.Invoke($"{warning} Moved {nodeViewMap.Count} node(s).");
            }
            else
            {
                StatusMessage?.Invoke($"Auto Layout: moved {nodeViewMap.Count} node(s) in {(selectedOnly ? "selection" : "active layer")}.");
            }
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
            // RoutePointNode видалено разом зі старим алгоритмом доріг.
            // Метод лишається як no-op для сумісності зі старими графами.
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
