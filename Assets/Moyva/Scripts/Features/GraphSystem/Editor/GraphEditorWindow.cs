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

            toolbar.Add(new ToolbarButton(() => SaveGraph())
                { text = "Save" });

            rootVisualElement.Insert(0, toolbar);
        }

        private void ConstructStatusBar()
        {
            _statusLabel = new Label("No graph loaded")
            {
                style =
                {
                    position = Position.Absolute,
                    bottom = 0,
                    left = 0,
                    right = 0,
                    height = 20,
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingLeft = 8
                }
            };
            rootVisualElement.Add(_statusLabel);
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
    }
}
