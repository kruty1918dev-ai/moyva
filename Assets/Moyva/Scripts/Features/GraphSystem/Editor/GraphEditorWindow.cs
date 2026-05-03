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
        private GeneratorGraphView _graphView;
        private VisualElement _contentContainer;
        private ScrollView _rightPanel;
        private IMGUIContainer _nodeInspectorGui;
        private IMGUIContainer _graphSettingsGui;
        [SerializeField] private bool _isInspectorVisible = true;
      
        private VisualElement _nodeInspectorSection;
        private VisualElement _nodeInspectorDivider;

        private Label _statusLabel;
        private ProgressBar _progressBar;

        private NodeBase _selectedNode;
        private UnityEditor.Editor _selectedNodeEditor;

        private enum InspectorTab { Settings = 0, Preview = 1 }
        [SerializeField] private InspectorTab _activeInspectorTab = InspectorTab.Preview;
        private VisualElement _inspectorTabsHeader;
        private VisualElement _tabSettingsContent;
        private VisualElement _tabPreviewContent;
        private Button _tabSettingsButton;
        private Button _tabPreviewButton;
        [SerializeField] private bool _isMultiSelection;

        // Survives domain reload / play mode transition
        [SerializeField] private string _graphAssetGuid;
        private GraphAsset _graphAsset;

        // Editor Preview Settings
        [SerializeField] private string _previewSettingsGuid;
        private EditorPreviewSettings _previewSettings;

        private const string SettingsAssetPath = "Assets/Moyva/Scripts/Features/GraphSystem/Editor/GraphEditorWindowSettings.asset";
        private GraphEditorWindowSettings _windowSettings;

        // Inline map size override (used when no EditorPreviewSettings assigned)
        [SerializeField] private int _previewWidth = 64;
        [SerializeField] private int _previewHeight = 64;
        [SerializeField] private bool _showInlinePreviews = true;
        [SerializeField] private bool _autoRunOnChange = true;
        [SerializeField] private int _previewResolution = 1; // 0=64,1=128,2=full
        [SerializeField] private bool _previewHeatmap;

        private double _nextAutoRunAt;
        private bool _isRunningGraph;

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

            if (_graphView != null)
                _graphView.GraphChanged -= OnGraphChanged;

            if (_contentContainer != null)
                rootVisualElement.Remove(_contentContainer);
        }

        private void OnUndoRedoPerformed()
        {
            if (_graphView != null && _graphAsset != null)
            {
                _graphView.RefreshFromAsset();
                UpdateStatusBar();
            }
        }

        private void RestoreGraphAsset()
        {
            if (_graphAsset != null)
            {
                MigrateLegacySharedSettingsNode(_graphAsset);
                _graphView.PopulateGraph(_graphAsset, EditorApplication.isPlaying);
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
                _graphView.PopulateGraph(_graphAsset, EditorApplication.isPlaying);
                UpdateStatusBar();
            }
        }

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

            _graphView = new GeneratorGraphView(this);
            _graphView.GraphChanged += OnGraphChanged;
            _graphView.style.flexGrow = 1;
            _contentContainer.Add(_graphView);

            ConstructRightPanel();
        }

        private void ConstructRightPanel()
        {
            _rightPanel = new ScrollView
            {
                style =
                {
                    width = 380,
                    minWidth = 320,
                    flexShrink = 0,
                    borderLeftWidth = 1,
                    borderLeftColor = new Color(0.22f, 0.22f, 0.22f),
                    backgroundColor = new Color(0.12f, 0.12f, 0.12f),
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 6,
                    paddingBottom = 8
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
                text = "Base Settings",
                tooltip = "Показати налаштування прев'ю та сервісів."
            };
            _tabSettingsButton.style.flexGrow = 1;
            _tabSettingsButton.style.marginRight = 4;
            tabHeaderRow.Add(_tabSettingsButton);

            _tabPreviewButton = new Button(() => SetInspectorTab(InspectorTab.Preview))
            {
                text = "Node Settings",
                tooltip = "Показати дані вибраної ноди."
            };
            _tabPreviewButton.style.flexGrow = 1;
            tabHeaderRow.Add(_tabPreviewButton);

            _nodeInspectorSection = new VisualElement();
            _nodeInspectorSection.Add(tabHeaderRow);

            // Preview tab (shows node data)
            _tabPreviewContent = new VisualElement();
            _nodeInspectorGui = new IMGUIContainer(DrawSelectedNodeInspector)
            {
                style =
                {
                    marginBottom = 10
                }
            };
            _tabPreviewContent.Add(_nodeInspectorGui);
            _nodeInspectorSection.Add(_tabPreviewContent);

            _nodeInspectorDivider = new VisualElement
            {
                style =
                {
                    height = 1,
                    marginBottom = 8,
                    backgroundColor = new Color(0.25f, 0.25f, 0.25f)
                }
            };
            _nodeInspectorSection.Add(_nodeInspectorDivider);

            // Settings tab (shows EditorPreviewSettings)
            _tabSettingsContent = new VisualElement();
            _graphSettingsGui = new IMGUIContainer(DrawGraphSettingsInspector);
            _tabSettingsContent.Add(_graphSettingsGui);
            _nodeInspectorSection.Add(_tabSettingsContent);

            _rightPanel.Add(_nodeInspectorSection);

            _contentContainer.Add(_rightPanel);
            SetInspectorVisible(_isInspectorVisible);
            UpdateInspectorTabVisibility();
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
            SanitizeGraphAsset(false);
            _graphView.PopulateGraph(asset, EditorApplication.isPlaying);
            _graphView.SetInlinePreviewsVisible(_showInlinePreviews);
            SetSelectedNode(null);
            RefreshInspectorPanel();
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
            _graphView?.RefreshFromAsset();
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

            var validator = new GraphValidator();
            var errors = validator.Validate(_graphAsset);
            int errorCount = errors.Count(e => e.Severity == ValidationSeverity.Error);
            int warningCount = errors.Count(e => e.Severity == ValidationSeverity.Warning);

            if (errors.Count == 0)
            {
                _statusLabel.text = "✓ Validation passed — no errors.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var error in errors)
                sb.AppendLine(error.ToString());

            _statusLabel.text = $"Validation: {errorCount} error(s), {warningCount} warning(s).";
            Debug.LogWarning($"[GraphValidator] {errors.Count} issue(s):\n{sb}");
        }

        private void SaveGraph()
        {
            if (_graphAsset == null) return;

            EditorUtility.SetDirty(_graphAsset);
            AssetDatabase.SaveAssets();
            _statusLabel.text = $"Saved: {_graphAsset.name}";
        }

        private void RunGraph()
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

            SanitizeGraphAsset(true);

            // Validate first
            var validator = new GraphValidator();
            var errors = validator.Validate(_graphAsset);
            int errorCount = errors.Count(e => e.Severity == ValidationSeverity.Error);
            int warningCount = errors.Count(e => e.Severity == ValidationSeverity.Warning);

            if (errorCount > 0)
            {
                _statusLabel.text = $"✗ Cannot run: {errorCount} validation error(s).";

                var details = new System.Text.StringBuilder();
                foreach (var err in errors)
                    details.AppendLine($"  - {err}");

                Debug.LogWarning($"[GraphRunner] Validation failed with {errorCount} error(s) and {warningCount} warning(s).\n{details}");
                _isRunningGraph = false;
                return;
            }

            if (warningCount > 0)
            {
                Debug.LogWarning($"[GraphRunner] Running with {warningCount} validation warning(s).");

                var warningDetails = new System.Text.StringBuilder();
                foreach (var warning in errors.Where(e => e.Severity == ValidationSeverity.Warning))
                    warningDetails.AppendLine($"  - {warning}");

                Debug.LogWarning($"[GraphRunner] Validation warnings:\n{warningDetails}");
            }

            _progressBar.visible = true;
            _progressBar.value = 0;
            _statusLabel.text = "Running graph...";

            // Determine map size: PreviewSettings > SharedSettings > inline fields
            int mapW = _previewWidth;
            int mapH = _previewHeight;
            if (_previewSettings != null)
            {
                mapW = _previewSettings.PreviewWidth;
                mapH = _previewSettings.PreviewHeight;
            }

            var sharedSettings = _graphAsset.SharedSettings;
            if (sharedSettings != null && sharedSettings.HasMapSize)
            {
                mapW = sharedSettings.MapWidth;
                mapH = sharedSettings.MapHeight;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Compute deterministic seed from graph asset + preview settings + runtime options
            int seed = ComputeDeterministicSeed(mapW, mapH);
            _statusLabel.text = $"Running graph... (seed {seed})";

            // Save previous Unity random state and set deterministic seed for UnityEngine.Random
            var prevRandomState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);

            try
            {
                var context = new NodeContext(seed, CancellationToken.None);
                context.MapSize = new Vector2Int(mapW, mapH);

                // Register shared settings
                if (sharedSettings != null)
                    context.RegisterService(sharedSettings);

                // Register generator services with fallbacks
                RegisterEditorServices(context);

                // Register layer data list so SingleTileLayerNode can populate it
                var layerDataList = new List<WorldLayerData>();
                context.RegisterService(layerDataList);

                var runner = new GraphRunner();
                var result = runner.Execute(_graphAsset, context);
            int previewSize = ResolvePreviewSize(mapW, mapH);
            _graphView?.UpdateNodePreviews(result, _previewSettings, layerDataList, previewSize, _previewHeatmap);
            GraphPreviewWindow.RequestRepaint();

            sw.Stop();
            _progressBar.visible = false;

            if (result.Success)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"✓ Run completed in {sw.ElapsedMilliseconds}ms ({mapW}×{mapH})");
                sb.Append($" | {result.Logs.Count} nodes executed");

                float totalMs = 0;
                foreach (var log in result.Logs)
                    totalMs += log.DurationMs;

                sb.Append($" | Total node time: {totalMs:F1}ms");

                if (layerDataList.Count > 0)
                    sb.Append($" | {layerDataList.Count} layer(s)");

                _statusLabel.text = sb.ToString();

                // Log per-node timing
                Debug.Log($"[GraphRunner] Execution completed in {sw.ElapsedMilliseconds}ms:");
                foreach (var log in result.Logs)
                {
                    string icon = log.Status == NodeStatus.Warning ? "⚠" : "✓";
                    string msg = string.IsNullOrEmpty(log.Message) ? "" : $" — {log.Message}";
                    Debug.Log($"  {icon} [{log.DurationMs:F1}ms | alloc {log.AllocationBytes} B | iter {log.IterationCount}] {log.NodeTitle}{msg}");
                }

                // Highlight node execution times in the graph view
                HighlightExecutionResults(result);
            }
            else
            {
                _statusLabel.text = $"✗ Run failed at node {result.ErrorNodeId}: {result.ErrorMessage}";
                Debug.LogError($"[GraphRunner] Execution failed: {result.ErrorMessage}");
            }

            }
            finally
            {
                UnityEngine.Random.state = prevRandomState;
            }

            _isRunningGraph = false;
        }

        internal bool TryGetBestPreview(out Texture2D previewTexture, out string status)
        {
            if (_graphView != null)
                return _graphView.TryGetBestPreview(out previewTexture, out status);

            previewTexture = null;
            status = "Graph view is not ready";
            return false;
        }

        /// <summary>
        /// Реєструє сервіси генератора з EditorPreviewSettings.
        /// Кожен сервіс реєструється опціонально — якщо ScriptableObject не задано,
        /// лог попереджує, але Run не зупиняється (вузли самі отримають помилку при GetService).
        /// </summary>
        private void RegisterEditorServices(NodeContext context)
        {
            // INoiseProvider — не потребує залежностей
            context.RegisterService<INoiseProvider>(new NoiseMapGeneratorService());
            Debug.Log("[EditorPreview] ✓ INoiseProvider registered.");

            // IVirtualHeightMapGenerator — потребує HeightMapSettings
            var heightSettings = _previewSettings?.HeightMapSettings;
            if (heightSettings != null)
            {
                context.RegisterService<IVirtualHeightMapGenerator>(
                    new VirtualHeightMapGenerator(heightSettings));
                Debug.Log("[EditorPreview] ✓ IVirtualHeightMapGenerator registered.");
            }
            else
            {
                Debug.LogWarning("[EditorPreview] ⚠ HeightMapSettings not assigned in EditorPreviewSettings. " +
                    "HeightToTileNode will fail. Assign it in the Preview Settings asset.");
            }

            // IBiomeResolver — потребує DataBiomesSettings
            var biomesSettings = _previewSettings?.BiomesSettings;
            if (biomesSettings != null)
            {
                context.RegisterService<IBiomeResolver>(
                    new BiomeResolver(biomesSettings));
                Debug.Log("[EditorPreview] ✓ IBiomeResolver registered.");
            }
            else
            {
                Debug.LogWarning("[EditorPreview] ⚠ BiomesSettings not assigned in EditorPreviewSettings. " +
                    "BiomeResolverNode will fail. Assign it in the Preview Settings asset.");
            }

            // IRiverPathfinder — не потребує залежностей
            context.RegisterService<IRiverPathfinder>(new RiverPathfinder());
            Debug.Log("[EditorPreview] ✓ IRiverPathfinder registered.");

            // IWFCService — потребує WFCDataSettings
            var wfcSettings = _previewSettings?.WFCDataSettings;
            if (wfcSettings != null)
            {
                context.RegisterService<IWFCService>(new WFCService(wfcSettings));
                Debug.Log("[EditorPreview] ✓ IWFCService registered.");
            }
            else
            {
                Debug.LogWarning("[EditorPreview] ⚠ WFCDataSettings not assigned in EditorPreviewSettings. " +
                    "WFC nodes will fail. Assign it in the Preview Settings asset.");
            }
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
            _activeInspectorTab = (InspectorTab)Mathf.Clamp(settings.inspectorTabIndex, 0, 1);
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
                return;

            if (_selectedNodeEditor != null)
                DestroyImmediate(_selectedNodeEditor);

            _selectedNodeEditor = null;
            _selectedNode = node;
        }

        private void RefreshInspectorPanel()
        {
            _nodeInspectorGui?.MarkDirtyRepaint();
            _graphSettingsGui?.MarkDirtyRepaint();
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

            EditorGUILayout.LabelField(_selectedNode.Title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Type", _selectedNode.GetType().Name);
            EditorGUILayout.Space(4);

            DrawSerializedObjectWithoutScript(new SerializedObject(_selectedNode));

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_selectedNode);
                EditorUtility.SetDirty(_graphAsset);
                RequestAutoRun();
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

        private void OnGraphChanged()
        {
            RequestAutoRun();
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
            RunGraph();
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
            if (_activeInspectorTab == tab) return;
            _activeInspectorTab = tab;
            UpdateInspectorTabVisibility();
            SaveWindowSettings();
        }

        private void UpdateInspectorTabVisibility()
        {
            if (_tabSettingsContent != null)
                _tabSettingsContent.style.display = _activeInspectorTab == InspectorTab.Settings
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            if (_tabPreviewContent != null)
                _tabPreviewContent.style.display = _activeInspectorTab == InspectorTab.Preview
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

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

                var fieldRect = GUILayoutUtility.GetLastRect();
                if (!string.IsNullOrEmpty(tooltip) && fieldRect.Contains(Event.current.mousePosition))
                    hoveredTooltip = tooltip;

                expanded = false;
            }

            DrawInspectorHoverTooltip(hoveredTooltip);

            serializedObject.ApplyModifiedProperties();
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
                _graphView?.RefreshFromAsset();

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
}
