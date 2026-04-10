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

        // Editable fields
        private NetworkProviderType _providerType;
        private SessionMode _sessionMode;
        private int _maxParticipants = 4;
        private int _maxHumans = 4;
        private int _maxBots = 0;
        private bool _strictParticipantLock;
        private bool _allowBotsFallback;
        private bool _enableMatchmaking;
        private bool _allowMatchSave;
        private bool _enforceConfigConsistency = true;

        private string _validationMessage;
        private MessageType _validationMessageType;
        private Vector2 _scroll;

        [MenuItem("Moyva/Multiplayer/Config Hub")]
        public static void Open()
        {
            var window = GetWindow<MultiplayerConfigEditorWindow>("Multiplayer Config Hub");
            window.minSize = new Vector2(480, 520);
            window.LoadFromFile();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Multiplayer Configuration Hub", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure session rules and multiplayer flags. Saved to a binary config file.",
                MessageType.Info);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawNetworkSection();
            DrawSessionRulesSection();
            DrawFlagsSection();
            DrawValidation();
            DrawButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawNetworkSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Network Provider", EditorStyles.boldLabel);
            _providerType = (NetworkProviderType)EditorGUILayout.EnumPopup("Provider Type", _providerType);
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

            var config = new MultiplayerConfig(
                MultiplayerConfig.CurrentSchemaVersion,
                _providerType,
                rules,
                _strictParticipantLock,
                _enforceConfigConsistency,
                _enableMatchmaking);

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
            _sessionMode = config.DefaultSessionRules.Mode;
            _maxParticipants = config.DefaultSessionRules.MaxParticipants;
            _maxHumans = config.DefaultSessionRules.MaxHumans;
            _maxBots = config.DefaultSessionRules.MaxBots;
            _allowBotsFallback = config.DefaultSessionRules.AllowBotsFallbackOnLeave;
            _allowMatchSave = config.DefaultSessionRules.AllowMatchSaveForAnalysis;
            _strictParticipantLock = config.StrictParticipantLock;
            _enforceConfigConsistency = config.EnforceConfigConsistency;
            _enableMatchmaking = config.MatchmakingEnabled;
        }
    }
}
