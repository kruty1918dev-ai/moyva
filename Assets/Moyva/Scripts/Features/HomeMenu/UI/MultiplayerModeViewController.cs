using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контролер вибору мережевого режиму (Relay/LAN/WebSocket/Offline).
    /// Синхронізує dropdown/toggle UI та подію <see cref="OnModeChanged"/>.
    /// </summary>
    public sealed class MultiplayerModeViewController : MonoBehaviour, IMultiplayerModeViewController, IInitializable
    {
        private static readonly NetworkProviderType[] ModeOrder =
        {
            NetworkProviderType.Relay,
            NetworkProviderType.Lan,
            NetworkProviderType.WebSocket,
            NetworkProviderType.Offline
        };

        [SerializeField] private TMP_Dropdown _modeDropdown;
        [SerializeField] private Toggle _relayToggle;
        [SerializeField] private Toggle _lanToggle;
        [SerializeField] private Toggle _webSocketToggle;
        [SerializeField] private Toggle _offlineToggle;

        private NetworkProviderType _selectedMode = NetworkProviderType.Relay;
        private bool _isInitialized;

        public event Action<NetworkProviderType> OnModeChanged;

        public NetworkProviderType SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (_selectedMode == value)
                    return;

                _selectedMode = value;
                Refresh();
            }
        }

        public void Initialize()
        {
            InitializeView();
        }

        private void Awake()
        {
            InitializeView();
        }

        private void InitializeView()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            if (_modeDropdown != null)
            {
                ConfigureDropdown();
                _modeDropdown.onValueChanged.AddListener(OnDropdownModeChanged);
            }

            AddToggleListener(_relayToggle, NetworkProviderType.Relay);
            AddToggleListener(_lanToggle, NetworkProviderType.Lan);
            AddToggleListener(_webSocketToggle, NetworkProviderType.WebSocket);
            AddToggleListener(_offlineToggle, NetworkProviderType.Offline);

            Refresh();
        }

        private void OnDestroy()
        {
            if (!_isInitialized)
                return;

            _isInitialized = false;

            if (_modeDropdown != null)
                _modeDropdown.onValueChanged.RemoveListener(OnDropdownModeChanged);

            RemoveToggleListener(_relayToggle, OnRelayToggleChanged);
            RemoveToggleListener(_lanToggle, OnLanToggleChanged);
            RemoveToggleListener(_webSocketToggle, OnWebSocketToggleChanged);
            RemoveToggleListener(_offlineToggle, OnOfflineToggleChanged);
        }

        public void Refresh()
        {
            if (_modeDropdown != null)
                _modeDropdown.SetValueWithoutNotify(GetModeIndex(_selectedMode));

            SetToggleWithoutNotify(_relayToggle, _selectedMode == NetworkProviderType.Relay);
            SetToggleWithoutNotify(_lanToggle, _selectedMode == NetworkProviderType.Lan);
            SetToggleWithoutNotify(_webSocketToggle, _selectedMode == NetworkProviderType.WebSocket);
            SetToggleWithoutNotify(_offlineToggle, _selectedMode == NetworkProviderType.Offline);
        }

        private void ConfigureDropdown()
        {
            _modeDropdown.ClearOptions();
            _modeDropdown.AddOptions(new List<string>
            {
                "Global (Relay)",
                "Local LAN",
                "Global (WebSocket)",
                "Offline"
            });
        }

        private void OnDropdownModeChanged(int index)
        {
            if (index < 0 || index >= ModeOrder.Length)
                return;

            SelectMode(ModeOrder[index]);
        }

        private void AddToggleListener(Toggle toggle, NetworkProviderType mode)
        {
            if (toggle == null)
                return;

            switch (mode)
            {
                case NetworkProviderType.Relay:
                    toggle.onValueChanged.AddListener(OnRelayToggleChanged);
                    break;
                case NetworkProviderType.Lan:
                    toggle.onValueChanged.AddListener(OnLanToggleChanged);
                    break;
                case NetworkProviderType.WebSocket:
                    toggle.onValueChanged.AddListener(OnWebSocketToggleChanged);
                    break;
                case NetworkProviderType.Offline:
                    toggle.onValueChanged.AddListener(OnOfflineToggleChanged);
                    break;
            }
        }

        private static void RemoveToggleListener(Toggle toggle, UnityEngine.Events.UnityAction<bool> listener)
        {
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(listener);
        }

        private void OnRelayToggleChanged(bool isOn)
        {
            if (isOn)
                SelectMode(NetworkProviderType.Relay);
        }

        private void OnLanToggleChanged(bool isOn)
        {
            if (isOn)
                SelectMode(NetworkProviderType.Lan);
        }

        private void OnWebSocketToggleChanged(bool isOn)
        {
            if (isOn)
                SelectMode(NetworkProviderType.WebSocket);
        }

        private void OnOfflineToggleChanged(bool isOn)
        {
            if (isOn)
                SelectMode(NetworkProviderType.Offline);
        }

        private void SelectMode(NetworkProviderType mode)
        {
            // 1: Ігноруємо повторний вибір того ж режиму.
            if (_selectedMode == mode)
                return;

            // 2: Фіксуємо новий режим, оновлюємо UI і повідомляємо підписників.
            _selectedMode = mode;
            Refresh();
            OnModeChanged?.Invoke(mode);
        }

        private static int GetModeIndex(NetworkProviderType mode)
        {
            for (int i = 0; i < ModeOrder.Length; i++)
            {
                if (ModeOrder[i] == mode)
                    return i;
            }

            return 0;
        }

        private static void SetToggleWithoutNotify(Toggle toggle, bool value)
        {
            if (toggle != null)
                toggle.SetIsOnWithoutNotify(value);
        }
    }
}