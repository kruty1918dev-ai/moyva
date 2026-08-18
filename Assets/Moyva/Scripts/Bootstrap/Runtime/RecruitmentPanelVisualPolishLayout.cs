using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// P24UI.2 — compact recruitment panel layout.
    ///
    /// UI-only. No recruitment/economy/unit service access.
    /// The layout deliberately replaces the oversized P24UI/P24UI.1 geometry.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1600)]
    public sealed class RecruitmentPanelVisualPolishLayout : MonoBehaviour
    {
        private const float PanelWidth = 548f;
        private const float PanelHeight = 476f;
        private const float Edge = 12f;
        private const float Gap = 8f;
        private const float TitleHeight = 28f;
        private const float StateHeight = 48f;
        private const float ListWidth = 184f;
        private const float QueueEmptyHeight = 38f;
        private const float QueueRowHeight = 26f;
        private const float QueueMaxHeight = 116f;
        private const float WorldInfoGap = 12f;

        private RectTransform _panel;
        private RectTransform _hudRoot;
        private RectTransform _title;
        private RectTransform _statePanel;
        private RectTransform _availableCard;
        private RectTransform _availableHeader;
        private RectTransform _recipeViewport;
        private ScrollRect _recipeScroll;
        private RectTransform _selectionPanel;
        private RectTransform _detailsHeader;
        private RectTransform _queueCard;
        private RectTransform _queueHeader;
        private RectTransform _queueRows;

        private Vector2 _lastHudSize = new(-1f, -1f);
        private float _lastReserve = float.NaN;
        private int _lastQueueRows = -1;
        private bool _ready;
        private bool _resetScrollPending;

        private void OnEnable()
        {
            _ready = Cache();
            _resetScrollPending = true;
            ApplyNow();
        }

        private void OnTransformChildrenChanged()
        {
            _ready = Cache();
            _resetScrollPending = true;
            ApplyNow();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled)
                ApplyNow();
        }

        private void LateUpdate()
        {
            if (!_ready && !Cache())
                return;

            Vector2 hud = _hudRoot.rect.size;
            float reserve = CalculateRightReserve();
            int queueRows = CountActiveQueueRows();

            if (!Approximately(hud, _lastHudSize)
                || float.IsNaN(_lastReserve)
                || Mathf.Abs(reserve - _lastReserve) > 0.5f
                || queueRows != _lastQueueRows
                || !LayoutStillApplied())
            {
                ApplyNow();
            }

            if (_resetScrollPending && gameObject.activeInHierarchy)
            {
                _resetScrollPending = false;
                Canvas.ForceUpdateCanvases();
                ResetScrollToTop();
            }
        }

        public bool ApplyNow()
        {
            if (!_ready && !Cache())
                return false;

            Vector2 canvas = _hudRoot.rect.size;
            if (canvas.x <= 1f || canvas.y <= 1f)
                return false;

            float reserve = CalculateRightReserve();
            int queueRows = CountActiveQueueRows();
            float queueHeight = CalculateQueueHeight(queueRows);

            ApplyPanelRect(canvas, reserve);
            ApplySections(queueHeight);
            ApplyRecipeRows();
            ApplySelection();
            ApplyQueueRows();

            _lastHudSize = canvas;
            _lastReserve = reserve;
            _lastQueueRows = queueRows;
            return true;
        }

        private bool Cache()
        {
            _panel = transform as RectTransform;
            _hudRoot = transform.parent as RectTransform;
            if (_panel == null || _hudRoot == null)
                return false;

            _title = R("Title");
            _statePanel = R("BuildingStatePanel");
            _availableCard = R("AvailableUnitsCard");
            _availableHeader = R("AvailableUnitsCard/Header");
            _recipeViewport = R("RecipeViewport");
            _recipeScroll = _recipeViewport != null
                ? _recipeViewport.GetComponent<ScrollRect>()
                : null;
            _selectionPanel = R("SelectionPanel");
            _detailsHeader = R("SelectionPanel/DetailsHeader");
            _queueCard = R("QueueCard");
            _queueHeader = R("Queue");
            _queueRows = R("QueueRows");

            return _title != null
                && _statePanel != null
                && _availableCard != null
                && _availableHeader != null
                && _recipeViewport != null
                && _selectionPanel != null
                && _detailsHeader != null
                && _queueCard != null
                && _queueHeader != null
                && _queueRows != null;
        }

        private void ApplyPanelRect(Vector2 canvas, float reserve)
        {
            float width = Mathf.Min(
                PanelWidth,
                Mathf.Max(440f, canvas.x - reserve - 340f));

            float height = Mathf.Min(
                PanelHeight,
                Mathf.Max(420f, canvas.y - 54f));

            if (canvas.x < 1180f)
            {
                width = Mathf.Clamp(canvas.x - 32f, 440f, 520f);
                reserve = 12f;
            }

            _panel.anchorMin = new Vector2(1f, 0.5f);
            _panel.anchorMax = new Vector2(1f, 0.5f);
            _panel.pivot = new Vector2(1f, 0.5f);
            _panel.anchoredPosition = new Vector2(-reserve, 0f);
            _panel.sizeDelta = new Vector2(width, height);
        }

        private void ApplySections(float queueHeight)
        {
            float stateTop = 8f + TitleHeight + 6f;
            float bodyTop = stateTop + StateHeight + 8f;
            float queueBottom = 10f;
            float bodyBottom = queueBottom + queueHeight + 8f;

            SetTopStretch(_title, Edge, Edge, 8f, TitleHeight);
            SetTopStretch(_statePanel, Edge, Edge, stateTop, StateHeight);

            // Left compact recipe list.
            _availableCard.anchorMin = new Vector2(0f, 0f);
            _availableCard.anchorMax = new Vector2(0f, 1f);
            _availableCard.pivot = new Vector2(0f, 0.5f);
            _availableCard.offsetMin = new Vector2(Edge, bodyBottom);
            _availableCard.offsetMax = new Vector2(
                Edge + ListWidth,
                -bodyTop);

            SetTopStretch(
                _availableHeader,
                8f,
                8f,
                6f,
                18f);

            _recipeViewport.anchorMin = new Vector2(0f, 0f);
            _recipeViewport.anchorMax = new Vector2(0f, 1f);
            _recipeViewport.pivot = new Vector2(0f, 0.5f);
            _recipeViewport.offsetMin = new Vector2(
                Edge + 6f,
                bodyBottom + 6f);
            _recipeViewport.offsetMax = new Vector2(
                Edge + ListWidth - 6f,
                -(bodyTop + 28f));

            // Right detail area.
            _selectionPanel.anchorMin = new Vector2(0f, 0f);
            _selectionPanel.anchorMax = new Vector2(1f, 1f);
            _selectionPanel.offsetMin = new Vector2(
                Edge + ListWidth + Gap,
                bodyBottom);
            _selectionPanel.offsetMax = new Vector2(
                -Edge,
                -bodyTop);

            SetTopStretch(
                _detailsHeader,
                10f,
                10f,
                6f,
                18f);

            // Compact queue across full width.
            SetBottomStretch(
                _queueCard,
                Edge,
                Edge,
                queueBottom,
                queueHeight);

            SetBottomStretch(
                _queueHeader,
                Edge + 10f,
                Edge + 10f,
                queueBottom + queueHeight - 25f,
                18f);

            float rowsHeight = Mathf.Max(0f, queueHeight - 29f);
            SetBottomStretch(
                _queueRows,
                Edge + 8f,
                Edge + 8f,
                queueBottom + 4f,
                rowsHeight);
        }

        private void ApplyRecipeRows()
        {
            Transform content = transform.Find("RecipeViewport/RecipeSlots");
            if (content == null)
                return;

            if (content.TryGetComponent(out VerticalLayoutGroup layout))
            {
                layout.padding = new RectOffset(2, 2, 2, 2);
                layout.spacing = 5f;
                layout.childControlWidth = true;
                layout.childForceExpandWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandHeight = false;
            }

            for (int i = 0; i < content.childCount; i++)
            {
                Transform row = content.GetChild(i);
                if (row == null
                    || !row.name.StartsWith(
                        "RecipeSlot_",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                LayoutElement element = row.GetComponent<LayoutElement>();
                if (element != null)
                {
                    element.minHeight = 42f;
                    element.preferredHeight = 42f;
                }

                RectTransform icon = row.Find("UnitIcon") as RectTransform;
                if (icon != null)
                {
                    icon.anchorMin = icon.anchorMax =
                        new Vector2(0f, 0.5f);
                    icon.pivot = new Vector2(0f, 0.5f);
                    icon.anchoredPosition = new Vector2(7f, 0f);
                    icon.sizeDelta = new Vector2(26f, 26f);
                }

                TMP_Text label =
                    row.Find("Label")?.GetComponent<TMP_Text>();
                if (label != null)
                {
                    label.rectTransform.anchorMin = Vector2.zero;
                    label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.offsetMin =
                        new Vector2(40f, 4f);
                    label.rectTransform.offsetMax =
                        new Vector2(-6f, -4f);
                    label.alignment =
                        TextAlignmentOptions.MidlineLeft;
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 9.5f;
                    label.fontSizeMax = 11.5f;
                    label.textWrappingMode =
                        TextWrappingModes.Normal;
                    label.overflowMode =
                        TextOverflowModes.Ellipsis;
                }
            }
        }

        private void ApplySelection()
        {
            RectTransform icon =
                _selectionPanel.Find("UnitIcon") as RectTransform;
            TMP_Text name =
                _selectionPanel.Find("UnitName")
                    ?.GetComponent<TMP_Text>();
            TMP_Text stats =
                _selectionPanel.Find("UnitStats")
                    ?.GetComponent<TMP_Text>();
            RectTransform costs =
                _selectionPanel.Find("Costs") as RectTransform;
            RectTransform hire =
                _selectionPanel.Find("HireButton") as RectTransform;

            if (icon != null)
            {
                icon.anchorMin = icon.anchorMax =
                    new Vector2(0f, 1f);
                icon.pivot = new Vector2(0f, 1f);
                icon.anchoredPosition =
                    new Vector2(10f, -32f);
                icon.sizeDelta = new Vector2(48f, 48f);
            }

            if (name != null)
            {
                name.rectTransform.anchorMin =
                    name.rectTransform.anchorMax =
                        new Vector2(0f, 1f);
                name.rectTransform.pivot =
                    new Vector2(0f, 1f);
                name.rectTransform.anchoredPosition =
                    new Vector2(68f, -31f);
                name.rectTransform.sizeDelta =
                    new Vector2(
                        Mathf.Max(
                            120f,
                            _selectionPanel.rect.width - 82f),
                        22f);

                name.fontStyle = FontStyles.Bold;
                name.enableAutoSizing = true;
                name.fontSizeMin = 12f;
                name.fontSizeMax = 15f;
                name.alignment =
                    TextAlignmentOptions.MidlineLeft;
                name.textWrappingMode =
                    TextWrappingModes.NoWrap;
                name.overflowMode =
                    TextOverflowModes.Ellipsis;
            }

            if (stats != null)
            {
                stats.rectTransform.anchorMin =
                    stats.rectTransform.anchorMax =
                        new Vector2(0f, 1f);
                stats.rectTransform.pivot =
                    new Vector2(0f, 1f);
                stats.rectTransform.anchoredPosition =
                    new Vector2(68f, -56f);
                stats.rectTransform.sizeDelta =
                    new Vector2(
                        Mathf.Max(
                            120f,
                            _selectionPanel.rect.width - 82f),
                        58f);

                stats.enableAutoSizing = true;
                stats.fontSizeMin = 9.5f;
                stats.fontSizeMax = 11f;
                stats.alignment =
                    TextAlignmentOptions.TopLeft;
                stats.textWrappingMode =
                    TextWrappingModes.Normal;
                stats.overflowMode =
                    TextOverflowModes.Ellipsis;
            }

            if (costs != null)
            {
                costs.anchorMin = new Vector2(0f, 0f);
                costs.anchorMax = new Vector2(1f, 0f);
                costs.pivot = new Vector2(0f, 0f);
                costs.anchoredPosition =
                    new Vector2(10f, 8f);
                costs.sizeDelta =
                    new Vector2(-128f, 28f);
            }

            if (hire != null)
            {
                hire.anchorMin = hire.anchorMax =
                    new Vector2(1f, 0f);
                hire.pivot = new Vector2(1f, 0f);
                hire.anchoredPosition =
                    new Vector2(-10f, 8f);
                hire.sizeDelta =
                    new Vector2(110f, 28f);

                TMP_Text label =
                    hire.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 9.5f;
                    label.fontSizeMax = 11f;
                    label.textWrappingMode =
                        TextWrappingModes.NoWrap;
                    label.alignment =
                        TextAlignmentOptions.Center;
                }
            }
        }

        private void ApplyQueueRows()
        {
            if (_queueRows.TryGetComponent(
                    out VerticalLayoutGroup layout))
            {
                layout.padding = new RectOffset(0, 0, 0, 0);
                layout.spacing = 3f;
                layout.childControlWidth = true;
                layout.childForceExpandWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandHeight = false;
            }

            for (int i = 0; i < _queueRows.childCount; i++)
            {
                Transform row = _queueRows.GetChild(i);
                if (row == null
                    || !row.name.StartsWith(
                        "QueueRow_",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                LayoutElement element = row.GetComponent<LayoutElement>();
                if (element != null)
                {
                    element.minHeight = QueueRowHeight;
                    element.preferredHeight = QueueRowHeight;
                }

                RectTransform icon =
                    row.Find("UnitIcon") as RectTransform;
                if (icon != null)
                {
                    icon.anchorMin = icon.anchorMax =
                        new Vector2(0f, 0.5f);
                    icon.pivot = new Vector2(0f, 0.5f);
                    icon.anchoredPosition = new Vector2(5f, 0f);
                    icon.sizeDelta = new Vector2(18f, 18f);
                }

                TMP_Text label =
                    row.Find("Label")?.GetComponent<TMP_Text>();
                if (label != null)
                {
                    label.rectTransform.anchorMin = Vector2.zero;
                    label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.offsetMin =
                        new Vector2(29f, 3f);
                    label.rectTransform.offsetMax =
                        new Vector2(-6f, -3f);
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 8.5f;
                    label.fontSizeMax = 10f;
                    label.textWrappingMode =
                        TextWrappingModes.NoWrap;
                    label.alignment =
                        TextAlignmentOptions.MidlineLeft;
                    label.overflowMode =
                        TextOverflowModes.Ellipsis;
                }

                RectTransform track =
                    row.Find("ProgressTrack") as RectTransform;
                if (track != null)
                {
                    track.anchorMin = new Vector2(0f, 0f);
                    track.anchorMax = new Vector2(1f, 0f);
                    track.offsetMin = new Vector2(4f, 1f);
                    track.offsetMax = new Vector2(-4f, 3f);
                }
            }
        }

        private int CountActiveQueueRows()
        {
            int count = 0;
            if (_queueRows == null)
                return count;

            for (int i = 0; i < _queueRows.childCount; i++)
            {
                GameObject go = _queueRows.GetChild(i).gameObject;
                if (go.activeSelf)
                    count++;
            }

            return count;
        }

        private static float CalculateQueueHeight(int rows)
        {
            if (rows <= 0)
                return QueueEmptyHeight;

            return Mathf.Min(
                QueueMaxHeight,
                28f + rows * (QueueRowHeight + 3f));
        }

        private float CalculateRightReserve()
        {
            if (_hudRoot == null)
                return 18f;

            RectTransform rail = FindWorldInfoRail();
            if (rail == null)
                return 18f;

            Vector3[] corners = new Vector3[4];
            rail.GetWorldCorners(corners);
            float left = float.PositiveInfinity;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 local =
                    _hudRoot.InverseTransformPoint(corners[i]);
                left = Mathf.Min(left, local.x);
            }

            float reserve =
                _hudRoot.rect.xMax - left + WorldInfoGap;

            return Mathf.Clamp(
                reserve,
                18f,
                Mathf.Max(18f, _hudRoot.rect.width * 0.48f));
        }

        private RectTransform FindWorldInfoRail()
        {
            RectTransform[] all =
                Resources.FindObjectsOfTypeAll<RectTransform>();

            RectTransform best = null;
            float bestLeft = float.NegativeInfinity;

            for (int i = 0; i < all.Length; i++)
            {
                RectTransform rect = all[i];
                if (rect == null
                    || rect == _panel
                    || rect.gameObject == null
                    || !rect.gameObject.scene.IsValid()
                    || !rect.gameObject.activeInHierarchy)
                {
                    continue;
                }

                string n = rect.name ?? string.Empty;
                bool candidate =
                    n.IndexOf(
                        "WorldInfoPanel",
                        StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf(
                        "BuildingInfoPanel",
                        StringComparison.OrdinalIgnoreCase) >= 0;

                if (!candidate)
                    continue;

                Vector3[] c = new Vector3[4];
                rect.GetWorldCorners(c);
                float left = float.PositiveInfinity;

                for (int k = 0; k < c.Length; k++)
                {
                    Vector3 local =
                        _hudRoot.InverseTransformPoint(c[k]);
                    left = Mathf.Min(left, local.x);
                }

                if (left > bestLeft)
                {
                    bestLeft = left;
                    best = rect;
                }
            }

            return best;
        }

        private void ResetScrollToTop()
        {
            if (_recipeScroll == null)
                return;

            _recipeScroll.StopMovement();
            _recipeScroll.velocity = Vector2.zero;
            _recipeScroll.verticalNormalizedPosition = 1f;

            if (_recipeScroll.content != null)
            {
                Vector2 p =
                    _recipeScroll.content.anchoredPosition;
                p.y = 0f;
                _recipeScroll.content.anchoredPosition = p;
            }

            Canvas.ForceUpdateCanvases();
            _recipeScroll.verticalNormalizedPosition = 1f;
        }

        private bool LayoutStillApplied()
        {
            return _panel != null
                && _panel.anchorMin == new Vector2(1f, 0.5f)
                && _panel.anchorMax == new Vector2(1f, 0.5f)
                && _panel.pivot == new Vector2(1f, 0.5f)
                && _panel.rect.width <= 570f
                && _panel.rect.height <= 500f;
        }

        private RectTransform R(string path)
            => transform.Find(path) as RectTransform;

        private static bool Approximately(Vector2 a, Vector2 b)
            => Mathf.Abs(a.x - b.x) < 0.25f
               && Mathf.Abs(a.y - b.y) < 0.25f;

        private static void SetTopStretch(
            RectTransform rect,
            float left,
            float right,
            float top,
            float height)
        {
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(
                -(left + right),
                height);
        }

        private static void SetBottomStretch(
            RectTransform rect,
            float left,
            float right,
            float bottom,
            float height)
        {
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottom);
            rect.sizeDelta = new Vector2(
                -(left + right),
                height);
        }
    }
}
