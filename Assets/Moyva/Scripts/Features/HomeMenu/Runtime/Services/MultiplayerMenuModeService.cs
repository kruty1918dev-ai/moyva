using System;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    public sealed class MultiplayerMenuModeService : IInitializable, IDisposable
    {
        private INavigation _navigation;
        private IMultiplayerModeSelector _modeSelector;

        [Inject]
        public void Construct(
            INavigation navigation,
            [InjectOptional] IMultiplayerModeSelector modeSelector = null)
        {
            _navigation = navigation;
            _modeSelector = modeSelector;
        }

        public void Initialize()
        {
            if (_navigation == null)
                return;

            _navigation.OnMenuChanged -= OnMenuChanged;
            _navigation.OnMenuChanged += OnMenuChanged;
            _ = ApplyModeForMenuAsync(_navigation.CurrentMenu);
        }

        public void Dispose()
        {
            if (_navigation != null)
                _navigation.OnMenuChanged -= OnMenuChanged;
        }

        public Task ApplyModeForNavigationAsync(string menuToOpen, string currentMenu)
        {
            if (TryResolveMode(menuToOpen, out var targetMode))
                return ApplyModeAsync(targetMode, menuToOpen);

            if (TryResolveMode(currentMenu, out var currentMode))
                return ApplyModeAsync(currentMode, currentMenu);

            return Task.CompletedTask;
        }

        public Task ApplyModeForMenuAsync(string menuName)
        {
            return TryResolveMode(menuName, out var mode)
                ? ApplyModeAsync(mode, menuName)
                : Task.CompletedTask;
        }

        private async void OnMenuChanged(NavigationChangeEventArgs args)
        {
            try
            {
                await ApplyModeForMenuAsync(args.CurrentMenu);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MultiplayerMenuModeService] Failed to apply multiplayer mode for menu '{args.CurrentMenu}': {e.Message}");
                Debug.LogException(e);
            }
        }

        private async Task ApplyModeAsync(NetworkProviderType mode, string menuName)
        {
            if (_modeSelector == null)
                return;

            var effectiveBefore = _modeSelector.EffectiveMode;
            if (_modeSelector.CurrentMode == mode && effectiveBefore == mode)
                return;

            await _modeSelector.SetModeAsync(mode);
            if (_modeSelector.CurrentMode != mode || _modeSelector.EffectiveMode != mode)
            {
                Debug.LogWarning($"[MultiplayerMenuModeService] Menu '{menuName}' requested {mode} multiplayer mode but effective mode is {_modeSelector.EffectiveMode}.");
                return;
            }

            Debug.Log($"[MultiplayerMenuModeService] Menu '{menuName}' selected {mode} multiplayer mode.");
        }

        private static bool TryResolveMode(string menuName, out NetworkProviderType mode)
        {
            mode = NetworkProviderType.Relay;

            if (string.IsNullOrWhiteSpace(menuName))
                return false;

            var normalized = menuName.Trim();
            if (normalized.IndexOf("LocalMultiplayer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                normalized.IndexOf("LanMultiplayer", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                mode = NetworkProviderType.Lan;
                return true;
            }

            if (normalized.IndexOf("GlobalMultiplayer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                normalized.IndexOf("RelayMultiplayer", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                mode = NetworkProviderType.Relay;
                return true;
            }

            return false;
        }
    }
}