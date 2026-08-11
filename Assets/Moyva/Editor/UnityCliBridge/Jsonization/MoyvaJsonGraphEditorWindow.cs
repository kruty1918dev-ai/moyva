using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    /// <summary>
    /// JSON-native graph editor surface. It edits node positions/types/layers directly
    /// in *.json; it never creates ScriptableObject nodes/sub-assets.
    /// </summary>
    public sealed class MoyvaJsonGraphEditorWindow : EditorWindow
    {
        private sealed class NodeView : VisualElement
        {
            public readonly JObject Json;
            private Vector2 _startMouse;
            private Vector2 _startPos;
            public NodeView(JObject json, Action changed)
            {
                Json = json;
                style.position = Position.Absolute;
                style.width = 190;
                style.minHeight = 76;
                style.paddingLeft = 8; style.paddingRight = 8; style.paddingTop = 6; style.paddingBottom = 6;
                style.backgroundColor = new Color(0.12f, 0.13f, 0.15f, 0.98f);
                style.borderTopWidth = style.borderRightWidth = style.borderBottomWidth = style.borderLeftWidth = 1;
                style.borderTopColor = style.borderRightColor = style.borderBottomColor = style.borderLeftColor = new Color(0.45f,0.48f,0.52f,1);
                string type = json.Value<string>("$type") ?? json.Value<string>("type") ?? "node";
                string id = json.Value<string>("id") ?? json.Value<string>("nodeId") ?? "?";
                string layer = json.Value<string>("layer") ?? json.Value<string>("layerId") ?? "global";
                Add(new Label(type) { style = { unityFontStyleAndWeight = FontStyle.Bold } });
                Add(new Label(id));
                Add(new Label("layer: " + layer));
                SetPosition(ReadPosition(json));
                RegisterCallback<PointerDownEvent>(e =>
                {
                    if (e.button != 0) return;
                    _startMouse = new Vector2(e.position.x, e.position.y);
                    _startPos = new Vector2(resolvedStyle.left, resolvedStyle.top);
                    PointerCaptureHelper.CapturePointer(this, e.pointerId);
                });
                RegisterCallback<PointerMoveEvent>(e =>
                {
                    if (!PointerCaptureHelper.HasPointerCapture(this, e.pointerId)) return;
                    Vector2 delta = new Vector2(e.position.x, e.position.y) - _startMouse;
                    SetPosition(_startPos + delta);
                });
                RegisterCallback<PointerUpEvent>(e =>
                {
                    if (!PointerCaptureHelper.HasPointerCapture(this, e.pointerId)) return;
                    PointerCaptureHelper.ReleasePointer(this, e.pointerId);
                    WritePosition(json, new Vector2(resolvedStyle.left, resolvedStyle.top));
                    changed?.Invoke();
                });
            }
            private void SetPosition(Vector2 p) { style.left = p.x; style.top = p.y; }
            private static Vector2 ReadPosition(JObject node)
            {
                JObject p = node["editorPosition"] as JObject ?? node["position"] as JObject;
                return p != null ? new Vector2(p.Value<float?>("x") ?? 20, p.Value<float?>("y") ?? 20) : new Vector2(20,20);
            }
            private static void WritePosition(JObject node, Vector2 p)
            {
                string key = node.Property("editorPosition") != null ? "editorPosition" : "position";
                node[key] = new JObject { ["x"] = Mathf.Round(p.x), ["y"] = Mathf.Round(p.y) };
            }
        }

        private string _path;
        private JObject _graph;
        private VisualElement _canvas;
        private Label _status;
        private bool _dirty;

        [MenuItem("Moyva/Generator/JSON Graph Editor")]
        public static void OpenWindow()
        {
            var w = GetWindow<MoyvaJsonGraphEditorWindow>();
            w.titleContent = new GUIContent("Moyva JSON Graph");
            w.minSize = new Vector2(900, 600);
        }

        public void CreateGUI()
        {
            var bar = new VisualElement { style = { flexDirection = FlexDirection.Row, height = 30 } };
            bar.Add(new Button(OpenGraph) { text = "Open Graph JSON" });
            bar.Add(new Button(SaveGraph) { text = "Save" });
            bar.Add(new Button(RefreshGraph) { text = "Refresh" });
            _status = new Label("JSON graph — no ScriptableObject sub-assets") { style = { flexGrow = 1 } };
            bar.Add(_status);
            _canvas = new VisualElement();
            _canvas.style.flexGrow = 1;
            _canvas.style.position = Position.Relative;
            _canvas.generateVisualContent += DrawConnections;
            rootVisualElement.Add(bar);
            rootVisualElement.Add(_canvas);
        }

        private void OpenGraph()
        {
            string file = EditorUtility.OpenFilePanel("Open generator graph JSON", Path.GetFullPath("Assets/Moyva/Presets/Graphs"), "json");
            if (string.IsNullOrWhiteSpace(file)) return;
            _path = file;
            Load();
        }

        private void RefreshGraph() { if (!string.IsNullOrWhiteSpace(_path)) Load(); }

        private void Load()
        {
            try
            {
                _graph = JObject.Parse(File.ReadAllText(_path));
                _dirty = false;
                Rebuild();
                _status.text = $"PASS — {_graph.Value<string>("id")} nodes={GetNodes().Count}";
            }
            catch (Exception ex) { _status.text = "FAIL — " + ex.Message; }
        }

        private void SaveGraph()
        {
            if (_graph == null || string.IsNullOrWhiteSpace(_path)) return;
            try
            {
                File.WriteAllText(_path, _graph.ToString(Formatting.Indented) + "\n");
                AssetDatabase.Refresh();
                _dirty = false;
                _status.text = "PASS — saved " + _path;
            }
            catch (Exception ex) { _status.text = "FAIL — " + ex.Message; }
        }

        private JArray GetNodes() => _graph?["nodes"] as JArray ?? new JArray();
        private JArray GetConnections() => _graph?["connections"] as JArray ?? new JArray();

        private void Rebuild()
        {
            _canvas.Clear();
            foreach (JObject node in GetNodes().OfType<JObject>())
                _canvas.Add(new NodeView(node, () => { _dirty = true; _canvas.MarkDirtyRepaint(); }));
            _canvas.MarkDirtyRepaint();
        }

        private void DrawConnections(MeshGenerationContext ctx)
        {
            if (_graph == null) return;
            var painter = ctx.painter2D;
            painter.lineWidth = 2f;
            painter.strokeColor = new Color(0.55f, 0.66f, 0.82f, 0.8f);
            var views = _canvas.Children().OfType<NodeView>().ToDictionary(
                v => v.Json.Value<string>("id") ?? v.Json.Value<string>("nodeId") ?? string.Empty,
                v => v,
                StringComparer.OrdinalIgnoreCase);
            foreach (JObject c in GetConnections().OfType<JObject>())
            {
                string from = c.SelectToken("from.node")?.Value<string>() ?? c.Value<string>("sourceNodeId");
                string to = c.SelectToken("to.node")?.Value<string>() ?? c.Value<string>("targetNodeId");
                if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || !views.TryGetValue(from, out var a) || !views.TryGetValue(to, out var b)) continue;
                Vector2 p1 = new Vector2(a.resolvedStyle.left + a.resolvedStyle.width, a.resolvedStyle.top + 28);
                Vector2 p2 = new Vector2(b.resolvedStyle.left, b.resolvedStyle.top + 28);
                painter.BeginPath(); painter.MoveTo(p1); painter.BezierCurveTo(p1 + Vector2.right * 50, p2 + Vector2.left * 50, p2); painter.Stroke();
            }
        }

        private void OnDestroy()
        {
            if (_dirty) Debug.LogWarning("[MoyvaJson] Graph editor closed with unsaved JSON changes: " + _path);
        }
    }
}
