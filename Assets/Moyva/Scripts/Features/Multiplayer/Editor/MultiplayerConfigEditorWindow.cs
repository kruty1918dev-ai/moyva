using System.IO;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Editor
{
    /// <summary>
    /// Unity EditorWindow for configuring MultiplayerConfig.
    /// Loads/saves the config from/to a binary file via BinaryConfigStore.
    /// </summary>
    public sealed class MultiplayerConfigEditorWindow : EditorWindow
    {
        private const string ConfigPath = "Assets/Moyva/multiplayer_config.dat";
        private const int HardMaxParticipants = 4;

        // ── General fields ─────────────────────────────────────────────────────────
        private NetworkProviderType _providerType;
        private NetworkProviderType _fallbackProviderType;
        private SessionMode _sessionMode;
        private int _maxParticipants = 4;
        private int _maxHumans = 4;
        private int _maxBots = 0;
        private bool _strictParticipantLock;
        private bool _allowBotsFallback;
        private bool _enableMatchmaking;
        private bool _allowMatchSave;
        private bool _enforceConfigConsistency = true;

        // ── Relay settings ─────────────────────────────────────────────────────────
        private bool _relayFoldout = true;
        private string _relayProjectId = string.Empty;
        private string _relayEnvironment = "production";
        private string _relayRegion = string.Empty;
        private int _relayMaxConnections = 4;

        // ── WebSocket settings ─────────────────────────────────────────────────────
        private bool _wsFoldout = true;
        private string _wsServerUrl = "ws://localhost";
        private int _wsPort = 9999;
        private string _wsAuthToken = string.Empty;
        private int _wsReconnectAttempts = 3;
        private float _wsReconnectDelay = 2f;

        // ── UI state ───────────────────────────────────────────────────────────────
        private string _validationMessage;
        private MessageType _validationMessageType;
        private Vector2 _scroll;

        [MenuItem("Moyva/Multiplayer/Config Hub")]
        public static void Open()
        {
            var window = GetWindow<MultiplayerConfigEditorWindow>("Multiplayer Config Hub");
            window.minSize = new Vector2(480, 560);
            window.LoadFromFile();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Multiplayer Configuration Hub", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure the networking provider, session rules, and per-provider settings. Saved to a binary config file.",
                MessageType.Info);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawNetworkSection();
            DrawProviderSettingsSection();
            DrawSessionRulesSection();
            DrawFlagsSection();
            DrawValidation();
            DrawButtons();

            EditorGUILayout.EndScrollView();
        }

        // ── Section drawers ────────────────────────────────────────────────────────

        private void DrawNetworkSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Network Provider", EditorStyles.boldLabel);
            _providerType = (NetworkProviderType)EditorGUILayout.EnumPopup("Primary Provider", _providerType);
            _fallbackProviderType = (NetworkProviderType)EditorGUILayout.EnumPopup("Fallback Provider", _fallbackProviderType);

            if (_providerType == _fallbackProviderType && _providerType != NetworkProviderType.Offline)
            {
                EditorGUILayout.HelpBox(
                    "Primary and fallback providers are the same. Set a different fallback for automatic recovery.",
                    MessageType.Warning);
            }
            else if (_providerType != NetworkProviderType.Offline)
            {
                EditorGUILayout.HelpBox(
                    $"If {_providerType} fails, the system will automatically switch to {_fallbackProviderType}.",
                    MessageType.Info);
            }
        }

        private void DrawProviderSettingsSection()
        {
            bool needsRelay = _providerType == NetworkProviderType.Relay ||
                              _fallbackProviderType == NetworkProviderType.Relay;
            bool needsWs = _providerType == NetworkProviderType.WebSocket ||
                           _fallbackProviderType == NetworkProviderType.WebSocket;

            if (needsRelay)
                DrawRelaySettings();

            if (needsWs)
                DrawWebSocketSettings();
        }

        private void DrawRelaySettings()
        {
            EditorGUILayout.Space(6);
            _relayFoldout = EditorGUILayout.Foldout(_relayFoldout, "Unity Relay Settings", true, EditorStyles.foldoutHeader);
            if (!_relayFoldout) return;

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Requires Unity Gaming Services Relay SDK (com.unity.services.relay).\n" +
                "After installing, add the MOYVA_UGS_RELAY scripting define under Player Settings.\n" +
                "Relay uses Unity Services initialization + anonymous auth (no API key).",
                MessageType.Info);

            _relayRegion = EditorGUILayout.TextField(new GUIContent("Region", "Allocation region, e.g. eu-west-1. Leave empty for automatic selection."), _relayRegion);
            _relayMaxConnections = EditorGUILayout.IntField(new GUIContent("Max Connections", "Max peers supported by the Relay allocation."), _relayMaxConnections);
            EditorGUI.indentLevel--;
        }

        private void DrawWebSocketSettings()
        {
            EditorGUILayout.Space(6);
            _wsFoldout = EditorGUILayout.Foldout(_wsFoldout, "WebSocket Settings", true, EditorStyles.foldoutHeader);
            if (!_wsFoldout) return;

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Connect to a custom WebSocket relay server (ws:// or wss://).\n" +
                "The server must implement the Moyva WebSocket protocol (see docs/systems/multiplayer/network-providers.md).",
                MessageType.Info);

            _wsServerUrl = EditorGUILayout.TextField(new GUIContent("Server URL", "WebSocket server URL, e.g. ws://localhost or wss://example.com"), _wsServerUrl);
            _wsPort = EditorGUILayout.IntField(new GUIContent("Port", "Port to append to the server URL."), _wsPort);
            _wsAuthToken = EditorGUILayout.PasswordField(new GUIContent("Auth Token", "Optional Bearer token sent in the Authorization header on connect."), _wsAuthToken);
            _wsReconnectAttempts = EditorGUILayout.IntField(new GUIContent("Reconnect Attempts", "How many times to retry after a disconnect (0 = no retry)."), _wsReconnectAttempts);
            _wsReconnectDelay = EditorGUILayout.FloatField(new GUIContent("Reconnect Delay (s)", "Seconds to wait between reconnection attempts."), _wsReconnectDelay);
            EditorGUI.indentLevel--;
        }

        private void DrawSessionRulesSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Session Rules", EditorStyles.boldLabel);
            _sessionMode = (SessionMode)EditorGUILayout.EnumPopup("Default Session Mode", _sessionMode);

            _maxParticipants = EditorGUILayout.IntSlider("Max Participants", _maxParticipants, 1, HardMaxParticipants);
            _maxHumans = EditorGUILayout.IntSlider("Max Humans", _maxHumans, 0, HardMaxParticipants);
            _maxBots = EditorGUILayout.IntSlider("Max Bots", _maxBots, 0, HardMaxParticipants);
        }

        private void DrawFlagsSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Flags", EditorStyles.boldLabel);
            _strictParticipantLock = EditorGUILayout.Toggle("Strict 4-Player World Lock", _strictParticipantLock);
            _allowBotsFallback = EditorGUILayout.Toggle("Allow Bots Fallback On Leave", _allowBotsFallback);
            _enableMatchmaking = EditorGUILayout.Toggle("Enable Matchmaking", _enableMatchmaking);
            _allowMatchSave = EditorGUILayout.Toggle("Allow Match Save For Analysis", _allowMatchSave);
            _enforceConfigConsistency = EditorGUILayout.Toggle("Enforce Config Consistency", _enforceConfigConsistency);
        }

        private void DrawValidation()
        {
            EditorGUILayout.Space(8);

            _validationMessage = null;
            _validationMessageType = MessageType.None;

            if (_maxParticipants > HardMaxParticipants)
            {
                _validationMessage = $"Max Participants cannot exceed {HardMaxParticipants}.";
                _validationMessageType = MessageType.Error;
            }
            else if (_maxHumans + _maxBots > _maxParticipants)
            {
                _validationMessage = $"Max Humans ({_maxHumans}) + Max Bots ({_maxBots}) = {_maxHumans + _maxBots} exceeds Max Participants ({_maxParticipants}).";
                _validationMessageType = MessageType.Error;
            }
            else if (_providerType == NetworkProviderType.WebSocket && string.IsNullOrEmpty(_wsServerUrl))
            {
                _validationMessage = "WebSocket selected but Server URL is empty.";
                _validationMessageType = MessageType.Error;
            }
            else if (_strictParticipantLock && _maxBots > 0)
            {
                _validationMessage = "Warning: Strict 4-player lock with bots may prevent re-joining if bots are replaced.";
                _validationMessageType = MessageType.Warning;
            }
            else if (_sessionMode == SessionMode.PeacefulSolo && _maxParticipants > 1)
            {
                _validationMessage = "Info: PeacefulSolo mode is single-player. Consider setting Max Participants to 1.";
                _validationMessageType = MessageType.Info;
            }

            if (_validationMessage != null)
                EditorGUILayout.HelpBox(_validationMessage, _validationMessageType);
        }

        private void DrawButtons()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            bool hasErrors = _validationMessageType == MessageType.Error;
            using (new EditorGUI.DisabledScope(hasErrors))
            {
                if (GUILayout.Button("Save Config", GUILayout.Height(30)))
                    SaveToFile();
            }

            if (GUILayout.Button("Load Config", GUILayout.Height(30)))
                LoadFromFile();

            if (GUILayout.Button("Reset to Defaults", GUILayout.Height(30)))
                ApplyConfig(MultiplayerConfig.Default());

            EditorGUILayout.EndHorizontal();
        }

        // ── Load / Save ────────────────────────────────────────────────────────────

        private void LoadFromFile()
        {
            var store = new BinaryConfigStore(ConfigPath);
            var config = store.Load();
            ApplyConfig(config);
            Repaint();
        }

        private void SaveToFile()
        {
            var rules = new SessionRules(
                _sessionMode,
                _maxParticipants,
                _maxHumans,
                _maxBots,
                _allowBotsFallback,
                _allowMatchSave,
                _strictParticipantLock);

            var relay = new RelayProviderSettings(
                _relayProjectId,
                _relayEnvironment,
                _relayRegion,
                _relayMaxConnections);

            var ws = new WebSocketProviderSettings(
                _wsServerUrl,
                _wsPort,
                _wsAuthToken,
                _wsReconnectAttempts,
                _wsReconnectDelay);

            var config = new MultiplayerConfig(
                MultiplayerConfig.CurrentSchemaVersion,
                _providerType,
                rules,
                _strictParticipantLock,
                _enforceConfigConsistency,
                _enableMatchmaking,
                relay,
                ws,
                _fallbackProviderType);

            string dir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var store = new BinaryConfigStore(ConfigPath);
            store.Save(config);
            AssetDatabase.Refresh();
            Debug.Log($"[Multiplayer] Config saved to {ConfigPath}");
        }

        private void ApplyConfig(MultiplayerConfig config)
        {
            _providerType = config.ProviderType;
            _fallbackProviderType = config.FallbackProviderType;
            _sessionMode = config.DefaultSessionRules.Mode;
            _maxParticipants = config.DefaultSessionRules.MaxParticipants;
            _maxHumans = config.DefaultSessionRules.MaxHumans;
            _maxBots = config.DefaultSessionRules.MaxBots;
            _allowBotsFallback = config.DefaultSessionRules.AllowBotsFallbackOnLeave;
            _allowMatchSave = config.DefaultSessionRules.AllowMatchSaveForAnalysis;
            _strictParticipantLock = config.StrictParticipantLock;
            _enforceConfigConsistency = config.EnforceConfigConsistency;
            _enableMatchmaking = config.MatchmakingEnabled;

            var relay = config.RelaySettings;
            _relayProjectId = relay.ProjectId;
            _relayEnvironment = relay.Environment;
            _relayRegion = relay.Region;
            _relayMaxConnections = relay.MaxConnections;

            var ws = config.WebSocketSettings;
            _wsServerUrl = ws.ServerUrl;
            _wsPort = ws.Port;
            _wsAuthToken = ws.AuthToken;
            _wsReconnectAttempts = ws.ReconnectAttempts;
            _wsReconnectDelay = ws.ReconnectDelaySeconds;
        }
    }
}
