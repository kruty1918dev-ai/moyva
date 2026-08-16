using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Keeps the scene-authored gameplay HUD inside the current canvas/safe area and
    /// adapts its panels when the Game view, window aspect ratio, or device safe area changes.
    /// This component never creates HUD objects; it only lays out the authored hierarchy.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1000)]
    public sealed class GameplayResponsiveHudLayout : MonoBehaviour
    {
        private const float EdgeMargin = 20f;
        private const float PanelGap = 16f;
        private const float MinimumTurnWidth = 320f;
        private const float PreferredTurnWidth = 540f;

        private RectTransform _root;
        private Canvas _canvas;
        private RectTransform _turnPanel;
        private RectTransform _unitPanel;
        private RectTransform _recruitmentPanel;
        private RectTransform _resourcePanel;
        private RectTransform _buildButton;
        private Button _endTurnButton;
        private TMP_Text _turnText;
        private TMP_Text _statusText;
        private TMP_Text _unitText;
        private TMP_Text _recruitmentTitle;
        private TMP_Text _queueText;
        private RectTransform _viewport;
        private RectTransform _slots;
        private LayoutPlan _lastPlan;
        private bool _hasLastPlan;
        private bool _referencesReady;

        public readonly struct SafeInsets
        {
            public float Left { get; }
            public float Right { get; }
            public float Top { get; }
            public float Bottom { get; }

            public SafeInsets(float left, float right, float top, float bottom)
            {
                Left = Mathf.Max(0f, left);
                Right = Mathf.Max(0f, right);
                Top = Mathf.Max(0f, top);
                Bottom = Mathf.Max(0f, bottom);
            }
        }

        public readonly struct LayoutPlan
        {
            public Rect TurnRect { get; }
            public Rect UnitRect { get; }
            public Rect RecruitmentRect { get; }
            public bool RecruitmentBottomSheet { get; }
            public bool TurnMovedBelowTopReservation { get; }

            public LayoutPlan(
                Rect turnRect,
                Rect unitRect,
                Rect recruitmentRect,
                bool recruitmentBottomSheet,
                bool turnMovedBelowTopReservation)
            {
                TurnRect = turnRect;
                UnitRect = unitRect;
                RecruitmentRect = recruitmentRect;
                RecruitmentBottomSheet = recruitmentBottomSheet;
                TurnMovedBelowTopReservation = turnMovedBelowTopReservation;
            }
        }

        /// <summary>
        /// Pure layout calculation used by the runtime component and EditMode tests.
        /// All rectangles use canvas coordinates with (0,0) at the canvas bottom-left.
        /// </summary>
        public static LayoutPlan CalculateLayout(
            Vector2 canvasSize,
            SafeInsets safeInsets,
            Rect topCenterReservation)
        {
            float canvasWidth = Mathf.Max(1f, canvasSize.x);
            float canvasHeight = Mathf.Max(1f, canvasSize.y);

            float leftEdge = Mathf.Min(canvasWidth, safeInsets.Left + EdgeMargin);
            float rightEdge = Mathf.Max(leftEdge, canvasWidth - safeInsets.Right - EdgeMargin);
            float bottomEdge = Mathf.Min(canvasHeight, safeInsets.Bottom + EdgeMargin);
            float topEdge = Mathf.Max(bottomEdge, canvasHeight - safeInsets.Top - EdgeMargin);

            float usableWidth = Mathf.Max(1f, rightEdge - leftEdge);
            float usableHeight = Mathf.Max(1f, topEdge - bottomEdge);

            float desiredTurnWidth = Mathf.Clamp(usableWidth * 0.30f, 420f, PreferredTurnWidth);
            desiredTurnWidth = Mathf.Min(desiredTurnWidth, usableWidth);
            float turnHeight = Mathf.Min(Mathf.Clamp(usableHeight * 0.105f, 96f, 112f), usableHeight);
            float turnTop = topEdge;
            float turnWidth = desiredTurnWidth;
            bool movedBelowReservation = false;

            bool hasTopReservation = topCenterReservation.width > 1f &&
                                     topCenterReservation.height > 1f &&
                                     topCenterReservation.yMax > canvasHeight * 0.70f;
            if (hasTopReservation)
            {
                float availableLeft = topCenterReservation.xMin - PanelGap - leftEdge;
                if (availableLeft >= MinimumTurnWidth)
                {
                    turnWidth = Mathf.Min(turnWidth, availableLeft);
                }
                else if (topCenterReservation.yMin - PanelGap - turnHeight >= bottomEdge)
                {
                    turnTop = Mathf.Min(turnTop, topCenterReservation.yMin - PanelGap);
                    movedBelowReservation = true;
                }
                else
                {
                    turnWidth = Mathf.Min(turnWidth, Mathf.Max(MinimumTurnWidth, usableWidth * 0.48f));
                }
            }

            turnWidth = Mathf.Clamp(turnWidth, Mathf.Min(MinimumTurnWidth, usableWidth), usableWidth);
            float turnBottom = Mathf.Clamp(turnTop - turnHeight, bottomEdge, Mathf.Max(bottomEdge, topEdge - turnHeight));
            Rect turnRect = new(leftEdge, turnBottom, turnWidth, turnHeight);

            float unitWidth = Mathf.Min(Mathf.Clamp(usableWidth * 0.23f, 280f, 380f), usableWidth);
            float unitHeight = Mathf.Min(46f, usableHeight);
            Rect unitRect = new(leftEdge, bottomEdge, unitWidth, unitHeight);

            float aspect = usableWidth / Mathf.Max(1f, usableHeight);
            bool bottomSheet = aspect < 1.05f;
            float recruitmentWidth;
            float recruitmentHeight;
            float recruitmentX;
            float recruitmentY;

            if (bottomSheet)
            {
                recruitmentWidth = usableWidth;
                recruitmentHeight = Mathf.Min(Mathf.Clamp(usableHeight * 0.52f, 340f, 520f), usableHeight * 0.68f);
                recruitmentX = leftEdge;
                recruitmentY = bottomEdge;
            }
            else
            {
                float widthFraction = aspect < 1.45f ? 0.28f : 0.205f;
                recruitmentWidth = Mathf.Min(
                    Mathf.Clamp(usableWidth * widthFraction, 300f, 372f),
                    Mathf.Max(1f, usableWidth * 0.46f));
                recruitmentHeight = Mathf.Min(
                    Mathf.Clamp(usableHeight * 0.55f, 390f, 500f),
                    usableHeight);
                recruitmentX = rightEdge - recruitmentWidth;
                recruitmentY = bottomEdge + (usableHeight - recruitmentHeight) * 0.5f;
            }

            Rect recruitmentRect = new(
                recruitmentX,
                recruitmentY,
                recruitmentWidth,
                recruitmentHeight);

            return new LayoutPlan(
                turnRect,
                unitRect,
                recruitmentRect,
                bottomSheet,
                movedBelowReservation);
        }

        private void OnEnable()
        {
            _referencesReady = CacheReferences();
            _hasLastPlan = false;
            ApplyLayoutNow();
        }

        private void OnTransformChildrenChanged()
        {
            _referencesReady = CacheReferences();
            _hasLastPlan = false;
            ApplyLayoutNow();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
                return;
            _hasLastPlan = false;
            ApplyLayoutNow();
        }

        private void LateUpdate()
        {
            ApplyLayoutNow();
        }

        public bool ApplyLayoutNow()
        {
            if (!_referencesReady && !CacheReferences())
                return false;

            Vector2 canvasSize = _root.rect.size;
            if (canvasSize.x <= 1f || canvasSize.y <= 1f)
                return false;

            SafeInsets safeInsets = CalculateSafeInsets(canvasSize);
            Rect reservation = CalculateResourceReservation(canvasSize);
            LayoutPlan plan = CalculateLayout(canvasSize, safeInsets, reservation);

            if (_hasLastPlan && PlansApproximatelyEqual(_lastPlan, plan))
            {
                ApplyBuildButtonLayout(canvasSize, safeInsets);
                return true;
            }

            ApplyPlan(plan);
            ApplyBuildButtonLayout(canvasSize, safeInsets);
            _lastPlan = plan;
            _hasLastPlan = true;
            return true;
        }

        private bool CacheReferences()
        {
            _root = transform as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
            if (_root == null || _canvas == null)
                return false;

            _turnPanel = FindRect("TurnPanel");
            _unitPanel = FindRect("UnitStaminaPanel");
            _recruitmentPanel = FindRect("RecruitmentPanel");
            if (_turnPanel == null || _unitPanel == null || _recruitmentPanel == null)
                return false;

            _turnText = FindComponent<TMP_Text>("TurnPanel/TurnSummary");
            _statusText = FindComponent<TMP_Text>("TurnPanel/TurnStatus");
            _endTurnButton = FindComponent<Button>("TurnPanel/EndTurnButton");
            _unitText = FindComponent<TMP_Text>("UnitStaminaPanel/UnitStamina");
            _recruitmentTitle = FindComponent<TMP_Text>("RecruitmentPanel/Title");
            _queueText = FindComponent<TMP_Text>("RecruitmentPanel/Queue");
            _viewport = FindRect("RecruitmentPanel/RecipeViewport");
            _slots = FindRect("RecruitmentPanel/RecipeViewport/RecipeSlots");
            _resourcePanel = FindResourcePanel();
            _buildButton = FindBuildButton();

            ConfigureTextSizing();
            return true;
        }

        private void ApplyPlan(LayoutPlan plan)
        {
            SetBottomLeftRect(_turnPanel, plan.TurnRect);
            SetBottomLeftRect(_unitPanel, plan.UnitRect);
            SetBottomLeftRect(_recruitmentPanel, plan.RecruitmentRect);

            ApplyTurnPanelInternals(plan.TurnRect.width);
            ApplyUnitPanelInternals();
            ApplyRecruitmentInternals(plan.RecruitmentRect.height, plan.RecruitmentBottomSheet);
        }

        private void ApplyTurnPanelInternals(float panelWidth)
        {
            if (_endTurnButton != null)
            {
                RectTransform endRect = _endTurnButton.transform as RectTransform;
                if (endRect != null)
                {
                    float buttonWidth = Mathf.Clamp(panelWidth * 0.30f, 118f, 150f);
                    endRect.anchorMin = new Vector2(1f, 1f);
                    endRect.anchorMax = new Vector2(1f, 1f);
                    endRect.pivot = new Vector2(1f, 1f);
                    endRect.anchoredPosition = new Vector2(-12f, -10f);
                    endRect.sizeDelta = new Vector2(buttonWidth, 50f);

                    TMP_Text label = _endTurnButton.GetComponentInChildren<TMP_Text>(true);
                    if (label != null)
                        SetStretch(label.rectTransform, 10f, 32f, 4f, 4f);
                }
            }

            float endButtonSpace = _endTurnButton != null && _endTurnButton.transform is RectTransform buttonRect
                ? buttonRect.sizeDelta.x + 28f
                : 174f;

            if (_turnText != null)
            {
                RectTransform rect = _turnText.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0f, 0.5f);
                rect.offsetMin = new Vector2(18f, 34f);
                rect.offsetMax = new Vector2(-endButtonSpace, -8f);
                _turnText.enableWordWrapping = false;
                _turnText.overflowMode = TextOverflowModes.Ellipsis;
            }

            if (_statusText != null)
            {
                RectTransform rect = _statusText.rectTransform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(18f, 9f);
                rect.sizeDelta = new Vector2(-36f, 23f);
            }
        }

        private void ApplyUnitPanelInternals()
        {
            if (_unitText != null)
                SetStretch(_unitText.rectTransform, 14f, 14f, 4f, 4f);
        }

        private void ApplyBuildButtonLayout(Vector2 canvasSize, SafeInsets safeInsets)
        {
            if (_buildButton == null)
                _buildButton = FindBuildButton();
            if (_buildButton == null)
                return;

            float width = Mathf.Clamp(canvasSize.x * 0.09f, 156f, 184f);
            float height = Mathf.Clamp(canvasSize.y * 0.05f, 48f, 58f);
            const float margin = 28f;

            _buildButton.anchorMin = new Vector2(1f, 0f);
            _buildButton.anchorMax = new Vector2(1f, 0f);
            _buildButton.pivot = new Vector2(1f, 0f);

            _buildButton.anchoredPosition = new Vector2(
                -(safeInsets.Right + margin),
                safeInsets.Bottom + margin);

            _buildButton.sizeDelta = new Vector2(width, height);

            TMP_Text label =
                _buildButton.GetComponentInChildren<TMP_Text>(true);

            ConfigureAutosize(label, 11f, 14f);
        }

        private void ApplyRecruitmentInternals(float panelHeight, bool bottomSheet)
        {
            if (_recruitmentTitle != null)
                SetTopStretch(_recruitmentTitle.rectTransform, 18f, 18f, 12f, bottomSheet ? 38f : 34f);

            float queueHeight = Mathf.Clamp(panelHeight * 0.22f, 88f, 108f);
            if (_queueText != null)
            {
                RectTransform queueRect = _queueText.rectTransform;
                queueRect.anchorMin = new Vector2(0f, 0f);
                queueRect.anchorMax = new Vector2(1f, 0f);
                queueRect.pivot = new Vector2(0.5f, 0f);
                queueRect.anchoredPosition = new Vector2(0f, 12f);
                queueRect.sizeDelta = new Vector2(-34f, queueHeight);
                _queueText.enableWordWrapping = true;
            }

            if (_viewport != null)
            {
                _viewport.anchorMin = Vector2.zero;
                _viewport.anchorMax = Vector2.one;
                _viewport.offsetMin = new Vector2(16f, queueHeight + 28f);
                _viewport.offsetMax = new Vector2(-16f, bottomSheet ? -62f : -58f);
            }

            if (_slots != null)
            {
                VerticalLayoutGroup layout = _slots.GetComponent<VerticalLayoutGroup>();
                if (layout != null)
                {
                    layout.padding = new RectOffset(7, 7, 7, 7);
                    layout.spacing = bottomSheet ? 6f : 7f;
                }

                LayoutElement[] elements = _slots.GetComponentsInChildren<LayoutElement>(true);
                float rowHeight = bottomSheet ? 42f : 39f;
                foreach (LayoutElement element in elements)
                {
                    if (element == null)
                        continue;
                    element.minHeight = rowHeight;
                    element.preferredHeight = rowHeight;
                }
            }
        }

        private void ConfigureTextSizing()
        {
            ConfigureAutosize(_turnText, 10.5f, 15.5f);
            ConfigureAutosize(_statusText, 10f, 12.5f);
            ConfigureAutosize(_unitText, 10.5f, 13.5f);
            ConfigureAutosize(_recruitmentTitle, 13f, 18f);
            ConfigureAutosize(_queueText, 10f, 12.5f);

            if (_endTurnButton != null)
            {
                TMP_Text label = _endTurnButton.GetComponentInChildren<TMP_Text>(true);
                ConfigureAutosize(label, 10f, 13.5f);
            }

            if (_slots != null)
            {
                foreach (TMP_Text label in _slots.GetComponentsInChildren<TMP_Text>(true))
                    ConfigureAutosize(label, 10f, 12.5f);
            }
        }

        private SafeInsets CalculateSafeInsets(Vector2 canvasSize)
        {
            if (Screen.width <= 0 || Screen.height <= 0)
                return new SafeInsets(0f, 0f, 0f, 0f);

            Rect safe = Screen.safeArea;
            float xScale = canvasSize.x / Screen.width;
            float yScale = canvasSize.y / Screen.height;
            return new SafeInsets(
                safe.xMin * xScale,
                (Screen.width - safe.xMax) * xScale,
                (Screen.height - safe.yMax) * yScale,
                safe.yMin * yScale);
        }

        private Rect CalculateResourceReservation(Vector2 canvasSize)
        {
            if (_resourcePanel == null)
                _resourcePanel = FindResourcePanel();
            if (_resourcePanel == null || !_resourcePanel.gameObject.activeInHierarchy)
                return Rect.zero;

            var corners = new Vector3[4];
            _resourcePanel.GetWorldCorners(corners);
            Vector3 bottomLeft = _root.InverseTransformPoint(corners[0]);
            Vector3 topRight = _root.InverseTransformPoint(corners[2]);
            Rect rootRect = _root.rect;

            float x = bottomLeft.x - rootRect.xMin;
            float y = bottomLeft.y - rootRect.yMin;
            float width = Mathf.Max(0f, topRight.x - bottomLeft.x);
            float height = Mathf.Max(0f, topRight.y - bottomLeft.y);
            Rect result = new(x, y, width, height);

            if (result.yMax < canvasSize.y * 0.70f)
                return Rect.zero;
            return result;
        }

        private RectTransform FindBuildButton()
        {
            if (_canvas == null)
                return null;

            Transform target =
                _canvas.transform.Find("GameModeUI/Build Button");

            if (target is RectTransform rect &&
                target.GetComponent<Button>() != null)
            {
                return rect;
            }

            return null;
        }

        private RectTransform FindResourcePanel()
        {
            if (_canvas == null)
                return null;

            TMP_Text[] texts = _canvas.GetComponentsInChildren<TMP_Text>(true)
                .Where(text => text != null && !IsUnder(text.transform, transform))
                .ToArray();

            TMP_Text resourceText = texts.FirstOrDefault(text => ContainsAny(
                text.text,
                "Materials", "Матеріали",
                "Food", "Їжа",
                "Money", "Гроші"));
            if (resourceText == null)
                return null;

            RectTransform fallback = null;
            Transform current = resourceText.transform;
            while (current != null && current != _canvas.transform)
            {
                if (current is RectTransform rect && current.GetComponent<Image>() != null)
                {
                    fallback = rect;
                    if (rect.rect.width >= 240f)
                        return rect;
                }
                current = current.parent;
            }
            return fallback;
        }

        private RectTransform FindRect(string path)
            => transform.Find(path) as RectTransform;

        private T FindComponent<T>(string path) where T : Component
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<T>() : null;
        }

        private static bool IsUnder(Transform target, Transform possibleAncestor)
        {
            Transform current = target;
            while (current != null)
            {
                if (current == possibleAncestor)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            foreach (string needle in needles)
            {
                if (value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static void ConfigureAutosize(TMP_Text text, float minimum, float maximum)
        {
            if (text == null)
                return;
            text.enableAutoSizing = true;
            text.fontSizeMin = minimum;
            text.fontSizeMax = maximum;
        }

        private static void SetBottomLeftRect(RectTransform rect, Rect target)
        {
            if (rect == null)
                return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = target.position;
            rect.sizeDelta = target.size;
        }

        private static void SetTopStretch(RectTransform rect, float left, float right, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-(left + right), height);
        }

        private static void SetStretch(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static bool PlansApproximatelyEqual(LayoutPlan a, LayoutPlan b)
            => RectApproximatelyEqual(a.TurnRect, b.TurnRect) &&
               RectApproximatelyEqual(a.UnitRect, b.UnitRect) &&
               RectApproximatelyEqual(a.RecruitmentRect, b.RecruitmentRect) &&
               a.RecruitmentBottomSheet == b.RecruitmentBottomSheet &&
               a.TurnMovedBelowTopReservation == b.TurnMovedBelowTopReservation;

        private static bool RectApproximatelyEqual(Rect a, Rect b)
            => Mathf.Abs(a.x - b.x) < 0.25f &&
               Mathf.Abs(a.y - b.y) < 0.25f &&
               Mathf.Abs(a.width - b.width) < 0.25f &&
               Mathf.Abs(a.height - b.height) < 0.25f;
    }
}
