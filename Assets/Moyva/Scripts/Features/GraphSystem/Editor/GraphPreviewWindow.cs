using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    public sealed class GraphPreviewWindow : EditorWindow
    {
        private static GraphPreviewWindow _instance;

        private GraphEditorWindow _owner;
        private Texture2D _previousTexture;
        private Texture2D _lastTexture;
        private string _lastStatus = "No preview";

        private float _zoom = 1f;
        private Vector2 _pan;
        private const float MinZoom = 0.25f;
        private const float MaxZoom = 16f;
        private bool _compareMode;
        private float _compareSplit = 0.5f;

        // Hover info
        private float[,]  _lastFloatMap;
        private string[,] _lastTileMap;
        private Vector2   _mouseInWindow;
        private Rect      _lastDrawRect;    // де намальована текстура у window-space

        public static void Open(GraphEditorWindow owner)
        {
            var window = GetWindow<GraphPreviewWindow>("Graph Preview");
            window.minSize = new Vector2(260, 220);
            window.titleContent = new GUIContent(
                "Graph Preview",
                "Окреме вікно перегляду згенерованої мапи. Підтримує zoom (wheel/slider) і pan (middle mouse drag)."
            );
            window._owner = owner;
            _instance = window;
            window.Repaint();
        }

        public static void RequestRepaint()
        {
            _instance?.Repaint();
        }

        private void Update()
        {
            // Lightweight polling to keep preview in sync with graph selection.
            if (_owner != null)
                Repaint();
        }

        private void OnGUI()
        {
            // Відстежуємо позицію миші (layout+repaint event)
            if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.Repaint)
                _mouseInWindow = Event.current.mousePosition;

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                _owner = EditorGUILayout.ObjectField("Graph Window", _owner,
                    typeof(GraphEditorWindow), true) as GraphEditorWindow;

                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                    Repaint();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Zoom", GUILayout.Width(40));
                float newZoom = EditorGUILayout.Slider(_zoom, MinZoom, MaxZoom);
                if (!Mathf.Approximately(newZoom, _zoom))
                    _zoom = newZoom;

                if (GUILayout.Button("1:1", GUILayout.Width(50)))
                    _zoom = 1f;

                if (GUILayout.Button("Fit", GUILayout.Width(50)))
                    FitToWindow();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _compareMode = EditorGUILayout.ToggleLeft("Compare before/after", _compareMode, GUILayout.Width(170));
                using (new EditorGUI.DisabledScope(!_compareMode || _previousTexture == null))
                {
                    _compareSplit = EditorGUILayout.Slider(_compareSplit, 0.05f, 0.95f);
                }
            }

            if (_owner == null)
            {
                EditorGUILayout.HelpBox(
                    "Open this window from Graph Editor toolbar: Preview Window.",
                    MessageType.Info);
                return;
            }

            if (!_owner.TryGetBestPreview(out var tex, out var status) || tex == null)
            {
                _lastTexture = null;
                _lastStatus = string.IsNullOrEmpty(status) ? "No preview" : status;
                EditorGUILayout.HelpBox(_lastStatus, MessageType.None);
                return;
            }

            if (_lastTexture != null && _lastTexture != tex)
                _previousTexture = _lastTexture;

            _lastTexture = tex;
            _lastStatus = string.IsNullOrEmpty(status) ? "Preview" : status;

            // Отримуємо raw-дані для hover-підказки
            _owner.TryGetBestRawMaps(out _lastFloatMap, out _lastTileMap);

            EditorGUILayout.LabelField(_lastStatus, EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            var rect = GUILayoutUtility.GetRect(
                position.width - 16f,
                position.height - 80f,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            HandleInput(rect);

            if (Event.current.type == EventType.Repaint)
            {
                DrawZoomablePreview(rect, _lastTexture);
            }
        }

        private void FitToWindow()
        {
            if (_lastTexture == null)
            {
                _zoom = 1f;
                _pan = Vector2.zero;
                return;
            }

            float sx = (position.width - 16f) / Mathf.Max(1f, _lastTexture.width);
            float sy = (position.height - 110f) / Mathf.Max(1f, _lastTexture.height);
            _zoom = Mathf.Clamp(Mathf.Min(sx, sy), MinZoom, MaxZoom);
            _pan = Vector2.zero;
        }

        private void HandleInput(Rect viewport)
        {
            if (_lastTexture == null) return;

            var evt = Event.current;
            if (evt == null) return;
            if (!viewport.Contains(evt.mousePosition)) return;

            if (evt.type == EventType.ScrollWheel)
            {
                float factor = evt.delta.y > 0f ? 0.9f : 1.1f;
                var mouseLocal = evt.mousePosition - viewport.position;
                var before = (mouseLocal - _pan) / Mathf.Max(0.0001f, _zoom);

                _zoom = Mathf.Clamp(_zoom * factor, MinZoom, MaxZoom);

                var after = before * _zoom + _pan;
                _pan += mouseLocal - after;
                evt.Use();
            }

            if (evt.type == EventType.MouseDrag && evt.button == 2)
            {
                _pan += evt.delta;
                evt.Use();
            }

            // Перемальовуємо при русі миші для оновлення hover-підказки
            if (evt.type == EventType.MouseMove)
                Repaint();
        }

        private void DrawZoomablePreview(Rect viewport, Texture2D texture)
        {
            EditorGUI.DrawRect(viewport, new Color(0.08f, 0.08f, 0.08f, 1f));

            float w = texture.width * _zoom;
            float h = texture.height * _zoom;

            var drawRect = new Rect(
                viewport.x + _pan.x,
                viewport.y + _pan.y,
                w,
                h);

            if (Mathf.Approximately(_pan.x, 0f) && Mathf.Approximately(_pan.y, 0f))
            {
                drawRect.x = viewport.x + (viewport.width - drawRect.width) * 0.5f;
                drawRect.y = viewport.y + (viewport.height - drawRect.height) * 0.5f;
            }

            _lastDrawRect = drawRect;

            GUI.BeginClip(viewport);
            var localRect = new Rect(
                drawRect.x - viewport.x,
                drawRect.y - viewport.y,
                drawRect.width,
                drawRect.height);

            if (_compareMode && _previousTexture != null)
            {
                GUI.DrawTexture(localRect, _previousTexture, ScaleMode.StretchToFill, true);

                float splitX = localRect.x + localRect.width * _compareSplit;
                var right = new Rect(splitX, localRect.y, localRect.xMax - splitX, localRect.height);
                GUI.BeginClip(right);
                GUI.DrawTexture(new Rect(localRect.x - right.x, localRect.y - right.y, localRect.width, localRect.height),
                    texture, ScaleMode.StretchToFill, true);
                GUI.EndClip();

                EditorGUI.DrawRect(new Rect(viewport.x + splitX, viewport.y, 1f, viewport.height), Color.white);
            }
            else
            {
                GUI.DrawTexture(localRect, texture, ScaleMode.StretchToFill, true);
            }

            GUI.EndClip();

            GUI.Label(
                new Rect(viewport.x + 8f, viewport.y + 8f, 220f, 20f),
                $"Zoom: {_zoom:F2}x  |  Drag: middle mouse");

            // ── Hover overlay: показуємо координати та значення пікселя ──
            DrawHoverOverlay(viewport, texture);
        }

        private void DrawHoverOverlay(Rect viewport, Texture2D texture)
        {
            if (!viewport.Contains(_mouseInWindow)) return;

            // Перетворюємо позицію миші в координати текстури
            float relX = (_mouseInWindow.x - _lastDrawRect.x) / Mathf.Max(1f, _lastDrawRect.width);
            float relY = (_mouseInWindow.y - _lastDrawRect.y) / Mathf.Max(1f, _lastDrawRect.height);

            if (relX < 0f || relX > 1f || relY < 0f || relY > 1f) return;

            int texW = texture.width;
            int texH = texture.height;

            int px = Mathf.Clamp(Mathf.FloorToInt(relX * texW), 0, texW - 1);
            int py = Mathf.Clamp(Mathf.FloorToInt(relY * texH), 0, texH - 1);

            // GUI.DrawTexture відображає y=0 текстури внизу екрану,
            // тому relY=0 (верх екрану) відповідає y=texH-1 у SetPixel-координатах.
            // Фліпуємо Y щоб отримати правильний рядок карти.
            int pyFlipped = texH - 1 - py;

            string valueText;

            if (_lastFloatMap != null)
            {
                int mapW = _lastFloatMap.GetLength(0);
                int mapH = _lastFloatMap.GetLength(1);

                int mx = Mathf.Clamp(Mathf.FloorToInt((float)px / texW * mapW), 0, mapW - 1);
                int my = Mathf.Clamp(Mathf.FloorToInt((float)pyFlipped / texH * mapH), 0, mapH - 1);

                float val = _lastFloatMap[mx, my];
                valueText = $"Height: {val:F4}  |  [{mx}, {my}]";
            }
            else if (_lastTileMap != null)
            {
                int mapW = _lastTileMap.GetLength(0);
                int mapH = _lastTileMap.GetLength(1);

                int mx = Mathf.Clamp(Mathf.FloorToInt((float)px / texW * mapW), 0, mapW - 1);
                int my = Mathf.Clamp(Mathf.FloorToInt((float)pyFlipped / texH * mapH), 0, mapH - 1);

                string tile = _lastTileMap[mx, my];
                valueText = $"Tile: {tile ?? "—"}  |  [{mx}, {my}]";
            }
            else
            {
                valueText = $"[{px}, {py}]";
            }

            // Малюємо підказку поруч з курсором
            var style = new GUIStyle(EditorStyles.label)
            {
                normal   = { textColor = Color.white },
                fontSize = 11,
                padding  = new RectOffset(6, 6, 3, 3)
            };

            var content = new GUIContent(valueText);
            var size    = style.CalcSize(content);
            float tipX  = _mouseInWindow.x + 14f;
            float tipY  = _mouseInWindow.y - size.y - 4f;

            // Не виходимо за межі вікна
            if (tipX + size.x > position.width - 4f) tipX = _mouseInWindow.x - size.x - 14f;
            if (tipY < viewport.y)                    tipY = _mouseInWindow.y + 14f;

            var bgRect = new Rect(tipX - 2f, tipY - 2f, size.x + 4f, size.y + 4f);
            EditorGUI.DrawRect(bgRect, new Color(0f, 0f, 0f, 0.78f));
            GUI.Label(new Rect(tipX, tipY, size.x, size.y), content, style);
        }
    }
}
