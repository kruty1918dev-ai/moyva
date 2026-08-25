using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Final recruitment panel layout owner. Presentation only.
    /// Owns RecruitmentPanel root placement and recruitment internals.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1600)]
    public sealed class RecruitmentPanelVisualPolishLayout : MonoBehaviour
    {
        public const bool RecruitmentInternalsOwner = true;

        private const float DesktopWidth = 610f;
        private const float TabletWidth = 548f;
        private const float CompactWidth = 520f;
        private const float DesktopHeight = 480f;
        private const float TabletHeight = 450f;
        private const float CompactHeight = 430f;
        private const float EdgeMargin = 20f;
        private const float RightGap = 14f;
        private const float HeaderHeight = 42f;
        private const float StateHeight = 52f;
        private const float CatalogWidth = 198f;
        private const float CatalogCompactWidth = 184f;
        private const float RecipeRowHeight = 44f;
        private const float QueueEmptyHeight = 38f;
        private const float QueueHeaderHeight = 24f;
        private const float QueueRowHeight = 30f;
        private const float QueueMaxHeight = 126f;
        private const float HireButtonWidth = 124f;
        private const float HireButtonHeight = 34f;
        private const float DetailIconSize = 64f;

        private RectTransform _panel;
        private RectTransform _hudRoot;
        private RectTransform _mainContent;
        private RectTransform _catalog;
        private RectTransform _details;
        private RectTransform _queueSection;
        private RectTransform _queueRows;
        private RectTransform _recipeSlots;
        private Button _hireButton;
        private Button _closeButton;

        private Vector2 _lastHudSize = new(-1f, -1f);
        private float _lastReserve = float.NaN;
        private int _lastQueueRows = -1;
        private bool _lastBottomSheet;
        private bool _ready;

        private void OnEnable()
        {
            _ready = Cache();
            ApplyNow();
        }

        private void OnTransformChildrenChanged()
        {
            _ready = Cache();
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

            Vector2 hudSize = _hudRoot.rect.size;
            float reserve = CalculateRightReserve();
            int queueRows = CountActiveQueueRows();
            bool bottomSheet = ShouldUseBottomSheet(hudSize, reserve);

            if (!Approximately(hudSize, _lastHudSize)
                || float.IsNaN(_lastReserve)
                || Mathf.Abs(reserve - _lastReserve) > 0.5f
                || queueRows != _lastQueueRows
                || bottomSheet != _lastBottomSheet
                || !LayoutStillApplied(hudSize, reserve, bottomSheet))
            {
                ApplyNow();
            }
        }

        public bool ApplyNow()
        {
            if (!_ready && !Cache())
                return false;

            Vector2 hudSize = _hudRoot.rect.size;
            if (hudSize.x <= 1f || hudSize.y <= 1f)
                return false;

            float reserve = CalculateRightReserve();
            int queueRows = CountActiveQueueRows();
            bool bottomSheet = ShouldUseBottomSheet(hudSize, reserve);

            ApplyPanelRect(hudSize, reserve, bottomSheet);
            ApplyRootLayout();
            ApplySectionLayout(queueRows, bottomSheet);
            ApplyRecipeRows();
            ApplyDetailsConstraints();
            ApplyQueueRows(queueRows);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);

            _lastHudSize = hudSize;
            _lastReserve = reserve;
            _lastQueueRows = queueRows;
            _lastBottomSheet = bottomSheet;
            return true;
        }

        private bool Cache()
        {
            _panel = transform as RectTransform;
            _hudRoot = transform.parent as RectTransform;
            if (_panel == null || _hudRoot == null)
                return false;

            _mainContent = R("MainContent");
            _catalog = R("MainContent/Catalog");
            _details = R("MainContent/Details");
            _queueSection = R("QueueSection");
            _queueRows = R("QueueSection/QueueRows");
            _recipeSlots = R("MainContent/Catalog/RecipeScroll/RecipeSlots");
            _hireButton = transform.Find("MainContent/Details/ActionRow/HireButton")
                ?.GetComponent<Button>();
            _closeButton = transform.Find("Header/CloseButton")
                ?.GetComponent<Button>();

            return _mainContent != null
                && _catalog != null
                && _details != null
                && _queueSection != null
                && _queueRows != null
                && _recipeSlots != null;
        }

        private void ApplyPanelRect(
            Vector2 hudSize,
            float reserve,
            bool bottomSheet)
        {
            if (bottomSheet)
            {
                float sheetWidth = Mathf.Min(680f, Mathf.Max(420f, hudSize.x - EdgeMargin * 2f));
                float sheetHeight = Mathf.Min(
                    Mathf.Max(360f, hudSize.y * 0.56f),
                    Mathf.Min(CompactHeight, hudSize.y - EdgeMargin * 2f));

                _panel.anchorMin = new Vector2(0.5f, 0f);
                _panel.anchorMax = new Vector2(0.5f, 0f);
                _panel.pivot = new Vector2(0.5f, 0f);
                _panel.anchoredPosition = new Vector2(0f, EdgeMargin);
                _panel.sizeDelta = new Vector2(sheetWidth, sheetHeight);
                return;
            }

            float targetWidth = hudSize.x >= 1600f
                ? DesktopWidth
                : hudSize.x >= 1360f
                    ? TabletWidth
                    : CompactWidth;
            float availableWidth = Mathf.Max(420f, hudSize.x - reserve - EdgeMargin * 2f);
            float width = Mathf.Min(targetWidth, availableWidth);

            float targetHeight = hudSize.y >= 900f
                ? DesktopHeight
                : hudSize.y >= 760f
                    ? TabletHeight
                    : CompactHeight;
            float height = Mathf.Min(targetHeight, Mathf.Max(390f, hudSize.y - EdgeMargin * 2f));

            _panel.anchorMin = new Vector2(1f, 0.5f);
            _panel.anchorMax = new Vector2(1f, 0.5f);
            _panel.pivot = new Vector2(1f, 0.5f);
            _panel.anchoredPosition = new Vector2(-reserve, 0f);
            _panel.sizeDelta = new Vector2(width, height);
        }

        private void ApplyRootLayout()
        {
            if (_panel.TryGetComponent(out VerticalLayoutGroup layout))
            {
                layout.padding = new RectOffset(12, 12, 10, 10);
                layout.spacing = 8f;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }

            if (_panel.TryGetComponent(out ContentSizeFitter fitter))
                fitter.enabled = false;
        }

        private void ApplySectionLayout(int queueRows, bool bottomSheet)
        {
            SetPreferred("Header", HeaderHeight, flexibleHeight: 0f);
            SetPreferred("StateStrip", StateHeight, flexibleHeight: 0f);
            SetPreferred("MainContent", 0f, flexibleHeight: 1f);

            float queueHeight = CalculateQueueHeight(queueRows);
            SetPreferred(_queueSection, queueHeight, flexibleHeight: 0f);

            if (_mainContent.TryGetComponent(out HorizontalLayoutGroup main))
            {
                main.padding = new RectOffset(0, 0, 0, 0);
                main.spacing = 10f;
                main.childControlWidth = true;
                main.childControlHeight = true;
                main.childForceExpandWidth = false;
                main.childForceExpandHeight = true;
            }

            SetPreferredWidth(_catalog, bottomSheet ? CatalogCompactWidth : CatalogWidth, 0f);
            SetPreferredWidth(_details, 0f, 1f);
        }

        private void ApplyRecipeRows()
        {
            if (_recipeSlots.TryGetComponent(out VerticalLayoutGroup layout))
            {
                layout.padding = new RectOffset(0, 0, 0, 0);
                layout.spacing = 5f;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }

            for (int index = 0; index < _recipeSlots.childCount; index++)
            {
                Transform row = _recipeSlots.GetChild(index);
                if (row == null
                    || !row.name.StartsWith("RecipeSlot_", StringComparison.Ordinal))
                    continue;

                RectTransform rect = row as RectTransform;
                SetPreferred(rect, RecipeRowHeight, 0f);
                if (rect != null)
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, RecipeRowHeight);
            }
        }

        private void ApplyDetailsConstraints()
        {
            SetPreferred("MainContent/Details/UnitHeader", DetailIconSize, 0f);
            SetPreferred("MainContent/Details/CostSection", 48f, 0f);
            SetPreferred("MainContent/Details/ActionRow", HireButtonHeight, 0f);

            RectTransform icon =
                transform.Find("MainContent/Details/UnitHeader/UnitIcon") as RectTransform;
            if (icon != null)
                icon.sizeDelta = new Vector2(DetailIconSize, DetailIconSize);

            if (_hireButton != null)
            {
                RectTransform rect = _hireButton.transform as RectTransform;
                SetPreferredWidth(rect, HireButtonWidth, 0f);
                SetPreferred(rect, HireButtonHeight, 0f);
            }

            if (_closeButton != null)
            {
                RectTransform rect = _closeButton.transform as RectTransform;
                SetPreferredWidth(rect, 28f, 0f);
                SetPreferred(rect, 28f, 0f);
            }
        }

        private void ApplyQueueRows(int queueRows)
        {
            if (_queueRows != null)
                _queueRows.gameObject.SetActive(queueRows > 0);

            if (_queueRows != null
                && _queueRows.TryGetComponent(out VerticalLayoutGroup layout))
            {
                layout.padding = new RectOffset(0, 0, 0, 0);
                layout.spacing = 4f;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }

            if (_queueRows == null)
                return;

            for (int index = 0; index < _queueRows.childCount; index++)
            {
                Transform row = _queueRows.GetChild(index);
                if (row == null
                    || !row.name.StartsWith("QueueRow_", StringComparison.Ordinal))
                    continue;

                RectTransform rect = row as RectTransform;
                SetPreferred(rect, QueueRowHeight, 0f);
                if (rect != null)
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, QueueRowHeight);
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
                QueueHeaderHeight + 6f + rows * QueueRowHeight + Mathf.Max(0, rows - 1) * 4f);
        }

        private static bool ShouldUseBottomSheet(Vector2 hudSize, float reserve)
            => hudSize.x < 980f || hudSize.x - reserve < 760f;

        private float CalculateRightReserve()
        {
            if (_hudRoot == null)
                return EdgeMargin;

            RectTransform rail = FindWorldInfoRail();
            if (rail == null)
                return EdgeMargin;

            Vector3[] corners = new Vector3[4];
            rail.GetWorldCorners(corners);
            float left = float.PositiveInfinity;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 local = _hudRoot.InverseTransformPoint(corners[i]);
                left = Mathf.Min(left, local.x);
            }

            float reserve = _hudRoot.rect.xMax - left + RightGap;
            return Mathf.Clamp(
                reserve,
                EdgeMargin,
                Mathf.Max(EdgeMargin, _hudRoot.rect.width * 0.48f));
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
                    n.IndexOf("WorldInfoPanel", StringComparison.OrdinalIgnoreCase) >= 0
                    || n.IndexOf("BuildingInfoPanel", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!candidate)
                    continue;

                Vector3[] c = new Vector3[4];
                rect.GetWorldCorners(c);
                float left = float.PositiveInfinity;

                for (int k = 0; k < c.Length; k++)
                {
                    Vector3 local = _hudRoot.InverseTransformPoint(c[k]);
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

        private bool LayoutStillApplied(
            Vector2 hudSize,
            float reserve,
            bool bottomSheet)
        {
            if (_panel == null)
                return false;

            if (bottomSheet)
                return _panel.anchorMin == new Vector2(0.5f, 0f)
                    && _panel.anchorMax == new Vector2(0.5f, 0f)
                    && _panel.rect.width <= Mathf.Min(680f, hudSize.x)
                    && _panel.rect.height <= CompactHeight + 0.5f;

            float expectedRight = -reserve;
            return _panel.anchorMin == new Vector2(1f, 0.5f)
                && _panel.anchorMax == new Vector2(1f, 0.5f)
                && Mathf.Abs(_panel.anchoredPosition.x - expectedRight) <= 0.5f
                && _panel.rect.width <= DesktopWidth + 0.5f
                && _panel.rect.height <= DesktopHeight + 0.5f;
        }

        private RectTransform R(string path)
            => transform.Find(path) as RectTransform;

        private void SetPreferred(
            string path,
            float height,
            float flexibleHeight)
            => SetPreferred(R(path), height, flexibleHeight);

        private static void SetPreferred(
            RectTransform rect,
            float height,
            float flexibleHeight)
        {
            if (rect == null)
                return;

            LayoutElement element =
                rect.GetComponent<LayoutElement>()
                ?? rect.gameObject.AddComponent<LayoutElement>();

            if (height > 0f)
            {
                element.minHeight = height;
                element.preferredHeight = height;
            }
            else
            {
                element.minHeight = 0f;
                element.preferredHeight = -1f;
            }

            element.flexibleHeight = flexibleHeight;
        }

        private static void SetPreferredWidth(
            RectTransform rect,
            float width,
            float flexibleWidth)
        {
            if (rect == null)
                return;

            LayoutElement element =
                rect.GetComponent<LayoutElement>()
                ?? rect.gameObject.AddComponent<LayoutElement>();

            if (width > 0f)
            {
                element.minWidth = width;
                element.preferredWidth = width;
            }
            else
            {
                element.minWidth = 0f;
                element.preferredWidth = -1f;
            }

            element.flexibleWidth = flexibleWidth;
        }

        private static bool Approximately(Vector2 a, Vector2 b)
            => Mathf.Abs(a.x - b.x) < 0.25f
               && Mathf.Abs(a.y - b.y) < 0.25f;
    }
}
