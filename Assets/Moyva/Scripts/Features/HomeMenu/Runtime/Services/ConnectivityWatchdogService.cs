using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Shared.Connectivity;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Стежить за станом інтернет-з'єднання та реагує на падіння при активному онлайн-режимі:
    /// показує InfoPanel і повертає користувача на стартову панель вибору типу гри.
    /// LAN/Offline режими не реагують (вони не залежать від інтернету).
    /// </summary>
    internal sealed class ConnectivityWatchdogService : IInitializable, IDisposable
    {
        private const string Prefix = "[ConnectivityWatchdog]";

        [Inject] private IConnectivityService _connectivity;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [InjectOptional] private INavigation _navigation;
        [InjectOptional] private IInfoPanelService _infoPanel;

        [Inject(Id = "MultiplayerTypePanelName", Optional = true)] private string _fallbackPanelName;

        private bool _wasOnline = true;
        private bool _subscribed;

        public void Initialize()
        {
            if (_connectivity == null) return;
            _wasOnline = _connectivity.IsOnline;
            _connectivity.StatusChanged += OnStatusChanged;
            _subscribed = true;
        }

        public void Dispose()
        {
            if (_subscribed && _connectivity != null)
            {
                _connectivity.StatusChanged -= OnStatusChanged;
                _subscribed = false;
            }
        }

        private void OnStatusChanged(bool isOnline)
        {
            try
            {
                if (isOnline == _wasOnline) return;
                _wasOnline = isOnline;

                if (isOnline)
                {
                    Debug.Log($"{Prefix} Connectivity restored.");
                    return;
                }

                // Інтернет зник — реагуємо лише, якщо поточний режим вимагає мережі.
                var mode = _modeSelector?.CurrentMode ?? NetworkProviderType.Offline;
                if (mode == NetworkProviderType.Offline || mode == NetworkProviderType.Lan)
                {
                    Debug.Log($"{Prefix} Connectivity lost, but mode={mode} — ignoring.");
                    return;
                }

                Debug.LogWarning($"{Prefix} Connectivity lost in mode={mode}; navigating to fallback panel.");

                if (!string.IsNullOrWhiteSpace(_fallbackPanelName) && _navigation != null)
                {
                    try { _navigation.OpenForce(_fallbackPanelName); }
                    catch (Exception navEx) { Debug.LogError($"{Prefix} OpenForce('{_fallbackPanelName}') failed: {navEx.Message}"); }
                }

                _infoPanel?.Show(new InfoMessage(
                    "З'єднання втрачено",
                    "Інтернет-з'єднання було перерване. Перевірте мережу та спробуйте знову."));
            }
            catch (Exception e)
            {
                Debug.LogError($"{Prefix} OnStatusChanged error: {e}");
            }
        }
    }
}
