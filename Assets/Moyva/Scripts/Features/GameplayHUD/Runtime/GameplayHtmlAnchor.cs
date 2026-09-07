using System;
using System.Collections.Generic;
using Kruty1918.Moyva.InputRouting.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityHTML.Runtime;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    [ExecuteAlways]
    public sealed class GameplayHtmlAnchor : MonoBehaviour
    {
        internal enum PreviewScreen
        {
            Normal,
            FirstCastle,
            Construction,
            Selection,
            Recruitment,
            Dashboard,
            Pause,
            Cargo,
            Route,
        }

        [SerializeField] private RectTransform _mountRoot;
        [SerializeField] private TextAsset _cssAsset;
        [SerializeField] private TMP_FontAsset _fontAsset;
        [SerializeField] private Sprite _notificationsIcon;
        [SerializeField] private Sprite _menuIcon;
        [SerializeField] private GameObject[] _legacyScreenRoots = Array.Empty<GameObject>();
        [SerializeField] private bool _editorLivePreview = true;
        [SerializeField] private PreviewScreen _editorPreviewScreen = PreviewScreen.FirstCastle;

        private IUnityHtmlHost _previewHost;
        private RectTransform _inputShieldRoot;
        private readonly List<RectTransform> _inputShields = new();
        private int _previewSignature;
        private int _lastWidth = -1;
        private int _lastHeight = -1;
        private Rect _lastSafeArea;

        public RectTransform MountRoot => _mountRoot != null
            ? _mountRoot
            : transform as RectTransform;
        public TextAsset CssAsset => _cssAsset;
        public TMP_FontAsset FontAsset => _fontAsset;
        public Sprite NotificationsIcon => _notificationsIcon;
        public Sprite MenuIcon => _menuIcon;

        public string ViewportClass
        {
            get
            {
                RectTransform root = MountRoot;
                float width = root != null ? root.rect.width : Screen.width;
                float height = root != null ? root.rect.height : Screen.height;
                if (height < 760f)
                    return "vp-short";
                if (width < 1200f)
                    return "vp-compact";
                return width >= 1900f ? "vp-wide" : "vp-standard";
            }
        }

        public void PrepareForMount()
        {
            RectTransform root = MountRoot;
            if (root == null)
                return;

            root.gameObject.SetActive(true);
            root.localScale = Vector3.one;
            ApplySafeArea();
        }

        public void SetLegacyUiVisible(bool visible)
        {
            for (int index = 0; index < _legacyScreenRoots.Length; index++)
            {
                GameObject root = _legacyScreenRoots[index];
                if (root != null && root.activeSelf != visible)
                    root.SetActive(visible);
            }
        }

        public void StopEditorPreview()
        {
            _previewHost?.Dispose();
            _previewHost = null;
            _previewSignature = 0;
            HideInputShields();
        }

        internal void SyncInputShields(GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            RectTransform root = MountRoot;
            if (root == null || snapshot == null || state == null)
                return;

            EnsureInputShieldRoot(root);

            Rect rect = root.rect;
            float width = Mathf.Max(1f, rect.width);
            float height = Mathf.Max(1f, rect.height);
            LayoutMetrics metrics = ResolveLayoutMetrics(width, height);
            int used = 0;

            if (state.IsPaused || state.OpenPanelId == GameplayHtmlPanel.Kingdom)
            {
                SetShield(used++, 0f, 0f, width, height);
                HideUnusedShields(used);
                return;
            }

            float availableWidth = Mathf.Max(1f, width - metrics.Padding * 2f);
            float topWidth = metrics.TopbarMaxWidth > 0f
                ? Mathf.Min(availableWidth, metrics.TopbarMaxWidth)
                : availableWidth;
            SetShield(
                used++,
                (width - topWidth) * 0.5f,
                height - metrics.Padding - metrics.TopbarHeight,
                topWidth,
                metrics.TopbarHeight);

            SetShield(
                used++,
                Mathf.Max(metrics.Padding, (width - metrics.CommandBarWidth) * 0.5f),
                metrics.Padding,
                metrics.CommandBarWidth,
                metrics.CommandBarHeight);

            if (HasSidePanel(snapshot, state))
            {
                float sideWidth = Mathf.Min(metrics.SidePanelWidth, width * metrics.SidePanelMaxWidthRatio);
                float top = metrics.Padding + metrics.TopbarHeight + metrics.WorkspaceTopPadding;
                float bottom = metrics.Padding + metrics.CommandBarHeight + metrics.WorkspaceBottomPadding;
                SetShield(
                    used++,
                    width - metrics.Padding - sideWidth,
                    bottom,
                    sideWidth,
                    Mathf.Max(1f, height - top - bottom));
            }

            if (!string.IsNullOrWhiteSpace(state.Feedback)
                && !snapshot.RequiresFirstCastle)
            {
                float toastWidth = Mathf.Min(420f, width * 0.7f);
                SetShield(
                    used++,
                    (width - toastWidth) * 0.5f,
                    metrics.Padding + metrics.CommandBarHeight + 30f,
                    toastWidth,
                    44f);
            }

            HideUnusedShields(used);
        }

        private void OnEnable()
        {
            if (_mountRoot == null)
                _mountRoot = transform as RectTransform;

            if (!Application.isPlaying && _editorLivePreview)
                RefreshPreview(true);
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                StopEditorPreview();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                _previewSignature = 0;
        }

        private void Update()
        {
            if (LayoutChanged())
                ApplySafeArea();

            if (!Application.isPlaying && _editorLivePreview)
                RefreshPreview(false);
        }

        private bool LayoutChanged()
            => _lastWidth != Screen.width
               || _lastHeight != Screen.height
               || _lastSafeArea != Screen.safeArea;

        private void ApplySafeArea()
        {
            RectTransform root = MountRoot;
            if (root == null)
                return;

            Rect safe = Screen.safeArea;
            float width = Mathf.Max(1f, Screen.width);
            float height = Mathf.Max(1f, Screen.height);
            root.anchorMin = new Vector2(safe.xMin / width, safe.yMin / height);
            root.anchorMax = new Vector2(safe.xMax / width, safe.yMax / height);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = Vector2.zero;
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _lastSafeArea = safe;
        }

        private void RefreshPreview(bool force)
        {
            RectTransform root = MountRoot;
            if (root == null || _cssAsset == null)
                return;

            int signature = HashCode.Combine(
                root.GetInstanceID(),
                _cssAsset.GetInstanceID(),
                _cssAsset.text?.Length ?? 0,
                _fontAsset != null ? _fontAsset.GetInstanceID() : 0,
                (int)_editorPreviewScreen,
                Mathf.RoundToInt(root.rect.width),
                Mathf.RoundToInt(root.rect.height));
            if (!force && signature == _previewSignature)
                return;

            _previewSignature = signature;
            PrepareForMount();
            SetLegacyUiVisible(false);
            _previewHost ??= new UnityHtmlHost();
            var state = new GameplayHtmlState();
            GameplayHtmlSnapshot snapshot = GameplayHtmlSnapshot.CreatePreview(_editorPreviewScreen);
            if (_editorPreviewScreen == PreviewScreen.Dashboard)
                state.OpenPanel(GameplayHtmlPanel.Kingdom);
            if (_editorPreviewScreen == PreviewScreen.Construction)
                state.OpenPanel(GameplayHtmlPanel.Construction);
            if (_editorPreviewScreen == PreviewScreen.Recruitment)
                state.SetSelectionTab(GameplaySelectionTab.Recruit);
            if (_editorPreviewScreen == PreviewScreen.Cargo)
                state.SetSelectionTab(GameplaySelectionTab.Cargo);
            if (_editorPreviewScreen == PreviewScreen.Route)
                state.SetSelectionTab(GameplaySelectionTab.Route);
            if (_editorPreviewScreen == PreviewScreen.Pause)
                state.SetPaused(true);

            var globals = new Dictionary<string, object>
            {
                ["gameplay"] = new GameplayHtmlPreviewBridge(),
            };
            if (_fontAsset != null)
                globals["moyvaFont"] = _fontAsset;
            globals["gameplay_notifications_icon"] = _notificationsIcon;
            globals["gameplay_menu_icon"] = _menuIcon;

            UnityHtmlMountResult result = _previewHost.Mount(
                root,
                new UnityHtmlDocument(
                    GameplayHtmlMarkup.Build(snapshot, state, ViewportClass),
                    _cssAsset.text,
                    "Moyva Gameplay Editor Preview"),
                globals);
            SyncInputShields(snapshot, state);
            if (!result.Succeeded)
                Debug.LogError($"[GameplayHTML] Editor preview failed: {result.ErrorMessage}", this);
        }

        private static bool HasSidePanel(GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            return snapshot.RequiresFirstCastle
                   || state.OpenPanelId == GameplayHtmlPanel.Construction
                   || state.OpenPanelId == GameplayHtmlPanel.Notifications
                   || !string.IsNullOrWhiteSpace(snapshot.SelectionKind);
        }

        private LayoutMetrics ResolveLayoutMetrics(float width, float height)
        {
            string viewportClass = ViewportClass;
            bool compact = string.Equals(viewportClass, "vp-compact", StringComparison.Ordinal);
            bool wide = string.Equals(viewportClass, "vp-wide", StringComparison.Ordinal);
            bool shortened = string.Equals(viewportClass, "vp-short", StringComparison.Ordinal);

            float padding = shortened ? 8f : compact ? 10f : 18f;
            float topbarHeight = shortened ? 50f : compact ? 54f : 62f;
            float commandHeight = shortened ? 50f : 58f;
            float sideWidth = wide ? 500f : compact ? 400f : 460f;
            float sideRatio = compact ? 0.50f : 0.48f;
            float commandWidth = Mathf.Min(width * 0.92f, 980f);
            float topbarMaxWidth = wide ? 1900f : 0f;

            return new LayoutMetrics(
                padding,
                topbarHeight,
                commandHeight,
                shortened ? 8f : 14f,
                shortened ? 7f : 12f,
                sideWidth,
                sideRatio,
                commandWidth,
                topbarMaxWidth);
        }

        private void EnsureInputShieldRoot(RectTransform mountRoot)
        {
            if (_inputShieldRoot != null)
            {
                _inputShieldRoot.SetAsFirstSibling();
                return;
            }

            _inputShields.Clear();
            var shieldRootObject = new GameObject(
                "__InputShield",
                typeof(RectTransform));
            shieldRootObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            _inputShieldRoot = (RectTransform)shieldRootObject.transform;
            _inputShieldRoot.SetParent(mountRoot, false);
            _inputShieldRoot.anchorMin = Vector2.zero;
            _inputShieldRoot.anchorMax = Vector2.one;
            _inputShieldRoot.offsetMin = Vector2.zero;
            _inputShieldRoot.offsetMax = Vector2.zero;
            _inputShieldRoot.pivot = new Vector2(0.5f, 0.5f);
            _inputShieldRoot.SetAsFirstSibling();
        }

        private void SetShield(int index, float left, float bottom, float width, float height)
        {
            RectTransform shield = GetShield(index);
            shield.gameObject.SetActive(width > 0f && height > 0f);
            shield.anchorMin = Vector2.zero;
            shield.anchorMax = Vector2.zero;
            shield.pivot = Vector2.zero;
            shield.anchoredPosition = new Vector2(left, bottom);
            shield.sizeDelta = new Vector2(width, height);
        }

        private RectTransform GetShield(int index)
        {
            while (_inputShields.Count <= index)
                _inputShields.Add(null);

            if (_inputShields[index] == null)
            {
                var shieldObject = new GameObject(
                    $"Zone{index}",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(GameplayInputBlocker));
                shieldObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                var rect = (RectTransform)shieldObject.transform;
                rect.SetParent(_inputShieldRoot, false);
                Image image = shieldObject.GetComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0f);
                image.raycastTarget = true;
                _inputShields[index] = rect;
            }

            RectTransform shield = _inputShields[index];
            shield.SetParent(_inputShieldRoot, false);
            return shield;
        }

        private void HideUnusedShields(int used)
        {
            for (int index = used; index < _inputShields.Count; index++)
            {
                if (_inputShields[index] != null)
                    _inputShields[index].gameObject.SetActive(false);
            }
        }

        private void HideInputShields()
        {
            HideUnusedShields(0);
        }

        private readonly struct LayoutMetrics
        {
            public LayoutMetrics(
                float padding,
                float topbarHeight,
                float commandBarHeight,
                float workspaceTopPadding,
                float workspaceBottomPadding,
                float sidePanelWidth,
                float sidePanelMaxWidthRatio,
                float commandBarWidth,
                float topbarMaxWidth)
            {
                Padding = padding;
                TopbarHeight = topbarHeight;
                CommandBarHeight = commandBarHeight;
                WorkspaceTopPadding = workspaceTopPadding;
                WorkspaceBottomPadding = workspaceBottomPadding;
                SidePanelWidth = sidePanelWidth;
                SidePanelMaxWidthRatio = sidePanelMaxWidthRatio;
                CommandBarWidth = commandBarWidth;
                TopbarMaxWidth = topbarMaxWidth;
            }

            public float Padding { get; }
            public float TopbarHeight { get; }
            public float CommandBarHeight { get; }
            public float WorkspaceTopPadding { get; }
            public float WorkspaceBottomPadding { get; }
            public float SidePanelWidth { get; }
            public float SidePanelMaxWidthRatio { get; }
            public float CommandBarWidth { get; }
            public float TopbarMaxWidth { get; }
        }

        private sealed class GameplayHtmlPreviewBridge
        {
            public void Kingdom() { }
            public void ShowOverview() { }
            public void ShowResources() { }
            public void ShowStorage() { }
            public void ShowBuildings() { }
            public void ShowUnits() { }
            public void ShowTurns() { }
            public void ShowSelectionDetails() { }
            public void ShowRecruitment() { }
            public void ShowRecruitmentQueue() { }
            public void ShowCargo() { }
            public void ShowCargoRoute() { }
            public void SetCargoOperation(object value) { }
            public void SetCargoWarehouse(object value) { }
            public void SetCargoTarget(object value) { }
            public void SetCargoResource(object value) { }
            public void SetCargoRepeat(object value) { }
            public void SetCargoAmount(object value) { }
            public void TransferCargo() { }
            public void StartCargoRoute() { }
            public void StopCargoRoute() { }
            public void FoundSettlement() { }
            public void AttackSelection() { }
            public void CaptureSelection() { }
            public void Recruit(object value) { }
            public void ClosePanel() { }
            public void Construction() { }
            public void SetConstructionCategory(object value) { }
            public void SetConstructionSearch(string value) { }
            public void PreviousConstructionPage() { }
            public void NextConstructionPage() { }
            public void SelectBuilding(object value) { }
            public void ConfirmPlacement() { }
            public void CancelPlacement() { }
            public void RotatePlacement() { }
            public void UndoPlacement() { }
            public void RedoPlacement() { }
            public void EndTurn() { }
            public void ClearSelection() { }
            public void Notifications() { }
            public void Pause() { }
            public void Resume() { }
            public void ExitToMenu() { }
            public void FocusWarehouse(object x, object y, object id) { }
        }
    }
}
