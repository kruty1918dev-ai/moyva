using System;
using System.Text;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.UI
{
    /// <summary>
    /// UI-панель лобі мультиплеєру.
    /// Показує список гравців, код кімнати, дозволяє хосту стартувати гру, іншим — вийти.
    /// </summary>
    public sealed class LobbyPanel : MonoBehaviour, IGameModePanel
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private TMP_Text _lobbyCodeLabel;
        [SerializeField] private TMP_Text _playersLabel;
        [SerializeField] private Button _copyCodeButton;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _leaveButton;

        private ILobbyService _lobby;
        private ISessionManager _sessionManager;
        private IGameCommandSyncService _commands;
        private SignalBus _signalBus;

        public GameModeType TargetMode => GameModeType.Lobby;

        [Inject]
        public void Construct(
            ILobbyService lobby,
            ISessionManager sessionManager,
            IGameCommandSyncService commands,
            SignalBus signalBus)
        {
            _lobby = lobby;
            _sessionManager = sessionManager;
            _commands = commands;
            _signalBus = signalBus;
        }

        private void Awake()
        {
            if (_copyCodeButton != null) _copyCodeButton.onClick.AddListener(OnCopyCode);
            if (_startGameButton != null) _startGameButton.onClick.AddListener(OnStartGame);
            if (_leaveButton != null) _leaveButton.onClick.AddListener(OnLeave);
        }

        private void OnEnable()
        {
            if (_lobby != null) _lobby.LobbyUpdated += OnLobbyUpdated;
            _commands?.RegisterHandler(GameCommandType.StartGame, OnStartGameCommandReceived);
            _commands?.RegisterHandler(GameCommandType.EndTurn, OnEndTurnCommandReceived);
            Refresh(_lobby?.Current);
        }

        private void OnDisable()
        {
            if (_lobby != null) _lobby.LobbyUpdated -= OnLobbyUpdated;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (_content != null) _content.SetActive(true);
            Refresh(_lobby?.Current);
        }

        public void Hide()
        {
            if (_content != null) _content.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnLobbyUpdated(LobbyRoom room) => Refresh(room);

        private void Refresh(LobbyRoom room)
        {
            if (room == null)
            {
                if (_lobbyCodeLabel != null) _lobbyCodeLabel.text = "—";
                if (_playersLabel != null) _playersLabel.text = string.Empty;
                if (_startGameButton != null) _startGameButton.interactable = false;
                return;
            }

            if (_lobbyCodeLabel != null)
                _lobbyCodeLabel.text = string.IsNullOrEmpty(room.LobbyCode) ? room.Name : room.LobbyCode;

            if (_playersLabel != null)
            {
                var sb = new StringBuilder();
                foreach (var p in room.Players)
                {
                    bool isHost = p.PlayerId == room.HostPlayerId;
                    sb.Append(p.DisplayName ?? p.PlayerId);
                    if (isHost) sb.Append("  (host)");
                    sb.AppendLine();
                }
                _playersLabel.text = sb.ToString();
            }

            // MVP: Start button is enabled for any client; only host-originated StartGame
            // commands are authoritative on the gameplay side (to be enforced there).
            if (_startGameButton != null) _startGameButton.interactable = true;
        }

        private void OnCopyCode()
        {
            var code = _lobbyCodeLabel != null ? _lobbyCodeLabel.text : string.Empty;
            if (!string.IsNullOrEmpty(code))
                GUIUtility.systemCopyBuffer = code;
        }

        private async void OnStartGame()
        {
            if (_commands == null) return;
            _commands.SendCommand(GameCommandType.StartGame, Array.Empty<byte>());
            RequestGameplayMode();
            if (_lobby != null)
            {
                try { await _lobby.LockAsync(true); }
                catch (Exception e) { Debug.LogWarning($"[LobbyPanel] LockAsync failed: {e.Message}", this); }
            }
        }

        private void OnStartGameCommandReceived(string senderId, byte[] payload)
        {
            RequestGameplayMode();
        }

        private static void OnEndTurnCommandReceived(string senderId, byte[] payload)
        {
            // Reserved for gameplay turn system hook.
        }

        private void RequestGameplayMode()
        {
            _signalBus?.Fire(new GameModeChangeRequestedSignal
            {
                RequestedMode = Signals.GameModeType.Normal
            });
        }

        private async void OnLeave()
        {
            if (_sessionManager == null) return;
            try { await _sessionManager.LeaveSessionAsync(); }
            catch (Exception e) { Debug.LogWarning($"[LobbyPanel] Leave failed: {e.Message}", this); }
        }
    }
}
