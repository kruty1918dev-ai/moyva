using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    public sealed class GraphEditorWindow : EditorWindow
    {
        private const string RuntimeGraphBindingTypeName = "Kruty1918.Moyva.Generator.Runtime.MoyvaTileWorldCreatorGraphBinding";
        private const string GridInstallerTypeName = "Kruty1918.Moyva.Grid.Runtime.GridInstaller";

        private sealed class RuntimeExecutionSettings
        {
            public HeightMapSettings HeightMapSettings;
            public DataBiomesSettings BiomesSettings;
            public WFCDataSettings WfcSettings;
            public TileRegistrySO TileRegistry;
            public int GridWidth;
            public int GridHeight;
            public bool HasGridSize;
            public string Source;
        }

        private enum LayerRunStatus
        {
            Pending,
            SkippedValidation,
            Prepared,
            Generated,
            Failed
        }

        private enum LayerRunFailurePolicy
        {
            SkipInvalidLayersAndContinueFailures
        }

        private sealed class LayerRunRecord
        {
            public string LayerId;
            public string LayerName;
            public int SortingOrder;
            public LayerRunStatus Status;
            public string Message;
            public int NodeCount;
            public float NodeTimeMs;
            public string ErrorNodeId;
            public string GraphId;
        }

        private sealed class GraphRunReport
        {
            public readonly List<LayerRunRecord> Layers = new List<LayerRunRecord>();
            public readonly LayerRunFailurePolicy FailurePolicy;
            public readonly int Seed;
            public readonly int MapWidth;
            public readonly int MapHeight;

            public GraphRunReport(int seed, int mapWidth, int mapHeight, LayerRunFailurePolicy failurePolicy)
            {
                Seed = seed;
                MapWidth = mapWidth;
                MapHeight = mapHeight;
                FailurePolicy = failurePolicy;
            }

            public int GeneratedCount => Layers.Count(layer => layer.Status == LayerRunStatus.Generated);
            public int FailedCount => Layers.Count(layer => layer.Status == LayerRunStatus.Failed);
            public int SkippedCount => Layers.Count(layer => layer.Status == LayerRunStatus.SkippedValidation);
            public int ExecutedNodeCount => Layers.Sum(layer => layer.NodeCount);
            public float NodeTimeMs => Layers.Sum(layer => layer.NodeTimeMs);

            public string BuildStatusText(long elapsedMs, int layerDataCount)
            {
                var sb = new StringBuilder();
                sb.Append(FailedCount == 0 ? "✓" : "✗");
                sb.Append($" Run layers in {elapsedMs}ms ({MapWidth}×{MapHeight}, seed {Seed})");
                sb.Append($" | generated {GeneratedCount}, failed {FailedCount}, skipped {SkippedCount}");
                sb.Append($" | {ExecutedNodeCount} nodes | node time {NodeTimeMs:F1}ms");
                sb.Append($" | policy {FailurePolicy}");
                if (layerDataCount > 0)
                    sb.Append($" | {layerDataCount} layer data");
                return sb.ToString();
            }

            public string BuildConsoleSummary()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Policy: {FailurePolicy}");
                sb.AppendLine($"Seed: {Seed} | Map: {MapWidth}×{MapHeight}");

                foreach (var layer in Layers.OrderBy(layer => layer.SortingOrder).ThenBy(layer => layer.LayerName, StringComparer.Ordinal))
                {
                    sb.Append("  - ");
                    sb.Append(layer.Status.ToString().ToUpperInvariant());
                    sb.Append(' ');
                    sb.Append(layer.LayerName);
                    sb.Append($" [order {layer.SortingOrder}]");

                    if (layer.NodeCount > 0)
                        sb.Append($": {layer.NodeCount} node(s), {layer.NodeTimeMs:F1}ms");
                    if (!string.IsNullOrEmpty(layer.Message))
                        sb.Append($": {layer.Message}");
                    if (!string.IsNullOrEmpty(layer.ErrorNodeId))
                        sb.Append($" (node {layer.ErrorNodeId})");

                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        private GeneratorGraphView _graphView;
        private VisualElement _contentContainer;
        private ScrollView _rightPanel;
        private VisualElement _leftPanel;
        private VisualElement _layerListContainer;
        private Image _compositePreviewImage;
        [SerializeField] private string _selectedLayerId;
        private bool _focusSelectedLayerRowAfterRebuild;
        private GraphExecutionResult _lastResult;
        private Texture2D _layerCompositeTexture;
        private readonly List<Texture2D> _layerThumbnails = new List<Texture2D>();
        private Dictionary<string, bool[,]> _layerMatrices;
        private IMGUIContainer _nodeInspectorGui;
        private VisualElement _twcNodeInspectorGui;
        private IMGUIContainer _graphSettingsGui;
        [SerializeField] private bool _isInspectorVisible = true;
      
        private VisualElement _nodeInspectorSection;
        private VisualElement _nodeInspectorDivider;

        private Label _statusLabel;
        private ProgressBar _progressBar;
        private VisualElement _validationPanel;
        private VisualElement _validationIssuesContainer;
        private GraphValidationReport _lastValidationReport;
        private GraphValidationReport _lastBlueprintLayerSyncReport;

        private NodeBase _selectedNode;
        [SerializeField] private string _selectedNodeId;
        private UnityEditor.Editor _selectedNodeEditor;

        private enum InspectorTab { Settings = 0, Preview = 1 }
        [SerializeField] private InspectorTab _activeInspectorTab = InspectorTab.Settings;
        private VisualElement _inspectorTabsHeader;
        private VisualElement _tabSettingsContent;
        private VisualElement _tabPreviewContent;
        private VisualElement _tabBuildLayersContent;
        private VisualElement _buildLayersHost;
        private Button _tabSettingsButton;
        private Button _tabPreviewButton;
        private Button _tabBuildLayersButton;
        [SerializeField] private bool _isMultiSelection;

        // Survives domain reload / play mode transition
        [SerializeField] private string _graphAssetGuid;
        private GraphAsset _graphAsset;

        // Editor Preview Settings
        [SerializeField] private string _previewSettingsGuid;
        private EditorPreviewSettings _previewSettings;

        private const string SettingsAssetPath = "Assets/Moyva/Scripts/Features/GraphSystem/Editor/GraphEditorWindowSettings.asset";
        private GraphEditorWindowSettings _windowSettings;

        // Saved camera state (pan + zoom), restored after PopulateGraph
        [SerializeField] private Vector3 _savedCameraPosition = Vector3.zero;
        [SerializeField] private Vector3 _savedCameraScale = Vector3.one;

        // Inline map size override (used when no EditorPreviewSettings assigned)
        [SerializeField] private int _previewWidth = 64;
        [SerializeField] private int _previewHeight = 64;
        [SerializeField] private bool _showInlinePreviews = true;
        [SerializeField] private bool _autoRunOnChange = true;
        [SerializeField] private int _previewResolution = 1; // 0=64,1=128,2=full
        [SerializeField] private bool _previewHeatmap;

        private double _nextAutoRunAt;
        private bool _isRunningGraph;

        private readonly Dictionary<string, bool> _inlineObjectFoldouts = new();
        private readonly Dictionary<string, UnityEditor.Editor> _inlineObjectEditors = new();

        private sealed class CopiedLayerData
        {
            public string SourceLayerId;
            public string Name;
            public Color Color;
            public int SortingOrder;
            public bool Enabled;
            public float DefaultHeight;
            public bool UseZeroLayerPadding;
            public int ExtraWidthCells;
            public int ExtraLengthCells;
            public bool GenerateFlatSurface;
            public Material FlatSurfaceMaterial;
            public string BuildLayerKey;
            public readonly List<CopiedLayerNodeData> Nodes = new();
            public readonly List<CopiedLayerConnectionData> Connections = new();
        }

        private readonly struct CopiedLayerNodeData
        {
            public readonly string OriginalNodeId;
            public readonly Type NodeType;
            public readonly Vector2 Position;
            public readonly string JsonData;

            public CopiedLayerNodeData(string originalNodeId, Type nodeType, Vector2 position, string jsonData)
            {
                OriginalNodeId = originalNodeId;
                NodeType = nodeType;
                Position = position;
                JsonData = jsonData;
            }
        }

        private readonly struct CopiedLayerConnectionData
        {
            public readonly string SourceNodeId;
            public readonly int SourcePortIndex;
            public readonly string TargetNodeId;
            public readonly int TargetPortIndex;
            public readonly int SourceElementIndex;

            public CopiedLayerConnectionData(Connection connection)
            {
                SourceNodeId = connection.SourceNodeId;
                SourcePortIndex = connection.SourcePortIndex;
                TargetNodeId = connection.TargetNodeId;
                TargetPortIndex = connection.TargetPortIndex;
                SourceElementIndex = connection.SourceElementIndex;
            }
        }

        private CopiedLayerData _copiedLayer;

        [MenuItem("Moyva/Graph Editor")]
        public static void Open()
        {
            var window = GetWindow<GraphEditorWindow>("Generator Graph");
            window.minSize = new Vector2(900, 600);
        }

        public static void Open(GraphAsset asset)
        {
            var window = GetWindow<GraphEditorWindow>("Generator Graph");
            window.minSize = new Vector2(900, 600);
            window.LoadGraph(asset);
        }

        private void OnEnable()
        {
            GraphEditorWindowLayoutRepair.ScheduleCloseFailedFallbackWindows();
            LoadWindowSettings();
            ConstructGraphView();
            ConstructToolbar();
            ConstructStatusBar();

            // Restore graph after domain reload
            RestoreGraphAsset();
            RestorePreviewSettings();
            RefreshInspectorPanel();

            rootVisualElement.schedule.Execute(PollSelectionForInspector).Every(120);
            rootVisualElement.schedule.Execute(PollAutoRun).Every(120);

            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            SaveWindowSettings();
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            foreach (var editor in _inlineObjectEditors.Values)
            {
                if (editor != null)
                    DestroyImmediate(editor);
            }
            _inlineObjectEditors.Clear();

            if (_graphView != null)
            {
                _graphView.GraphChanged -= OnGraphChanged;
                _graphView.CanvasBackgroundClicked -= OnGraphCanvasBackgroundClicked;
                _graphView.StatusMessage -= OnGraphStatusMessage;
                _graphView.viewTransformChanged -= OnGraphViewTransformChanged;
            }

            DisposeLayerPreviewTextures();

            if (_contentContainer != null)
                rootVisualElement.Remove(_contentContainer);
        }

        private void OnUndoRedoPerformed()
        {
            if (_graphView != null && _graphAsset != null)
            {
                CaptureCameraTransform();
                _graphAsset.EnsureLayerGraphStates();
                EnsureSelectedLayer();
                TrySyncCompanionBlueprintLayers(false);
                _graphView.SetActiveLayerWithoutRefresh(_selectedLayerId);
                RefreshGraphViewFromAsset();
                RebuildLayerList();
                RefreshInspectorPanel();
                UpdateStatusBar();
            }
        }

        private void RestoreGraphAsset()
        {
            if (_graphAsset != null)
            {
                MigrateLegacySharedSettingsNode(_graphAsset);
                EnsureSelectedLayer();
                EnsureSelectedLayerMatchesSelectedNode();
                _graphView.SetActiveLayerWithoutRefresh(_selectedLayerId);
                _graphView.PopulateGraph(_graphAsset, EditorApplication.isPlaying);
                RestoreGraphViewState();
                RebuildLayerList();
                UpdateStatusBar();
                return;
            }

            if (string.IsNullOrEmpty(_graphAssetGuid)) return;

            var path = AssetDatabase.GUIDToAssetPath(_graphAssetGuid);
            if (string.IsNullOrEmpty(path)) return;

            var asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(path);
            if (asset != null)
            {
                _graphAsset = asset;
                MigrateLegacySharedSettingsNode(_graphAsset);
                EnsureSelectedLayer();
                EnsureSelectedLayerMatchesSelectedNode();
                _graphView.SetActiveLayerWithoutRefresh(_selectedLayerId);
                _graphView.PopulateGraph(_graphAsset, EditorApplication.isPlaying);
                RestoreGraphViewState();
                RebuildLayerList();
                UpdateStatusBar();
            }
        }

        private void RestoreGraphViewState()
        {
            RestoreSelectedNodeFromId();
            RestoreCameraTransform();
        }

        private void RefreshGraphViewFromAsset(bool restoreSelection = true)
        {
            if (_graphView == null)
                return;

            CaptureCameraTransform();
            _graphView.RefreshFromAsset();
            if (restoreSelection)
                RestoreGraphViewState();
            else
                RestoreCameraTransform();
        }

        private void RestoreSelectedNodeFromId()
        {
            if (_graphAsset == null || _graphView == null || string.IsNullOrEmpty(_selectedNodeId))
                return;

            var node = _graphAsset.GetNodeById(_selectedNodeId);
            if (node == null)
            {
                SetSelectedNode(null);
                return;
            }

            if (!GraphAsset.IsGlobalNode(node)
                && !string.Equals(node.LayerId, _selectedLayerId, StringComparison.Ordinal))
            {
                if (_graphAsset.GetLayerById(node.LayerId) != null)
                {
                    _selectedLayerId = node.LayerId;
                    _graphView.SetActiveLayerWithoutRefresh(_selectedLayerId);
                    _graphView.PopulateGraph(_graphAsset, EditorApplication.isPlaying);
                }
                else
                {
                    SetSelectedNode(null);
                    return;
                }
            }

            if (!_graphView.SelectNodeById(_selectedNodeId))
            {
                SetSelectedNode(null);
                return;
            }

            _isMultiSelection = false;
            SetSelectedNode(node);
            RefreshInspectorPanel();
        }

        private void EnsureSelectedLayerMatchesSelectedNode()
        {
            if (_graphAsset == null || string.IsNullOrEmpty(_selectedNodeId))
                return;

            var node = _graphAsset.GetNodeById(_selectedNodeId);
            if (node == null || GraphAsset.IsGlobalNode(node))
                return;

            if (_graphAsset.GetLayerById(node.LayerId) != null)
                _selectedLayerId = node.LayerId;
        }

        private void RestoreCameraTransform()
        {
            if (_savedCameraScale == Vector3.zero) _savedCameraScale = Vector3.one;
            var pos = _savedCameraPosition;
            var scale = _savedCameraScale;
            // Defer by one frame so the graph view has finished layout
            rootVisualElement.schedule.Execute(() =>
            {
                _graphView?.UpdateViewTransform(pos, scale);
            });
        }

        private void CaptureCameraTransform()
        {
            if (_graphView?.contentViewContainer == null)
                return;

            var translation = _graphView.contentViewContainer.resolvedStyle.translate;
            var scale = _graphView.contentViewContainer.resolvedStyle.scale.value;

            if (!IsFinite(translation.x) || !IsFinite(translation.y) || !IsFinite(scale.x) || !IsFinite(scale.y))
                return;

            _savedCameraPosition = new Vector3(translation.x, translation.y, 0f);
            _savedCameraScale = scale == Vector3.zero ? Vector3.one : scale;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                _graphView?.SetReadOnly(true);
                UpdateStatusBar();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                RestoreGraphAsset();
                _graphView?.SetReadOnly(false);
                UpdateStatusBar();
            }
        }

        private void ConstructGraphView()
        {
            _contentContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    marginTop = 0,
                    marginBottom = 20
                }
            };
            rootVisualElement.Add(_contentContainer);

            ConstructLeftPanel();

            _graphView = new GeneratorGraphView(this);
            _graphView.GraphChanged += OnGraphChanged;
            _graphView.CanvasBackgroundClicked += OnGraphCanvasBackgroundClicked;
            _graphView.StatusMessage += OnGraphStatusMessage;
            _graphView.viewTransformChanged += OnGraphViewTransformChanged;
            _graphView.style.flexGrow = 1;
            _contentContainer.Add(_graphView);

            ConstructRightPanel();
        }

        private void ConstructLeftPanel()
        {
            _leftPanel = new VisualElement
            {
                style =
                {
                    width = 240,
                    minWidth = 200,
                    flexShrink = 0,
                    borderRightWidth = 1,
                    borderRightColor = new Color(0.26f, 0.28f, 0.32f),
                    backgroundColor = new Color(0.075f, 0.08f, 0.095f),
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 8,
                    paddingBottom = 10
                }
            };

            var headerCard = new VisualElement
            {
                style =
                {
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 7,
                    paddingBottom = 7,
                    marginBottom = 8,
                    backgroundColor = new Color(0.105f, 0.115f, 0.135f),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6
                }
            };
            headerCard.Add(new Label("Шари генератора")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13,
                    marginBottom = 1
                }
            });
            headerCard.Add(new Label("order → graph → runtime")
            {
                style =
                {
                    fontSize = 10,
                    color = new Color(0.62f, 0.66f, 0.72f)
                }
            });
            _leftPanel.Add(headerCard);

            _layerListContainer = new VisualElement { focusable = true };
            _layerListContainer.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                evt.menu.AppendAction(
                    "Вставити скопійований шар",
                    _ => PasteCopiedLayerAfter(null),
                    _ => CanPasteCopiedLayer()
                        ? DropdownMenuAction.Status.Normal
                        : DropdownMenuAction.Status.Disabled);
            });
            _leftPanel.Add(_layerListContainer);

            var buttonsRow = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, marginTop = 8, marginBottom = 8 }
            };

            var addButton = new Button(AddLayer) { text = "+ Layer" };
            addButton.style.flexGrow = 1;
            addButton.style.marginRight = 5;
            addButton.style.height = 26;
            addButton.tooltip = "Додати новий шар генератора.";
            buttonsRow.Add(addButton);

            var removeButton = new Button(RemoveSelectedLayer) { text = "Delete" };
            removeButton.style.width = 58;
            removeButton.style.height = 26;
            removeButton.tooltip = "Видалити вибраний шар (разом із його вузлами).";
            buttonsRow.Add(removeButton);

            _leftPanel.Add(buttonsRow);

            var compositeHeader = new Label("Ізометричне превʼю")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 8,
                    marginBottom = 5,
                    color = new Color(0.82f, 0.86f, 0.9f)
                }
            };
            _leftPanel.Add(compositeHeader);

            _compositePreviewImage = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    height = 170,
                    marginBottom = 6,
                    backgroundColor = new Color(0.035f, 0.04f, 0.055f),
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftColor = new Color(0.22f, 0.24f, 0.28f),
                    borderRightColor = new Color(0.22f, 0.24f, 0.28f),
                    borderTopColor = new Color(0.22f, 0.24f, 0.28f),
                    borderBottomColor = new Color(0.22f, 0.24f, 0.28f)
                }
            };
            _compositePreviewImage.tooltip =
                "Складений ізометричний 3D-вид усіх шарів (Run для оновлення).";
            _leftPanel.Add(_compositePreviewImage);

            _contentContainer.Add(_leftPanel);
        }

        private void RebuildLayerList()
        {
            if (_layerListContainer == null)
                return;

            _layerListContainer.Clear();

            // Старі мініатюри більше не на екрані — звільняємо (композит лишаємо).
            foreach (var thumb in _layerThumbnails)
            {
                if (thumb != null)
                    DestroyImmediate(thumb);
            }
            _layerThumbnails.Clear();

            if (_graphAsset == null)
                return;

            _graphAsset.EnsureDefaultLayer();
            var syncIssueByLayer = BuildLayerSyncIssueLookup();

            if (!string.IsNullOrEmpty(_selectedLayerId)
                && _graphAsset.GetLayerById(_selectedLayerId) == null)
                _selectedLayerId = null;
            if (string.IsNullOrEmpty(_selectedLayerId))
                _selectedLayerId = _graphAsset.EnsureDefaultLayer();

            var orderedLayers = _graphAsset.Layers
                .Where(l => l != null)
                .OrderBy(l => l.SortingOrder)
                .ToList();

            foreach (var layer in orderedLayers)
            {
                string layerId = layer.Id;
                bool isSelected = layerId == _selectedLayerId;
                bool hasSyncIssue = syncIssueByLayer.TryGetValue(layerId, out var syncIssue);
                int nodeCount = _graphAsset.GetNodesForLayer(layerId)?.Count() ?? 0;
                int tileVariantCount = TileSettingsNode.GetNodesForLayer(_graphAsset, layerId)
                    .Where(node => node != null)
                    .Sum(node => node.ConfiguredVariantCount);

                var row = new VisualElement
                {
                    focusable = true,
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginBottom = 6,
                        paddingLeft = 6,
                        paddingRight = 6,
                        paddingTop = 6,
                        paddingBottom = 6,
                        minHeight = 42,
                        borderLeftWidth = 3,
                        borderLeftColor = layer.Enabled ? layer.Color : new Color(0.28f, 0.28f, 0.28f),
                        borderTopLeftRadius = 5,
                        borderTopRightRadius = 5,
                        borderBottomLeftRadius = 5,
                        borderBottomRightRadius = 5,
                        backgroundColor = isSelected
                            ? new Color(0.17f, 0.25f, 0.36f)
                            : hasSyncIssue
                                ? new Color(0.28f, 0.12f, 0.1f)
                            : new Color(0.115f, 0.12f, 0.14f)
                    }
                };

                var orderPill = new Label(layer.SortingOrder.ToString())
                {
                    tooltip = "Sorting order",
                    style =
                    {
                        minWidth = 24,
                        height = 18,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        fontSize = 10,
                        marginRight = 6,
                        color = new Color(0.86f, 0.88f, 0.92f),
                        backgroundColor = new Color(0.06f, 0.065f, 0.08f),
                        borderTopLeftRadius = 9,
                        borderTopRightRadius = 9,
                        borderBottomLeftRadius = 9,
                        borderBottomRightRadius = 9
                    }
                };
                row.Add(orderPill);

                var textColumn = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                        flexDirection = FlexDirection.Column
                    }
                };
                var nameLabel = new Label(layer.Enabled ? layer.Name : $"{layer.Name} (off)")
                {
                    style =
                    {
                        flexGrow = 1,
                        unityTextOverflowPosition = TextOverflowPosition.End,
                        unityFontStyleAndWeight = isSelected ? FontStyle.Bold : FontStyle.Normal,
                        color = layer.Enabled ? Color.white : new Color(0.58f, 0.58f, 0.58f)
                    }
                };
                textColumn.Add(nameLabel);

                string meta = tileVariantCount > 0
                    ? $"{nodeCount} nodes · {tileVariantCount} tile variants"
                    : $"{nodeCount} nodes";
                textColumn.Add(new Label(meta)
                {
                    style =
                    {
                        fontSize = 10,
                        color = new Color(0.62f, 0.66f, 0.72f),
                        marginTop = 1
                    }
                });
                row.Add(textColumn);

                if (hasSyncIssue)
                {
                    var warning = new Label("ERR")
                    {
                        tooltip = syncIssue.Message,
                        style =
                        {
                            unityFontStyleAndWeight = FontStyle.Bold,
                            color = new Color(1f, 0.62f, 0.48f),
                            marginLeft = 4
                        }
                    };
                    row.tooltip = syncIssue.Message;
                    row.Add(warning);
                }

                row.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button == 0 || evt.button == 1)
                    {
                        _focusSelectedLayerRowAfterRebuild = true;
                        SelectLayer(layerId);
                    }
                });
                row.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (layerId != _selectedLayerId)
                        return;

                    if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
                    {
                        RemoveSelectedLayer();
                        evt.StopPropagation();
                    }
                });
                row.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
                {
                    _focusSelectedLayerRowAfterRebuild = true;
                    SelectLayer(layerId);
                    PopulateLayerContextMenu(evt, layerId);
                    evt.StopPropagation();
                });
                _layerListContainer.Add(row);

                if (isSelected && _focusSelectedLayerRowAfterRebuild)
                {
                    var rowToFocus = row;
                    rowToFocus.schedule.Execute(() => rowToFocus.Focus());
                    _focusSelectedLayerRowAfterRebuild = false;
                }

                // Мініатюра матриці шару (якщо граф уже виконано).
                if (_layerMatrices != null && _layerMatrices.TryGetValue(layerId, out var matrix))
                {
                    var thumb = GeneratorLayerPreviewBuilder.BuildLayerThumbnail(matrix, layer.Color);
                    if (thumb != null)
                    {
                        _layerThumbnails.Add(thumb);
                        var thumbImage = new Image
                        {
                            image = thumb,
                            scaleMode = ScaleMode.ScaleToFit,
                            style =
                            {
                                height = 54,
                                marginBottom = 7,
                                marginLeft = 8,
                                marginRight = 8,
                                backgroundColor = new Color(0.04f, 0.045f, 0.06f),
                                borderLeftWidth = 1,
                                borderRightWidth = 1,
                                borderTopWidth = 1,
                                borderBottomWidth = 1,
                                borderLeftColor = new Color(0.18f, 0.2f, 0.24f),
                                borderRightColor = new Color(0.18f, 0.2f, 0.24f),
                                borderTopColor = new Color(0.18f, 0.2f, 0.24f),
                                borderBottomColor = new Color(0.18f, 0.2f, 0.24f)
                            }
                        };
                        _layerListContainer.Add(thumbImage);
                    }
                }
            }

            if (_graphView != null)
            {
                bool activeLayerChanged = !string.Equals(_graphView.ActiveLayerId, _selectedLayerId, StringComparison.Ordinal);
                if (activeLayerChanged)
                    CaptureCameraTransform();

                _graphView.SetVisibleLayer(_selectedLayerId);

                if (activeLayerChanged)
                    RestoreGraphViewState();
            }
        }

        private string EnsureSelectedLayer()
        {
            if (_graphAsset == null)
            {
                _selectedLayerId = null;
                return null;
            }

            _graphAsset.EnsureLayerGraphStates();
            if (!string.IsNullOrEmpty(_selectedLayerId)
                && _graphAsset.GetLayerById(_selectedLayerId) != null)
                return _selectedLayerId;

            _selectedLayerId = _graphAsset.EnsureDefaultLayer();
            return _selectedLayerId;
        }

        /// <summary>
        /// Перераховує матриці шарів, мініатюри та складене ізометричне превʼю
        /// з останнього результату виконання графа.
        /// </summary>
        private void RebuildLayerPreviews()
        {
            DisposeLayerPreviewTextures();

            if (_graphAsset == null || _lastResult == null)
            {
                _layerMatrices = null;
                RebuildLayerList();
                return;
            }

            _layerMatrices = GeneratorLayerPreviewBuilder.ComputeLayerMatrices(
                _graphAsset, _lastResult, out int w, out int h);

            _layerCompositeTexture = GeneratorLayerPreviewBuilder.BuildIsometricComposite(
                _graphAsset, _layerMatrices, w, h);

            if (_compositePreviewImage != null)
                _compositePreviewImage.image = _layerCompositeTexture;

            RebuildLayerList();
            GraphPreviewWindow.RequestRepaint();
        }

        private void DisposeLayerPreviewTextures()
        {
            foreach (var thumb in _layerThumbnails)
            {
                if (thumb != null)
                    DestroyImmediate(thumb);
            }
            _layerThumbnails.Clear();

            if (_layerCompositeTexture != null)
            {
                if (_compositePreviewImage != null)
                    _compositePreviewImage.image = null;
                DestroyImmediate(_layerCompositeTexture);
                _layerCompositeTexture = null;
            }
        }

        private void SelectLayer(string layerId)
        {
            if (_graphAsset == null || string.IsNullOrEmpty(layerId))
                return;
            if (_graphAsset.GetLayerById(layerId) == null)
                return;

            GUI.FocusControl(null);
            EditorGUIUtility.editingTextField = false;
            CaptureCameraTransform();
            _graphView?.ClearSelection();
            _isMultiSelection = false;
            SetSelectedNode(null);

            _selectedLayerId = layerId;
            if (_graphView != null)
                _graphView.ActiveLayerId = _selectedLayerId;
            RebuildLayerList();
            RestoreCameraTransform();
            SetInspectorTab(InspectorTab.Settings);
            RefreshInspectorPanel();
        }

        private void AddLayer()
        {
            if (_graphAsset == null)
                return;

            Undo.RecordObject(_graphAsset, "Add Layer");
            var layer = _graphAsset.AddLayer($"Layer {_graphAsset.Layers.Count + 1}");
            layer.SortingOrder = _graphAsset.Layers
                .Where(existing => existing != null && existing != layer)
                .Select(existing => existing.SortingOrder)
                .DefaultIfEmpty(-1)
                .Max() + 1;
            layer.DefaultHeight = _graphAsset.Layers
                .Where(existing => existing != null && existing != layer)
                .Select(existing => existing.DefaultHeight)
                .DefaultIfEmpty(0f)
                .Max() + 0.1f;
            EditorUtility.SetDirty(_graphAsset);
            TrySyncCompanionBlueprintLayers(false);
            SelectLayer(layer.Id);
        }

        private void PopulateLayerContextMenu(ContextualMenuPopulateEvent evt, string layerId)
        {
            if (evt == null)
                return;

            bool hasLayer = _graphAsset != null && _graphAsset.GetLayerById(layerId) != null;
            evt.menu.AppendAction(
                "Скопіювати шар",
                _ => CopyLayerToBuffer(layerId),
                _ => hasLayer ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction(
                "Вставити скопійований шар після цього",
                _ => PasteCopiedLayerAfter(layerId),
                _ => CanPasteCopiedLayer()
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction(
                "Дублювати шар",
                _ =>
                {
                    CopyLayerToBuffer(layerId);
                    PasteCopiedLayerAfter(layerId);
                },
                _ => hasLayer ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendSeparator();
            evt.menu.AppendAction(
                "Видалити шар",
                _ => RemoveSelectedLayer(),
                _ => hasLayer && _graphAsset.Layers.Count > 1
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);
        }

        private bool CanPasteCopiedLayer()
        {
            return _graphAsset != null && _copiedLayer != null;
        }

        private void CopyLayerToBuffer(string layerId)
        {
            if (_graphAsset == null || string.IsNullOrEmpty(layerId))
                return;

            var layer = _graphAsset.GetLayerById(layerId);
            if (layer == null)
                return;

            _graphAsset.EnsureLayerGraphStates();
            var copied = new CopiedLayerData
            {
                SourceLayerId = layer.Id,
                Name = layer.Name,
                Color = layer.Color,
                SortingOrder = layer.SortingOrder,
                Enabled = layer.Enabled,
                DefaultHeight = layer.DefaultHeight,
                UseZeroLayerPadding = layer.UseZeroLayerPadding,
                ExtraWidthCells = layer.ExtraWidthCells,
                ExtraLengthCells = layer.ExtraLengthCells,
                GenerateFlatSurface = layer.GenerateFlatSurface,
                FlatSurfaceMaterial = layer.FlatSurfaceMaterial,
                BuildLayerKey = layer.BuildLayerKey
            };

            var nodes = _graphAsset.GetNodesForLayer(layerId)
                .Where(node => node != null && !GraphStaticNodeUtility.IsStaticGraphNode(node))
                .ToList();
            var nodeIds = new HashSet<string>(nodes.Select(node => node.NodeId));

            foreach (var node in nodes)
            {
                copied.Nodes.Add(new CopiedLayerNodeData(
                    node.NodeId,
                    node.GetType(),
                    node.EditorPosition,
                    EditorJsonUtility.ToJson(node)));
            }

            foreach (var connection in _graphAsset.GetConnectionsForLayer(layerId, false))
            {
                if (connection == null)
                    continue;

                bool sourceCopied = nodeIds.Contains(connection.SourceNodeId);
                bool targetCopied = nodeIds.Contains(connection.TargetNodeId);
                if (!sourceCopied && !targetCopied)
                    continue;
                if (!sourceCopied && !IsReusableGlobalEndpoint(connection.SourceNodeId))
                    continue;
                if (!targetCopied && !IsReusableGlobalEndpoint(connection.TargetNodeId))
                    continue;

                copied.Connections.Add(new CopiedLayerConnectionData(connection));
            }

            _copiedLayer = copied;
            if (_statusLabel != null)
                _statusLabel.text = $"Скопійовано шар '{layer.Name}' ({copied.Nodes.Count} нод, {copied.Connections.Count} зв'язків).";
        }

        private void PasteCopiedLayerAfter(string afterLayerId)
        {
            if (!CanPasteCopiedLayer())
                return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Paste Layer");
            Undo.RecordObject(_graphAsset, "Paste Layer");

            int sortingOrder = ResolveInsertedLayerSortingOrder(afterLayerId);
            var layer = _graphAsset.AddLayer(GenerateUniqueLayerCopyName(_copiedLayer.Name));
            layer.Color = _copiedLayer.Color;
            layer.SortingOrder = sortingOrder;
            layer.Enabled = _copiedLayer.Enabled;
            layer.DefaultHeight = _copiedLayer.DefaultHeight;
            layer.UseZeroLayerPadding = _copiedLayer.UseZeroLayerPadding;
            layer.ExtraWidthCells = _copiedLayer.ExtraWidthCells;
            layer.ExtraLengthCells = _copiedLayer.ExtraLengthCells;
            layer.GenerateFlatSurface = _copiedLayer.GenerateFlatSurface;
            layer.FlatSurfaceMaterial = _copiedLayer.FlatSurfaceMaterial;
            layer.BuildLayerKey = _copiedLayer.BuildLayerKey;

            var idMap = new Dictionary<string, string>();
            foreach (var data in _copiedLayer.Nodes)
            {
                var node = _graphAsset.AddNode(data.NodeType, false, layer.Id);
                if (node == null)
                    continue;

                Undo.RegisterCreatedObjectUndo(node, "Paste Layer Node");
                if (!string.IsNullOrWhiteSpace(data.JsonData))
                    EditorJsonUtility.FromJsonOverwrite(data.JsonData, node);

                node.NodeId = Guid.NewGuid().ToString();
                node.LayerId = layer.Id;
                node.EditorPosition = data.Position;
                RemapSerializedLayerReferences(node, _copiedLayer.SourceLayerId, layer.Id);
                EditorUtility.SetDirty(node);

                if (!string.IsNullOrEmpty(data.OriginalNodeId))
                    idMap[data.OriginalNodeId] = node.NodeId;
            }

            foreach (var connection in _copiedLayer.Connections)
            {
                string sourceId = ResolvePastedEndpoint(connection.SourceNodeId, idMap);
                string targetId = ResolvePastedEndpoint(connection.TargetNodeId, idMap);
                if (string.IsNullOrEmpty(sourceId) || string.IsNullOrEmpty(targetId))
                    continue;

                _graphAsset.AddConnection(
                    sourceId,
                    connection.SourcePortIndex,
                    targetId,
                    connection.TargetPortIndex,
                    connection.SourceElementIndex);
            }

            _graphAsset.EnsureLayerGraphStates();
            EditorUtility.SetDirty(_graphAsset);
            TrySyncCompanionBlueprintLayers(false);
            Undo.CollapseUndoOperations(undoGroup);

            SelectLayer(layer.Id);
            RefreshGraphViewFromAsset(false);
            RebuildLayerList();
            RefreshInspectorPanel();
            RequestAutoRun();

            if (_statusLabel != null)
                _statusLabel.text = $"Вставлено шар '{layer.Name}' ({idMap.Count} нод, {_copiedLayer.Connections.Count} зв'язків).";
        }

        private int ResolveInsertedLayerSortingOrder(string afterLayerId)
        {
            var afterLayer = !string.IsNullOrEmpty(afterLayerId)
                ? _graphAsset.GetLayerById(afterLayerId)
                : null;
            int order = afterLayer != null
                ? afterLayer.SortingOrder + 1
                : _graphAsset.Layers.Where(layer => layer != null).Select(layer => layer.SortingOrder).DefaultIfEmpty(-1).Max() + 1;

            foreach (var layer in _graphAsset.Layers)
            {
                if (layer != null && layer.SortingOrder >= order)
                    layer.SortingOrder++;
            }

            return order;
        }

        private string GenerateUniqueLayerCopyName(string sourceName)
        {
            string baseName = string.IsNullOrWhiteSpace(sourceName) ? "Layer" : sourceName.Trim();
            string candidate = $"{baseName} Copy";
            int index = 2;
            while (_graphAsset.Layers.Any(layer =>
                       layer != null && string.Equals(layer.Name, candidate, StringComparison.Ordinal)))
            {
                candidate = $"{baseName} Copy {index++}";
            }

            return candidate;
        }

        private bool IsReusableGlobalEndpoint(string nodeId)
        {
            var node = _graphAsset?.GetNodeById(nodeId);
            return node != null && GraphStaticNodeUtility.IsStaticGraphNode(node);
        }

        private string ResolvePastedEndpoint(string originalNodeId, IReadOnlyDictionary<string, string> idMap)
        {
            if (string.IsNullOrEmpty(originalNodeId))
                return null;
            if (idMap != null && idMap.TryGetValue(originalNodeId, out string mappedId))
                return mappedId;
            return IsReusableGlobalEndpoint(originalNodeId) ? originalNodeId : null;
        }

        private static void RemapSerializedLayerReferences(NodeBase node, string oldLayerId, string newLayerId)
        {
            if (node == null || string.IsNullOrEmpty(oldLayerId) || string.IsNullOrEmpty(newLayerId))
                return;

            var serialized = new SerializedObject(node);
            var property = serialized.GetIterator();
            bool enterChildren = true;
            bool changed = false;
            while (property.Next(enterChildren))
            {
                enterChildren = false;
                if (property.propertyType != SerializedPropertyType.String)
                    continue;
                if (!string.Equals(property.stringValue, oldLayerId, StringComparison.Ordinal))
                    continue;

                property.stringValue = newLayerId;
                changed = true;
            }

            if (changed)
                serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private void ApplyLayerPreset(string layerName, Color color)
        {
            if (_graphAsset == null)
                return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Add {layerName} Preset");
            Undo.RecordObject(_graphAsset, $"Add {layerName} Preset");

            var result = GraphBuiltInPresetUtility.AddLayerPreset(_graphAsset, layerName, color);
            RegisterPresetCreatedObjectsForUndo(result);
            Undo.CollapseUndoOperations(undoGroup);

            if (result == null || result.Layer == null)
            {
                if (result != null)
                    RenderPresetResult(result);
                return;
            }

            EditorUtility.SetDirty(_graphAsset);
            SelectLayer(result.Layer.Id);
            RefreshGraphViewFromAsset(false);
            RenderPresetResult(result);
        }

        private delegate GraphPresetApplyResult BranchPresetAction(
            GraphAsset graph,
            string layerId,
            IReadOnlyList<NodeBase> selectedNodes);

        private void ApplyBranchPreset(BranchPresetAction action)
        {
            if (_graphAsset == null || action == null)
                return;

            string layerId = EnsureSelectedLayer();
            var selectedNodes = _graphView?.GetSelectedNodesForActiveLayer() ?? Array.Empty<NodeBase>();

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Add Graph Branch Preset");
            Undo.RecordObject(_graphAsset, "Add Graph Branch Preset");

            var result = action(_graphAsset, layerId, selectedNodes);
            RegisterPresetCreatedObjectsForUndo(result);
            Undo.CollapseUndoOperations(undoGroup);

            EditorUtility.SetDirty(_graphAsset);

            if (result?.Changed == true)
            {
                RefreshGraphViewFromAsset();
            }

            RenderPresetResult(result);
        }

        private static void RegisterPresetCreatedObjectsForUndo(GraphPresetApplyResult result)
        {
            if (result == null)
                return;

            foreach (var node in result.CreatedNodes)
            {
                if (node != null)
                    Undo.RegisterCreatedObjectUndo(node, $"Create {result.PresetName} Node");
            }
        }

        private void RemoveSelectedLayer()
        {
            if (_graphAsset == null || string.IsNullOrEmpty(_selectedLayerId))
                return;

            if (_graphAsset.Layers.Count <= 1)
            {
                EditorUtility.DisplayDialog("Шари генератора",
                    "Не можна видалити останній шар.", "OK");
                return;
            }

            var layer = _graphAsset.GetLayerById(_selectedLayerId);
            string layerName = layer != null ? layer.Name : _selectedLayerId;

            if (!EditorUtility.DisplayDialog("Видалити шар",
                    $"Видалити шар '{layerName}' разом з усіма його вузлами?", "Видалити", "Скасувати"))
                return;

            Undo.RecordObject(_graphAsset, "Remove Layer");
            _graphAsset.RemoveLayer(_selectedLayerId, true);
            EditorUtility.SetDirty(_graphAsset);

            _selectedLayerId = _graphAsset.Layers.Count > 0 ? _graphAsset.Layers[0].Id : null;
            TrySyncCompanionBlueprintLayers(false);
            if (_graphView != null)
            {
                CaptureCameraTransform();
                _graphView.SetActiveLayerWithoutRefresh(_selectedLayerId);
                _graphView.PopulateGraph(_graphAsset, EditorApplication.isPlaying);
                RestoreCameraTransform();
            }
            RebuildLayerList();
        }

        private void ConstructRightPanel()
        {
            _rightPanel = new ScrollView
            {
                style =
                {
                    width = 410,
                    minWidth = 340,
                    flexShrink = 0,
                    borderLeftWidth = 1,
                    borderLeftColor = new Color(0.26f, 0.28f, 0.32f),
                    backgroundColor = new Color(0.085f, 0.09f, 0.105f),
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 8,
                    paddingBottom = 10
                }
            };

            var tabHeaderRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            _tabSettingsButton = new Button(() => SetInspectorTab(InspectorTab.Settings))
            {
                text = "Загальні",
                tooltip = "Загальні налаштування графа або вибраного шару."
            };
            _tabSettingsButton.style.flexGrow = 1;
            _tabSettingsButton.style.marginRight = 4;
            tabHeaderRow.Add(_tabSettingsButton);

            _tabPreviewButton = new Button(() => SetInspectorTab(InspectorTab.Preview))
            {
                text = "Ноди",
                tooltip = "Налаштування вибраної ноди."
            };
            _tabPreviewButton.style.flexGrow = 1;
            _tabPreviewButton.style.marginRight = 4;
            tabHeaderRow.Add(_tabPreviewButton);
            _nodeInspectorSection = new VisualElement();
            _nodeInspectorSection.Add(tabHeaderRow);

            _tabSettingsContent = new VisualElement();
            _graphSettingsGui = new IMGUIContainer(DrawGeneralInspectorTab)
            {
                style = { marginBottom = 10 }
            };
            _tabSettingsContent.Add(_graphSettingsGui);
            _nodeInspectorSection.Add(_tabSettingsContent);

            _tabPreviewContent = new VisualElement();
            _nodeInspectorGui = new IMGUIContainer(DrawNodeInspectorTab)
            {
                style = { marginBottom = 10 }
            };
            _tabPreviewContent.Add(_nodeInspectorGui);

            _twcNodeInspectorGui = new VisualElement
            {
                style =
                {
                    marginBottom = 10,
                    display = DisplayStyle.None
                }
            };
            _tabPreviewContent.Add(_twcNodeInspectorGui);
            _nodeInspectorSection.Add(_tabPreviewContent);
            _rightPanel.Add(_nodeInspectorSection);
            ConstructValidationPanel();

            _contentContainer.Add(_rightPanel);
            SetInspectorVisible(_isInspectorVisible);
            UpdateInspectorTabVisibility();
        }

        private void ConstructValidationPanel()
        {
            _validationPanel = new VisualElement
            {
                style =
                {
                    marginTop = 10,
                    paddingTop = 6,
                    borderTopWidth = 1,
                    borderTopColor = new Color(0.24f, 0.24f, 0.24f)
                }
            };

            var headerRow = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 }
            };

            var title = new Label("Validation")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 }
            };
            headerRow.Add(title);

            var autoFixButton = new Button(ApplyValidationAutoFix)
            {
                text = "Auto Fix",
                tooltip = "Автоматично виправити прості проблеми: null-ноди, missing ids, invalid connections та orphan-ноди."
            };
            headerRow.Add(autoFixButton);
            _validationPanel.Add(headerRow);

            _validationIssuesContainer = new VisualElement();
            _validationPanel.Add(_validationIssuesContainer);
            _rightPanel.Add(_validationPanel);
            RenderValidationReport(null);
        }

        private void ConstructToolbar()
        {
            var toolbar = new Toolbar
            {
                style =
                {
                    flexWrap = Wrap.Wrap,
                    height = StyleKeyword.Auto,
                    minHeight = 24,
                    alignItems = Align.FlexStart,
                    paddingTop = 2,
                    paddingBottom = 2,
                    flexShrink = 0
                }
            };

            var assetField = new ObjectField("Graph")
            {
                objectType = typeof(GraphAsset),
                allowSceneObjects = false,
                style = { minWidth = 200 },
                tooltip = "Активний граф генерації, який відкритий у редакторі."
            };
            assetField.value = _graphAsset;
            assetField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is GraphAsset asset)
                {
                    LoadGraph(asset);
                    SaveWindowSettings();
                }
            });
            toolbar.Add(assetField);

            toolbar.Add(new ToolbarSpacer());

            // Preview Settings
            var settingsField = new ObjectField("Preview")
            {
                objectType = typeof(EditorPreviewSettings),
                allowSceneObjects = false,
                style = { minWidth = 160 },
                tooltip = "Набір налаштувань для прев'ю графа та тестового запуску."
            };
            settingsField.value = _previewSettings;
            settingsField.RegisterValueChangedCallback(evt =>
            {
                _previewSettings = evt.newValue as EditorPreviewSettings;
                if (_previewSettings != null)
                {
                    var path = AssetDatabase.GetAssetPath(_previewSettings);
                    _previewSettingsGuid = AssetDatabase.AssetPathToGUID(path);
                }
                else
                {
                    _previewSettingsGuid = null;
                }
                SaveWindowSettings();
            });
            toolbar.Add(settingsField);

            // Map size fields
            var widthField = new IntegerField("W")
            {
                value = _previewWidth,
                style = { width = 60 },
                tooltip = "Ширина карти для preview/run, якщо її не перевизначено в налаштуваннях графа."
            };
            widthField.RegisterValueChangedCallback(evt =>
                _previewWidth = Mathf.Max(4, evt.newValue));
            toolbar.Add(widthField);

            var heightField = new IntegerField("H")
            {
                value = _previewHeight,
                style = { width = 60 },
                tooltip = "Висота карти для preview/run, якщо її не перевизначено в налаштуваннях графа."
            };
            heightField.RegisterValueChangedCallback(evt =>
                _previewHeight = Mathf.Max(4, evt.newValue));
            toolbar.Add(heightField);

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(new ToolbarButton(() => ValidateGraph())
                {
                    text = "Validate",
                    tooltip = "Перевірити граф на помилки структури, типів і непідключені входи."
                });

            toolbar.Add(new ToolbarButton(() => RunGraph())
                {
                    text = "▶ Run",
                    tooltip = "Запустити граф з поточними preview-настройками."
                });

            var presetsMenu = new ToolbarMenu
            {
                text = "Presets",
                tooltip = "Швидко створити типовий шар або object-placement гілку для активного шару."
            };
            presetsMenu.menu.AppendAction("New Layer/Base Tile Layer", _ => ApplyLayerPreset("Base Tile Layer", new Color(0.48f, 0.62f, 0.36f)));
            presetsMenu.menu.AppendAction("New Layer/Shoreline", _ => ApplyLayerPreset("Shoreline", new Color(0.56f, 0.72f, 0.74f)));
            presetsMenu.menu.AppendAction("New Layer/Grass Small", _ => ApplyLayerPreset("Grass Small", new Color(0.42f, 0.58f, 0.28f)));
            presetsMenu.menu.AppendAction("New Layer/Bush", _ => ApplyLayerPreset("Bush", new Color(0.28f, 0.47f, 0.24f)));
            presetsMenu.menu.AppendAction("New Layer/Dry Grass", _ => ApplyLayerPreset("Dry Grass", new Color(0.62f, 0.58f, 0.34f)));
            presetsMenu.menu.AppendAction("New Layer/Rock", _ => ApplyLayerPreset("Rock", new Color(0.48f, 0.48f, 0.44f)));
            presetsMenu.menu.AppendAction("New Layer/Resource Item", _ => ApplyLayerPreset("Resource Item", new Color(0.68f, 0.50f, 0.32f)));
            presetsMenu.menu.AppendAction("New Layer/Tree Leaves", _ => ApplyLayerPreset("Tree Leaves", new Color(0.32f, 0.50f, 0.28f)));
            presetsMenu.menu.AppendSeparator();
            presetsMenu.menu.AppendAction("Active Layer/Add Grass Objects From Mask", _ => ApplyBranchPreset(GraphBuiltInPresetUtility.AddGrassObjectsBranch));
            presetsMenu.menu.AppendAction("Active Layer/Add Edge Objects", _ => ApplyBranchPreset(GraphBuiltInPresetUtility.AddEdgeObjectsBranch));
            presetsMenu.menu.AppendAction("Active Layer/Add Cluster Objects", _ => ApplyBranchPreset(GraphBuiltInPresetUtility.AddClusterObjectsBranch));
            presetsMenu.menu.AppendAction("Active Layer/Add Shoreline Decor", _ => ApplyBranchPreset(GraphBuiltInPresetUtility.AddShorelineDecorBranch));
            presetsMenu.menu.AppendAction("Active Layer/Add Resource Scatter", _ => ApplyBranchPreset(GraphBuiltInPresetUtility.AddResourceScatterBranch));
            toolbar.Add(presetsMenu);

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(new ToolbarButton(() => CleanGraph())
                {
                    text = "Clean",
                    tooltip = "Прибрати null-ноди й відновити зв'язки через відсутні проміжні вузли."
                });

            toolbar.Add(new ToolbarButton(() => _graphView?.AutoLayout())
                {
                    text = "Auto-Layout",
                    tooltip = "Автоматично розкласти ноди по шарах для читабельності графа."
                });

            toolbar.Add(new ToolbarButton(() => _graphView?.GroupSelection())
                {
                    text = "Group",
                    tooltip = "Згрупувати вибрані елементи у visual group."
                });

            toolbar.Add(new ToolbarButton(() => _graphView?.AddStickyNote())
                {
                    text = "Note",
                    tooltip = "Додати sticky note для коментарів у графі."
                });

            var inspectorToggle = new ToolbarToggle
            {
                text = "Inspector",
                value = _isInspectorVisible,
                tooltip = "Показати або сховати праву панель інспектора."
            };
            inspectorToggle.RegisterValueChangedCallback(evt =>
            {
                SetInspectorVisible(evt.newValue);
            });
            toolbar.Add(inspectorToggle);

            // Minimap toggle
            var minimapToggle = new ToolbarToggle
            {
                text = "Minimap",
                value = true,
                tooltip = "Показати або сховати мінікарту графа."
            };
            minimapToggle.RegisterValueChangedCallback(evt =>
                _graphView?.SetMinimapVisible(evt.newValue));
            toolbar.Add(minimapToggle);

            var inlinePreviewToggle = new ToolbarToggle
            {
                text = "Inline Preview",
                value = _showInlinePreviews,
                tooltip = "Показати або сховати inline preview усередині нод."
            };
            inlinePreviewToggle.RegisterValueChangedCallback(evt =>
            {
                _showInlinePreviews = evt.newValue;
                _graphView?.SetInlinePreviewsVisible(_showInlinePreviews);
            });
            toolbar.Add(inlinePreviewToggle);

            var autoRunToggle = new ToolbarToggle
            {
                text = "Auto Run",
                value = _autoRunOnChange,
                tooltip = "Автоматично перезапускати граф після змін."
            };
            autoRunToggle.RegisterValueChangedCallback(evt => _autoRunOnChange = evt.newValue);
            toolbar.Add(autoRunToggle);

            var previewModeField = new PopupField<string>(
                new List<string> { "64", "128", "Full" },
                Mathf.Clamp(_previewResolution, 0, 2))
            {
                label = "Preview",
                tooltip = "Роздільність прев'ю для швидкого перегляду результатів."
            };
            previewModeField.RegisterValueChangedCallback(_ =>
            {
                _previewResolution = previewModeField.index;
                RequestAutoRun();
            });
            toolbar.Add(previewModeField);

            var heatmapToggle = new ToolbarToggle
            {
                text = "Heatmap",
                value = _previewHeatmap,
                tooltip = "Показувати height map у heatmap-представленні."
            };
            heatmapToggle.RegisterValueChangedCallback(evt =>
            {
                _previewHeatmap = evt.newValue;
                RequestAutoRun();
            });
            toolbar.Add(heatmapToggle);

            toolbar.Add(new ToolbarButton(() => GraphPreviewWindow.Open(this))
                {
                    text = "Preview Window",
                    tooltip = "Відкрити окреме вікно великого прев'ю результату графа."
                });

            toolbar.Add(new ToolbarButton(() => _graphView?.ExportNodesToFile())
                {
                    text = "Export",
                    tooltip = "Експортувати вибрані ноди у файл."
                });

            toolbar.Add(new ToolbarButton(() => _graphView?.ImportNodesFromFile())
                {
                    text = "Import",
                    tooltip = "Імпортувати ноди з раніше експортованого файла."
                });

            toolbar.Add(new ToolbarSpacer { flex = true });

            toolbar.Add(new ToolbarButton(() => SaveGraph())
                {
                    text = "Save",
                    tooltip = "Зберегти зміни graph asset на диск."
                });

            rootVisualElement.Insert(0, toolbar);
        }

        private void ConstructStatusBar()
        {
            var statusContainer = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    bottom = 0,
                    left = 0,
                    right = 0,
                    height = 20,
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            _statusLabel = new Label("No graph loaded")
            {
                style =
                {
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingLeft = 8,
                    flexGrow = 1
                }
            };
            statusContainer.Add(_statusLabel);

            _progressBar = new ProgressBar
            {
                style =
                {
                    width = 150,
                    height = 14,
                    marginRight = 8
                }
            };
            _progressBar.visible = false;
            statusContainer.Add(_progressBar);

            rootVisualElement.Add(statusContainer);
        }

        public void LoadGraph(GraphAsset asset)
        {
            _graphAsset = asset;

            // Persist GUID for domain reload
            if (asset != null)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                _graphAssetGuid = AssetDatabase.AssetPathToGUID(path);
            }
            else
            {
                _graphAssetGuid = null;
            }

            MigrateLegacySharedSettingsNode(_graphAsset);
            GraphStaticNodeUtility.EnsureStaticNodes(_graphAsset);
            EnsureSelectedLayer();
            if (_graphView != null)
                _graphView.SetActiveLayerWithoutRefresh(_selectedLayerId);
            SanitizeGraphAsset(false);
            TrySyncCompanionBlueprintLayers(false);
            _graphView.PopulateGraph(asset, EditorApplication.isPlaying);
            _graphView.SetInlinePreviewsVisible(_showInlinePreviews);
            SetSelectedNode(null);
            RefreshInspectorPanel();
            RebuildLayerList();
            UpdateStatusBar();
        }

        public GraphAsset GraphAsset => _graphAsset;

        private void UpdateStatusBar()
        {
            if (_statusLabel == null) return;

            if (_graphAsset == null)
            {
                _statusLabel.text = "No graph loaded";
                return;
            }

            string mode = EditorApplication.isPlaying ? " | READ-ONLY (Play Mode)" : "";
            _statusLabel.text =
                $"Graph: {_graphAsset.name} | Nodes: {_graphAsset.Nodes.Count} | Connections: {_graphAsset.Connections.Count}{mode}";
        }

        private void CleanGraph()
        {
            if (_graphAsset == null)
            {
                EditorUtility.DisplayDialog("Clean Graph",
                    "No graph loaded.", "OK");
                return;
            }

            int repaired = _graphAsset.RepairMissingNodeConnections();
            int removed = _graphAsset.RemoveNullNodes();
            if (repaired == 0 && removed == 0)
            {
                _statusLabel.text = "✓ Graph is clean — no null nodes found.";
                return;
            }

            AssetDatabase.SaveAssets();
            RefreshGraphViewFromAsset();
            _statusLabel.text = $"✓ Repaired {repaired} broken chain(s), removed {removed} null node(s).";
            Debug.Log($"[GraphCleaner] Repaired {repaired} broken chain(s) and removed {removed} null node(s) from '{_graphAsset.name}'.");
        }

        private void ValidateGraph()
        {
            if (_graphAsset == null)
            {
                EditorUtility.DisplayDialog("Validation",
                    "No graph loaded.", "OK");
                return;
            }

            SanitizeGraphAsset(true);
            TrySyncCompanionBlueprintLayers(false);

            var validator = new GraphValidator();
            var report = MergeValidationReports(
                validator.ValidateDetailed(_graphAsset),
                _lastBlueprintLayerSyncReport);
            RenderValidationReport(report);
            int errorCount = report.ErrorCount;
            int warningCount = report.WarningCount;

            if (report.Issues.Count == 0)
            {
                _statusLabel.text = "✓ Validation passed — no errors.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var issue in report.Issues)
                sb.AppendLine(issue.ToString());

            _statusLabel.text = $"Validation: {errorCount} error(s), {warningCount} warning(s).";
            Debug.LogWarning($"[GraphValidator] {report.Issues.Count} issue(s):\n{sb}");
        }

        private void RenderValidationReport(GraphValidationReport report)
        {
            _lastValidationReport = report;
            RebuildLayerList();

            if (_validationIssuesContainer == null)
                return;

            _validationIssuesContainer.Clear();
            if (report == null || report.Issues.Count == 0)
            {
                var empty = new Label("Немає актуальних помилок. Натисніть Validate.")
                {
                    style =
                    {
                        color = new Color(0.72f, 0.72f, 0.72f),
                        whiteSpace = WhiteSpace.Normal
                    }
                };
                _validationIssuesContainer.Add(empty);
                return;
            }

            var summary = new Label($"{report.ErrorCount} error(s), {report.WarningCount} warning(s)")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 4,
                    color = report.HasErrors
                        ? new Color(1f, 0.48f, 0.42f)
                        : new Color(1f, 0.78f, 0.32f)
                }
            };
            _validationIssuesContainer.Add(summary);

            foreach (var issue in report.Issues.Take(12))
            {
                var issueButton = new Button(() => FocusValidationIssue(issue))
                {
                    text = FormatValidationIssue(issue),
                    tooltip = issue.Message
                };
                issueButton.style.unityTextAlign = TextAnchor.MiddleLeft;
                issueButton.style.whiteSpace = WhiteSpace.Normal;
                issueButton.style.marginBottom = 2;
                issueButton.style.backgroundColor = issue.Severity == ValidationSeverity.Error
                    ? new Color(0.34f, 0.14f, 0.12f)
                    : new Color(0.30f, 0.23f, 0.10f);
                _validationIssuesContainer.Add(issueButton);
            }

            if (report.Issues.Count > 12)
            {
                _validationIssuesContainer.Add(new Label($"+ {report.Issues.Count - 12} more in Console")
                {
                    style = { color = new Color(0.72f, 0.72f, 0.72f) }
                });
            }
        }

        private void RenderPresetResult(GraphPresetApplyResult result)
        {
            if (result == null)
                return;

            RenderValidationReport(result.ValidationReport);

            if (_validationIssuesContainer != null)
            {
                var presetBox = new VisualElement
                {
                    style =
                    {
                        paddingLeft = 6,
                        paddingRight = 6,
                        paddingTop = 5,
                        paddingBottom = 5,
                        marginBottom = 6,
                        backgroundColor = result.Success
                            ? new Color(0.12f, 0.26f, 0.16f)
                            : new Color(0.32f, 0.16f, 0.12f)
                    }
                };

                presetBox.Add(new Label(result.Message ?? "Preset applied.")
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        whiteSpace = WhiteSpace.Normal,
                        color = result.Success
                            ? new Color(0.70f, 1f, 0.72f)
                            : new Color(1f, 0.62f, 0.48f)
                    }
                });

                string connectionText = $"Connections created: {result.CreatedConnections.Count}";
                presetBox.Add(new Label(connectionText)
                {
                    style = { color = new Color(0.78f, 0.78f, 0.78f) }
                });

                foreach (var warning in result.Warnings.Take(4))
                {
                    presetBox.Add(new Label("WARN " + warning)
                    {
                        style =
                        {
                            color = new Color(1f, 0.78f, 0.36f),
                            whiteSpace = WhiteSpace.Normal
                        }
                    });
                }

                foreach (var node in result.CreatedNodes.Take(8))
                {
                    if (node == null)
                        continue;

                    var nodeId = node.NodeId;
                    var button = new Button(() => _graphView?.FocusNode(nodeId))
                    {
                        text = "+ " + node.Title,
                        tooltip = nodeId
                    };
                    button.style.unityTextAlign = TextAnchor.MiddleLeft;
                    presetBox.Add(button);
                }

                _validationIssuesContainer.Insert(0, presetBox);
            }

            _statusLabel.text = result.Message ?? "Preset applied.";

            if (result.ValidationReport?.Issues.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine(result.Message);
                foreach (var issue in result.ValidationReport.Issues)
                    sb.AppendLine("  - " + issue);
                Debug.LogWarning($"[GraphPreset] {sb}");
            }
            else
            {
                Debug.Log($"[GraphPreset] {result.Message}");
            }
        }

        private static string FormatValidationIssue(GraphValidationIssue issue)
        {
            if (issue == null)
                return string.Empty;

            string icon = issue.Severity == ValidationSeverity.Error ? "ERR" : "WARN";
            return $"{icon} {issue.Code}: {issue.Message}";
        }

        private Dictionary<string, GraphValidationIssue> BuildLayerSyncIssueLookup()
        {
            var result = new Dictionary<string, GraphValidationIssue>();
            AddLayerSyncIssues(result, _lastBlueprintLayerSyncReport);
            AddLayerSyncIssues(result, _lastValidationReport);
            return result;
        }

        private static void AddLayerSyncIssues(
            Dictionary<string, GraphValidationIssue> target,
            GraphValidationReport report)
        {
            if (target == null || report?.Issues == null)
                return;

            foreach (var issue in report.Issues)
            {
                if (issue == null
                    || issue.Severity != ValidationSeverity.Error
                    || string.IsNullOrEmpty(issue.LayerId)
                    || string.IsNullOrEmpty(issue.Code))
                    continue;

                if (!target.ContainsKey(issue.LayerId))
                    target.Add(issue.LayerId, issue);
            }
        }

        private void TrySyncCompanionBlueprintLayers(bool logWarnings)
        {
            _lastBlueprintLayerSyncReport = null;
            if (_graphAsset == null)
                return;

            var type = ResolveGraphBuildLayerStoreType();
            if (type == null)
            {
                _lastBlueprintLayerSyncReport = CreateBlueprintSyncFailureReport(
                    "BLUEPRINT_SYNC_MODULE_MISSING",
                    "Модуль Generator.Editor недоступний, тому companion Blueprint Layers не можуть бути синхронізовані.");
                return;
            }

            var syncMethod = type.GetMethod(
                "Sync",
                BindingFlags.Public | BindingFlags.Static);
            if (syncMethod == null)
            {
                _lastBlueprintLayerSyncReport = CreateBlueprintSyncFailureReport(
                    "BLUEPRINT_SYNC_METHOD_MISSING",
                    "GraphBuildLayerStore.Sync не знайдено.");
                return;
            }

            object config = null;
            try
            {
                config = syncMethod.Invoke(null, new object[] { _graphAsset });
            }
            catch (Exception e)
            {
                var error = UnwrapReflectionException(e);
                _lastBlueprintLayerSyncReport = CreateBlueprintSyncFailureReport(
                    "BLUEPRINT_SYNC_FAILED",
                    "Не вдалося синхронізувати companion Blueprint Layers: " + error.Message);
                if (logWarnings)
                    Debug.LogWarning("[GraphEditorWindow] " + error);
                return;
            }

            _lastBlueprintLayerSyncReport = ValidateCompanionBlueprintLayerSync(type, config);
        }

        private GraphValidationReport ValidateCompanionBlueprintLayerSync(Type storeType, object config)
        {
            if (storeType == null)
                return null;

            var validateMethod = storeType.GetMethod(
                "ValidateBlueprintLayerSync",
                BindingFlags.Public | BindingFlags.Static);
            if (validateMethod == null)
            {
                return CreateBlueprintSyncFailureReport(
                    "BLUEPRINT_SYNC_VALIDATE_MISSING",
                    "GraphBuildLayerStore.ValidateBlueprintLayerSync не знайдено.");
            }

            try
            {
                return validateMethod.Invoke(null, new[] { _graphAsset, config }) as GraphValidationReport
                       ?? CreateBlueprintSyncFailureReport(
                           "BLUEPRINT_SYNC_VALIDATE_EMPTY",
                           "ValidateBlueprintLayerSync не повернув GraphValidationReport.");
            }
            catch (Exception e)
            {
                var error = UnwrapReflectionException(e);
                return CreateBlueprintSyncFailureReport(
                    "BLUEPRINT_SYNC_VALIDATE_FAILED",
                    "Не вдалося перевірити companion Blueprint Layers: " + error.Message);
            }
        }

        private static GraphValidationReport CreateBlueprintSyncFailureReport(string code, string message)
        {
            var report = new GraphValidationReport();
            report.Add(new GraphValidationIssue(
                code,
                ValidationSeverity.Error,
                message));
            return report;
        }

        private static Exception UnwrapReflectionException(Exception exception)
        {
            return exception is TargetInvocationException { InnerException: not null }
                ? exception.InnerException
                : exception;
        }

        private static GraphValidationReport MergeValidationReports(params GraphValidationReport[] reports)
        {
            var merged = new GraphValidationReport();
            if (reports == null)
                return merged;

            foreach (var report in reports)
            {
                if (report?.Issues == null)
                    continue;

                merged.AddRange(report.Issues);
            }

            return merged;
        }

        private void FocusValidationIssue(GraphValidationIssue issue)
        {
            if (_graphAsset == null || issue == null)
                return;

            if (!string.IsNullOrEmpty(issue.NodeId))
            {
                var node = _graphAsset.GetNodeById(issue.NodeId);
                if (node != null && !GraphAsset.IsGlobalNode(node))
                {
                    SelectLayer(node.LayerId);
                }

                _graphView?.FocusNode(issue.NodeId);
                return;
            }

            if (!string.IsNullOrEmpty(issue.LayerId)
                && _graphAsset.GetLayerById(issue.LayerId) != null)
                SelectLayer(issue.LayerId);
        }

        private void ApplyValidationAutoFix()
        {
            if (_graphAsset == null)
                return;

            Undo.RegisterCompleteObjectUndo(_graphAsset, "Auto Fix Graph Validation");
            foreach (var node in _graphAsset.Nodes)
            {
                if (node != null)
                    Undo.RecordObject(node, "Auto Fix Graph Validation");
            }

            int fixedCount = 0;

            // IDs are internal implementation details. They must be repaired before any other fix,
            // because duplicate NodeId values make connections, validation focus and layer-state sync ambiguous.
            fixedCount += _graphAsset.NormalizeGraphIds();
            fixedCount += _graphAsset.RepairMissingNodeConnections();
            fixedCount += _graphAsset.RemoveNullNodes();
            fixedCount += NormalizeDuplicateNodeIds();
            fixedCount += NormalizeDuplicateConnectionIds();
            fixedCount += AssignInvalidLayerNodesToActiveLayer();
            fixedCount += AutoFixTileSettingsOutputKinds();
            fixedCount += CreateMissingLayerOutputs();
            fixedCount += RemoveInvalidConnections();

            _graphAsset.EnsureLayerGraphStates();

            // Blueprint/order mismatch is not a user-editable error. Auto Fix should run the same sync
            // that the validation text asks the user to run manually.
            TrySyncCompanionBlueprintLayers(true);

            fixedCount += _graphAsset.NormalizeGraphIds();
            _graphAsset.EnsureLayerGraphStates();
            EditorUtility.SetDirty(_graphAsset);
            AssetDatabase.SaveAssets();

            RefreshGraphViewFromAsset();
            RebuildLayerList();
            RefreshInspectorPanel();

            var validator = new GraphValidator();
            var report = validator.ValidateDetailed(_graphAsset);
            RenderValidationReport(report);

            _statusLabel.text = fixedCount > 0
                ? $"✓ Auto Fix applied {fixedCount} change(s)."
                : "Auto Fix: graph ids/output/sync checked.";
        }

        private int NormalizeDuplicateNodeIds()
        {
            var seen = new HashSet<string>();
            int changed = 0;
            foreach (var node in _graphAsset.Nodes)
            {
                if (node == null)
                    continue;

                string id = node.NodeId;
                if (!string.IsNullOrEmpty(id) && seen.Add(id))
                    continue;

                do
                {
                    node.NodeId = Guid.NewGuid().ToString();
                    id = node.NodeId;
                }
                while (string.IsNullOrEmpty(id) || !seen.Add(id));

                EditorUtility.SetDirty(node);
                changed++;
            }

            if (changed > 0)
                EditorUtility.SetDirty(_graphAsset);

            return changed;
        }

        private int NormalizeDuplicateConnectionIds()
        {
            var seen = new HashSet<string>();
            int changed = 0;
            foreach (var connection in _graphAsset.Connections)
            {
                if (connection == null)
                    continue;

                string id = connection.ConnectionId;
                if (!string.IsNullOrEmpty(id) && seen.Add(id))
                    continue;

                do
                {
                    connection.ResetConnectionId();
                    id = connection.ConnectionId;
                }
                while (string.IsNullOrEmpty(id) || !seen.Add(id));

                changed++;
            }

            if (changed > 0)
                EditorUtility.SetDirty(_graphAsset);

            return changed;
        }

        private int AssignInvalidLayerNodesToActiveLayer()
        {
            string targetLayerId = !string.IsNullOrEmpty(_selectedLayerId)
                && _graphAsset.GetLayerById(_selectedLayerId) != null
                    ? _selectedLayerId
                    : _graphAsset.EnsureDefaultLayer();
            var validLayers = new HashSet<string>(_graphAsset.Layers.Where(layer => layer != null).Select(layer => layer.Id));
            int changed = 0;

            foreach (var node in _graphAsset.Nodes)
            {
                if (node == null || GraphAsset.IsGlobalNode(node))
                    continue;

                if (!string.IsNullOrEmpty(node.LayerId) && validLayers.Contains(node.LayerId))
                    continue;

                node.LayerId = targetLayerId;
                changed++;
            }

            return changed;
        }

        private int AutoFixTileSettingsOutputKinds()
        {
            int changed = 0;
            foreach (var layer in _graphAsset.Layers)
            {
                if (layer == null || !layer.Enabled)
                    continue;

                var nodes = _graphAsset.GetNodesForLayer(layer.Id);
                var output = nodes.OfType<OutputNode>().FirstOrDefault();
                if (output == null)
                    continue;

                bool hasRenderableTiles = TileSettingsNode.HasRenderableTiles(_graphAsset, layer.Id);
                bool hasAnyTileSettings = nodes.Any(node => node is TileSettingsNode);

                // If the user placed a configured TileSettingsNode, the layer is explicitly renderable.
                // Output Kind must follow that so TileSettings is not silently ignored.
                if (hasRenderableTiles && output.OutputKind != LayerOutputKind.Tiles)
                {
                    output.OutputKind = LayerOutputKind.Tiles;
                    EditorUtility.SetDirty(output);
                    changed++;
                    continue;
                }

                // A layer without renderable TileSettings is a helper/data layer. Do not force Tiles.
                // If it currently exposes a bool mask, keep it as Masks so Layer Ref can consume it.
                if (!hasAnyTileSettings && output.OutputKind == LayerOutputKind.Tiles)
                {
                    var layerConnections = _graphAsset.GetConnectionsForLayer(layer.Id, includeGlobal: false);
                    bool hasMaskInput = layerConnections.Any(connection =>
                        connection != null
                        && connection.TargetNodeId == output.NodeId
                        && connection.TargetPortIndex == OutputNode.MaskInputIndex);

                    if (hasMaskInput)
                    {
                        output.OutputKind = LayerOutputKind.Masks;
                        EditorUtility.SetDirty(output);
                        changed++;
                    }
                }
            }

            return changed;
        }

        private int CreateMissingLayerOutputs()
        {
            int changed = 0;
            foreach (var layer in _graphAsset.Layers)
            {
                if (layer == null || !layer.Enabled)
                    continue;

                var nodes = _graphAsset.GetNodesForLayer(layer.Id);
                bool hasObjectLayerNode = nodes.Any(node => node is ObjectLayerNode);
                bool hasObjectOutputNode = nodes.Any(node => node is ObjectOutputToTWCNode);

                if (hasObjectLayerNode && !hasObjectOutputNode)
                {
                    var objectLayer = nodes.OfType<ObjectLayerNode>().FirstOrDefault();
                    var output = _graphAsset.AddNode(typeof(ObjectOutputToTWCNode), false, layer.Id);
                    if (output != null)
                    {
                        output.EditorPosition = (objectLayer?.EditorPosition ?? Vector2.zero) + new Vector2(280f, 0f);
                        if (objectLayer != null)
                            _graphAsset.AddConnection(objectLayer.NodeId, 0, output.NodeId, 0);
                        changed++;
                    }
                }

                nodes = _graphAsset.GetNodesForLayer(layer.Id);
                var outputNodes = nodes.OfType<OutputNode>().ToList();
                if (outputNodes.Count > 1)
                    continue;

                var layerOutput = outputNodes.FirstOrDefault();
                if (layerOutput == null)
                {
                    layerOutput = _graphAsset.AddNode(typeof(OutputNode), false, layer.Id) as OutputNode;
                    if (layerOutput != null)
                    {
                        layerOutput.EditorPosition = ResolveNewLayerOutputPosition(nodes);
                        changed++;
                    }
                }

                if (layerOutput == null)
                    continue;

                var layerConnections = _graphAsset.GetConnectionsForLayer(layer.Id, includeGlobal: false);
                bool hasIncomingOutput = layerConnections.Any(connection =>
                    connection != null && connection.TargetNodeId == layerOutput.NodeId);
                if (hasIncomingOutput)
                    continue;

                if (TryFindLayerOutputSource(
                    nodes,
                    layerConnections,
                    layerOutput,
                    out var source,
                    out int sourcePort,
                    out int targetPort,
                    out var outputKind))
                {
                    if (outputKind == LayerOutputKind.Tiles && !TileSettingsNode.HasRenderableTiles(_graphAsset, layer.Id))
                        outputKind = LayerOutputKind.Masks;

                    layerOutput.OutputKind = outputKind;
                    _graphAsset.AddConnection(source.NodeId, sourcePort, layerOutput.NodeId, targetPort);
                    if (layerOutput.EditorPosition == Vector2.zero)
                        layerOutput.EditorPosition = source.EditorPosition + new Vector2(320f, 0f);
                    changed++;
                }
            }

            return changed;
        }

        private static Vector2 ResolveNewLayerOutputPosition(IReadOnlyList<NodeBase> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return new Vector2(260f, 120f);

            var rightMost = nodes
                .Where(node => node != null)
                .OrderByDescending(node => node.EditorPosition.x)
                .FirstOrDefault();

            return rightMost != null
                ? rightMost.EditorPosition + new Vector2(320f, 0f)
                : new Vector2(260f, 120f);
        }

        private static bool TryFindLayerOutputSource(
            IReadOnlyList<NodeBase> nodes,
            IReadOnlyList<Connection> connections,
            OutputNode output,
            out NodeBase source,
            out int sourcePort,
            out int targetPort,
            out LayerOutputKind outputKind)
        {
            source = null;
            sourcePort = -1;
            targetPort = -1;
            outputKind = LayerOutputKind.Other;

            if (nodes == null || output == null)
                return false;

            var candidates = nodes
                .Where(node => node != null && node != output)
                .OrderByDescending(node => node.EditorPosition.x)
                .ToList();

            if (TrySelectLayerOutputSource(
                candidates,
                connections,
                output,
                node => node is ObjectOutputToTWCNode,
                port => true,
                OutputNode.DataInputIndex,
                LayerOutputKind.Objects,
                out source,
                out sourcePort,
                out targetPort,
                out outputKind))
                return true;

            if (TrySelectLayerOutputSource(
                candidates,
                connections,
                output,
                node => node is TwcModifierNode,
                port => port.ValueType == typeof(bool[,]),
                OutputNode.MaskInputIndex,
                LayerOutputKind.Masks,
                out source,
                out sourcePort,
                out targetPort,
                out outputKind))
                return true;

            if (TrySelectLayerOutputSource(
                candidates,
                connections,
                output,
                node => true,
                port => port.ValueType == typeof(bool[,]),
                OutputNode.MaskInputIndex,
                LayerOutputKind.Masks,
                out source,
                out sourcePort,
                out targetPort,
                out outputKind))
                return true;

            foreach (var candidate in candidates)
            {
                var outputs = candidate.Outputs;
                if (outputs == null)
                    continue;

                for (int i = 0; i < outputs.Length; i++)
                {
                    if (!IsTerminalOutput(candidate, i, connections, output))
                        continue;

                    if (!TryResolveOutputTarget(output, outputs[i], out targetPort, out outputKind))
                        continue;

                    source = candidate;
                    sourcePort = i;
                    return true;
                }
            }

            return false;
        }

        private static bool TrySelectLayerOutputSource(
            IReadOnlyList<NodeBase> candidates,
            IReadOnlyList<Connection> connections,
            OutputNode output,
            Func<NodeBase, bool> nodePredicate,
            Func<PortDefinition, bool> portPredicate,
            int preferredTargetPort,
            LayerOutputKind preferredOutputKind,
            out NodeBase source,
            out int sourcePort,
            out int targetPort,
            out LayerOutputKind outputKind)
        {
            source = null;
            sourcePort = -1;
            targetPort = -1;
            outputKind = preferredOutputKind;

            foreach (var candidate in candidates)
            {
                if (candidate == null || !nodePredicate(candidate))
                    continue;

                var outputs = candidate.Outputs;
                if (outputs == null)
                    continue;

                for (int i = 0; i < outputs.Length; i++)
                {
                    if (!portPredicate(outputs[i]))
                        continue;
                    if (!IsOutputConnectionCompatible(output, outputs[i], preferredTargetPort))
                        continue;
                    if (!IsTerminalOutput(candidate, i, connections, output))
                        continue;

                    source = candidate;
                    sourcePort = i;
                    targetPort = preferredTargetPort;
                    return true;
                }
            }

            return false;
        }

        private static bool TryResolveOutputTarget(
            OutputNode output,
            PortDefinition sourcePort,
            out int targetPort,
            out LayerOutputKind outputKind)
        {
            if (sourcePort?.ValueType == typeof(string[,]))
            {
                targetPort = OutputNode.BiomeMapInputIndex;
                outputKind = LayerOutputKind.Tiles;
                return IsOutputConnectionCompatible(output, sourcePort, targetPort);
            }

            if (sourcePort?.ValueType == typeof(float[,]))
            {
                targetPort = OutputNode.HeightMapInputIndex;
                outputKind = LayerOutputKind.Tiles;
                return IsOutputConnectionCompatible(output, sourcePort, targetPort);
            }

            if (sourcePort?.ValueType == typeof(bool[,]))
            {
                targetPort = OutputNode.MaskInputIndex;
                outputKind = LayerOutputKind.Masks;
                return IsOutputConnectionCompatible(output, sourcePort, targetPort);
            }

            targetPort = OutputNode.DataInputIndex;
            outputKind = LayerOutputKind.Other;
            return IsOutputConnectionCompatible(output, sourcePort, targetPort);
        }

        private static bool IsOutputConnectionCompatible(
            OutputNode output,
            PortDefinition sourcePort,
            int targetPort)
        {
            var inputs = output?.Inputs;
            if (sourcePort == null || inputs == null || targetPort < 0 || targetPort >= inputs.Length)
                return false;

            return PortDefinition.AreValueTypesCompatible(sourcePort.ValueType, inputs[targetPort].ValueType);
        }

        private static bool IsTerminalOutput(
            NodeBase node,
            int outputPort,
            IReadOnlyList<Connection> connections,
            OutputNode layerOutput)
        {
            if (node == null || connections == null)
                return true;

            return !connections.Any(connection =>
                connection != null
                && connection.SourceNodeId == node.NodeId
                && connection.SourcePortIndex == outputPort
                && connection.TargetNodeId != layerOutput?.NodeId);
        }

        private int RemoveInvalidConnections()
        {
            var validator = new GraphValidator();
            var report = validator.ValidateDetailed(_graphAsset);
            var removableConnectionIds = new HashSet<string>(
                report.Issues
                    .Where(issue => issue.CanAutoFix
                        && !string.IsNullOrEmpty(issue.ConnectionId)
                        && issue.Severity == ValidationSeverity.Error)
                    .Select(issue => issue.ConnectionId));

            if (removableConnectionIds.Count == 0)
                return 0;

            int removed = 0;
            foreach (var connection in _graphAsset.Connections.ToList())
            {
                if (connection != null && removableConnectionIds.Contains(connection.ConnectionId))
                {
                    _graphAsset.RemoveConnection(connection);
                    removed++;
                }
            }

            return removed;
        }

        private void SaveGraph()
        {
            if (_graphAsset == null) return;

            EditorUtility.SetDirty(_graphAsset);
            AssetDatabase.SaveAssets();
            _statusLabel.text = $"Saved: {_graphAsset.name}";
        }

        private void RunGraph(bool isAutoRun = false)
        {
            if (_isRunningGraph) return;
            _isRunningGraph = true;

            if (_graphAsset == null)
            {
                EditorUtility.DisplayDialog("Run Graph",
                    "No graph loaded.", "OK");
                _isRunningGraph = false;
                return;
            }

            _progressBar.visible = true;
            _progressBar.value = 0;
            _statusLabel.text = "Running graph...";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var prevRandomState = UnityEngine.Random.state;

            try
            {
                SanitizeGraphAsset(true);
                TrySyncCompanionBlueprintLayers(false);

                // Validate first. Global errors stop the run; layer errors are skipped by the run policy.
                var validator = new GraphValidator();
                var validationReport = MergeValidationReports(
                    validator.ValidateDetailed(_graphAsset),
                    _lastBlueprintLayerSyncReport);
                RenderValidationReport(validationReport);
                int errorCount = validationReport.ErrorCount;
                int warningCount = validationReport.WarningCount;
                int globalErrorCount = validationReport.Issues.Count(issue =>
                    issue.Severity == ValidationSeverity.Error && string.IsNullOrEmpty(issue.LayerId));
                var invalidLayerIds = new HashSet<string>(
                    validationReport.Issues
                        .Where(issue => issue.Severity == ValidationSeverity.Error && !string.IsNullOrEmpty(issue.LayerId))
                        .Select(issue => issue.LayerId));

                if (globalErrorCount > 0)
                {
                    _statusLabel.text = $"✗ Cannot run: {globalErrorCount} global validation error(s).";

                    if (!isAutoRun)
                    {
                        var details = new StringBuilder();
                        foreach (var issue in validationReport.Issues.Where(issue => issue.Severity == ValidationSeverity.Error))
                            details.AppendLine($"  - {issue}");

                        Debug.LogWarning($"[GraphRunner] Validation failed with {errorCount} error(s) and {warningCount} warning(s).\n{details}");
                    }

                    return;
                }

                if (warningCount > 0 && !isAutoRun)
                {
                    Debug.LogWarning($"[GraphRunner] Running with {warningCount} validation warning(s).");

                    var warningDetails = new StringBuilder();
                    foreach (var warning in validationReport.Issues.Where(e => e.Severity == ValidationSeverity.Warning))
                        warningDetails.AppendLine($"  - {warning}");

                    Debug.LogWarning($"[GraphRunner] Validation warnings:\n{warningDetails}");
                }

                var runtimeSettings = ResolveRuntimeExecutionSettings();

                // Determine map size: Runtime(GridInstaller) > PreviewSettings > SharedSettings > inline fields
                int mapW = _previewWidth;
                int mapH = _previewHeight;
                if (_previewSettings != null)
                {
                    mapW = _previewSettings.PreviewWidth;
                    mapH = _previewSettings.PreviewHeight;
                }

                if (runtimeSettings.HasGridSize)
                {
                    mapW = runtimeSettings.GridWidth;
                    mapH = runtimeSettings.GridHeight;
                }

                var sharedSettings = _graphAsset.SharedSettings;
                if (sharedSettings != null && sharedSettings.HasMapSize)
                {
                    mapW = sharedSettings.MapWidth;
                    mapH = sharedSettings.MapHeight;
                }

                int seed = GetSeedFromGraph();
                GlobalSeed.Set(seed);
                _statusLabel.text = $"Running graph... (seed {seed})";

                // Save previous Unity random state and set deterministic seed for UnityEngine.Random
                UnityEngine.Random.InitState(seed);

                var layerDataList = new List<WorldLayerData>();
                var layerMaskRegistry = new LayerMaskRegistry();
                LayerMaskPrewarmUtility.PrewarmAllLayerMasks(
                    _graphAsset,
                    seed,
                    new Vector2Int(mapW, mapH),
                    layerMaskRegistry,
                    context => RegisterEditorServices(context, runtimeSettings, false),
                    invalidLayerIds);

                var runner = new GraphRunner();
                var scopes = _graphAsset.CreateEnabledLayerExecutionScopes();
                var runReport = new GraphRunReport(
                    seed,
                    mapW,
                    mapH,
                    LayerRunFailurePolicy.SkipInvalidLayersAndContinueFailures);
                var layerResults = new List<GraphExecutionResult>();
                GraphExecutionResult previewResult = null;

                for (int i = 0; i < scopes.Count; i++)
                {
                    var scope = scopes[i];
                    var layer = _graphAsset.GetLayerById(scope.LayerId);
                    string layerName = layer?.Name ?? scope.LayerId ?? "Global";
                    var record = new LayerRunRecord
                    {
                        LayerId = scope.LayerId,
                        LayerName = layerName,
                        SortingOrder = layer?.SortingOrder ?? i,
                        Status = LayerRunStatus.Pending,
                        GraphId = scope.GraphId
                    };
                    runReport.Layers.Add(record);

                    if (invalidLayerIds.Contains(scope.LayerId))
                    {
                        record.Status = LayerRunStatus.SkippedValidation;
                        record.Message = "Validation errors in this layer.";
                        _progressBar.value = scopes.Count == 0 ? 1f : (float)(i + 1) / scopes.Count;
                        continue;
                    }

                    try
                    {
                        record.Status = LayerRunStatus.Prepared;
                        var context = CreateEditorNodeContext(seed, mapW, mapH, sharedSettings, runtimeSettings, layerDataList, layerMaskRegistry);
                        var result = runner.Execute(scope, context);
                        layerResults.Add(result);
                        record.GraphId = result.GraphId;
                        record.NodeCount = result.Logs.Count;
                        record.NodeTimeMs = 0f;
                        foreach (var log in result.Logs)
                            record.NodeTimeMs += log.DurationMs;

                        if (previewResult == null || scope.LayerId == _selectedLayerId)
                            previewResult = result;

                        if (result.Success)
                        {
                            record.Status = LayerRunStatus.Generated;
                            record.Message = "Generated.";
                        }
                        else
                        {
                            record.Status = LayerRunStatus.Failed;
                            record.Message = result.ErrorMessage;
                            record.ErrorNodeId = result.ErrorNodeId;
                        }
                    }
                    catch (Exception ex)
                    {
                        record.Status = LayerRunStatus.Failed;
                        record.Message = ex.Message;
                        Debug.LogException(ex);
                    }

                    _progressBar.value = scopes.Count == 0 ? 1f : (float)(i + 1) / scopes.Count;
                }

                var aggregateResult = layerResults.Count > 0
                    ? GraphExecutionResult.Combine(layerResults)
                    : previewResult;

                int previewSize = ResolvePreviewSize(mapW, mapH);
                _graphView?.UpdateNodePreviews(aggregateResult, _previewSettings, layerDataList, previewSize, _previewHeatmap);
                _lastResult = aggregateResult;
                RebuildLayerPreviews();
                GraphPreviewWindow.RequestRepaint();

                sw.Stop();
                _statusLabel.text = runReport.BuildStatusText(sw.ElapsedMilliseconds, layerDataList.Count);

                if (!isAutoRun || runReport.FailedCount > 0 || runReport.SkippedCount > 0)
                    Debug.Log($"[GraphRunner] Layer execution summary:\n{runReport.BuildConsoleSummary()}");

                if (aggregateResult != null)
                    HighlightExecutionResults(aggregateResult);

            }
            catch (Exception ex)
            {
                sw.Stop();
                _statusLabel.text = $"✗ Run failed: {ex.Message}";
                Debug.LogException(ex);
            }
            finally
            {
                _progressBar.visible = false;
                UnityEngine.Random.state = prevRandomState;
                _isRunningGraph = false;
            }
        }

        internal bool TryGetBestPreview(out Texture2D previewTexture, out string status)
        {
            if (_graphView != null && _graphView.TryGetBestPreview(out previewTexture, out status))
                return true;

            // Фолбек: складене ізометричне 3D-превʼю шарів (TWC-конвеєр).
            if (_layerCompositeTexture != null)
            {
                previewTexture = _layerCompositeTexture;
                status = "Ізометричне превʼю шарів";
                return true;
            }

            previewTexture = null;
            status = "Graph view is not ready";
            return false;
        }

        internal bool TryGetBestRawMaps(out float[,] floatMap, out string[,] tileMap)
        {
            if (_graphView != null)
                return _graphView.TryGetBestRawMaps(out floatMap, out tileMap);

            floatMap = null;
            tileMap  = null;
            return false;
        }

        /// <summary>
        /// Реєструє сервіси генератора з EditorPreviewSettings.
        /// Кожен сервіс реєструється опціонально — якщо ScriptableObject не задано,
        /// лог попереджує, але Run не зупиняється (вузли самі отримають помилку при GetService).
        /// </summary>
        private NodeContext CreateEditorNodeContext(
            int seed,
            int mapW,
            int mapH,
            GraphSharedSettings sharedSettings,
            RuntimeExecutionSettings runtimeSettings,
            List<WorldLayerData> layerDataList,
            LayerMaskRegistry layerMaskRegistry)
        {
            var context = new NodeContext(seed, CancellationToken.None)
            {
                MapSize = new Vector2Int(mapW, mapH)
            };

            if (sharedSettings != null)
            {
                context.ApplySharedSettings(sharedSettings);
                context.RegisterService(sharedSettings);
            }

            RegisterEditorServices(context, runtimeSettings);

            if (layerDataList != null)
                context.RegisterService(layerDataList);
            if (layerMaskRegistry != null)
                context.RegisterService(layerMaskRegistry);

            return context;
        }

        private void RegisterEditorServices(
            NodeContext context,
            RuntimeExecutionSettings runtimeSettings,
            bool log = true)
        {
            context.RegisterService<IGeneratorDataRegistry>(new GeneratorDataRegistry());
            if (log)
                Debug.Log("[EditorPreview] ✓ IGeneratorDataRegistry registered.");

            var tileRegistry = runtimeSettings.TileRegistry ?? _previewSettings?.TileRegistry;
            if (tileRegistry != null)
            {
                context.RegisterService(tileRegistry);
                if (log)
                    Debug.Log("[EditorPreview] ✓ TileRegistrySO registered.");
            }
            else if (log)
            {
                Debug.LogWarning("[EditorPreview] ⚠ TileRegistrySO not assigned in EditorPreviewSettings. " +
                    "SingleTileLayerNode sprite fallback will not be available.");
            }

            if (log && !string.IsNullOrEmpty(runtimeSettings.Source))
            {
                Debug.Log($"[EditorPreview] Runtime-equivalent settings source: {runtimeSettings.Source}");
            }
        }

        private RuntimeExecutionSettings ResolveRuntimeExecutionSettings()
        {
            var resolved = new RuntimeExecutionSettings();
            var graphBinding = FindRuntimeGraphBindingForActiveGraph();

            if (graphBinding != null)
            {
                var so = new SerializedObject(graphBinding);
                resolved.HeightMapSettings = so.FindProperty("_heightMapSettings")?.objectReferenceValue as HeightMapSettings;
                resolved.BiomesSettings = so.FindProperty("_biomesSettings")?.objectReferenceValue as DataBiomesSettings;
                resolved.WfcSettings = so.FindProperty("_wfcDataSettings")?.objectReferenceValue as WFCDataSettings;
                resolved.Source = $"MoyvaTWCGraphBinding: {graphBinding.gameObject.scene.name}/{graphBinding.name}";
            }

            var gridInstaller = FindGridInstallerInSameScene(graphBinding);
            if (gridInstaller != null)
            {
                var so = new SerializedObject(gridInstaller);
                resolved.TileRegistry = so.FindProperty("tileRegistry")?.objectReferenceValue as TileRegistrySO;

                var widthProp = so.FindProperty("gridWidth");
                var heightProp = so.FindProperty("gridHeight");
                if (widthProp != null && heightProp != null)
                {
                    resolved.GridWidth = Mathf.Max(1, widthProp.intValue);
                    resolved.GridHeight = Mathf.Max(1, heightProp.intValue);
                    resolved.HasGridSize = true;
                }

                if (string.IsNullOrEmpty(resolved.Source))
                    resolved.Source = $"GridInstaller: {gridInstaller.gameObject.scene.name}/{gridInstaller.name}";
                else
                    resolved.Source += $" | GridInstaller: {gridInstaller.gameObject.scene.name}/{gridInstaller.name}";
            }

            return resolved;
        }

        private MonoBehaviour FindRuntimeGraphBindingForActiveGraph()
        {
            var all = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            foreach (var installer in all)
            {
                if (installer == null || installer.GetType().FullName != RuntimeGraphBindingTypeName)
                    continue;
                if (!IsSceneObject(installer))
                    continue;

                var so = new SerializedObject(installer);
                var graph = so.FindProperty("_graphAsset")?.objectReferenceValue as GraphAsset;
                if (graph == null)
                    continue;

                if (_graphAsset == null || graph == _graphAsset)
                    return installer;
            }

            return null;
        }

        private MonoBehaviour FindGridInstallerInSameScene(MonoBehaviour graphBinding)
        {
            var all = Resources.FindObjectsOfTypeAll<MonoBehaviour>();

            if (graphBinding != null)
            {
                var targetScene = graphBinding.gameObject.scene;
                foreach (var installer in all)
                {
                    if (installer == null || installer.GetType().FullName != GridInstallerTypeName)
                        continue;
                    if (!IsSceneObject(installer))
                        continue;
                    if (installer.gameObject.scene == targetScene)
                        return installer;
                }
            }

            foreach (var installer in all)
            {
                if (installer == null || installer.GetType().FullName != GridInstallerTypeName)
                    continue;
                if (IsSceneObject(installer))
                    return installer;
            }

            return null;
        }

        private static bool IsSceneObject(UnityEngine.Object obj)
        {
            if (obj == null)
                return false;

            if (EditorUtility.IsPersistent(obj))
                return false;

            if (obj is Component component)
            {
                var scene = component.gameObject.scene;
                return scene.IsValid() && scene.isLoaded;
            }

            return true;
        }

        private void RestorePreviewSettings()
        {
            if (_previewSettings != null) return;
            if (string.IsNullOrEmpty(_previewSettingsGuid)) return;

            var path = AssetDatabase.GUIDToAssetPath(_previewSettingsGuid);
            if (string.IsNullOrEmpty(path)) return;

            _previewSettings = AssetDatabase.LoadAssetAtPath<EditorPreviewSettings>(path);
        }

        private void LoadWindowSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GraphEditorWindowSettings>(SettingsAssetPath);
            if (settings == null)
            {
                _windowSettings = null;
                return;
            }

            _windowSettings = settings;
            // Prefer direct references stored in the settings asset. Fall back to GUIDs for backward compatibility.
            if (settings.graphAsset != null)
            {
                _graphAsset = settings.graphAsset;
                var path = AssetDatabase.GetAssetPath(_graphAsset);
                _graphAssetGuid = string.IsNullOrEmpty(path) ? null : AssetDatabase.AssetPathToGUID(path);
            }
            else if (!string.IsNullOrEmpty(settings.graphAssetGuid))
            {
                var path = AssetDatabase.GUIDToAssetPath(settings.graphAssetGuid);
                if (!string.IsNullOrEmpty(path))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(path);
                    if (asset != null)
                        _graphAsset = asset;
                    else
                    {
                        Debug.LogWarning("[GraphEditorWindow] Saved GraphAsset not found; clearing saved reference in settings.");
                        settings.graphAssetGuid = null;
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            if (settings.previewSettings != null)
            {
                _previewSettings = settings.previewSettings;
                var path = AssetDatabase.GetAssetPath(_previewSettings);
                _previewSettingsGuid = string.IsNullOrEmpty(path) ? null : AssetDatabase.AssetPathToGUID(path);
            }
            else if (!string.IsNullOrEmpty(settings.previewSettingsGuid))
            {
                var path = AssetDatabase.GUIDToAssetPath(settings.previewSettingsGuid);
                if (!string.IsNullOrEmpty(path))
                {
                    var p = AssetDatabase.LoadAssetAtPath<EditorPreviewSettings>(path);
                    if (p != null)
                        _previewSettings = p;
                    else
                    {
                        Debug.LogWarning("[GraphEditorWindow] Saved EditorPreviewSettings not found; clearing saved reference in settings.");
                        settings.previewSettingsGuid = null;
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            _previewWidth = Mathf.Max(4, settings.previewWidth);
            _previewHeight = Mathf.Max(4, settings.previewHeight);
            _showInlinePreviews = settings.showInlinePreviews;
            _autoRunOnChange = settings.autoRunOnChange;
            _previewResolution = Mathf.Clamp(settings.previewResolution, 0, 2);
            _previewHeatmap = settings.previewHeatmap;
            _isInspectorVisible = settings.isInspectorVisible;
            _activeInspectorTab = (InspectorTab)Mathf.Clamp(settings.inspectorTabIndex, 0, 2);
            _selectedLayerId = settings.selectedLayerId;
            _selectedNodeId = settings.selectedNodeId;
            _savedCameraPosition = settings.cameraPosition;
            _savedCameraScale = settings.cameraScale;
        }

        private void SaveWindowSettings()
        {
            GraphEditorWindowSettings settings = AssetDatabase.LoadAssetAtPath<GraphEditorWindowSettings>(SettingsAssetPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<GraphEditorWindowSettings>();
                AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            }
            // Save both direct references and GUIDs for backward compatibility
            settings.graphAsset = _graphAsset;
            settings.graphAssetGuid = _graphAssetGuid ?? "";

            settings.previewSettings = _previewSettings;
            settings.previewSettingsGuid = _previewSettingsGuid ?? "";
            settings.previewWidth = _previewWidth;
            settings.previewHeight = _previewHeight;
            settings.showInlinePreviews = _showInlinePreviews;
            settings.autoRunOnChange = _autoRunOnChange;
            settings.previewResolution = _previewResolution;
            settings.previewHeatmap = _previewHeatmap;
            settings.isInspectorVisible = _isInspectorVisible;
            settings.inspectorTabIndex = (int)_activeInspectorTab;
            settings.selectedLayerId = _selectedLayerId ?? "";
            settings.selectedNodeId = _selectedNodeId ?? "";

            CaptureCameraTransform();
            settings.cameraPosition = _savedCameraPosition;
            settings.cameraScale = _savedCameraScale == Vector3.zero ? Vector3.one : _savedCameraScale;

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            _windowSettings = settings;
        }

        private int ComputeDeterministicSeed(int mapW, int mapH)
        {
            try
            {
                using (var md5 = MD5.Create())
                using (var ms = new MemoryStream())
                {
                    // Include graph asset file bytes when available
                    if (_graphAsset != null)
                    {
                        var path = AssetDatabase.GetAssetPath(_graphAsset);
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            var b = File.ReadAllBytes(path);
                            ms.Write(b, 0, b.Length);
                        }
                    }
                    else if (!string.IsNullOrEmpty(_graphAssetGuid))
                    {
                        var path = AssetDatabase.GUIDToAssetPath(_graphAssetGuid);
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            var b = File.ReadAllBytes(path);
                            ms.Write(b, 0, b.Length);
                        }
                    }

                    // Include preview settings asset bytes when available
                    if (_previewSettings != null)
                    {
                        var ppath = AssetDatabase.GetAssetPath(_previewSettings);
                        if (!string.IsNullOrEmpty(ppath) && File.Exists(ppath))
                        {
                            var pb = File.ReadAllBytes(ppath);
                            ms.Write(pb, 0, pb.Length);
                        }
                    }
                    else if (!string.IsNullOrEmpty(_previewSettingsGuid))
                    {
                        var ppath = AssetDatabase.GUIDToAssetPath(_previewSettingsGuid);
                        if (!string.IsNullOrEmpty(ppath) && File.Exists(ppath))
                        {
                            var pb = File.ReadAllBytes(ppath);
                            ms.Write(pb, 0, pb.Length);
                        }
                    }

                    // Append runtime preview parameters
                    var meta = $":{mapW}:{mapH}:{_previewWidth}:{_previewHeight}:{_previewResolution}:{_previewHeatmap}:{_showInlinePreviews}:{_autoRunOnChange}";
                    var metaBytes = Encoding.UTF8.GetBytes(meta);
                    ms.Write(metaBytes, 0, metaBytes.Length);

                    // Include referenced ScriptableObjects from nodes (e.g., DataNoiseSettings, WFCDataSettings)
                    if (_graphAsset != null && _graphAsset.Nodes != null)
                    {
                        foreach (var node in _graphAsset.Nodes)
                        {
                            if (node == null) continue;
                            try
                            {
                                var serialized = new UnityEditor.SerializedObject(node);
                                var prop = serialized.GetIterator();
                                bool enter = true;
                                while (prop.NextVisible(enter))
                                {
                                    enter = false;
                                    if (prop.propertyType == UnityEditor.SerializedPropertyType.ObjectReference)
                                    {
                                        var o = prop.objectReferenceValue;
                                        if (o == null) continue;
                                        var ap = AssetDatabase.GetAssetPath(o);
                                        if (!string.IsNullOrEmpty(ap) && File.Exists(ap))
                                        {
                                            var db = File.ReadAllBytes(ap);
                                            ms.Write(db, 0, db.Length);
                                        }
                                        else
                                        {
                                            var nameb = Encoding.UTF8.GetBytes(o.name ?? "");
                                            ms.Write(nameb, 0, nameb.Length);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // ignore serialization issues for safety
                            }
                        }
                    }

                    var hash = md5.ComputeHash(ms.ToArray());
                    int seed = BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;
                    return seed;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GraphEditorWindow] Failed to compute deterministic seed: {ex.Message}");
                return (int)(Environment.TickCount & 0x7FFFFFFF);
            }
        }

        private int GetSeedFromGraph()
        {
            if (_graphAsset?.Nodes == null)
                return GlobalSeed.DefaultSeed;

            foreach (var node in _graphAsset.Nodes)
            {
                if (node is ISeedProvider seedProvider)
                    return seedProvider.Seed;
            }

            return GlobalSeed.DefaultSeed;
        }

        private void HighlightExecutionResults(GraphExecutionResult result)
        {
            if (_graphView == null) return;

            foreach (var log in result.Logs)
            {
                foreach (var element in _graphView.graphElements)
                {
                    if (element is GeneratorNodeView nodeView
                        && nodeView.NodeData.NodeId == log.NodeId)
                    {
                        Color borderColor;
                        if (log.Status == NodeStatus.Error)
                            borderColor = new Color(1f, 0.2f, 0.2f);
                        else if (log.Status == NodeStatus.Warning)
                            borderColor = new Color(1f, 0.8f, 0.2f);
                        else if (log.DurationMs > 100)
                            borderColor = new Color(1f, 0.6f, 0.2f); // slow node
                        else
                            borderColor = new Color(0.2f, 0.9f, 0.3f); // success

                        nodeView.style.borderBottomColor = borderColor;
                        nodeView.style.borderTopColor = borderColor;
                        nodeView.style.borderLeftColor = borderColor;
                        nodeView.style.borderRightColor = borderColor;
                        nodeView.style.borderBottomWidth = 2;
                        nodeView.style.borderTopWidth = 2;
                        nodeView.style.borderLeftWidth = 2;
                        nodeView.style.borderRightWidth = 2;

                        // Add timing badge
                        nodeView.HoverTooltipText =
                            $"{log.NodeTitle}: {log.DurationMs:F1}ms\n" +
                            $"Allocations: {log.AllocationBytes} B\n" +
                            $"Iterations: {log.IterationCount}"
                            + (string.IsNullOrEmpty(log.Message) ? "" : $"\n{log.Message}");
                        break;
                    }
                }
            }
        }

        private void PollSelectionForInspector()
        {
            if (_graphView == null) return;

            int count = _graphView.GetSelectedNodeCount();
            if (count == 0)
            {
                if (_selectedNode != null || _isMultiSelection)
                {
                    _isMultiSelection = false;
                    SetSelectedNode(null);
                    RefreshInspectorPanel();
                }
                return;
            }

            if (count > 1)
            {
                if (!_isMultiSelection || _selectedNode != null)
                {
                    _isMultiSelection = true;
                    SetSelectedNode(null);
                    RefreshInspectorPanel();
                }
                return;
            }

            var selected = _graphView.GetPrimarySelectedNodeData();
            if (ReferenceEquals(selected, _selectedNode) && !_isMultiSelection)
                return;

            _isMultiSelection = false;
            SetSelectedNode(selected);
            RefreshInspectorPanel();
        }

        private void SetSelectedNode(NodeBase node)
        {
            if (ReferenceEquals(_selectedNode, node))
            {
                _selectedNodeId = node?.NodeId;
                return;
            }

            if (_selectedNodeEditor != null)
                DestroyImmediate(_selectedNodeEditor);

            _selectedNodeEditor = null;
            _selectedNode = node;
            _selectedNodeId = node?.NodeId;
        }

        private void RefreshInspectorPanel()
        {
            RebuildTwcNodeInspectorPanel();
            _nodeInspectorGui?.MarkDirtyRepaint();
            _graphSettingsGui?.MarkDirtyRepaint();
        }

        private void RebuildTwcNodeInspectorPanel()
        {
            if (_nodeInspectorGui == null || _twcNodeInspectorGui == null)
                return;

            bool isTwcNode = _selectedNode is TwcModifierNode;
            _nodeInspectorGui.style.display = isTwcNode ? DisplayStyle.None : DisplayStyle.Flex;
            _twcNodeInspectorGui.style.display = isTwcNode ? DisplayStyle.Flex : DisplayStyle.None;
            _twcNodeInspectorGui.Clear();

            if (_selectedNode is not TwcModifierNode twcNode)
                return;

            _twcNodeInspectorGui.Add(new Label(twcNode.Title)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 2
                }
            });

            _twcNodeInspectorGui.Add(new Label($"Type: {twcNode.GetType().Name}")
            {
                style =
                {
                    color = new Color(0.65f, 0.65f, 0.65f),
                    marginBottom = 4
                }
            });

            _twcNodeInspectorGui.Add(new Label(twcNode.IsGenerator ? "Тип: Генератор" : "Тип: Модифікатор")
            {
                style =
                {
                    color = new Color(0.75f, 0.75f, 0.75f),
                    marginBottom = 6
                }
            });

            if (!twcNode.TryRestoreModifierInEditor())
            {
                _twcNodeInspectorGui.Add(new HelpBox(
                    $"TWC-модифікатор '{twcNode.ModifierTypeName}' не ініціалізовано.",
                    HelpBoxMessageType.Warning));
                return;
            }

            var modifier = twcNode.ModifierAsset;
            if (modifier == null)
            {
                _twcNodeInspectorGui.Add(new HelpBox("TWC-модифікатор не знайдено.", HelpBoxMessageType.Warning));
                return;
            }

            VisualElement nativeInspector = null;
            try
            {
                nativeInspector = twcNode.CreateModifierInspectorElement(ResolveInspectorMapSize());
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GraphEditorWindow] Failed to build native TWC inspector for {twcNode.Title}: {ex.Message}");
            }

            if (nativeInspector != null && nativeInspector.childCount > 0)
            {
                nativeInspector.RegisterCallback<SerializedPropertyChangeEvent>(_ => OnTwcNodeInspectorChanged(twcNode));
                _twcNodeInspectorGui.Add(nativeInspector);
            }
            else
            {
                _twcNodeInspectorGui.Add(new IMGUIContainer(() => DrawTwcModifierSerializedFallback(twcNode)));
            }
        }

        private Vector2Int ResolveInspectorMapSize()
        {
            var size = _graphAsset?.SharedSettings?.MapSize ?? Vector2Int.zero;
            if (size.x > 0 && size.y > 0)
                return size;

            return new Vector2Int(Mathf.Max(1, _previewWidth), Mathf.Max(1, _previewHeight));
        }

        private void OnTwcNodeInspectorChanged(TwcModifierNode node)
        {
            if (node == null)
                return;

            EditorUtility.SetDirty(node);
            if (node.ModifierAsset != null)
                EditorUtility.SetDirty(node.ModifierAsset);
            if (_graphAsset != null)
                EditorUtility.SetDirty(_graphAsset);
            RequestAutoRun();
        }

        private void DrawTwcModifierSerializedFallback(TwcModifierNode node)
        {
            if (node == null || node.ModifierAsset == null)
                return;

            var serializedModifier = new SerializedObject(node.ModifierAsset);
            serializedModifier.Update();

            EditorGUI.BeginChangeCheck();
            var property = serializedModifier.GetIterator();
            bool enterChildren = true;
            while (property.Next(enterChildren))
            {
                enterChildren = false;
                if (property.propertyPath.StartsWith("m_", StringComparison.Ordinal)
                    || property.propertyPath is "asset" or "isEnabled")
                    continue;

                EditorGUILayout.PropertyField(property, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedModifier.ApplyModifiedProperties();
                OnTwcNodeInspectorChanged(node);
            }
            else
            {
                serializedModifier.ApplyModifiedProperties();
            }
        }

        private void DrawSelectedNodeInspector()
        {
            if (_graphAsset == null)
            {
                EditorGUILayout.HelpBox("Спочатку відкрийте GraphAsset.", MessageType.Info);
                return;
            }

            if (_selectedNode == null)
            {
                if (_isMultiSelection)
                {
                    EditorGUILayout.HelpBox("Множинний вибір не підтримується. Оберіть одну ноду.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Виберіть ноду в графі, щоб переглянути її дані.", MessageType.Info);
                }
                return;
            }

            if (_selectedNodeEditor == null)
                UnityEditor.Editor.CreateCachedEditor(_selectedNode, null, ref _selectedNodeEditor);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(_selectedNode.Title, EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Type", _selectedNode.GetType().Name, EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Layer", ResolveLayerDisplayName(_selectedNode.LayerId), EditorStyles.miniLabel);
                }
            }
            EditorGUILayout.Space(6);

            bool layerRefChanged = false;
            if (_selectedNode is LayerMaskReferenceNode layerRefNode)
                layerRefChanged = DrawLayerReferenceControls(layerRefNode);

            EditorGUI.BeginChangeCheck();
            _selectedNodeEditor.OnInspectorGUI();
            bool inspectorChanged = EditorGUI.EndChangeCheck();
            bool seedChanged = DrawSeedNodeControls(_selectedNode);

            if (inspectorChanged || seedChanged || layerRefChanged)
            {
                EditorUtility.SetDirty(_selectedNode);
                EditorUtility.SetDirty(_graphAsset);
                RequestAutoRun();
            }
        }

        private string ResolveLayerDisplayName(string layerId)
        {
            if (_graphAsset == null || string.IsNullOrEmpty(layerId))
                return "-";

            return _graphAsset.GetLayerById(layerId)?.Name ?? layerId;
        }

        private bool DrawLayerReferenceControls(LayerMaskReferenceNode node)
        {
            if (node == null || _graphAsset == null)
                return false;

            var layers = _graphAsset.Layers?.Where(l => l != null).OrderBy(l => l.SortingOrder).ToList();
            if (layers == null || layers.Count == 0)
            {
                EditorGUILayout.HelpBox("У графі немає шарів для посилання.", MessageType.Info);
                return false;
            }

            var options = new List<string>(layers.Count);
            int selectedIndex = 0;
            string currentId = node.SourceLayerId;
            string currentLayerId = node.LayerId;
            var currentLayer = _graphAsset.GetLayerById(currentLayerId);

            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                options.Add($"{layer.Name} (order {layer.SortingOrder})");
                if (!string.IsNullOrEmpty(currentId) && layer.Id == currentId)
                    selectedIndex = i;
            }

            DrawCurrentLayerReferenceWarning(node);

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Шар-джерело", selectedIndex, options.ToArray());
            if (!EditorGUI.EndChangeCheck())
                return false;

            newIndex = Mathf.Clamp(newIndex, 0, layers.Count - 1);
            var selectedLayer = layers[newIndex];
            if (!CanAssignLayerReference(currentLayer, selectedLayer, out string error))
            {
                EditorUtility.DisplayDialog("Invalid Layer Ref", error, "OK");
                return false;
            }

            Undo.RecordObject(node, "Change Layer Ref Source");
            node.SetSourceLayerId(selectedLayer.Id);
            return true;
        }

        private void DrawCurrentLayerReferenceWarning(LayerMaskReferenceNode node)
        {
            if (node == null)
                return;

            var report = new GraphValidator().ValidateDetailed(_graphAsset);
            var issue = report.Issues.FirstOrDefault(issue =>
                issue.Severity == ValidationSeverity.Error
                && issue.NodeId == node.NodeId
                && issue.Code.StartsWith("LAYER_REF_", StringComparison.Ordinal));

            if (issue != null)
                EditorGUILayout.HelpBox(issue.Message, MessageType.Error);
        }

        private bool CanAssignLayerReference(
            GeneratorLayerDefinition currentLayer,
            GeneratorLayerDefinition sourceLayer,
            out string error)
        {
            error = null;

            if (currentLayer == null)
            {
                error = "Layer Ref node does not belong to a valid graph layer.";
                return false;
            }

            if (sourceLayer == null)
            {
                error = "Source layer is missing.";
                return false;
            }

            if (!sourceLayer.Enabled)
            {
                error = $"Шар '{sourceLayer.Name}' вимкнений і не буде оброблений перед '{currentLayer.Name}'. Увімкни source layer або обери інший.";
                return false;
            }

            if (sourceLayer.Id == currentLayer.Id)
            {
                error = $"Шар '{currentLayer.Name}' не може посилатися сам на себе.";
                return false;
            }

            if (sourceLayer.SortingOrder >= currentLayer.SortingOrder)
            {
                error = $"Шар '{currentLayer.Name}' може посилатися тільки на шари, які виконуються раніше. '{sourceLayer.Name}' має order {sourceLayer.SortingOrder}, а поточний шар має order {currentLayer.SortingOrder}.";
                return false;
            }

            return true;
        }

        private void DrawGeneralInspectorTab()
        {
            if (_graphAsset == null)
            {
                EditorGUILayout.HelpBox("Спочатку відкрийте GraphAsset.", MessageType.Info);
                return;
            }

            if (_selectedNode != null)
            {
                EditorGUILayout.HelpBox("Обрано ноду. Перейдіть у вкладку 'Ноди' для редагування ноди.", MessageType.Info);
                return;
            }

            if (_isMultiSelection)
            {
                EditorGUILayout.HelpBox("Множинний вибір нод. Вкладка 'Загальні' показує лише параметри графа/шару.", MessageType.Info);
                return;
            }

            var layer = _graphAsset.GetLayerById(_selectedLayerId);
            if (layer != null)
            {
                DrawSelectedLayerInspector(layer);
                return;
            }

            DrawGraphSettingsInspector();
        }

        private void DrawNodeInspectorTab()
        {
            DrawSelectedNodeInspector();
        }

        private void DrawSelectedLayerInspector(GeneratorLayerDefinition layer)
        {
            if (layer == null || _graphAsset == null)
            {
                EditorGUILayout.HelpBox("Шар не знайдено.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Layer Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();

            string newName = EditorGUILayout.TextField("Name", layer.Name ?? string.Empty);
            int newOrder = EditorGUILayout.IntField("Sorting Order", layer.SortingOrder);
            bool newEnabled = EditorGUILayout.Toggle("Enabled", layer.Enabled);
            float newHeight = EditorGUILayout.FloatField("Default Height", layer.DefaultHeight);
            bool newZeroPadding = EditorGUILayout.Toggle(new GUIContent("Zero Layer Padding (+16)", "Позначає шар як розширений: #sym:width/#sym:height стають більшими на 16."), layer.UseZeroLayerPadding);
            int newExtraWidth = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Extra Width Cells", "Додає клітинки до ширини цього шару понад розмір мапи. Шар центрується навколо основної мапи."), layer.ExtraWidthCells));
            int newExtraLength = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Extra Length Cells", "Додає клітинки до довжини цього шару понад розмір мапи. Корисно для води/океану навколо острова."), layer.ExtraLengthCells));
            bool newGenerateFlatSurface = EditorGUILayout.Toggle(
                new GUIContent(
                    "Generate Flat Surface",
                    "Якщо увімкнено, TWC build-шар створює одну пласку grid-поверхню на весь розмір шару замість TilePreset-тайлів."),
                layer.GenerateFlatSurface);
            Material newFlatSurfaceMaterial;
            using (new EditorGUI.DisabledScope(!newGenerateFlatSurface))
            {
                newFlatSurfaceMaterial = (Material)EditorGUILayout.ObjectField(
                    new GUIContent("Flat Surface Material", "Матеріал, який буде призначений одному пласкому mesh шару."),
                    layer.FlatSurfaceMaterial,
                    typeof(Material),
                    false);
            }
            Color newColor = EditorGUILayout.ColorField("Color", layer.Color);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_graphAsset, "Edit Layer Settings");
                layer.Name = string.IsNullOrWhiteSpace(newName) ? "Layer" : newName;
                layer.SortingOrder = newOrder;
                layer.Enabled = newEnabled;
                layer.DefaultHeight = newHeight;
                layer.UseZeroLayerPadding = newZeroPadding;
                layer.ExtraWidthCells = newExtraWidth;
                layer.ExtraLengthCells = newExtraLength;
                layer.GenerateFlatSurface = newGenerateFlatSurface;
                layer.FlatSurfaceMaterial = newFlatSurfaceMaterial;
                layer.Color = newColor;

                EditorUtility.SetDirty(_graphAsset);
                TrySyncCompanionBlueprintLayers(false);
                RebuildLayerList();
                RefreshGraphViewFromAsset();
                RequestAutoRun();
            }

            if (layer.GenerateFlatSurface)
            {
                EditorGUILayout.HelpBox(
                    "Flat Surface режим ігнорує TilePreset-и цього build-шару і генерує один mesh з grid subdivisions за розміром шару.",
                    MessageType.Info);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Build Layer Key", EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(string.IsNullOrEmpty(layer.BuildLayerKey)
                    ? "(буде призначено після синхронізації Build-шарів)"
                    : layer.BuildLayerKey);
            }
        }

        private void DrawGraphSettingsInspector()
        {
            EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newSettings = (EditorPreviewSettings)EditorGUILayout.ObjectField(
                "Preview Settings",
                _previewSettings,
                typeof(EditorPreviewSettings),
                false);
            if (EditorGUI.EndChangeCheck())
            {
                _previewSettings = newSettings;
                if (_previewSettings != null)
                {
                    var path = AssetDatabase.GetAssetPath(_previewSettings);
                    _previewSettingsGuid = AssetDatabase.AssetPathToGUID(path);
                }
                else
                {
                    _previewSettingsGuid = null;
                }
                SaveWindowSettings();
            }

            _previewWidth = Mathf.Max(4, EditorGUILayout.IntField("Preview Width", _previewWidth));
            _previewHeight = Mathf.Max(4, EditorGUILayout.IntField("Preview Height", _previewHeight));
            _showInlinePreviews = EditorGUILayout.Toggle("Inline Previews", _showInlinePreviews);
            _graphView?.SetInlinePreviewsVisible(_showInlinePreviews);
            _previewHeatmap = EditorGUILayout.Toggle("Heatmap", _previewHeatmap);

            if (_previewSettings != null)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Preview Settings Details", EditorStyles.miniBoldLabel);
                DrawSerializedObjectWithoutScript(new SerializedObject(_previewSettings));
            }
            else
            {
                EditorGUILayout.HelpBox("Призначте EditorPreviewSettings для реалістичного preview сервісів.", MessageType.Info);
            }

            if (GUI.changed)
                RequestAutoRun();
        }

        private void OnGraphCanvasBackgroundClicked()
        {
            _isMultiSelection = false;
            SetSelectedNode(null);
            RebuildLayerList();
            SetInspectorTab(InspectorTab.Settings);
            RefreshInspectorPanel();
        }

        private void OnGraphChanged()
        {
            RequestAutoRun();
        }

        private void OnGraphStatusMessage(string message)
        {
            if (_statusLabel != null && !string.IsNullOrWhiteSpace(message))
                _statusLabel.text = message;
        }

        private void OnGraphViewTransformChanged(UnityEditor.Experimental.GraphView.GraphView graphView)
        {
            CaptureCameraTransform();
        }

        private void RequestAutoRun()
        {
            if (!_autoRunOnChange || EditorApplication.isPlaying)
                return;

            _nextAutoRunAt = EditorApplication.timeSinceStartup + 0.35d;
        }

        private void PollAutoRun()
        {
            if (!_autoRunOnChange || _isRunningGraph)
                return;
            if (_nextAutoRunAt <= 0d)
                return;
            if (EditorApplication.timeSinceStartup < _nextAutoRunAt)
                return;

            _nextAutoRunAt = 0d;
            RunGraph(true);
        }

        private int ResolvePreviewSize(int mapW, int mapH)
        {
            return _previewResolution switch
            {
                0 => 64,
                1 => 128,
                2 => Mathf.Clamp(Mathf.Max(mapW, mapH), 32, 256),
                _ => 128
            };
        }

        private void SetInspectorVisible(bool visible)
        {
            _isInspectorVisible = visible;
            if (_rightPanel == null) return;

            _rightPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetInspectorTab(InspectorTab tab)
        {
            if (tab != InspectorTab.Settings && tab != InspectorTab.Preview)
                tab = InspectorTab.Settings;

            if (_activeInspectorTab == tab) return;
            _activeInspectorTab = tab;
            UpdateInspectorTabVisibility();
            SaveWindowSettings();
        }

        private void UpdateInspectorTabVisibility()
        {
            if (_activeInspectorTab != InspectorTab.Settings && _activeInspectorTab != InspectorTab.Preview)
                _activeInspectorTab = InspectorTab.Settings;

            if (_tabSettingsContent != null)
                _tabSettingsContent.style.display = _activeInspectorTab == InspectorTab.Settings
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            if (_tabPreviewContent != null)
                _tabPreviewContent.style.display = _activeInspectorTab == InspectorTab.Preview
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            if (_tabBuildLayersContent != null)
                _tabBuildLayersContent.style.display = DisplayStyle.None;

            if (_nodeInspectorDivider != null)
                _nodeInspectorDivider.style.display = DisplayStyle.Flex;

            if (_tabSettingsButton != null)
                _tabSettingsButton.style.unityFontStyleAndWeight = _activeInspectorTab == InspectorTab.Settings
                    ? FontStyle.Bold
                    : FontStyle.Normal;

            if (_tabPreviewButton != null)
                _tabPreviewButton.style.unityFontStyleAndWeight = _activeInspectorTab == InspectorTab.Preview
                    ? FontStyle.Bold
                    : FontStyle.Normal;

            _nodeInspectorGui?.MarkDirtyRepaint();
            _graphSettingsGui?.MarkDirtyRepaint();
        }

        /// <summary>
        /// Будує node-based summary для Tile Settings.
        /// Сирий TileWorldCreator build-layer editor навмисно прихований: graph node є джерелом правди.
        /// </summary>
        private void RebuildBuildLayersPanel()
        {
            if (_buildLayersHost == null)
                return;

            _buildLayersHost.Clear();

            if (_graphAsset == null)
            {
                _buildLayersHost.Add(new HelpBox(
                    "Відкрийте граф-асет, щоб налаштувати tile nodes.",
                    HelpBoxMessageType.Info));
                return;
            }

            TrySyncCompanionBlueprintLayers(true);

            _buildLayersHost.Add(new HelpBox(
                "Налаштування тайлів більше не редагуються через окреме TWC build-layer вікно. " +
                "Додай Tile Settings node у потрібний graph layer: один node відповідає одному TilePreset/tileset і його build-параметрам. " +
                "Шари без Tile Settings node не створюють runtime tile GameObject-и.",
                HelpBoxMessageType.Info));

            if (GUILayoutButtonElement("+ Add Tile Settings Node", AddTileSettingsNodeToSelectedLayer, out var addButton))
            {
                addButton.tooltip = "Створити Tile Settings node у поточному вибраному шарі.";
                _buildLayersHost.Add(addButton);
            }

            var selectedLayer = _graphAsset.GetLayerById(_selectedLayerId);
            if (selectedLayer != null)
            {
                var nodes = TileSettingsNode.GetNodesForLayer(_graphAsset, selectedLayer.Id);
                _buildLayersHost.Add(new Label($"Selected layer: {selectedLayer.Name}")
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        marginTop = 8,
                        marginBottom = 4
                    }
                });

                if (nodes.Count == 0)
                {
                    _buildLayersHost.Add(new HelpBox(
                        "У цьому шарі немає Tile Settings node. Якщо Output Kind = Tiles, валідатор вимагатиме додати її й вибрати TilePreset.",
                        HelpBoxMessageType.Warning));
                }
                else
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (node == null)
                            continue;

                        var row = new VisualElement
                        {
                            style =
                            {
                                flexDirection = FlexDirection.Row,
                                alignItems = Align.Center,
                                marginBottom = 4,
                                paddingTop = 4,
                                paddingBottom = 4,
                                paddingLeft = 6,
                                paddingRight = 6,
                                borderBottomWidth = 1,
                                borderBottomColor = new Color(0.18f, 0.18f, 0.18f)
                            }
                        };

                        row.Add(new Label(node.Title)
                        {
                            style = { flexGrow = 1 }
                        });

                        var selectButton = new Button(() =>
                        {
                            _graphView?.SelectNodeById(node.NodeId, true);
                            SetSelectedNode(node);
                            SetInspectorTab(InspectorTab.Preview);
                            RefreshInspectorPanel();
                        })
                        {
                            text = "Select"
                        };
                        row.Add(selectButton);
                        _buildLayersHost.Add(row);
                    }
                }
            }

            _buildLayersHost.Add(new HelpBox(
                "Legacy TWC build-layer editor intentionally hidden here. Якщо потрібно подивитися сирий companion Configuration, відкрий його як sub-asset через Project/Inspector, але джерелом правди має бути Tile Settings node.",
                HelpBoxMessageType.None));
        }

        private static bool GUILayoutButtonElement(string text, Action action, out Button button)
        {
            button = new Button(action) { text = text };
            button.style.marginTop = 6;
            button.style.marginBottom = 6;
            return true;
        }

        private void AddTileSettingsNodeToSelectedLayer()
        {
            if (_graphAsset == null || _graphView == null)
                return;

            string layerId = EnsureSelectedLayer();
            if (string.IsNullOrEmpty(layerId))
                return;

            _graphView.SetActiveLayerWithoutRefresh(layerId);
            var node = _graphView.CreateNode(typeof(TileSettingsNode), new Vector2(360f, 180f));
            if (node != null)
            {
                _graphView.SelectNodeById(node.NodeId, true);
                SetSelectedNode(node);
                SetInspectorTab(InspectorTab.Preview);
            }

            RefreshInspectorPanel();
            RequestAutoRun();
        }

        private static Type ResolveGraphBuildLayerStoreType()
        {
            return ResolveType(
                "Kruty1918.Moyva.Generator.Editor.GraphBuildLayerStore, Kruty1918.Moyva.Generator.Editor",
                "Kruty1918.Moyva.Generator.Editor.GraphBuildLayerStore");
        }

        private static Type ResolveType(string assemblyQualifiedName, string fullName)
        {
            var type = Type.GetType(assemblyQualifiedName);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullName);
                if (type != null)
                    return type;
            }

            return null;
        }

        private void DrawSerializedObjectWithoutScript(SerializedObject serializedObject)
        {
            if (serializedObject == null) return;

            serializedObject.Update();
            var iterator = serializedObject.GetIterator();
            bool expanded = true;
            string hoveredTooltip = null;

            while (iterator.NextVisible(expanded))
            {
                if (iterator.propertyPath == "m_Script")
                {
                    expanded = false;
                    continue;
                }

                var property = iterator.Copy();
                string tooltip = GetTooltipForProperty(serializedObject.targetObject, property.propertyPath);
                var label = BuildPropertyLabel(property);
                EditorGUILayout.PropertyField(property, label, true);

                DrawInlineObjectReferenceControls(serializedObject, property);

                var fieldRect = GUILayoutUtility.GetLastRect();
                if (!string.IsNullOrEmpty(tooltip) && fieldRect.Contains(Event.current.mousePosition))
                    hoveredTooltip = tooltip;

                expanded = false;
            }

            DrawInspectorHoverTooltip(hoveredTooltip);

            serializedObject.ApplyModifiedProperties();
        }

        private static bool DrawSeedNodeControls(NodeBase node)
        {
            if (node is not ISeedProvider)
                return false;

            EditorGUILayout.Space(4);
            if (!GUILayout.Button("Random Seed"))
                return false;

            var serializedNode = new SerializedObject(node);
            var seedProperty = serializedNode.FindProperty("seed");
            if (seedProperty == null)
                return false;

            Undo.RecordObject(node, "Randomize Seed");
            serializedNode.Update();
            seedProperty.intValue = GenerateRandomSeed();
            serializedNode.ApplyModifiedProperties();
            EditorUtility.SetDirty(node);
            GUI.changed = true;
            return true;
        }

        private static int GenerateRandomSeed()
        {
            int value;
            do
            {
                value = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            while (value == 0);

            return value;
        }

        private void DrawInlineObjectReferenceControls(SerializedObject ownerSerializedObject, SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            var referencedObject = property.objectReferenceValue;
            if (referencedObject == null)
                return;

            string key = $"{ownerSerializedObject.targetObject.GetInstanceID()}:{property.propertyPath}";

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(14f);

                if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(52f)))
                    EditorGUIUtility.PingObject(referencedObject);

                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(52f)))
                    Selection.activeObject = referencedObject;

                if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(52f)))
                    AssetDatabase.OpenAsset(referencedObject);
            }

            if (referencedObject is not ScriptableObject scriptableObject)
                return;

            _inlineObjectFoldouts.TryGetValue(key, out bool expanded);
            expanded = EditorGUILayout.Foldout(expanded, $"Inline: {property.displayName}", true);
            _inlineObjectFoldouts[key] = expanded;
            if (!expanded)
                return;

            if (!_inlineObjectEditors.TryGetValue(key, out var nestedEditor)
                || nestedEditor == null
                || nestedEditor.target != scriptableObject)
            {
                if (nestedEditor != null)
                    DestroyImmediate(nestedEditor);

                nestedEditor = UnityEditor.Editor.CreateEditor(scriptableObject);
                _inlineObjectEditors[key] = nestedEditor;
            }

            EditorGUI.indentLevel++;
            nestedEditor.OnInspectorGUI();
            EditorGUI.indentLevel--;
        }

        private static GUIContent BuildPropertyLabel(SerializedProperty property)
        {
            return new GUIContent(property.displayName);
        }

        private bool SanitizeGraphAsset(bool refreshView)
        {
            if (_graphAsset == null)
                return false;

            int repaired = _graphAsset.RepairMissingNodeConnections();
            int removed = _graphAsset.RemoveNullNodes();
            bool changed = repaired > 0 || removed > 0;

            if (!changed)
                return false;

            EditorUtility.SetDirty(_graphAsset);
            if (refreshView)
                RefreshGraphViewFromAsset();

            return true;
        }

        private static string GetTooltipForProperty(UnityEngine.Object targetObject, string propertyPath)
        {
            if (targetObject == null || string.IsNullOrEmpty(propertyPath))
                return null;

            Type currentType = targetObject.GetType();
            FieldInfo field = null;
            var pathParts = propertyPath.Split('.');

            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];
                if (part == "Array")
                    continue;

                if (part.StartsWith("data[", StringComparison.Ordinal))
                    continue;

                field = GetFieldInHierarchy(currentType, part);
                if (field == null)
                    return null;

                currentType = GetFieldValueType(field.FieldType);
            }

            return field?.GetCustomAttribute<TooltipAttribute>(true)?.tooltip;
        }

        private static Type GetFieldValueType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType() ?? type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];

            return type;
        }

        private static FieldInfo GetFieldInHierarchy(Type type, string fieldName)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }

        private void DrawInspectorHoverTooltip(string tooltip)
        {
            if (string.IsNullOrEmpty(tooltip) || Event.current.type != EventType.Repaint)
                return;

            var content = new GUIContent(tooltip);
            var style = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                richText = false,
                normal = { textColor = new Color(0.94f, 0.94f, 0.94f, 1f) },
                padding = new RectOffset(0, 0, 0, 0)
            };

            const float maxWidth = 360f;
            const float margin = 12f;
            const float offset = 18f;

            float textWidth = Mathf.Min(maxWidth, style.CalcSize(content).x);
            float textHeight = style.CalcHeight(content, textWidth);
            float boxWidth = textWidth + 16f;
            float boxHeight = textHeight + 12f;

            var mouse = Event.current.mousePosition;
            float rightSpace = position.width - mouse.x - offset - margin;
            float leftSpace = mouse.x - offset - margin;
            float belowSpace = position.height - mouse.y - offset - margin;
            float aboveSpace = mouse.y - offset - margin;

            float x = (rightSpace >= boxWidth || rightSpace >= leftSpace)
                ? mouse.x + offset
                : mouse.x - boxWidth - offset;
            float y = (belowSpace >= boxHeight || belowSpace >= aboveSpace)
                ? mouse.y + offset
                : mouse.y - boxHeight - offset;

            x = Mathf.Clamp(x, margin, Mathf.Max(margin, position.width - boxWidth - margin));
            y = Mathf.Clamp(y, margin, Mathf.Max(margin, position.height - boxHeight - margin));

            var rect = new Rect(x, y, boxWidth, boxHeight);
            int previousDepth = GUI.depth;
            GUI.depth = -100000;

            EditorGUI.DrawRect(rect, new Color(0.11f, 0.11f, 0.11f, 1f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), new Color(0.35f, 0.35f, 0.35f, 1f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(0.35f, 0.35f, 0.35f, 1f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), new Color(0.35f, 0.35f, 0.35f, 1f));
            EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), new Color(0.35f, 0.35f, 0.35f, 1f));

            GUI.Label(new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, rect.height - 12f), content, style);
            GUI.depth = previousDepth;
        }

        private static void MigrateLegacySharedSettingsNode(GraphAsset asset)
        {
            if (asset == null)
                return;

            var legacyNodes = asset.Nodes
                .OfType<SharedSettingsNode>()
                .ToList();

            if (legacyNodes.Count == 0)
                return;

            Undo.RecordObject(asset, "Remove Legacy Shared Settings Node");
            for (int i = 0; i < legacyNodes.Count; i++)
                asset.RemoveNode(legacyNodes[i]);

            EditorUtility.SetDirty(asset);
        }
    }

    [InitializeOnLoad]
    internal static class GraphEditorWindowLayoutRepair
    {
        static GraphEditorWindowLayoutRepair()
        {
            ScheduleCloseFailedFallbackWindows();
        }

        internal static void ScheduleCloseFailedFallbackWindows()
        {
            EditorApplication.delayCall -= CloseFailedFallbackWindows;
            EditorApplication.delayCall += CloseFailedFallbackWindows;
        }

        [MenuItem("Moyva/Graph Editor/Repair Failed Editor Windows")]
        internal static void CloseFailedFallbackWindows()
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                var window = windows[i];
                if (window == null)
                    continue;

                if (window.GetType().FullName != "UnityEditor.FallbackEditorWindow")
                    continue;

                string title = window.titleContent?.text;
                if (!string.Equals(title, "Failed to load", StringComparison.Ordinal))
                    continue;

                CloseFailedFallbackWindow(window);
            }
        }

        private static void CloseFailedFallbackWindow(EditorWindow window)
        {
            try
            {
                window.Close();
            }
            catch (NullReferenceException)
            {
                DestroyFailedFallbackWindow(window);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to close fallback editor window: {exception.Message}");
            }
        }

        private static void DestroyFailedFallbackWindow(EditorWindow window)
        {
            if (window == null)
                return;

            try
            {
                UnityEngine.Object.DestroyImmediate(window);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to destroy fallback editor window: {exception.Message}");
            }
        }
    }
}