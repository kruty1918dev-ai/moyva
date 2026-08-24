using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    public sealed class BotAnalyzerWindow : OdinEditorWindow
    {
        private BotAnalyzerSettings _settings;
        private BotAnalyzerViewState _view;
        private BotAnalyzerSession _session;
        private BotAnalyzerDiffEngine _diff;
        private BotAnalyzerRuntimeBridge _bridge;
        private BotAnalyzerSceneOverlay _overlay;

        private BotAnalyzerOverviewModel _overview = new();
        private List<BotAnalyzerResourceRow> _resources = new();
        private List<BotAnalyzerUnitRow> _ownUnits = new();
        private List<BotAnalyzerUnitRow> _visibleEnemies = new();
        private List<BotAnalyzerBuildingRow> _ownBuildings = new();
        private List<BotAnalyzerBuildingRow> _visibleEnemyBuildings = new();
        private List<BotAnalyzerRecruitmentRow> _recruitment = new();
        private List<BotAnalyzerCandidateRow> _candidates = new();
        private List<BotAnalyzerReasoningRow> _reasoning = new();
        private List<BotAnalyzerTimelineRow> _timeline = new();
        private List<BotAnalyzerMemoryRow> _memory = new();
        private BotAnalyzerFogSummary _fog = new();

        [MenuItem("Moyva/Bot AI/Bot Analyzer", priority = 40)]
        public static void Open()
        {
            var window = GetWindow<BotAnalyzerWindow>("Moyva Bot Analyzer");
            window.titleContent = new GUIContent("Bot Analyzer");
            window.minSize = new Vector2(720f, 560f);
            window.Show();
            window.Focus();
        }

        [PropertyOrder(-100)]
        [ShowInInspector, ReadOnly]
        [LabelText("Runtime")]
        public string RuntimeStatus => BuildRuntimeStatus();

        [PropertyOrder(-99)]
        [ShowInInspector]
        [LabelText("Bot")]
        [ValueDropdown(nameof(GetBotIds))]
        public string SelectedBot
        {
            get => _view?.SelectedOwnerId ?? string.Empty;
            set
            {
                if (_view == null) return;
                _view.SelectedOwnerId = value ?? string.Empty;
                _bridge?.SetSelectedOwner(_view.SelectedOwnerId);
                Repaint();
            }
        }

        [PropertyOrder(-98)]
        [ShowInInspector, ToggleLeft]
        public bool FollowActiveBot
        {
            get => _view?.FollowActiveBot ?? true;
            set
            {
                if (_view == null) return;
                _view.FollowActiveBot = value;
                _bridge?.SetFollowActiveBot(value);
                Repaint();
            }
        }

        [HorizontalGroup("RuntimeButtons", Order = -90)]
        [Button(ButtonSizes.Medium)]
        public void PauseOrResumeCapture()
        {
            if (_view == null || _bridge == null) return;
            _view.CapturePaused = !_view.CapturePaused;
            _bridge.SetCapturePaused(_view.CapturePaused);
            Repaint();
        }

        [HorizontalGroup("RuntimeButtons")]
        [Button(ButtonSizes.Medium)]
        public void RefreshNow()
        {
            _bridge?.ForceCapture();
            Repaint();
        }

        [HorizontalGroup("RuntimeButtons")]
        [Button(ButtonSizes.Medium)]
        public void Reconnect()
        {
            _bridge?.ForceReconnect();
            Repaint();
        }

        [HorizontalGroup("RuntimeButtons")]
        [Button(ButtonSizes.Medium)]
        public void ClearSession()
        {
            _session?.Clear();
            RebuildModels();
            _overlay?.NotifyDataChanged();
            Repaint();
        }

        [TabGroup("Overview", Order = 0)]
        [ShowInInspector, ReadOnly, InlineProperty, HideLabel]
        public BotAnalyzerOverviewModel Overview => _overview;

        [TabGroup("Overview")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        public List<BotAnalyzerResourceRow> Resources => _resources;

        [TabGroup("Timeline")]
        [ShowInInspector]
        public BotAnalyzerTimelineFilter TimelineFilter
        {
            get => _view?.TimelineFilter ?? BotAnalyzerTimelineFilter.All;
            set
            {
                if (_view == null) return;
                _view.TimelineFilter = value;
                RebuildTimeline();
            }
        }

        [TabGroup("Timeline")]
        [ShowInInspector]
        public string Search
        {
            get => _view?.SearchText ?? string.Empty;
            set
            {
                if (_view == null) return;
                _view.SearchText = value ?? string.Empty;
                RebuildTimeline();
            }
        }

        [TabGroup("Timeline")]
        [ShowInInspector, ToggleLeft]
        public bool LatestFirst
        {
            get => _view?.FollowLatest ?? true;
            set
            {
                if (_view == null) return;
                _view.FollowLatest = value;
                RebuildTimeline();
            }
        }

        [TabGroup("Timeline")]
        [ShowInInspector, ToggleLeft]
        [LabelText("Auto-scroll / keep latest first")]
        public bool AutoScrollTimeline
        {
            get => _view?.AutoScrollTimeline ?? true;
            set
            {
                if (_view == null) return;
                _view.AutoScrollTimeline = value;
                if (value)
                    _view.FollowLatest = true;
                RebuildTimeline();
            }
        }

        [TabGroup("Timeline")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        public List<BotAnalyzerTimelineRow> Timeline => _timeline;

        [TabGroup("Decisions")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        public List<BotAnalyzerCandidateRow> DecisionCandidates => _candidates;

        [TabGroup("Decisions")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        [LabelText("Algorithm Reasoning / Людське пояснення")]
        public List<BotAnalyzerReasoningRow> ReasoningTrace => _reasoning;

        [TabGroup("World")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        public List<BotAnalyzerUnitRow> OwnUnits => _ownUnits;

        [TabGroup("World")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        public List<BotAnalyzerBuildingRow> OwnBuildings => _ownBuildings;

        [TabGroup("World")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        public List<BotAnalyzerRecruitmentRow> Recruitment => _recruitment;

        [TabGroup("World")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        public List<BotAnalyzerUnitRow> VisibleEnemyUnits => _visibleEnemies;

        [TabGroup("World")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        public List<BotAnalyzerBuildingRow> VisibleEnemyBuildings => _visibleEnemyBuildings;

        [TabGroup("World")]
        [ShowInInspector, ReadOnly, TableList(AlwaysExpanded = true)]
        public List<BotAnalyzerMemoryRow> BotMemory => _memory;

        [TabGroup("Fog")]
        [ShowInInspector, ReadOnly, InlineProperty, HideLabel]
        public BotAnalyzerFogSummary FogKnowledge => _fog;

        [TabGroup("Fog")]
        [ShowInInspector, InlineProperty, HideLabel]
        public BotAnalyzerSettings AnalyzerSettings => _settings;

        [TabGroup("Export")]
        [ShowInInspector, ReadOnly, MultiLineProperty(12)]
        [LabelText("Current Summary")]
        public string CurrentSummary =>
            BotAnalyzerTimelineBuilder.BuildCurrentSummary(_session?.LatestFrame);

        [TabGroup("Export")]
        [Button(ButtonSizes.Large)]
        public void ExportJson()
        {
            if (_session == null) return;
            string path = EditorUtility.SaveFilePanel(
                "Export Moyva Bot Analyzer JSON",
                ProjectRoot(),
                ExportFileStem() + ".json",
                "json");
            if (string.IsNullOrWhiteSpace(path)) return;

            File.WriteAllText(path, BotAnalyzerJsonExporter.BuildJson(_session, _settings));
            Debug.Log($"[BotAnalyzer] JSON exported: {path}");
        }

        [TabGroup("Export")]
        [Button(ButtonSizes.Large)]
        public void ExportMarkdown()
        {
            if (_session == null) return;
            string path = EditorUtility.SaveFilePanel(
                "Export Moyva Bot Analyzer Markdown",
                ProjectRoot(),
                ExportFileStem() + ".md",
                "md");
            if (string.IsNullOrWhiteSpace(path)) return;

            File.WriteAllText(path, BotAnalyzerMarkdownExporter.Build(_session, _settings));
            Debug.Log($"[BotAnalyzer] Markdown exported: {path}");
        }

        [TabGroup("Export")]
        [Button]
        public void CopySummary()
        {
            BotAnalyzerClipboardExporter.CopySummary(_session);
        }

        [TabGroup("Export")]
        [Button]
        public void CopyTimeline()
        {
            BotAnalyzerClipboardExporter.CopyTimeline(_session);
        }

        [TabGroup("Export")]
        [Button]
        public void CopyFullReport()
        {
            BotAnalyzerClipboardExporter.CopyFullReport(_session, _settings);
        }

        [TabGroup("Export")]
        [Button]
        public void CopyJson()
        {
            BotAnalyzerClipboardExporter.CopyJson(_session, _settings);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InitializeAnalyzer();
        }

        protected override void OnDisable()
        {
            Shutdown();
            base.OnDisable();
        }

        protected override void OnImGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Moyva Bot Analyzer is runtime-only. Enter Play Mode to capture Bot AI state, decisions, resources, fog knowledge and SceneView overlays.",
                    MessageType.Info);

                using (new EditorGUI.DisabledScope(true))
                    base.OnImGUI();

                return;
            }

            base.OnImGUI();
        }

        private void InitializeAnalyzer()
        {
            Shutdown();

            _settings = BotAnalyzerSettings.Load();
            _view = new BotAnalyzerViewState();
            _session = new BotAnalyzerSession(_settings);
            _diff = new BotAnalyzerDiffEngine();
            _bridge = new BotAnalyzerRuntimeBridge(_settings);
            _overlay = new BotAnalyzerSceneOverlay(_bridge, _session, _settings);

            _bridge.FrameCaptured += OnFrameCaptured;
            _bridge.StateChanged += OnBridgeStateChanged;
            _bridge.Warning += OnBridgeWarning;
            _bridge.SetFollowActiveBot(_view.FollowActiveBot);
            _bridge.Start();
            _overlay.Enable();

            RebuildModels();
        }

        private void Shutdown()
        {
            if (_settings != null)
                _settings.Save();

            if (_bridge != null)
            {
                _bridge.FrameCaptured -= OnFrameCaptured;
                _bridge.StateChanged -= OnBridgeStateChanged;
                _bridge.Warning -= OnBridgeWarning;
            }

            _overlay?.Dispose();
            _bridge?.Dispose();

            _overlay = null;
            _bridge = null;
            _diff = null;
        }

        private void OnFrameCaptured(BotAnalyzerFrame frame)
        {
            BotAnalyzerFrame previous = _session?.LatestFrame;
            List<BotAnalyzerEvent> events = _diff?.Diff(previous, frame) ?? new List<BotAnalyzerEvent>();
            _session?.Append(frame, events);

            if (_view != null &&
                (_view.FollowActiveBot || string.IsNullOrWhiteSpace(_view.SelectedOwnerId)))
            {
                _view.SelectedOwnerId = frame.OwnerId;
            }

            RebuildModels();
            _overlay?.NotifyDataChanged();
            Repaint();
        }

        private void OnBridgeStateChanged()
        {
            if (_view != null &&
                _bridge != null &&
                _view.FollowActiveBot &&
                !string.IsNullOrWhiteSpace(_bridge.SelectedOwnerId))
            {
                _view.SelectedOwnerId = _bridge.SelectedOwnerId;
            }

            Repaint();
        }

        private void OnBridgeWarning(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            BotAnalyzerFrame frame = _session?.LatestFrame;
            _session?.AppendEvent(new BotAnalyzerEvent
            {
                EditorTime = EditorApplication.timeSinceStartup,
                UtcTimestamp = DateTime.UtcNow.ToString("O"),
                GlobalTurn = frame?.GlobalTurn ?? 0,
                OwnerId = frame?.OwnerId ?? _view?.SelectedOwnerId ?? string.Empty,
                Type = BotAnalyzerEventType.Warning,
                Severity = BotAnalyzerEventSeverity.Warning,
                Title = "Analyzer warning",
                Detail = message,
            });

            RebuildTimeline();
            Repaint();
        }

        private void RebuildModels()
        {
            BotAnalyzerFrame current = _session?.LatestFrame;
            BotAnalyzerFrame previous = PreviousFrameForCurrentOwner(current);

            _overview = BotAnalyzerWindowModels.BuildOverview(current, BuildRuntimeStatus());
            _resources = BotAnalyzerWindowModels.BuildResources(current, previous);
            _ownUnits = BotAnalyzerWindowModels.BuildUnits(current?.OwnUnits);
            _visibleEnemies = BotAnalyzerWindowModels.BuildUnits(current?.VisibleEnemyUnits);
            _ownBuildings = BotAnalyzerWindowModels.BuildBuildings(current?.OwnBuildings);
            _visibleEnemyBuildings = BotAnalyzerWindowModels.BuildBuildings(current?.VisibleEnemyBuildings);
            _recruitment = BotAnalyzerWindowModels.BuildRecruitment(current?.Recruitment);
            _candidates = BotAnalyzerWindowModels.BuildCandidates(current?.Candidates);
            _reasoning = BotAnalyzerWindowModels.BuildReasoning(current?.Reasoning);
            _memory = BotAnalyzerWindowModels.BuildMemory(current?.Memory);
            _fog = BotAnalyzerWindowModels.BuildFog(current?.Fog);
            RebuildTimeline();
        }

        private void RebuildTimeline()
        {
            if (_session == null || _settings == null || _view == null)
            {
                _timeline = new List<BotAnalyzerTimelineRow>();
                return;
            }

            List<BotAnalyzerEvent> filtered = _session.GetFilteredEvents(
                _view.TimelineFilter,
                _view.SearchText,
                _settings.MaxDisplayedEvents,
                _view.FollowLatest);

            _timeline = BotAnalyzerWindowModels.BuildTimeline(filtered);
            Repaint();
        }

        private BotAnalyzerFrame PreviousFrameForCurrentOwner(BotAnalyzerFrame current)
        {
            if (_session == null || current == null)
                return null;

            IReadOnlyList<BotAnalyzerFrame> frames = _session.Frames;
            for (int i = frames.Count - 2; i >= 0; i--)
            {
                BotAnalyzerFrame candidate = frames[i];
                if (candidate != null &&
                    string.Equals(candidate.OwnerId, current.OwnerId, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }
            return null;
        }

        private IEnumerable<string> GetBotIds()
        {
            if (_bridge?.BotIds == null)
                yield break;

            for (int i = 0; i < _bridge.BotIds.Count; i++)
                yield return _bridge.BotIds[i];
        }

        private string BuildRuntimeStatus()
        {
            if (!EditorApplication.isPlaying)
                return "EDIT MODE — DISABLED";

            if (_bridge == null)
                return "PLAY MODE — INITIALIZING";

            if (!_bridge.Connected)
                return "PLAY MODE — CONNECTING: " + (_bridge.LastConnectionReason ?? string.Empty);

            if (_bridge.CapturePaused)
                return "CONNECTED — CAPTURE PAUSED";

            string owner = string.IsNullOrWhiteSpace(_bridge.SelectedOwnerId)
                ? "no bot selected"
                : _bridge.SelectedOwnerId;

            return $"CONNECTED — {owner} — {_bridge.LastConnectionReason}";
        }

        private string ExportFileStem()
        {
            string owner = _session?.LatestFrame?.OwnerId;
            if (string.IsNullOrWhiteSpace(owner))
                owner = "bot";
            return $"moyva-bot-analyzer-{owner}-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        private static string ProjectRoot()
        {
            DirectoryInfo assets = Directory.GetParent(Application.dataPath);
            return assets?.FullName ?? Application.dataPath;
        }
    }
}
