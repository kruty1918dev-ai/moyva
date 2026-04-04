using System.Threading;
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
        private Label _statusLabel;
        private ProgressBar _progressBar;

        // Survives domain reload / play mode transition
        [SerializeField] private string _graphAssetGuid;
        private GraphAsset _graphAsset;

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
            ConstructGraphView();
            ConstructToolbar();
            ConstructStatusBar();

            // Restore graph after domain reload
            RestoreGraphAsset();

            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            if (_graphView != null)
                rootVisualElement.Remove(_graphView);
        }

        private void RestoreGraphAsset()
        {
            if (_graphAsset != null)
            {
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
            _graphView = new GeneratorGraphView(this);
            _graphView.StretchToParentSize();
            _graphView.style.marginTop = 22;
            _graphView.style.marginBottom = 20;
            rootVisualElement.Add(_graphView);
        }

        private void ConstructToolbar()
        {
            var toolbar = new Toolbar();

            var assetField = new ObjectField("Graph")
            {
                objectType = typeof(GraphAsset),
                allowSceneObjects = false,
                style = { minWidth = 200 }
            };
            assetField.value = _graphAsset;
            assetField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is GraphAsset asset)
                    LoadGraph(asset);
            });
            toolbar.Add(assetField);

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(new ToolbarButton(() => ValidateGraph())
                { text = "Validate" });

            toolbar.Add(new ToolbarButton(() => RunGraph())
                { text = "▶ Run" });

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(new ToolbarButton(() => _graphView?.AutoLayout())
                { text = "Auto-Layout" });

            toolbar.Add(new ToolbarButton(() => _graphView?.GroupSelection())
                { text = "Group" });

            toolbar.Add(new ToolbarButton(() => _graphView?.AddStickyNote())
                { text = "Note" });

            // Minimap toggle
            var minimapToggle = new ToolbarToggle { text = "Minimap", value = true };
            minimapToggle.RegisterValueChangedCallback(evt =>
                _graphView?.SetMinimapVisible(evt.newValue));
            toolbar.Add(minimapToggle);

            toolbar.Add(new ToolbarSpacer { flex = true });

            toolbar.Add(new ToolbarButton(() => SaveGraph())
                { text = "Save" });

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

            _graphView.PopulateGraph(asset, EditorApplication.isPlaying);
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

        private void ValidateGraph()
        {
            if (_graphAsset == null)
            {
                EditorUtility.DisplayDialog("Validation",
                    "No graph loaded.", "OK");
                return;
            }

            var validator = new GraphValidator();
            var errors = validator.Validate(_graphAsset);

            if (errors.Count == 0)
            {
                _statusLabel.text = "✓ Validation passed — no errors.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var error in errors)
                sb.AppendLine(error.ToString());

            _statusLabel.text = $"✗ {errors.Count} validation issue(s).";
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
            if (_graphAsset == null)
            {
                EditorUtility.DisplayDialog("Run Graph",
                    "No graph loaded.", "OK");
                return;
            }

            // Validate first
            var validator = new GraphValidator();
            var errors = validator.Validate(_graphAsset);
            if (errors.Count > 0)
            {
                _statusLabel.text = $"✗ Cannot run: {errors.Count} validation error(s).";
                Debug.LogWarning($"[GraphRunner] Validation failed with {errors.Count} error(s).");
                return;
            }

            _progressBar.visible = true;
            _progressBar.value = 0;
            _statusLabel.text = "Running graph...";

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var context = new NodeContext(
                seed: UnityEngine.Random.Range(0, int.MaxValue),
                cancellation: CancellationToken.None);
            context.MapSize = new Vector2Int(64, 64);

            var runner = new GraphRunner();
            var result = runner.Execute(_graphAsset, context);

            sw.Stop();
            _progressBar.visible = false;

            if (result.Success)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"✓ Run completed in {sw.ElapsedMilliseconds}ms");
                sb.Append($" | {result.Logs.Count} nodes executed");

                float totalMs = 0;
                foreach (var log in result.Logs)
                    totalMs += log.DurationMs;

                sb.Append($" | Total node time: {totalMs:F1}ms");
                _statusLabel.text = sb.ToString();

                // Log per-node timing
                Debug.Log($"[GraphRunner] Execution completed in {sw.ElapsedMilliseconds}ms:");
                foreach (var log in result.Logs)
                {
                    string icon = log.Status == NodeStatus.Warning ? "⚠" : "✓";
                    string msg = string.IsNullOrEmpty(log.Message) ? "" : $" — {log.Message}";
                    Debug.Log($"  {icon} [{log.DurationMs:F1}ms] {log.NodeTitle}{msg}");
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
                        nodeView.tooltip = $"{log.NodeTitle}: {log.DurationMs:F1}ms"
                            + (string.IsNullOrEmpty(log.Message) ? "" : $"\n{log.Message}");
                        break;
                    }
                }
            }
        }
    }
}
