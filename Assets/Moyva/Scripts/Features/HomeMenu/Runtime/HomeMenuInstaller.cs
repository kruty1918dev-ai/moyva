using System;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.HomeMenu.Runtime.Startup;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.Shared.UI;
using Kruty1918.Moyva.WorldCreation.API;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;
using UnityHTML.Runtime;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    public sealed class HomeMenuInstaller : MonoInstaller
    {
        private const bool UseDynamicMoyvaUi = true;

        [SerializeField] private string _lobbyPanelName = "LobbyPanel";
        [SerializeField] private string _worldSetupPanelName = "WorldSetupPanel";
        [SerializeField] private string _createRoomPanelName = "CreateRoomPanel";
        [SerializeField] private string _joinRoomPanelName = "JoinRoomPanel";
        [SerializeField] private string _kickPlayerPanelName = "KickPlayerPanel";
        [SerializeField] private string _infoPanelName = "InfoPanel";
        [SerializeField] private string _multiplayerTypePanelName = "SelectMultiplayerType";
        [SerializeField] private AudioMixerBindingsSO _audioMixerBindings;
        [SerializeField] private HomeMenuConfigSO _config;
        [SerializeField] private WorldCreationDefaultsSO _worldCreationDefaults;

        [Header("Navigation")]
        [Tooltip("Names of panels that require confirmation when the player navigates back from them.")]
        [SerializeField] private string[] _confirmOnBackMenuNames = new string[0];

        [Header("Menu Reveal")]
        [SerializeField] private HomeMenuRevealFadeSettings _menuRevealFade = new();

        public override void InstallBindings()
        {
            var config = _config != null ? _config : MoyvaJsonObjectFactory.Create<HomeMenuConfigSO>();
            var useMoyvaUi = UseDynamicMoyvaUi;

            MenuWorldPreviewKingdomPlacementFeatureBindings.Install(Container);
            MenuWorldPreviewTextureBuilderFeatureBindings.Install(Container);

            Container.BindInstance(_menuRevealFade ?? new HomeMenuRevealFadeSettings()).AsSingle();
            Container.BindInstance(config).AsSingle().IfNotBound();

            Container.BindInterfacesTo<HomeMenuRevealOverlayService>().AsSingle().NonLazy();
            Container.Bind<INavigation>().To<HomeMenuNavigation>().AsSingle();
            Container.BindInterfacesAndSelfTo<HomeMenuInitializer>().AsSingle();

            if (!useMoyvaUi)
                BindSharedSceneUi();
            BindCoreServices();
            BindUiLayer(useMoyvaUi);
            BindPanelServices();
            BindPanelNames();

            Container.BindInterfacesAndSelfTo<ConnectivityWatchdogService>().AsSingle();
            Container.BindInterfacesAndSelfTo<WorldCreationPanelService>().AsSingle();
        }

        private void BindSharedSceneUi()
        {
            Container.BindInterfacesTo<PlayerNameTextComponent>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<ConfirmButton>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();
        }

        private void BindCoreServices()
        {
            Container.BindInterfacesAndSelfTo<ConformationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LocalGameSettingsService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AudioSettingsRuntimeSyncService>().AsSingle();

            if (_audioMixerBindings != null)
                Container.BindInstance(_audioMixerBindings).AsSingle();

            if (_worldCreationDefaults != null)
                Container.BindInstance(_worldCreationDefaults).AsSingle();

            Container.Bind<IUnityHtmlHost>().To<UnityHtmlHost>().AsSingle();
        }

        private void BindUiLayer(bool useMoyvaUi)
        {
            if (useMoyvaUi)
            {
                Container.Bind<HomeMenuMoyvaUiState>().AsSingle();

                Container.BindInterfacesAndSelfTo<HomeMenuMoyvaUiViewController>()
                    .AsSingle();

                BindMoyvaUiPanel("PlayModePanel");
                BindMoyvaUiPanel("ContinuePanel");
                BindMoyvaUiPanel(_multiplayerTypePanelName);
                BindMoyvaUiPanel("MultiplayerPanel");
                BindMoyvaUiPanel("SettingsPanel");
                BindMoyvaUiPanel(_createRoomPanelName);
                BindMoyvaUiPanel(_joinRoomPanelName);
                BindMoyvaUiPanel(_worldSetupPanelName);
                BindMoyvaUiPanel(_lobbyPanelName);
                BindMoyvaUiPanel(_kickPlayerPanelName);
                BindMoyvaUiPanel(_infoPanelName);

                Container.Bind<HomeMenuMoyvaUiAnchor>()
                    .FromComponentsInHierarchy(includeInactive: true)
                    .AsCached();

                Container.BindInterfacesAndSelfTo<HomeMenuMoyvaUiPresenter>()
                    .AsSingle()
                    .NonLazy();
                return;
            }

            HomeMenuRuntimeUiFactory.EnsureRequiredPanels(_infoPanelName);
            Container.Bind<IOverlayLoader>().To<HomeMenuOverlayLoader>().AsSingle();
            Container.Bind<OverlayPanelLoader>().FromComponentInHierarchy(includeInactive: true).AsSingle();
            Container.Bind<IConfiremationPanel>().To<ConfirmationPanel>().FromComponentInHierarchy(includeInactive: true).AsSingle();

            Container.BindInterfacesTo<NavigationPanel>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.Bind<NavigationButton>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.Bind<JoinRoomOpenButton>().FromComponentsInHierarchy(includeInactive: true).AsCached();

            Container.Bind<HomeMenuHtmlShellAnchor>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesAndSelfTo<HomeMenuHtmlShellPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<ContinueViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<CreateRoomViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<LobbyPanelViewController>().FromComponentInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<KickPlayerPanelViewController>().FromComponentInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<JoinRoomViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<InfoPanelViewController>().FromComponentInHierarchy(includeInactive: true).AsSingle();
            Container.BindInterfacesTo<PasswordPanelViewController>().FromComponentInHierarchy(includeInactive: true).AsSingle();
            Container.BindInterfacesTo<GameSettingsViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<MultiplayerViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<MultiplayerModeViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<WorldSetupViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
        }

        private void BindPanelServices()
        {
            Container.BindInterfacesAndSelfTo<JoinRoomUiGateway>().AsSingle();
            Container.BindInterfacesAndSelfTo<ContinuePanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<MultiplayerPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LobbyFlowContext>().AsSingle();
            Container.BindInterfacesAndSelfTo<CreateRoomPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<JoinRoomPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<MultiplayerModePanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<MultiplayerMenuModeService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LobbyPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<KickPlayerPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameplaySession>().AsSingle();
            Container.BindInterfacesTo<MenuApi>().AsSingle();
            Container.BindInterfacesAndSelfTo<HomeMenuGameStarter>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameplayStartupPipeline>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameStartListenerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<InfoPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<HostDisconnectNoticePresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PasswordPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameSettingsPanelService>().AsSingle();
            Container.BindInterfacesTo<HomeMenuUiPerformanceGuard>().AsSingle().NonLazy();
            Container.BindInterfacesTo<HomeMenuViewportLayoutService>().AsSingle().NonLazy();
        }

        private void BindPanelNames()
        {
            Container.BindInstance(_lobbyPanelName).WithId("LobbyPanelName");
            Container.BindInstance(_worldSetupPanelName).WithId("WorldSetupPanelName");
            Container.BindInstance(_createRoomPanelName).WithId("CreateRoomPanelName");
            Container.BindInstance(_joinRoomPanelName).WithId("JoinRoomPanelName");
            Container.BindInstance(_kickPlayerPanelName).WithId("KickPlayerPanelName");
            Container.BindInstance(_infoPanelName).WithId("InfoPanelName");
            Container.BindInstance(_multiplayerTypePanelName).WithId("MultiplayerTypePanelName");
            Container.BindInstance(_confirmOnBackMenuNames).AsSingle();
        }

        private void BindMoyvaUiPanel(string panelName)
        {
            if (string.IsNullOrWhiteSpace(panelName))
                return;

            Container.Bind<INavigationPanel>()
                .To<HomeMenuMoyvaUiNavigationPanel>()
                .AsCached()
                .WithArguments(panelName.Trim());
        }
    }

    internal sealed class HomeMenuUiPerformanceGuard : IInitializable
    {
        private static readonly ProfilerMarker OptimizeMarker = new("Moyva.HomeMenu.UI.OptimizeRaycasts");

        private readonly Transform _root;

        public HomeMenuUiPerformanceGuard()
        {
        }

        internal HomeMenuUiPerformanceGuard(Transform root)
        {
            _root = root;
        }

        public void Initialize()
        {
            using (OptimizeMarker.Auto())
            {
                var disabled = DisableNonInteractiveRaycastTargets();
                HomeMenuUiPerformanceMetrics.RaycastTargetsDisabled = disabled;
            }
        }

        private int DisableNonInteractiveRaycastTargets()
        {
            var disabled = 0;
            var graphics = _root != null
                ? _root.GetComponentsInChildren<Graphic>(true)
                : UnityEngine.Object.FindObjectsByType<Graphic>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            for (var i = 0; i < graphics.Length; i++)
            {
                var graphic = graphics[i];
                if (graphic == null || !graphic.raycastTarget)
                    continue;

                if (!ShouldDisableRaycast(graphic))
                    continue;

                graphic.raycastTarget = false;
                disabled++;
            }

            return disabled;
        }

        private static bool ShouldDisableRaycast(Graphic graphic)
        {
            if (graphic is TMP_Text)
                return true;

            if (graphic.GetComponent<Selectable>() != null)
                return false;

            if (graphic.GetComponentInParent<Selectable>() != null)
                return false;

            if (IsFullscreenInputBlocker(graphic))
                return false;

            if (IsDecorativeByName(graphic.gameObject.name))
                return true;

            return graphic.color.a <= 0.01f;
        }

        private static bool IsFullscreenInputBlocker(Graphic graphic)
        {
            var rect = graphic.transform as RectTransform;
            if (rect == null)
                return false;

            var stretchesToParent =
                rect.anchorMin == Vector2.zero &&
                rect.anchorMax == Vector2.one &&
                rect.offsetMin == Vector2.zero &&
                rect.offsetMax == Vector2.zero;

            if (!stretchesToParent)
                return false;

            var name = graphic.gameObject.name;
            return name.Contains("Panel") ||
                   name.Contains("Dialog") ||
                   name.Contains("Overlay") ||
                   name.Contains("Modal") ||
                   name.Contains("Scrim");
        }

        private static bool IsDecorativeByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return name.Contains("Background") ||
                   name.Contains("Decor") ||
                   name.Contains("Decoration") ||
                   name.Contains("Divider") ||
                   name.Contains("Line") ||
                   name.Contains("Icon") ||
                   name.Contains("Veil") ||
                   name.Contains("Glow") ||
                   name.Contains("Frame");
        }
    }

    internal static class HomeMenuUiPerformanceMetrics
    {
        internal static readonly ProfilerMarker NavigationOpenMarker = new("Moyva.HomeMenu.Navigation.Open");
        internal static readonly ProfilerMarker NavigationCloseMarker = new("Moyva.HomeMenu.Navigation.Close");
        internal static readonly ProfilerMarker HtmlMountMarker = new("Moyva.HomeMenu.HTML.Mount");
        internal static readonly ProfilerMarker ViewportLayoutMarker = new("Moyva.HomeMenu.UI.ViewportLayout");

        public static int HtmlMountCount { get; private set; }
        public static int RaycastTargetsDisabled { get; set; }
        public static int ViewportLayoutUpdates { get; set; }

        public static void RecordHtmlMount()
        {
            HtmlMountCount++;
        }

        public static void Reset()
        {
            HtmlMountCount = 0;
            RaycastTargetsDisabled = 0;
            ViewportLayoutUpdates = 0;
        }
    }

    internal sealed class HomeMenuViewportLayoutService : IInitializable, ITickable
    {
        private readonly HomeMenuMoyvaUiAnchor[] _moyvaUiAnchors;
        private readonly HomeMenuHtmlShellAnchor[] _htmlShellAnchors;

        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;
        private Rect _lastSafeArea;

        public HomeMenuViewportLayoutService(
            [InjectOptional] HomeMenuMoyvaUiAnchor[] moyvaUiAnchors = null,
            [InjectOptional] HomeMenuHtmlShellAnchor[] htmlShellAnchors = null)
        {
            _moyvaUiAnchors = moyvaUiAnchors ?? Array.Empty<HomeMenuMoyvaUiAnchor>();
            _htmlShellAnchors = htmlShellAnchors ?? Array.Empty<HomeMenuHtmlShellAnchor>();
        }

        public void Initialize() => ApplyLayout();

        public void Tick()
        {
            if (_lastScreenWidth == Screen.width &&
                _lastScreenHeight == Screen.height &&
                _lastSafeArea == Screen.safeArea)
                return;

            ApplyLayout();
        }

        private void ApplyLayout()
        {
            using (HomeMenuUiPerformanceMetrics.ViewportLayoutMarker.Auto())
            {
                ConfigureCanvases();
                AddBoundsGuards();
                ApplyAnchorLayouts();
                HomeMenuUiPerformanceMetrics.ViewportLayoutUpdates++;
                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;
                _lastSafeArea = Screen.safeArea;
            }
        }

        private static void ConfigureCanvases()
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                    continue;

                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                    UiCanvasScalePolicy.Apply(canvas, scaler);
            }
        }

        private static void AddBoundsGuards()
        {
            var panels = UnityEngine.Object.FindObjectsByType<NavigationPanel>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (var i = 0; i < panels.Length; i++)
            {
                if (panels[i] != null && panels[i].transform is RectTransform rect)
                    HomeMenuScreenBoundsGuard.EnsureClamp(rect, 16f);
            }

            var guards = UnityEngine.Object.FindObjectsByType<HomeMenuScreenBoundsGuard>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (var i = 0; i < guards.Length; i++)
                guards[i]?.ApplyNow();
        }

        private void ApplyAnchorLayouts()
        {
            for (var i = 0; i < _moyvaUiAnchors.Length; i++)
                _moyvaUiAnchors[i]?.ApplyViewportLayoutNow();

            for (var i = 0; i < _htmlShellAnchors.Length; i++)
                _htmlShellAnchors[i]?.ApplyViewportLayoutNow();
        }
    }

    internal sealed class HomeMenuScreenBoundsGuard : MonoBehaviour
    {
        internal enum LayoutMode
        {
            StretchToSafeArea,
            ClampInsideSafeArea
        }

        [SerializeField] private LayoutMode _mode = LayoutMode.ClampInsideSafeArea;
        [SerializeField] private float _margin = 16f;

        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;
        private Rect _lastSafeArea;
        private Vector2 _lastParentSize;

        internal static HomeMenuScreenBoundsGuard EnsureStretch(RectTransform rect, float margin = 0f)
            => Ensure(rect, LayoutMode.StretchToSafeArea, margin);

        internal static HomeMenuScreenBoundsGuard EnsureClamp(RectTransform rect, float margin = 16f)
            => Ensure(rect, LayoutMode.ClampInsideSafeArea, margin);

        internal void Configure(LayoutMode mode, float margin)
        {
            _mode = mode;
            _margin = Mathf.Max(0f, margin);
            Invalidate();
        }

        internal bool ApplyNow()
        {
            var rect = transform as RectTransform;
            var parent = rect != null ? rect.parent as RectTransform : null;
            if (rect == null || parent == null)
                return false;

            var canvas = rect.GetComponentInParent<Canvas>();
            var safeRect = HomeMenuViewportUtility.CalculateSafeRectInParent(parent, canvas);
            if (_margin > 0f)
                safeRect = HomeMenuViewportUtility.Shrink(safeRect, _margin);

            if (safeRect.width <= 1f || safeRect.height <= 1f)
                return false;

            if (_mode == LayoutMode.StretchToSafeArea)
                HomeMenuViewportUtility.StretchToRect(rect, safeRect);
            else
                ClampInside(rect, safeRect);

            RecordSignature(parent);
            return true;
        }

        private void OnEnable()
        {
            Invalidate();
            ApplyNow();
        }

        private void OnRectTransformDimensionsChange()
        {
            Invalidate();
        }

        private void LateUpdate()
        {
            if (SignatureChanged())
                ApplyNow();
        }

        private static HomeMenuScreenBoundsGuard Ensure(RectTransform rect, LayoutMode mode, float margin)
        {
            if (rect == null)
                return null;

            var guard = rect.GetComponent<HomeMenuScreenBoundsGuard>();
            if (guard == null)
                guard = rect.gameObject.AddComponent<HomeMenuScreenBoundsGuard>();

            guard.Configure(mode, margin);
            return guard;
        }

        private static void ClampInside(RectTransform rect, Rect safeRect)
        {
            var width = Mathf.Min(Mathf.Max(1f, rect.rect.width), safeRect.width);
            var height = Mathf.Min(Mathf.Max(1f, rect.rect.height), safeRect.height);

            if (rect.anchorMin == rect.anchorMax)
                rect.sizeDelta = new Vector2(width, height);

            var pos = rect.anchoredPosition;
            var left = pos.x - rect.pivot.x * width;
            var right = pos.x + (1f - rect.pivot.x) * width;
            var bottom = pos.y - rect.pivot.y * height;
            var top = pos.y + (1f - rect.pivot.y) * height;

            if (left < safeRect.xMin)
                pos.x += safeRect.xMin - left;
            if (right > safeRect.xMax)
                pos.x -= right - safeRect.xMax;
            if (bottom < safeRect.yMin)
                pos.y += safeRect.yMin - bottom;
            if (top > safeRect.yMax)
                pos.y -= top - safeRect.yMax;

            rect.anchoredPosition = pos;
        }

        private bool SignatureChanged()
        {
            var rect = transform as RectTransform;
            var parent = rect != null ? rect.parent as RectTransform : null;
            if (parent == null)
                return false;

            return _lastScreenWidth != Screen.width ||
                   _lastScreenHeight != Screen.height ||
                   _lastSafeArea != Screen.safeArea ||
                   Vector2.SqrMagnitude(parent.rect.size - _lastParentSize) > 0.25f;
        }

        private void RecordSignature(RectTransform parent)
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastSafeArea = Screen.safeArea;
            _lastParentSize = parent != null ? parent.rect.size : Vector2.zero;
        }

        private void Invalidate()
        {
            _lastScreenWidth = -1;
            _lastScreenHeight = -1;
            _lastSafeArea = new Rect(float.NaN, float.NaN, float.NaN, float.NaN);
            _lastParentSize = new Vector2(float.NaN, float.NaN);
        }
    }

    internal static class HomeMenuViewportUtility
    {
        public static string ResolveViewportClass(Vector2 size)
        {
            var width = size.x > 1f ? size.x : Screen.width;
            var height = size.y > 1f ? size.y : Screen.height;
            var shortest = Mathf.Min(width, height);
            var aspect = height > 1f ? width / height : 1f;
            var orientation = aspect < 0.9f ? " vp-portrait" : " vp-landscape";

            if (shortest <= 560f || aspect < 0.9f)
                return "vp-tiny" + orientation;
            if (width <= 1000f || height <= 640f)
                return "vp-compact" + orientation;
            if (width >= 1600f)
                return "vp-1080" + orientation;
            return "vp-720" + orientation;
        }

        public static Rect CalculateSafeRectInParent(RectTransform parent, Canvas canvas)
        {
            if (parent == null)
                return Rect.zero;

            var canvasRect = canvas != null ? canvas.transform as RectTransform : null;
            if (canvasRect == null)
                return parent.rect;

            var canvasSize = canvasRect.rect.size;
            if (canvasSize.x <= 1f || canvasSize.y <= 1f)
                return parent.rect;

            var safeRect = CalculateSafeRect(
                canvasSize,
                Screen.safeArea,
                new Vector2(Screen.width, Screen.height));

            var canvasBounds = canvasRect.rect;
            var bottomLeft = canvasRect.TransformPoint(new Vector3(
                canvasBounds.xMin + safeRect.xMin,
                canvasBounds.yMin + safeRect.yMin,
                0f));
            var topRight = canvasRect.TransformPoint(new Vector3(
                canvasBounds.xMin + safeRect.xMax,
                canvasBounds.yMin + safeRect.yMax,
                0f));

            var localBottomLeft = parent.InverseTransformPoint(bottomLeft);
            var localTopRight = parent.InverseTransformPoint(topRight);
            return Rect.MinMaxRect(
                Mathf.Min(localBottomLeft.x, localTopRight.x),
                Mathf.Min(localBottomLeft.y, localTopRight.y),
                Mathf.Max(localBottomLeft.x, localTopRight.x),
                Mathf.Max(localBottomLeft.y, localTopRight.y));
        }

        public static Rect CalculateSafeRect(Vector2 canvasSize, Rect safeArea, Vector2 screenSize)
        {
            if (canvasSize.x <= 1f || canvasSize.y <= 1f || screenSize.x <= 1f || screenSize.y <= 1f)
                return new Rect(0f, 0f, Mathf.Max(1f, canvasSize.x), Mathf.Max(1f, canvasSize.y));

            var xScale = canvasSize.x / screenSize.x;
            var yScale = canvasSize.y / screenSize.y;
            var xMin = Mathf.Clamp(safeArea.xMin * xScale, 0f, canvasSize.x);
            var yMin = Mathf.Clamp(safeArea.yMin * yScale, 0f, canvasSize.y);
            var xMax = Mathf.Clamp(safeArea.xMax * xScale, xMin, canvasSize.x);
            var yMax = Mathf.Clamp(safeArea.yMax * yScale, yMin, canvasSize.y);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static Rect Shrink(Rect rect, float margin)
        {
            if (margin <= 0f)
                return rect;

            var clampedX = Mathf.Min(margin, Mathf.Max(0f, rect.width * 0.5f - 1f));
            var clampedY = Mathf.Min(margin, Mathf.Max(0f, rect.height * 0.5f - 1f));
            return Rect.MinMaxRect(
                rect.xMin + clampedX,
                rect.yMin + clampedY,
                rect.xMax - clampedX,
                rect.yMax - clampedY);
        }

        public static void StretchToRect(RectTransform rect, Rect target)
        {
            if (rect == null || rect.parent is not RectTransform parent)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(
                target.xMin - parent.rect.xMin,
                target.yMin - parent.rect.yMin);
            rect.offsetMax = new Vector2(
                target.xMax - parent.rect.xMax,
                target.yMax - parent.rect.yMax);
            rect.localScale = Vector3.one;
        }
    }
}
