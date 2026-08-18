using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Responsive layout owner for the complete gameplay Canvas.
    /// It coordinates the existing scene-authored HUD, economy bar,
    /// game-mode button and construction UI without recreating runtime UI objects.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1200)]
    public sealed class GameplayAdaptiveCanvasLayout : MonoBehaviour
    {
        private static readonly Vector2 ReferenceResolution = new(1920f, 1080f);

        private Canvas _canvas;
        private CanvasScaler _scaler;
        private RectTransform _canvasRect;

        private RectTransform _hudRoot;
        private RectTransform _turnPanel;
        private RectTransform _unitPanel;
        private Button _endTurnButton;
        private TMP_Text _turnSummary;
        private TMP_Text _turnStatus;
        private TMP_Text _unitText;

        private RectTransform _resourceBar;
        private RectTransform _buildButton;

        private RectTransform _constructionRoot;
        private RectTransform _constructionStatus;
        private RectTransform _constructionSelection;
        private RectTransform _constructionActionBar;
        private RectTransform _constructionClose;
        private RectTransform _constructionPreview;

        private Behaviour _oldHudLayout;

        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;
        private Rect _lastSafeArea;
        private Vector2 _lastCanvasSize;
        private bool _referencesReady;

        public bool ApplyLayoutNow()
        {
            if (!_referencesReady && !CacheReferences())
                return false;

            ConfigureScaler();
            Canvas.ForceUpdateCanvases();

            Vector2 canvasSize = _canvasRect.rect.size;
            if (canvasSize.x <= 1f || canvasSize.y <= 1f)
                return false;

            Insets safeInsets = CalculateSafeInsets(canvasSize);
            Rect safeRect = new(
                safeInsets.Left,
                safeInsets.Bottom,
                Mathf.Max(1f, canvasSize.x - safeInsets.Left - safeInsets.Right),
                Mathf.Max(1f, canvasSize.y - safeInsets.Top - safeInsets.Bottom));

            ApplyEconomyBar(safeRect);
            ApplyBuildButton(safeRect);
            ApplyTurnHud(safeRect);
            ApplyConstructionUi(safeRect);

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastSafeArea = Screen.safeArea;
            _lastCanvasSize = canvasSize;
            return true;
        }

        public string DescribeCurrentLayout()
        {
            if (!_referencesReady && !CacheReferences())
                return "P18_2_DESCRIBE_FAILED references";

            Vector2 canvasSize = _canvasRect != null ? _canvasRect.rect.size : Vector2.zero;
            string turn = RectSummary(_turnPanel);
            string resource = RectSummary(_resourceBar);
            string build = RectSummary(_buildButton);
            string construction = RectSummary(_constructionPreview);
            return $"P18_2_LAYOUT canvas={canvasSize} screen={Screen.width}x{Screen.height} safe={Screen.safeArea} " +
                   $"turn={turn} resource={resource} build={build} constructionPreview={construction}";
        }

        private void OnEnable()
        {
            _referencesReady = CacheReferences();
            InvalidateSignature();
            ApplyLayoutNow();
        }

        private void OnValidate()
        {
            _referencesReady = false;
            InvalidateSignature();
        }

        private void OnTransformChildrenChanged()
        {
            _referencesReady = false;
            InvalidateSignature();
        }

        private void OnRectTransformDimensionsChange()
        {
            InvalidateSignature();
        }

        private void LateUpdate()
        {
            if (!isActiveAndEnabled)
                return;

            if (LayoutSignatureChanged())
                ApplyLayoutNow();
        }

        private bool CacheReferences()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null)
                return false;

            _canvasRect = _canvas.transform as RectTransform;
            if (_canvasRect == null)
                return false;

            _scaler = _canvas.GetComponent<CanvasScaler>();

            _hudRoot = FindRect("GameplayTurnHud");
            _turnPanel = FindRect("GameplayTurnHud/TurnPanel");
            _unitPanel = FindRect("GameplayTurnHud/UnitStaminaPanel");
            _endTurnButton = FindComponent<Button>("GameplayTurnHud/TurnPanel/EndTurnButton");
            _turnSummary = FindComponent<TMP_Text>("GameplayTurnHud/TurnPanel/TurnSummary");
            _turnStatus = FindComponent<TMP_Text>("GameplayTurnHud/TurnPanel/TurnStatus");
            _unitText = FindComponent<TMP_Text>("GameplayTurnHud/UnitStaminaPanel/UnitStamina");

            _resourceBar = FindRect("EconomyPlayerSymmary/Root/Top");
            _buildButton = FindRect("GameModeUI/Build Button");

            _constructionRoot = FindRect("ConstructionUI/Root");
            _constructionStatus = FindRect("ConstructionUI/Root/StatusPanel");
            _constructionSelection = FindRect("ConstructionUI/Root/BuildingSelectionPanel");
            _constructionActionBar = FindRect("ConstructionUI/Root/ActionBar");
            _constructionClose = FindRect("ConstructionUI/Root/CloseButton");
            _constructionPreview = FindRect("ConstructionUI/Root/PreviewPanelInfo");

            if (_hudRoot != null)
            {
                foreach (MonoBehaviour behaviour in _hudRoot.GetComponents<MonoBehaviour>())
                {
                    if (behaviour == null ||
                        behaviour.GetType().FullName != "Kruty1918.Moyva.Bootstrap.Runtime.GameplayResponsiveHudLayout")
                    {
                        continue;
                    }

                    _oldHudLayout = behaviour;
                    if (_oldHudLayout.enabled)
                        _oldHudLayout.enabled = false;
                    break;
                }
            }

            StretchRoot(_hudRoot);
            StretchRoot(FindRect("GameModeUI"));
            StretchRoot(FindRect("EconomyPlayerSymmary"));
            StretchRoot(FindRect("EconomyPlayerSymmary/Root"));
            StretchRoot(FindRect("ConstructionUI"));
            StretchRoot(_constructionRoot);

            ConfigureTextAutosizing();

            return _turnPanel != null && _resourceBar != null && _buildButton != null;
        }

        private void ConfigureScaler()
        {
            if (_scaler == null)
                _scaler = _canvas.GetComponent<CanvasScaler>();
            if (_scaler == null)
                return;

            float aspect = Screen.height > 0
                ? Screen.width / (float)Screen.height
                : _canvasRect.rect.width / Mathf.Max(1f, _canvasRect.rect.height);

            float match;
            if (aspect >= 1.60f)
                match = 0f;
            else if (aspect >= 1.30f)
                match = 0.25f;
            else
                match = 0.5f;

            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = ReferenceResolution;
            _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _scaler.matchWidthOrHeight = match;
            _scaler.referencePixelsPerUnit = 100f;
        }

        private void ApplyEconomyBar(Rect safe)
        {
            if (_resourceBar == null)
                return;

            float width = Mathf.Clamp(safe.width * 0.29f, 520f, 620f);
            float height = Mathf.Clamp(safe.height * 0.055f, 50f, 58f);
            SetTopCenter(_resourceBar, safe.center.x, safe.yMax - 18f, width, height);

            HorizontalLayoutGroup group = _resourceBar.GetComponent<HorizontalLayoutGroup>();
            if (group != null)
            {
                group.padding = new RectOffset(14, 14, 6, 6);
                group.spacing = 8f;
            }

            foreach (TMP_Text text in _resourceBar.GetComponentsInChildren<TMP_Text>(true))
                ConfigureAutosize(text, 12f, 16f);
        }

        private void ApplyTurnHud(Rect safe)
        {
            if (_turnPanel == null)
                return;

            const float edge = 24f;
            const float gap = 18f;
            float top = safe.yMax - edge;
            float left = safe.xMin + edge;
            float width = Mathf.Clamp(safe.width * 0.30f, 520f, 600f);
            float height = Mathf.Clamp(safe.height * 0.122f, 112f, 128f);

            if (_resourceBar != null)
            {
                Rect resource = CanvasRect(_resourceBar);
                float maxWidthBeforeResource = resource.xMin - gap - left;
                if (maxWidthBeforeResource >= 430f)
                    width = Mathf.Min(width, maxWidthBeforeResource);
                else if (resource.yMin - gap - height > safe.yMin)
                    top = resource.yMin - gap;
            }

            SetTopLeft(_turnPanel, left, top, width, height);

            if (_endTurnButton != null && _endTurnButton.transform is RectTransform endRect)
            {
                float buttonWidth = Mathf.Clamp(width * 0.31f, 150f, 176f);
                float buttonHeight = Mathf.Clamp(height * 0.47f, 50f, 58f);
                endRect.anchorMin = new Vector2(1f, 1f);
                endRect.anchorMax = new Vector2(1f, 1f);
                endRect.pivot = new Vector2(1f, 1f);
                endRect.anchoredPosition = new Vector2(-12f, -10f);
                endRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

                TMP_Text label = _endTurnButton.GetComponentInChildren<TMP_Text>(true);
                ConfigureAutosize(label, 11f, 15f);
            }

            float reservedForButton = _endTurnButton != null && _endTurnButton.transform is RectTransform button
                ? button.sizeDelta.x + 32f
                : 190f;

            if (_turnSummary != null)
            {
                RectTransform rect = _turnSummary.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0f, 0.5f);
                rect.offsetMin = new Vector2(18f, 38f);
                rect.offsetMax = new Vector2(-reservedForButton, -10f);
                _turnSummary.overflowMode = TextOverflowModes.Ellipsis;
            }

            if (_turnStatus != null)
            {
                RectTransform rect = _turnStatus.rectTransform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(18f, 10f);
                rect.sizeDelta = new Vector2(-36f, 25f);
            }

            if (_unitPanel != null)
            {
                float unitWidth = Mathf.Clamp(safe.width * 0.22f, 330f, 420f);
                float unitHeight = 50f;
                SetBottomLeft(_unitPanel, safe.xMin + edge, safe.yMin + edge, unitWidth, unitHeight);
            }

        }

        private void ApplyBuildButton(Rect safe)
        {
            if (_buildButton == null)
                return;

            float width = Mathf.Clamp(safe.width * 0.108f, 190f, 220f);
            float height = Mathf.Clamp(safe.height * 0.062f, 54f, 64f);
            const float edge = 28f;

            _buildButton.anchorMin = new Vector2(1f, 0f);
            _buildButton.anchorMax = new Vector2(1f, 0f);
            _buildButton.pivot = new Vector2(1f, 0f);
            _buildButton.anchoredPosition = new Vector2(
                -(Mathf.Max(0f, _canvasRect.rect.width - safe.xMax) + edge),
                safe.yMin + edge);
            _buildButton.sizeDelta = new Vector2(width, height);

            TMP_Text label = _buildButton.GetComponentInChildren<TMP_Text>(true);
            ConfigureAutosize(label, 11f, 15f);
        }

        private void ApplyConstructionUi(Rect safe)
        {
            if (_constructionRoot == null)
                return;

            const float edge = 24f;
            const float gap = 12f;

            if (_constructionSelection != null)
            {
                float selectionHeight = Mathf.Clamp(safe.height * 0.205f, 178f, 220f);
                SetBottomStretch(
                    _constructionSelection,
                    safe.xMin + edge,
                    Mathf.Max(0f, _canvasRect.rect.width - safe.xMax) + edge,
                    safe.yMin + edge,
                    selectionHeight);
            }

            float selectionTop = _constructionSelection != null
                ? CanvasRect(_constructionSelection).yMax
                : safe.yMin + 200f;

            if (_constructionActionBar != null)
            {
                float width = Mathf.Clamp(safe.width * 0.40f, 700f, 820f);
                float height = Mathf.Clamp(safe.height * 0.06f, 54f, 64f);
                SetBottomCenter(
                    _constructionActionBar,
                    safe.center.x,
                    selectionTop + gap,
                    width,
                    height);

                HorizontalLayoutGroup group = _constructionActionBar.GetComponent<HorizontalLayoutGroup>();
                if (group != null)
                {
                    group.padding = new RectOffset(8, 8, 7, 7);
                    group.spacing = 8f;
                    group.childControlWidth = true;
                    group.childControlHeight = true;
                    group.childForceExpandWidth = true;
                    group.childForceExpandHeight = true;
                }
            }

            if (_constructionClose != null)
            {
                float y = selectionTop + gap;
                SetBottomRight(
                    _constructionClose,
                    safe.xMax - edge,
                    y,
                    46f,
                    42f);
            }

            if (_constructionPreview != null)
            {
                float width = Mathf.Clamp(safe.width * 0.19f, 320f, 380f);
                float height = Mathf.Clamp(safe.height * 0.27f, 220f, 280f);
                float minY = selectionTop + 86f;
                float y = Mathf.Max(minY, safe.center.y - height * 0.5f);
                y = Mathf.Min(y, safe.yMax - edge - height);
                SetBottomLeft(
                    _constructionPreview,
                    safe.xMax - edge - width,
                    y,
                    width,
                    height);

                foreach (TMP_Text text in _constructionPreview.GetComponentsInChildren<TMP_Text>(true))
                    ConfigureAutosize(text, 11f, 16f);
            }

            if (_constructionStatus != null)
            {
                float targetWidth = Mathf.Clamp(safe.width * 0.48f, 700f, 940f);
                float height = Mathf.Clamp(safe.height * 0.055f, 48f, 58f);
                float top = safe.yMax - 82f;
                if (_resourceBar != null)
                {
                    Rect resource = CanvasRect(_resourceBar);
                    top = Mathf.Min(top, resource.yMin - 14f);
                }

                float leftBoundary = safe.xMin + edge;
                if (_turnPanel != null && _turnPanel.gameObject.activeInHierarchy)
                    leftBoundary = Mathf.Max(leftBoundary, CanvasRect(_turnPanel).xMax + 18f);
                float rightBoundary = safe.xMax - edge;
                float available = Mathf.Max(1f, rightBoundary - leftBoundary);

                if (available >= 620f)
                {
                    float width = Mathf.Min(targetWidth, available);
                    float x = leftBoundary + (available - width) * 0.5f;
                    SetTopLeft(_constructionStatus, x, top, width, height);
                }
                else
                {
                    float width = Mathf.Max(1f, safe.width - edge * 2f);
                    if (_turnPanel != null && _turnPanel.gameObject.activeInHierarchy)
                        top = Mathf.Min(top, CanvasRect(_turnPanel).yMin - 14f);
                    SetTopLeft(_constructionStatus, safe.xMin + edge, top, width, height);
                }

                HorizontalLayoutGroup group = _constructionStatus.GetComponent<HorizontalLayoutGroup>();
                if (group != null)
                {
                    group.padding = new RectOffset(12, 12, 6, 6);
                    group.spacing = 10f;
                }

                foreach (TMP_Text text in _constructionStatus.GetComponentsInChildren<TMP_Text>(true))
                    ConfigureAutosize(text, 11f, 15f);
            }
        }

        private void ConfigureTextAutosizing()
        {
            ConfigureAutosize(_turnSummary, 11f, 16.5f);
            ConfigureAutosize(_turnStatus, 10.5f, 13.5f);
            ConfigureAutosize(_unitText, 11f, 14.5f);
        }

        private Insets CalculateSafeInsets(Vector2 canvasSize)
        {
            if (Screen.width <= 0 || Screen.height <= 0)
                return default;

            Rect safe = Screen.safeArea;
            float xScale = canvasSize.x / Screen.width;
            float yScale = canvasSize.y / Screen.height;
            return new Insets(
                safe.xMin * xScale,
                (Screen.width - safe.xMax) * xScale,
                (Screen.height - safe.yMax) * yScale,
                safe.yMin * yScale);
        }

        private bool LayoutSignatureChanged()
        {
            if (_canvasRect == null)
                return true;

            if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height)
                return true;
            if (_lastSafeArea != Screen.safeArea)
                return true;
            Vector2 current = _canvasRect.rect.size;
            return Vector2.SqrMagnitude(current - _lastCanvasSize) > 0.25f;
        }

        private void InvalidateSignature()
        {
            _lastScreenWidth = -1;
            _lastScreenHeight = -1;
            _lastSafeArea = new Rect(float.NaN, float.NaN, float.NaN, float.NaN);
            _lastCanvasSize = new Vector2(float.NaN, float.NaN);
        }

        private RectTransform FindRect(string path)
            => _canvas != null ? _canvas.transform.Find(path) as RectTransform : null;

        private T FindComponent<T>(string path) where T : Component
        {
            RectTransform rect = FindRect(path);
            return rect != null ? rect.GetComponent<T>() : null;
        }

        private Rect CanvasRect(RectTransform rect)
        {
            if (rect == null || _canvasRect == null)
                return Rect.zero;

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3 bl = _canvasRect.InverseTransformPoint(corners[0]);
            Vector3 tr = _canvasRect.InverseTransformPoint(corners[2]);
            Rect canvasBounds = _canvasRect.rect;
            return new Rect(
                bl.x - canvasBounds.xMin,
                bl.y - canvasBounds.yMin,
                Mathf.Max(0f, tr.x - bl.x),
                Mathf.Max(0f, tr.y - bl.y));
        }

        private static string RectSummary(RectTransform rect)
        {
            if (rect == null)
                return "missing";
            return $"anchor={rect.anchorMin}->{rect.anchorMax},pos={rect.anchoredPosition},size={rect.sizeDelta}";
        }

        private static void StretchRoot(RectTransform rect)
        {
            if (rect == null)
                return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static void SetTopLeft(RectTransform rect, float left, float top, float width, float height)
        {
            if (rect == null)
                return;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -(Mathf.Max(0f, rect.parent is RectTransform parent ? parent.rect.height - top : 0f)));
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetBottomLeft(RectTransform rect, float left, float bottom, float width, float height)
        {
            if (rect == null)
                return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(left, bottom);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetBottomRight(RectTransform rect, float right, float bottom, float width, float height)
        {
            if (rect == null)
                return;
            RectTransform parent = rect.parent as RectTransform;
            float parentWidth = parent != null ? parent.rect.width : 0f;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-(Mathf.Max(0f, parentWidth - right)), bottom);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetTopCenter(RectTransform rect, float centerX, float top, float width, float height)
        {
            if (rect == null)
                return;
            RectTransform parent = rect.parent as RectTransform;
            float parentWidth = parent != null ? parent.rect.width : 0f;
            float parentHeight = parent != null ? parent.rect.height : 0f;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(centerX - parentWidth * 0.5f, -(Mathf.Max(0f, parentHeight - top)));
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetBottomCenter(RectTransform rect, float centerX, float bottom, float width, float height)
        {
            if (rect == null)
                return;
            RectTransform parent = rect.parent as RectTransform;
            float parentWidth = parent != null ? parent.rect.width : 0f;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(centerX - parentWidth * 0.5f, bottom);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetBottomStretch(RectTransform rect, float left, float right, float bottom, float height)
        {
            if (rect == null)
                return;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2((left - right) * 0.5f, bottom);
            rect.sizeDelta = new Vector2(-(left + right), height);
        }

        private static void ConfigureAutosize(TMP_Text text, float min, float max)
        {
            if (text == null)
                return;
            text.enableAutoSizing = true;
            text.fontSizeMin = min;
            text.fontSizeMax = max;
        }

        private readonly struct Insets
        {
            public readonly float Left;
            public readonly float Right;
            public readonly float Top;
            public readonly float Bottom;

            public Insets(float left, float right, float top, float bottom)
            {
                Left = Mathf.Max(0f, left);
                Right = Mathf.Max(0f, right);
                Top = Mathf.Max(0f, top);
                Bottom = Mathf.Max(0f, bottom);
            }
        }
    }
}
