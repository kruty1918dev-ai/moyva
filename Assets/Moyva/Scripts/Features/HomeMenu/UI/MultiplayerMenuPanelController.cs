using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Runtime controller for multiplayer blocks in HomeMenu scene:
    /// - updates room max-players label while slider changes
    /// - fills list of available local game slots
    /// - handles create/join button actions
    /// </summary>
    public sealed class MultiplayerMenuPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject createRoomPanel;
        [SerializeField] private GameObject joinRoomPanel;
        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private TMP_InputField roomPasswordInput;
        [SerializeField] private TMP_InputField joinCodeInput;
        [SerializeField] private Slider maxPlayersSlider;
        [SerializeField] private TMP_Text maxPlayersValueLabel;
        [SerializeField] private Button createNextButton;
        [SerializeField] private Button joinByCodeButton;
        [SerializeField] private RectTransform roomsListContent;

        private readonly List<Button> _roomButtons = new();
        private readonly List<LobbyRoom> _lastLobbies = new();

        private ISessionManager _sessionManager;
        private ILobbyService _lobbyService;
        private IMultiplayerIdentityService _identityService;
        private ISaveService _saveService;
        private MultiplayerConfig _multiplayerConfig;
        private HomeMenuNavigationController _navigation;
        private CancellationTokenSource _refreshCts;

        private void Awake()
        {
            AutoFindReferences();
            ResolveDependencies();
            HookUi();
            RefreshMaxPlayersLabel();
            _ = RefreshRoomsListAsync();
        }

        private void OnEnable()
        {
            RefreshMaxPlayersLabel();
            _ = RefreshRoomsListAsync();
        }

        private void OnDestroy()
        {
            if (maxPlayersSlider != null)
                maxPlayersSlider.onValueChanged.RemoveListener(OnMaxPlayersChanged);

            _refreshCts?.Cancel();
            _refreshCts?.Dispose();
        }

        private void AutoFindReferences()
        {
            if (createRoomPanel == null)
                createRoomPanel = GameObject.Find("CreateRoomPanel");

            if (joinRoomPanel == null)
                joinRoomPanel = GameObject.Find("JoinRoomPanel");

            if (roomNameInput == null)
                roomNameInput = FindByName<TMP_InputField>("Input_Room_Name");

            if (roomPasswordInput == null)
                roomPasswordInput = FindByName<TMP_InputField>("Input_Room_Password");

            if (joinCodeInput == null)
                joinCodeInput = FindByName<TMP_InputField>("Input_Join_Code");

            if (maxPlayersSlider == null)
                maxPlayersSlider = FindByName<Slider>("Slider_Room_MaxPlayers");

            if (maxPlayersValueLabel == null)
                maxPlayersValueLabel = FindByNameInParent<TMP_Text>("Slider_Room_MaxPlayers", "Val");

            if (createNextButton == null)
                createNextButton = FindByNameInParent<Button>("CreateRoomPanel", "NextBtn");

            if (joinByCodeButton == null)
                joinByCodeButton = FindByName<Button>("Join_ByCodeBtn");

            if (roomsListContent == null)
                roomsListContent = FindByPath<RectTransform>("RoomsScroll/Viewport/Content");

            if (_navigation == null)
                _navigation = FindFirstObjectByType<HomeMenuNavigationController>();
        }

        private void ResolveDependencies()
        {
            try
            {
                var projectContainer = ProjectContext.Instance.Container;
                _sessionManager = projectContainer.TryResolve<ISessionManager>();
                _lobbyService = projectContainer.TryResolve<ILobbyService>();
                _identityService = projectContainer.TryResolve<IMultiplayerIdentityService>();
                _saveService = projectContainer.TryResolve<ISaveService>();
                _multiplayerConfig = projectContainer.TryResolve<MultiplayerConfig>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MultiplayerMenuPanelController] Cannot resolve project services: {e.Message}", this);
            }
        }

        private void HookUi()
        {
            if (maxPlayersSlider != null)
            {
                maxPlayersSlider.onValueChanged.RemoveListener(OnMaxPlayersChanged);
                maxPlayersSlider.onValueChanged.AddListener(OnMaxPlayersChanged);
            }

            if (createNextButton != null)
            {
                createNextButton.onClick.RemoveAllListeners();
                createNextButton.onClick.AddListener(() => _ = CreateRoomAndStartAsync());
            }

            if (joinByCodeButton != null)
            {
                joinByCodeButton.onClick.RemoveAllListeners();
                joinByCodeButton.onClick.AddListener(() => _ = JoinRoomAndStartAsync());
            }
        }

        private void OnMaxPlayersChanged(float _)
        {
            RefreshMaxPlayersLabel();
        }

        private void RefreshMaxPlayersLabel()
        {
            if (maxPlayersValueLabel == null || maxPlayersSlider == null)
                return;

            maxPlayersValueLabel.text = Mathf.RoundToInt(maxPlayersSlider.value).ToString();
        }

        private async Task RefreshRoomsListAsync()
        {
            if (roomsListContent == null)
                return;

            _refreshCts?.Cancel();
            _refreshCts?.Dispose();
            _refreshCts = new CancellationTokenSource();
            var ct = _refreshCts.Token;

            _roomButtons.Clear();
            _lastLobbies.Clear();

            IReadOnlyList<LobbyRoom> lobbies = Array.Empty<LobbyRoom>();
            if (_lobbyService != null)
            {
                try { lobbies = await _lobbyService.QueryRoomsAsync(ct); }
                catch (Exception e) { Debug.LogWarning($"[MultiplayerMenuPanelController] QueryRooms failed: {e.Message}", this); }
            }
            if (ct.IsCancellationRequested) return;
            _lastLobbies.AddRange(lobbies);

            int lobbyIndex = 0;
            for (int i = 0; i < roomsListContent.childCount; i++)
            {
                var row = roomsListContent.GetChild(i);
                var nameLabel = FindChildTextByName(row, "Name");
                if (nameLabel == null) continue;

                LobbyRoom lobby = lobbyIndex < _lastLobbies.Count ? _lastLobbies[lobbyIndex] : null;
                if (lobby != null)
                {
                    nameLabel.text = $"{lobby.Name}  ({lobby.Players.Count}/{lobby.MaxPlayers})  [{lobby.LobbyCode}]";
                    lobbyIndex++;
                }
                else
                {
                    nameLabel.text = "Порожньо";
                }

                var button = row.GetComponent<Button>();
                if (button == null) button = row.gameObject.AddComponent<Button>();

                button.onClick.RemoveAllListeners();
                if (lobby != null)
                {
                    string lobbyId = lobby.LobbyId;
                    string lobbyCode = lobby.LobbyCode;
                    button.onClick.AddListener(() =>
                    {
                        if (joinCodeInput != null)
                            joinCodeInput.text = lobbyCode;
                        _ = JoinRoomAndStartAsync();
                    });
                }
                _roomButtons.Add(button);
            }
        }

        private async Task CreateRoomAndStartAsync()
        {
            if (_sessionManager == null)
            {
                Debug.LogWarning("[MultiplayerMenuPanelController] ISessionManager is not available.", this);
                return;
            }

            string roomName = string.IsNullOrWhiteSpace(roomNameInput?.text)
                ? $"Room {Guid.NewGuid().ToString("N").Substring(0, 4)}"
                : roomNameInput.text.Trim();

            var rules = BuildRules();
            var identity = await BuildIdentityAsync(CancellationToken.None);
            uint checksum = _multiplayerConfig != null ? ComputeConfigChecksum(_multiplayerConfig) : 0;

            var options = new SessionConnectOptions(identity, roomName, createIfNotExists: true, rules, checksum);
            bool ok = await _sessionManager.CreateOrJoinSessionAsync(options, CancellationToken.None);
            if (!ok)
            {
                Debug.LogWarning($"[MultiplayerMenuPanelController] Failed to create room '{roomName}'.", this);
                return;
            }

            GameLaunchContext.ConfigureMenuNewGame();
            _navigation?.LaunchGameplay();
        }

        private async Task JoinRoomAndStartAsync()
        {
            if (_sessionManager == null)
            {
                Debug.LogWarning("[MultiplayerMenuPanelController] ISessionManager is not available.", this);
                return;
            }

            string lobbyCode = joinCodeInput != null ? joinCodeInput.text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(lobbyCode))
            {
                Debug.LogWarning("[MultiplayerMenuPanelController] Join code is empty.", this);
                return;
            }

            var rules = BuildRules();
            var identity = await BuildIdentityAsync(CancellationToken.None);
            uint checksum = _multiplayerConfig != null ? ComputeConfigChecksum(_multiplayerConfig) : 0;

            var options = new SessionConnectOptions(identity, lobbyCode, createIfNotExists: false, rules, checksum);
            bool ok = await _sessionManager.CreateOrJoinSessionAsync(options, CancellationToken.None);
            if (!ok)
            {
                Debug.LogWarning($"[MultiplayerMenuPanelController] Failed to join room '{lobbyCode}'.", this);
                return;
            }

            GameLaunchContext.ConfigureMenuJoinGame();
            _navigation?.LaunchGameplay();
        }

        private SessionRules BuildRules()
        {
            if (_multiplayerConfig?.DefaultSessionRules != null)
            {
                var d = _multiplayerConfig.DefaultSessionRules;
                int maxParticipants = maxPlayersSlider != null ? Mathf.RoundToInt(maxPlayersSlider.value) : d.MaxParticipants;
                return new SessionRules(
                    d.Mode,
                    maxParticipants,
                    Mathf.Min(maxParticipants, d.MaxHumans),
                    Mathf.Max(0, maxParticipants - Mathf.Min(maxParticipants, d.MaxHumans)),
                    d.AllowBotsFallbackOnLeave,
                    d.AllowMatchSaveForAnalysis,
                    d.StrictParticipantLock);
            }

            return SessionRules.Default();
        }

        private static uint ComputeConfigChecksum(MultiplayerConfig config)
        {
            const uint fnvOffsetBasis = 2166136261u;
            const uint fnvPrime = 16777619u;

            unchecked
            {
                uint crc = fnvOffsetBasis;
                crc = (crc ^ (uint)config.SchemaVersion) * fnvPrime;
                crc = (crc ^ (uint)config.ProviderType) * fnvPrime;
                crc = (crc ^ (config.StrictParticipantLock ? 1u : 0u)) * fnvPrime;
                crc = (crc ^ (config.EnforceConfigConsistency ? 1u : 0u)) * fnvPrime;
                crc = (crc ^ (uint)config.DefaultSessionRules.Mode) * fnvPrime;
                crc = (crc ^ (uint)config.DefaultSessionRules.MaxParticipants) * fnvPrime;
                return crc;
            }
        }

        private async Task<ParticipantIdentity> BuildIdentityAsync(CancellationToken ct)
        {
            string nickname = Environment.UserName;
            if (string.IsNullOrWhiteSpace(nickname))
                nickname = "Player";

            if (_identityService != null)
                return await _identityService.ResolveAsync(nickname, ct);

            string playerId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrWhiteSpace(playerId) || playerId == SystemInfo.unsupportedIdentifier)
                playerId = $"local-{Guid.NewGuid():N}";
            return new ParticipantIdentity(playerId, nickname);
        }

        private static T FindByName<T>(string name) where T : Component
        {
            var all = FindObjectsByType<T>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == name)
                    return all[i];
            }
            return null;
        }

        private static T FindByPath<T>(string path) where T : Component
        {
            var go = GameObject.Find(path);
            return go != null ? go.GetComponent<T>() : null;
        }

        private static T FindByNameInParent<T>(string parentName, string childName) where T : Component
        {
            var parent = GameObject.Find(parentName);
            if (parent == null)
                return null;

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i);
                if (child.name == childName)
                    return child.GetComponent<T>();
            }

            return null;
        }

        private static TMP_Text FindChildTextByName(Transform root, string childName)
        {
            if (root == null)
                return null;

            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                {
                    var t = child.GetComponent<TMP_Text>();
                    if (t != null)
                        return t;
                }
            }

            return null;
        }
    }
}