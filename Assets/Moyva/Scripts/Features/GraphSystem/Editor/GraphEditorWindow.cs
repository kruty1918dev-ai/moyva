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

        private GeneratorGraphView _graphView;
        private VisualElement _contentContainer;
        private ScrollView _rightPanel;
        private VisualElement _leftPanel;
        private VisualElement _layerListContainer;
        private Image _compositePreviewImage;
        private string _selectedLayerId;
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

        private NodeBase _selectedNode;
        private UnityEditor.Editor _selectedNodeEditor;

        private enum InspectorTab { Settings = 0, Preview = 1, BuildLayers = 2 }
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
        private Vector3 _savedCameraPosition = Vector3.zero;
        private Vector3 _savedCameraScale = Vector3.one;

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
            }

            DisposeLayerPreviewTextures();

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
                RestoreCameraTransform();
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
                _graphView.PopulateGraph(_graphAsset, EditorApplication.isPlaying);
                RestoreCameraTransform();
                RebuildLayerList();
                UpdateStatusBar();
            }
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
                    width = 200,
                    minWidth = 160,
                    flexShrink = 0,
                    borderRightWidth = 1,
                    borderRightColor = new Color(0.22f, 0.22f, 0.22f),
                    backgroundColor = new Color(0.12f, 0.12f, 0.12f),
                    paddingLeft = 6,
                    paddingRight = 6,
                    paddingTop = 6,
                    paddingBottom = 8
                }
            };

            var header = new Label("Шари генератора")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 6
                }
            };
            _leftPanel.Add(header);

            _layerListContainer = new VisualElement();
            _leftPanel.Add(_layerListContainer);

            var buttonsRow = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, marginTop = 6 }
            };

            var addButton = new Button(AddLayer) { text = "+ Шар" };
            addButton.style.flexGrow = 1;
            addButton.style.marginRight = 4;
            addButton.tooltip = "Додати новий шар генератора.";
            buttonsRow.Add(addButton);

            var removeButton = new Button(RemoveSelectedLayer) { text = "–" };
            removeButton.style.width = 28;
            removeButton.tooltip = "Видалити вибраний шар (разом із його вузлами).";
            buttonsRow.Add(removeButton);

            _leftPanel.Add(buttonsRow);

            var compositeHeader = new Label("Ізометричне превʼю")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 10,
                    marginBottom = 4
                }
            };
            _leftPanel.Add(compositeHeader);

            _compositePreviewImage = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    height = 150,
                    backgroundColor = new Color(0.05f, 0.06f, 0.08f)
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

            if (!string.IsNullOrEmpty(_selectedLayerId)
                && _graphAsset.GetLayerById(_selectedLayerId) == null)
                _selectedLayerId = null;

            var orderedLayers = _graphAsset.Layers
                .Where(l => l != null)
                .OrderBy(l => l.SortingOrder)
                .ToList();

            foreach (var layer in orderedLayers)
            {
                string layerId = layer.Id;
                bool isSelected = layerId == _selectedLayerId;

                var row = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginBottom = 2,
                        paddingLeft = 4,
                        paddingRight = 4,
                        paddingTop = 3,
                        paddingBottom = 3,
                        backgroundColor = isSelected
                            ? new Color(0.24f, 0.36f, 0.5f)
                            : new Color(0.16f, 0.16f, 0.16f)
                    }
                };

                var swatch = new VisualElement
                {
                    style =
                    {
                        width = 12,
                        height = 12,
                        marginRight = 6,
                        backgroundColor = layer.Color
                    }
                };
                row.Add(swatch);

                var nameLabel = new Label(layer.Name)
                {
                    style = { flexGrow = 1, unityTextOverflowPosition = TextOverflowPosition.End }
                };
                row.Add(nameLabel);

                row.RegisterCallback<MouseDownEvent>(_ => SelectLayer(layerId));
                _layerListContainer.Add(row);

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
                                height = 48,
                                marginBottom = 4,
                                marginLeft = 2,
                                marginRight = 2
                            }
                        };
                        _layerListContainer.Add(thumbImage);
                    }
                }
            }

            _graphView?.SetVisibleLayer(_selectedLayerId);
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
            _selectedLayerId = _selectedLayerId == layerId ? null : layerId;
            if (_graphView != null)
                _graphView.ActiveLayerId = _selectedLayerId;
            RebuildLayerList();
            RefreshInspectorPanel();
        }

        private void AddLayer()
        {
            if (_graphAsset == null)
                return;

            Undo.RecordObject(_graphAsset, "Add Layer");
            var layer = _graphAsset.AddLayer($"Layer {_graphAsset.Layers.Count + 1}");
            EditorUtility.SetDirty(_graphAsset);
            SelectLayer(layer.Id);
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
            _graphAsset.RemoveLayer(_selectedLayerId);
            EditorUtility.SetDirty(_graphAsset);

            _selectedLayerId = _graphAsset.Layers.Count > 0 ? _graphAsset.Layers[0].Id : null;
            if (_graphView != null)
            {
                _graphView.ActiveLayerId = _selectedLayerId;
                _graphView.PopulateGraph(_graphAsset, EditorApplication.isPlaying);
            }
            RebuildLayerList();
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

            _tabBuildLayersButton = new Button(() => SetInspectorTab(InspectorTab.BuildLayers))
            {
                text = "Тайли",
                tooltip = "Налаштування тайлів і build-шарів (TileWorldCreator)."
            };
            _tabBuildLayersButton.style.flexGrow = 1;
            tabHeaderRow.Add(_tabBuildLayersButton);

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

            _tabBuildLayersContent = new VisualElement();
            var buildLayersHeader = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 }
            };
            var buildLayersTitle = new Label("Build-шари (TileWorldCreator)")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 }
            };
            buildLayersHeader.Add(buildLayersTitle);
            var refreshBuildLayers = new Button(RebuildBuildLayersPanel) { text = "↻" };
            refreshBuildLayers.tooltip = "Пересинхронізувати build-шари зі шарами графа.";
            buildLayersHeader.Add(refreshBuildLayers);
            _tabBuildLayersContent.Add(buildLayersHeader);

            _buildLayersHost = new VisualElement();
            _tabBuildLayersContent.Add(_buildLayersHost);
            _nodeInspectorSection.Add(_tabBuildLayersContent);

            _rightPanel.Add(_nodeInspectorSection);

            _contentContainer.Add(_rightPanel);
            SetInspectorVisible(_isInspectorVisible);
            UpdateInspectorTabVisibility();
            if (_activeInspectorTab == InspectorTab.BuildLayers)
                RebuildBuildLayersPanel();
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
            GraphStaticNodeUtility.EnsureStaticNodes(_graphAsset);
            SanitizeGraphAsset(false);
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

            SanitizeGraphAsset(true);

            // Validate first
            var validator = new GraphValidator();
            var errors = validator.Validate(_graphAsset);
            int errorCount = errors.Count(e => e.Severity == ValidationSeverity.Error);
            int warningCount = errors.Count(e => e.Severity == ValidationSeverity.Warning);

            if (errorCount > 0)
            {
                _statusLabel.text = $"✗ Cannot run: {errorCount} validation error(s).";

                if (!isAutoRun)
                {
                    var details = new System.Text.StringBuilder();
                    foreach (var err in errors)
                        details.AppendLine($"  - {err}");

                    Debug.LogWarning($"[GraphRunner] Validation failed with {errorCount} error(s) and {warningCount} warning(s).\n{details}");
                }
                _isRunningGraph = false;
                return;
            }

            if (warningCount > 0 && !isAutoRun)
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

            var sw = System.Diagnostics.Stopwatch.StartNew();

            int seed = GetSeedFromGraph();
            GlobalSeed.Set(seed);
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
                {
                    context.ApplySharedSettings(sharedSettings);
                    context.RegisterService(sharedSettings);
                }

                // Register generator services with fallbacks
                RegisterEditorServices(context, runtimeSettings);

                // Register layer data list so SingleTileLayerNode can populate it
                var layerDataList = new List<WorldLayerData>();
                context.RegisterService(layerDataList);

                var runner = new GraphRunner();
                var result = runner.Execute(_graphAsset, context);
            int previewSize = ResolvePreviewSize(mapW, mapH);
            _graphView?.UpdateNodePreviews(result, _previewSettings, layerDataList, previewSize, _previewHeatmap);
            _lastResult = result;
            RebuildLayerPreviews();
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
                if (!isAutoRun)
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
        private void RegisterEditorServices(NodeContext context, RuntimeExecutionSettings runtimeSettings)
        {
            context.RegisterService<IGeneratorDataRegistry>(new GeneratorDataRegistry());
            Debug.Log("[EditorPreview] ✓ IGeneratorDataRegistry registered.");

            var tileRegistry = runtimeSettings.TileRegistry ?? _previewSettings?.TileRegistry;
            if (tileRegistry != null)
            {
                context.RegisterService(tileRegistry);
                Debug.Log("[EditorPreview] ✓ TileRegistrySO registered.");
            }
            else
            {
                Debug.LogWarning("[EditorPreview] ⚠ TileRegistrySO not assigned in EditorPreviewSettings. " +
                    "SingleTileLayerNode sprite fallback will not be available.");
            }

            if (!string.IsNullOrEmpty(runtimeSettings.Source))
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
            if (_graphView != null)
            {
                var tr = _graphView.contentViewContainer.resolvedStyle.translate;
                var sc = _graphView.contentViewContainer.resolvedStyle.scale;
                settings.cameraPosition = new Vector3(tr.x, tr.y, 0f);
                settings.cameraScale = sc.value;
            }

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
                return;

            if (_selectedNodeEditor != null)
                DestroyImmediate(_selectedNodeEditor);

            _selectedNodeEditor = null;
            _selectedNode = node;
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

            EditorGUILayout.LabelField(_selectedNode.Title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Type", _selectedNode.GetType().Name);
            EditorGUILayout.Space(4);

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

            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                options.Add($"{layer.Name} (order {layer.SortingOrder})");
                if (!string.IsNullOrEmpty(currentId) && layer.Id == currentId)
                    selectedIndex = i;
            }

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Шар-джерело", selectedIndex, options.ToArray());
            if (!EditorGUI.EndChangeCheck())
                return false;

            newIndex = Mathf.Clamp(newIndex, 0, layers.Count - 1);
            Undo.RecordObject(node, "Change Layer Ref Source");
            node.SetSourceLayerId(layers[newIndex].Id);
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
            Color newColor = EditorGUILayout.ColorField("Color", layer.Color);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_graphAsset, "Edit Layer Settings");
                layer.Name = string.IsNullOrWhiteSpace(newName) ? "Layer" : newName;
                layer.SortingOrder = newOrder;
                layer.Enabled = newEnabled;
                layer.DefaultHeight = newHeight;
                layer.Color = newColor;

                EditorUtility.SetDirty(_graphAsset);
                RebuildLayerList();
                _graphView?.RefreshFromAsset();
                RequestAutoRun();
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
            _selectedLayerId = null;
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
            if (_activeInspectorTab == tab) return;
            _activeInspectorTab = tab;
            UpdateInspectorTabVisibility();
            if (tab == InspectorTab.BuildLayers)
                RebuildBuildLayersPanel();
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

            if (_tabBuildLayersContent != null)
                _tabBuildLayersContent.style.display = _activeInspectorTab == InspectorTab.BuildLayers
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            if (_nodeInspectorDivider != null)
                _nodeInspectorDivider.style.display = _activeInspectorTab == InspectorTab.BuildLayers
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;

            if (_tabSettingsButton != null)
                _tabSettingsButton.style.unityFontStyleAndWeight = _activeInspectorTab == InspectorTab.Settings
                    ? FontStyle.Bold
                    : FontStyle.Normal;

            if (_tabPreviewButton != null)
                _tabPreviewButton.style.unityFontStyleAndWeight = _activeInspectorTab == InspectorTab.Preview
                    ? FontStyle.Bold
                    : FontStyle.Normal;

            if (_tabBuildLayersButton != null)
                _tabBuildLayersButton.style.unityFontStyleAndWeight = _activeInspectorTab == InspectorTab.BuildLayers
                    ? FontStyle.Bold
                    : FontStyle.Normal;

            _nodeInspectorGui?.MarkDirtyRepaint();
            _graphSettingsGui?.MarkDirtyRepaint();
        }

        /// <summary>
        /// Будує панель build-шарів через рефлексію до
        /// Kruty1918.Moyva.Generator.Editor.GraphBuildLayersPanel.Build(GraphAsset).
        /// GraphSystem.Editor не посилається на Generator.Editor напряму.
        /// </summary>
        private void RebuildBuildLayersPanel()
        {
            if (_buildLayersHost == null)
                return;

            _buildLayersHost.Clear();

            if (_graphAsset == null)
            {
                _buildLayersHost.Add(new HelpBox(
                    "Відкрийте граф-асет, щоб налаштувати build-шари.",
                    HelpBoxMessageType.Info));
                return;
            }

            try
            {
                var type = System.Type.GetType(
                    "Kruty1918.Moyva.Generator.Editor.GraphBuildLayersPanel, Kruty1918.Moyva.Generator.Editor");

                if (type == null)
                {
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType("Kruty1918.Moyva.Generator.Editor.GraphBuildLayersPanel");
                        if (type != null)
                            break;
                    }
                }

                var method = type?.GetMethod(
                    "Build",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (method == null)
                {
                    _buildLayersHost.Add(new HelpBox(
                        "Модуль Generator.Editor недоступний (GraphBuildLayersPanel не знайдено).",
                        HelpBoxMessageType.Warning));
                    return;
                }

                if (method.Invoke(null, new object[] { _graphAsset }) is VisualElement panel)
                    _buildLayersHost.Add(panel);
            }
            catch (System.Exception e)
            {
                _buildLayersHost.Add(new HelpBox(
                    "Помилка побудови панелі build-шарів: " + e.Message,
                    HelpBoxMessageType.Error));
            }
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
