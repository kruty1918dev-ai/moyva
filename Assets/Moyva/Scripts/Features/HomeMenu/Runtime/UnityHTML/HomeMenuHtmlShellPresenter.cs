using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using UnityHTML.Runtime;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class HomeMenuHtmlShellPresenter : IInitializable, IDisposable
    {
        private const string Prefix = "[HomeMenuHtmlShellPresenter]";

        private readonly HomeMenuConfigSO _config;
        private readonly INavigation _navigation;
        private readonly IConfirmationService _confirmationService;
        private readonly IUnityHtmlHost _host;
        private readonly HomeMenuHtmlShellAnchor[] _anchors;
        private HomeMenuHtmlShellAnchor _mountedAnchor;
        private bool _loggedFallback;

        public HomeMenuHtmlShellPresenter(
            HomeMenuConfigSO config,
            INavigation navigation,
            IConfirmationService confirmationService,
            IUnityHtmlHost host,
            [InjectOptional] HomeMenuHtmlShellAnchor[] anchors = null)
        {
            _config = config;
            _navigation = navigation;
            _confirmationService = confirmationService;
            _host = host;
            _anchors = anchors ?? Array.Empty<HomeMenuHtmlShellAnchor>();
        }

        public void Initialize()
        {
            var anchor = FindAnchor();
            if (_config == null || !_config.useUnityHtmlShell)
            {
                EnsureLegacyVisible(anchor);
                return;
            }

            if (anchor == null)
            {
                LogFallback("UnityHTML shell is enabled, but no HomeMenuHtmlShellAnchor exists in the scene.");
                return;
            }

            if (anchor.MountRoot == null)
            {
                LogFallback("UnityHTML shell mount root is not assigned.");
                EnsureLegacyVisible(anchor);
                return;
            }

            if (anchor.HtmlAsset == null)
            {
                LogFallback("UnityHTML shell HTML asset is not assigned.");
                EnsureLegacyVisible(anchor);
                return;
            }

            if (anchor.CssAsset == null || string.IsNullOrWhiteSpace(anchor.CssAsset.text))
            {
                LogFallback("UnityHTML shell CSS asset is missing or empty.");
                EnsureLegacyVisible(anchor);
                return;
            }

            anchor.PrepareForMount();
            var document = UnityHtmlDocument.FromTextAssets(anchor.HtmlAsset, anchor.CssAsset, "HomeMenuShell");
            var bridge = new HomeMenuHtmlMenuBridge(_navigation, _confirmationService);
            var globals = new Dictionary<string, object>
            {
                ["moyvaMenu"] = bridge
            };

            if (anchor.FontAsset != null)
                globals["moyvaFont"] = anchor.FontAsset;

            var result = _host.Mount(anchor.MountRoot, document, globals);

            if (!result.Succeeded)
            {
                LogFallback(result.ErrorMessage);
                EnsureLegacyVisible(anchor);
                return;
            }

            _mountedAnchor = anchor;
            _navigation.OnMenuChanged += HandleMenuChanged;
            UpdateShellVisibility();
        }

        public void Dispose()
        {
            _navigation.OnMenuChanged -= HandleMenuChanged;
            EnsureLegacyVisible(_mountedAnchor);
            _mountedAnchor = null;
            _host?.Dispose();
        }

        private void HandleMenuChanged(NavigationChangeEventArgs _) => UpdateShellVisibility();

        private void UpdateShellVisibility()
        {
            if (_mountedAnchor == null)
                return;

            var showHtmlShell = string.IsNullOrWhiteSpace(_navigation.CurrentMenu);
            _mountedAnchor.SetHtmlShellVisible(showHtmlShell);

            if (_mountedAnchor.LegacyShell != null)
                _mountedAnchor.LegacyShell.SetActive(!showHtmlShell);
        }

        private HomeMenuHtmlShellAnchor FindAnchor()
        {
            foreach (var anchor in _anchors)
            {
                if (anchor != null)
                    return anchor;
            }

            return null;
        }

        private static void EnsureLegacyVisible(HomeMenuHtmlShellAnchor anchor)
        {
            if (anchor == null)
                return;

            anchor.SetHtmlShellVisible(false);
            if (anchor.LegacyShell != null)
                anchor.LegacyShell.SetActive(true);
        }

        private void LogFallback(string reason)
        {
            if (_loggedFallback)
                return;

            _loggedFallback = true;
            Debug.LogError($"{Prefix} Falling back to legacy UGUI shell. {reason}");
        }
    }
}
