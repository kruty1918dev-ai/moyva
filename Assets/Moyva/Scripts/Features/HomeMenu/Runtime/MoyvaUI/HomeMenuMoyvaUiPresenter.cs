using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Shared.Localization;
using UnityEngine;
using UnityHTML.Runtime;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
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

        private HomeMenuMoyvaUiAnchor _mountedAnchor;
        private string _lastViewportClass = string.Empty;
        private string _mountedViewportClass = string.Empty;
        private string _lastRouteMarkup;
        private string _lastBrandMarkup;
        private string _lastModalsMarkup;
        private bool _loggedFallback;
        private bool _initialized;
        private int _lastStateChangeFrame = -1;

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

        public void Initialize()
        {
            _initialized = true;
            var anchor = FindAnchor();
            anchor?.SetLegacyUiVisible(false);
            if (_config == null || !_config.useUnityHtmlShell)
                return;

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

            _state.Changed += HandleStateChanged;
            _navigation.OnMenuChanged += HandleMenuChanged;
            RenderIfNeeded(force: true);
        }

        public void Tick()
        {
            if (!_initialized || _mountedAnchor == null || _state.IsFallback)
                return;

            _view.Controls.Tick();
            var viewportClass = _mountedAnchor.CurrentViewportClass;
            if (!string.Equals(_lastViewportClass, viewportClass, StringComparison.Ordinal))
            {
                _lastViewportClass = viewportClass;
                _state.MarkDirty();
            }

            if (_state.IsInteractionActive || _lastStateChangeFrame == Time.frameCount)
                return;

            RenderIfNeeded(force: false);
        }

        public void Dispose()
        {
            _view.Controls.CancelCapture();
            _state.Changed -= HandleStateChanged;

            if (_navigation != null)
                _navigation.OnMenuChanged -= HandleMenuChanged;

            if (_mountedAnchor != null)
            {
                _mountedAnchor.SetMoyvaUiVisible(false);
                _mountedAnchor.SetLegacyUiVisible(false);
            }

            _mountedAnchor = null;
            _host?.Dispose();
        }

        private void HandleStateChanged() => _lastStateChangeFrame = Time.frameCount;

        private void HandleMenuChanged(NavigationChangeEventArgs _) => _state.MarkDirty();

        private void RenderIfNeeded(bool force)
        {
            if (_mountedAnchor == null || _state.IsFallback)
                return;

            if (!force && !_state.ConsumeDirty())
                return;
            if (force)
                _state.ConsumeDirty();

            var viewportClass = _mountedAnchor.CurrentViewportClass;
            _lastViewportClass = viewportClass;
            var globals = new Dictionary<string, object>
            {
                ["moyvaMenu"] = _bridge
            };

            if (_mountedAnchor.FontAsset != null)
                globals["moyvaFont"] = _mountedAnchor.FontAsset;

            if (!force && TryApplyRegionalUpdate(viewportClass, globals))
                return;

            var route = HomeMenuMoyvaUiMarkup.BuildRouteMarkup(_state, _view);
            var brand = HomeMenuMoyvaUiMarkup.BuildBrandMarkup(_view);
            var html = HomeMenuMoyvaUiMarkup.Build(_state, _view, viewportClass);
            var css = _mountedAnchor.CssAsset != null ? _mountedAnchor.CssAsset.text : string.Empty;
            var document = new UnityHtmlDocument(html, css, "MoyvaUI HomeMenu");

            UnityHtmlMountResult result;
            using (HomeMenuUiPerformanceMetrics.HtmlMountMarker.Auto())
            {
                _mountedAnchor.PrepareForMount();
                HomeMenuUiPerformanceMetrics.RecordHtmlMount();
                result = _host.Mount(_mountedAnchor.MountRoot, document, globals);
            }

            if (!result.Succeeded)
            {
                Fallback(result.ErrorMessage);
                return;
            }

            _state.IsMounted = true;
            _mountedViewportClass = viewportClass;
            _lastRouteMarkup = route;
            _lastBrandMarkup = brand;
            _lastModalsMarkup = HomeMenuMoyvaUiMarkup.BuildModalsMarkup(_view);
            _mountedAnchor.SetMoyvaUiVisible(true);
            _mountedAnchor.SetLegacyUiVisible(false);
        }

        private bool TryApplyRegionalUpdate(string viewportClass, IReadOnlyDictionary<string, object> globals)
        {
            if (!_state.IsMounted ||
                !string.Equals(viewportClass, _mountedViewportClass, StringComparison.Ordinal))
                return false;

            var modals = HomeMenuMoyvaUiMarkup.BuildModalsMarkup(_view);
            if (!string.Equals(modals, _lastModalsMarkup, StringComparison.Ordinal))
                return false;

            var route = HomeMenuMoyvaUiMarkup.BuildRouteMarkup(_state, _view);
            var brand = HomeMenuMoyvaUiMarkup.BuildBrandMarkup(_view);
            if (string.Equals(route, _lastRouteMarkup, StringComparison.Ordinal) &&
                string.Equals(brand, _lastBrandMarkup, StringComparison.Ordinal))
            {
                _lastModalsMarkup = modals;
                return true;
            }

            var regions = new Dictionary<string, string>
            {
                [HomeMenuMoyvaUiMarkup.NavRegionId] = route,
                [HomeMenuMoyvaUiMarkup.BrandRegionId] = brand
            };
            if (!_host.UpdateRegions(regions, globals))
                return false;

            _lastRouteMarkup = route;
            _lastBrandMarkup = brand;
            _lastModalsMarkup = modals;
            return true;
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
