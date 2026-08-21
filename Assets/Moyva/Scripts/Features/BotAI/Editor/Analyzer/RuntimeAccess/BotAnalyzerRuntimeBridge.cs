using System;
using System.Collections.Generic;
using UnityEditor;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerRuntimeBridge : IDisposable
    {
        private readonly BotAnalyzerSettings _settings;
        private readonly BotAnalyzerServiceResolver _resolver = new();
        private BotAnalyzerServices _services;
        private BotAnalyzerSnapshotCollector _collector;
        private readonly List<string> _botIds = new();

        private bool _started;
        private bool _capturePaused;
        private bool _followActiveBot = true;
        private string _requestedOwnerId = string.Empty;
        private string _selectedOwnerId = string.Empty;
        private double _nextSampleTime;
        private double _nextReconnectTime;
        private string _lastConnectionReason = string.Empty;

        public BotAnalyzerRuntimeBridge(BotAnalyzerSettings settings)
        {
            _settings = settings ?? BotAnalyzerSettings.CreateDefault();
        }

        public event Action<BotAnalyzerFrame> FrameCaptured;
        public event Action StateChanged;
        public event Action<string> Warning;

        public bool Connected => _services?.CoreReady == true;
        public bool CapturePaused => _capturePaused;
        public bool FollowActiveBot => _followActiveBot;
        public string SelectedOwnerId => _selectedOwnerId;
        public string LastConnectionReason => _lastConnectionReason;
        public IReadOnlyList<string> BotIds => _botIds;
        public BotAnalyzerServices Services => _services;
        public BotAnalyzerFrame LatestFrame { get; private set; }

        public void Start()
        {
            if (_started)
                return;

            _started = true;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (EditorApplication.isPlaying)
                ForceReconnect();
        }

        public void SetCapturePaused(bool paused)
        {
            if (_capturePaused == paused)
                return;

            _capturePaused = paused;
            StateChanged?.Invoke();
        }

        public void SetFollowActiveBot(bool follow)
        {
            if (_followActiveBot == follow)
                return;

            _followActiveBot = follow;
            _nextSampleTime = 0d;
            StateChanged?.Invoke();
        }

        public void SetSelectedOwner(string ownerId)
        {
            _requestedOwnerId = ownerId ?? string.Empty;
            if (!_followActiveBot)
            {
                _selectedOwnerId = _requestedOwnerId;
                _nextSampleTime = 0d;
            }
            StateChanged?.Invoke();
        }

        public void ForceReconnect()
        {
            Disconnect("Reconnecting analyzer runtime bridge.");
            _nextReconnectTime = 0d;

            if (EditorApplication.isPlaying)
                TryConnect(EditorApplication.timeSinceStartup);
        }

        public void ForceCapture()
        {
            _nextSampleTime = 0d;
            if (EditorApplication.isPlaying)
                OnEditorUpdate();
        }

        public void Dispose()
        {
            if (!_started)
                return;

            _started = false;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Disconnect("Analyzer window closed.");
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    _nextReconnectTime = 0d;
                    _nextSampleTime = 0d;
                    TryConnect(EditorApplication.timeSinceStartup);
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                case PlayModeStateChange.EnteredEditMode:
                    Disconnect("Play Mode is not active.");
                    break;
            }
            StateChanged?.Invoke();
        }

        private void OnEditorUpdate()
        {
            if (!_started)
                return;

            double now = EditorApplication.timeSinceStartup;

            if (!EditorApplication.isPlaying)
            {
                if (Connected)
                    Disconnect("Play Mode is not active.");
                return;
            }

            if (!Connected || _services.Context == null)
            {
                if (now >= _nextReconnectTime)
                    TryConnect(now);
                return;
            }

            RefreshBotList();
            string ownerId = BotAnalyzerBotSelector.ResolveOwner(
                _services.Turns,
                _requestedOwnerId,
                _followActiveBot);

            if (!string.Equals(_selectedOwnerId, ownerId, StringComparison.Ordinal))
            {
                _selectedOwnerId = ownerId;
                if (!_followActiveBot && string.IsNullOrWhiteSpace(_requestedOwnerId))
                    _requestedOwnerId = ownerId;
                _nextSampleTime = 0d;
                StateChanged?.Invoke();
            }

            if (_capturePaused ||
                string.IsNullOrWhiteSpace(ownerId) ||
                now < _nextSampleTime)
            {
                return;
            }

            _nextSampleTime = now + Math.Max(0.05f, _settings.CoreSampleInterval);

            BotAnalyzerFrame frame = null;
            try
            {
                frame = _collector?.Capture(ownerId, now, _settings);
            }
            catch (Exception exception)
            {
                Warning?.Invoke($"Analyzer capture failed: {exception.Message}");
            }

            if (frame == null)
                return;

            LatestFrame = frame;
            FrameCaptured?.Invoke(frame);
            StateChanged?.Invoke();
        }

        private void TryConnect(double now)
        {
            _nextReconnectTime = now + 0.50d;

            if (!_resolver.TryResolve(out BotAnalyzerServices services, out string reason))
            {
                _lastConnectionReason = reason ?? "Runtime services unavailable.";
                return;
            }

            _services = services;
            _collector = new BotAnalyzerSnapshotCollector(services);
            _lastConnectionReason = services.BuildAvailabilitySummary();
            _nextSampleTime = 0d;
            RefreshBotList();

            if (_botIds.Count == 0)
            {
                Warning?.Invoke("Bot Analyzer connected, but the turn registry contains no bot factions.");
            }

            StateChanged?.Invoke();
        }

        private void RefreshBotList()
        {
            _botIds.Clear();
            if (_services?.Turns == null)
                return;

            List<string> found = BotAnalyzerBotSelector.GetBotIds(_services.Turns.Factions);
            _botIds.AddRange(found);
        }

        private void Disconnect(string reason)
        {
            _services = null;
            _collector = null;
            _botIds.Clear();
            _selectedOwnerId = string.Empty;
            LatestFrame = null;
            _lastConnectionReason = reason ?? string.Empty;
            _nextSampleTime = 0d;
        }
    }
}
