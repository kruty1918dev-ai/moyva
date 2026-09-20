using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Shared.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityHTML.Runtime;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>HomeMenuMoyvaUiPresenter — class: головного меню Moyva UI презентера.</summary>
    internal sealed class HomeMenuMoyvaUiPresenter : IInitializable, ITickable, IDisposable
    {
        private const string Prefix = "[HomeMenuMoyvaUI]";

        private readonly HomeMenuConfigSO _config;
        private readonly INavigation _navigation;
        private readonly IConfirmationService _confirmationService;
        private readonly IUnityHtmlHost _host;
        private readonly HomeMenuMoyvaUiState _state;
        private readonly HomeMenuMoyvaUiViewController _view;
        private readonly HomeMenuMoyvaUiBridge _bridge;
        private readonly HomeMenuMoyvaUiAnchor[] _anchors;
        private readonly ILocalizationService _localization;
        private readonly LocalizationFontService _localizationFonts;

        private const float RouteExitSeconds = 0.12f;
        private const float RouteEnterSeconds = 0.2f;
        private const string RouteRootId = HomeMenuMoyvaUiMarkup.NavRegionId;

        private HomeMenuMoyvaUiAnchor _mountedAnchor;
        private string _lastViewportClass = string.Empty;
        private bool _loggedFallback;
        private bool _initialized;

        /// <summary>The snapshot actually committed to the DOM; null before first mount.</summary>
        private HomeMenuUiSnapshot _mounted;

        private HomeMenuRenderPhase _phase = HomeMenuRenderPhase.Stable;
        private float _phaseDeadline = -1f;

        /// <summary>Time seam for EditMode tests; production uses unscaled time.</summary>
        internal Func<float> TimeNow = () => Time.unscaledTime;

        /// <summary>Тестовий seam: місток, який отримує всі callbacks з markup (Globals.moyvaMenu).</summary>
        internal HomeMenuMoyvaUiBridge Bridge => _bridge;

        /// <summary>Current transition phase (test seam).</summary>
        internal HomeMenuRenderPhase Phase => _phase;

        /// <summary>Виконує HomeMenuMoyvaUiPresenter.</summary>
        public HomeMenuMoyvaUiPresenter(
            HomeMenuConfigSO config,
            INavigation navigation,
            IConfirmationService confirmationService,
            IUnityHtmlHost host,
            HomeMenuMoyvaUiState state,
            HomeMenuMoyvaUiViewController view,
            [InjectOptional] ILobbyFlowContext lobbyFlowContext = null,
            [InjectOptional] HomeMenuMoyvaUiAnchor[] anchors = null,
            [InjectOptional] ILocalizationService localization = null,
            [InjectOptional] LocalizationFontService localizationFonts = null)
        {
            _config = config;
            _navigation = navigation;
            _confirmationService = confirmationService;
            _host = host;
            _state = state;
            _view = view;
            _bridge = new HomeMenuMoyvaUiBridge(_navigation, _confirmationService, _view, lobbyFlowContext, _state);
            _anchors = anchors ?? Array.Empty<HomeMenuMoyvaUiAnchor>();
            _localization = localization;
            _localizationFonts = localizationFonts;
        }

        /// <summary>Ініціалізує компонент і підписує на події.</summary>
        public void Initialize()
        {
            _initialized = true;
            if (_config == null || !_config.useUnityHtmlShell)
                return;

            var anchor = FindAnchor();

            if (!CanMount(anchor))
                return;

            _mountedAnchor = anchor;
            _mountedAnchor.StopEditorPreview();
            _mountedAnchor.PrepareForMount();
            _mountedAnchor.SetLegacyUiVisible(false);

            // Multilingual fallback chain + glyph warmup must exist before the first mount,
            // otherwise persisted non-Latin languages render as missing glyphs on frame one.
            _localizationFonts?.RegisterPrimaryFont(_mountedAnchor.FontAsset);
            if (_localization != null)
                _localizationFonts?.Warmup(_localization.CurrentLanguage);

            _navigation.OnMenuChanged += HandleMenuChanged;
            if (_localization != null)
                _localization.LanguageChanged += HandleLanguageChanged;

            RenderIfNeeded();
        }

        /// <summary>Оновлює стан за тік.</summary>
        public void Tick()
        {
            if (!_initialized || _mountedAnchor == null || _state.IsFallback)
                return;

            _view.Controls.Tick();
            HandleEscapeInput();

            // A pending route exit always completes on the latest desired state —
            // never on whatever was requested when the fade started.
            if (_phase == HomeMenuRenderPhase.ExitingRoute && TimeNow() >= _phaseDeadline)
            {
                _phase = HomeMenuRenderPhase.Stable;
                _phaseDeadline = -1f;
                Apply(desired: Capture(), opOverride: null, routeExitPlayed: true);
            }

            var viewportClass = _mountedAnchor.CurrentViewportClass;
            if (!string.Equals(_lastViewportClass, viewportClass, StringComparison.Ordinal))
            {
                _lastViewportClass = viewportClass;
                _state.MarkDirty();
            }

            if (_phase != HomeMenuRenderPhase.Stable || _state.IsInteractionActive)
                return;

            RenderIfNeeded();
        }

        /// <summary>Звільняє ресурси та відписує від подій.</summary>
        public void Dispose()
        {
            _view.Controls.CancelCapture();

            if (_navigation != null)
                _navigation.OnMenuChanged -= HandleMenuChanged;

            if (_localization != null)
                _localization.LanguageChanged -= HandleLanguageChanged;

            if (_mountedAnchor != null)
            {
                _mountedAnchor.SetMoyvaUiVisible(false);
                _mountedAnchor.SetLegacyUiVisible(false);
            }

            _mountedAnchor = null;
            _phase = HomeMenuRenderPhase.Stable;
            _phaseDeadline = -1f;
            _host?.Dispose();
        }

        private void HandleMenuChanged(NavigationChangeEventArgs _) => _state.MarkDirty();

        private void HandleLanguageChanged()
        {
            // Localization changes flow through the same snapshot diff as everything
            // else: text differences land via a region update, and the host escalates
            // to a fresh context only if reconciliation provably fails (e.g. pooled
            // TMP components holding destroyed font references). No flag, no timing.
            _state.MarkDirty();
        }

        private HomeMenuUiSnapshot Capture()
            => HomeMenuUiSnapshot.Capture(_state, _view, _mountedAnchor.CurrentViewportClass);

        private void RenderIfNeeded()
        {
            var desired = Capture();
            var op = HomeMenuRenderPlanner.ChooseOperation(_mounted, desired);
            if (op == HomeMenuRenderOperation.None)
            {
                _state.ConsumeDirty();
                return;
            }

            var routeChanged = _mounted != null && !desired.RouteEquals(_mounted);
            if (_mounted != null && routeChanged && !_state.ReducedMotion)
            {
                // Arm the route exit once; completion happens in Tick so that rapid
                // navigation inside the exit window collapses into a single swap.
                _phase = HomeMenuRenderPhase.ExitingRoute;
                _phaseDeadline = TimeNow() + RouteExitSeconds;
                _host.Motion?.Play(RouteRootId, "fade-out", RouteExitSeconds, 0f);
                return;
            }

            Apply(desired, op, routeExitPlayed: false);
        }

        private void Apply(HomeMenuUiSnapshot desired, HomeMenuRenderOperation? opOverride, bool routeExitPlayed)
        {
            var previousRoute = _mounted != null ? _mounted.Route : string.Empty;
            var op = opOverride ?? HomeMenuRenderPlanner.ChooseOperation(_mounted, desired);

            // op == None means the DOM already matches (e.g. the route reverted
            // inside the exit window); the commit + enter-motion still must run.
            var applied = op == HomeMenuRenderOperation.None;
            if (op == HomeMenuRenderOperation.UpdateRegions)
            {
                applied = TryUpdateRegions(desired);
                if (!applied)
                    op = HomeMenuRenderOperation.MountDocument;
            }

            if (op == HomeMenuRenderOperation.MountDocument)
                applied = TryMountDocument(desired);

            if (!applied)
            {
                Fallback("Document update could not be applied.");
                return;
            }

            // Commit only after the DOM actually changed: _mounted always describes
            // the rendered document, never a pending intent.
            _mounted = desired;
            _state.IsMounted = true;
            _state.ConsumeDirty();
            _mountedAnchor.SetMoyvaUiVisible(true);
            _mountedAnchor.SetLegacyUiVisible(false);
            PlayRouteEnter(previousRoute, desired.Route, routeExitPlayed);
        }

        private void PlayRouteEnter(string previousRoute, string currentRoute, bool routeExitPlayed)
        {
            var routeChanged = !string.Equals(previousRoute, currentRoute, StringComparison.Ordinal);
            // When an exit ran but the swap landed back on the same route (rapid
            // back-and-forth inside the exit window), still fade back in: the panel
            // is mid-fade and would otherwise stay transparent.
            if (!routeChanged && !routeExitPlayed)
                return;
            if (!_state.ReducedMotion)
                _host.Motion?.Play(RouteRootId, "fade", RouteEnterSeconds, 0f);
            else
                _host.Motion?.RestoreResting(RouteRootId);
        }

        private void HandleEscapeInput()
        {
            if (!_state.IsMounted)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
                return;

            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected != null && selected.GetComponentInParent<TMP_InputField>() != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                return;
            }

            _bridge.HandleEscape();
        }

        private bool TryUpdateRegions(HomeMenuUiSnapshot desired)
        {
            var regions = new Dictionary<string, string>
            {
                [HomeMenuMoyvaUiMarkup.NavRegionId] = desired.NavMarkup,
                [HomeMenuMoyvaUiMarkup.BrandRegionId] = desired.BrandMarkup
            };

            try
            {
                return _host.UpdateRegions(regions, BuildGlobals());
            }
            catch (Exception exception)
            {
                // A reconcile that throws may have already removed children;
                // escalate to a document mount instead of leaving a half-swapped,
                // invisible region.
                Debug.LogWarning($"{Prefix} Region update failed ({exception.GetBaseException().Message}); remounting document.");
                return false;
            }
        }

        private bool TryMountDocument(HomeMenuUiSnapshot desired)
        {
            var css = _mountedAnchor.CssAsset != null ? _mountedAnchor.CssAsset.text : string.Empty;
            var html = HomeMenuMoyvaUiMarkup.Build(_state, _view, desired.ViewportClass);
            var document = new UnityHtmlDocument(html, css, "MoyvaUI HomeMenu");

            UnityHtmlMountResult result;
            using (HomeMenuUiPerformanceMetrics.HtmlMountMarker.Auto())
            {
                _mountedAnchor.PrepareForMount();
                HomeMenuUiPerformanceMetrics.RecordHtmlMount();
                result = _host.Mount(_mountedAnchor.MountRoot, document, BuildGlobals());
            }

            if (!result.Succeeded)
            {
                Fallback(result.ErrorMessage);
                return false;
            }

            return true;
        }

        private Dictionary<string, object> BuildGlobals()
        {
            var globals = new Dictionary<string, object>
            {
                ["moyvaMenu"] = _bridge
            };

            if (_mountedAnchor.FontAsset != null)
                globals["moyvaFont"] = _mountedAnchor.FontAsset;

            return globals;
        }

        private bool CanMount(HomeMenuMoyvaUiAnchor anchor)
        {
            if (anchor == null)
                return Fallback("MoyvaUI is enabled, but no HomeMenuMoyvaUiAnchor exists in the scene.");
            if (anchor.MountRoot == null)
                return Fallback("MoyvaUI mount root is not assigned.");
            if (anchor.CssAsset == null || string.IsNullOrWhiteSpace(anchor.CssAsset.text))
                return Fallback("MoyvaUI CSS asset is missing or empty.");
            return true;
        }

        private bool Fallback(string reason)
        {
            _state.IsFallback = true;
            _phase = HomeMenuRenderPhase.Stable;
            _phaseDeadline = -1f;
            _host?.Unmount();
            var anchor = _mountedAnchor ?? FindAnchor();
            anchor?.SetMoyvaUiVisible(false);
            anchor?.SetLegacyUiVisible(false);
            if (!_loggedFallback)
            {
                _loggedFallback = true;
                Debug.LogError($"{Prefix} UnityHTML mount failed and legacy UGUI is disabled. {reason}");
            }

            return false;
        }

        private HomeMenuMoyvaUiAnchor FindAnchor()
        {
            foreach (var anchor in _anchors)
            {
                if (anchor != null)
                    return anchor;
            }

            return null;
        }

    }
}
