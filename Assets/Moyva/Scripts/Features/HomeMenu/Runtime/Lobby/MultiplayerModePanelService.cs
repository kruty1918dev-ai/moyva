using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    internal sealed class MultiplayerModePanelService : IInitializable, IDisposable
    {
        [InjectOptional] private List<IMultiplayerModeViewController> _viewControllers = new List<IMultiplayerModeViewController>();
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;

        public void Initialize()
        {
            if (_modeSelector == null || _viewControllers == null)
                return;

            foreach (var viewController in _viewControllers)
            {
                if (viewController == null)
                    continue;

                viewController.SelectedMode = _modeSelector.CurrentMode;
                viewController.OnModeChanged -= OnViewModeChanged;
                viewController.OnModeChanged += OnViewModeChanged;
                viewController.Refresh();
            }

            _modeSelector.OnModeChanged -= OnSelectorModeChanged;
            _modeSelector.OnModeChanged += OnSelectorModeChanged;
        }

        public void Dispose()
        {
            if (_viewControllers != null)
            {
                foreach (var viewController in _viewControllers)
                {
                    if (viewController != null)
                        viewController.OnModeChanged -= OnViewModeChanged;
                }
            }

            if (_modeSelector != null)
                _modeSelector.OnModeChanged -= OnSelectorModeChanged;
        }

        private async void OnViewModeChanged(NetworkProviderType mode)
        {
            if (_modeSelector == null)
                return;

            try
            {
                await _modeSelector.SetModeAsync(mode);
            }
            catch (Exception ex)
            {
                Debug.LogError($"MultiplayerModePanelService: failed to switch multiplayer mode to {mode}: {ex.Message}");
                Debug.LogException(ex);
                OnSelectorModeChanged(_modeSelector.CurrentMode);
            }
        }

        private void OnSelectorModeChanged(NetworkProviderType mode)
        {
            MainThreadDispatcher.Enqueue(() => SetModeWithoutNotify(mode));
        }

        private void SetModeWithoutNotify(NetworkProviderType mode)
        {
            if (_viewControllers == null)
                return;

            foreach (var viewController in _viewControllers)
            {
                if (viewController == null)
                    continue;

                viewController.SelectedMode = mode;
                viewController.Refresh();
            }
        }
    }
}