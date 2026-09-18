using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.AI.Bot;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    /// <summary>
    /// Read-only inspector for an already-trained MoyvaStrategy policy.
    /// Forward execution uses ML-Agents InferenceOnly. Backward navigation
    /// reconstructs the world by replaying recorded legal actions from the same seed.
    /// </summary>
    public sealed class TrainingModelInspectorController : MonoBehaviour
    {
        [Serializable]
        public sealed class CandidateView
        {
            public int slot;
            public string id;
            public string capability;
            public string intent;
            public string actor;
            public string target;
            public int x;
            public int y;
            public bool critical;
            public float[] features = Array.Empty<float>();
        }

        [Serializable]
        public sealed class DecisionRecord
        {
            public int index;
            public long episodeId;
            public int seed;
            public string scenarioId;
            public int scenarioStep;
            public long frameSequence;
            public long turn;
            public int phase;
            public string observationHash;
            public float[] observations = Array.Empty<float>();
            public CandidateView[] candidates = Array.Empty<CandidateView>();
            public string[] unavailable = Array.Empty<string>();
            public int selectedSlot = -1;
            public string selectedCandidateId;
            public string selectedIntent;
            public string selectedCapability;
            public string selectedActor;
            public string selectedTarget;
            public int selectedX;
            public int selectedY;
            public string executionResult;
            public string executionFailure;
            public string executionReason;
            public float latency;
            public float rewardBefore;
            public float rewardAfter;
            public float rewardDelta;
        }

        [Serializable]
        private sealed class SessionHeader
        {
            public int version = 1;
            public string runId;
            public string checkpointId;
            public long checkpointStep;
            public string contractHash;
            public string modelSha256;
            public string scenarioId;
            public int seed;
            public string startedUtc;
            public string note =
                "Frozen ML-Agents inference inspector. No trainer, optimizer, gradient update or PPO resume.";
        }

        [Serializable]
        private sealed class EpisodeRecord
        {
            public long episodeId;
            public int seed;
            public string scenarioId;
            public int decisionSequence;
            public string reason;
            public string utc;
        }

        private enum InspectorTab { Overview, Observations, Spatial, Candidates, Timeline, Raw }

        public static TrainingModelInspectorController Instance { get; private set; }

        private TrainingBootstrap _bootstrap;
        private TrainingEnvironment _environment;
        private BotDecisionOrchestrator _boundOrchestrator;
        private readonly List<DecisionRecord> _history = new List<DecisionRecord>();
        private DecisionRecord _pending;
        private InspectorTab _tab;
        private Vector2 _mainScroll;
        private Vector2 _candidateScroll;
        private Vector2 _timelineScroll;
        private bool _autoPlay;
        private bool _singleStepPending;
        private bool _replaying;
        private bool _details = true;
        private bool _showZeros;
        private int _cursor;
        private int _spatialChannel;
        private int _episodeSeed;
        private float _runSpeed = 1f;
        private string _status = "Initializing";
        private string _replayError;
        private string _sessionDirectory;
        private string _recordsPath;
        private string _episodesPath;
        private float _previousTimeScale = 1f;

        public bool IsLiveTip => _cursor >= _history.Count;

        public void Initialize(TrainingBootstrap bootstrap)
        {
            if (bootstrap == null) throw new ArgumentNullException(nameof(bootstrap));
            if (!bootstrap.Config.inspectorMode)
                throw new InvalidOperationException("TrainingModelInspectorController requires inspectorMode.");

            Instance = this;
            _bootstrap = bootstrap;
            _environment = bootstrap.Environments?.Environments.FirstOrDefault();
            if (_environment == null)
                throw new InvalidOperationException("Model Inspector requires one initialized training environment.");

            _previousTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
            _runSpeed = Mathf.Clamp(bootstrap.Config.visualTimeScale, 0.1f, 20f);
            _episodeSeed = _environment.Diagnostics.LastSeed;
            _sessionDirectory = bootstrap.Config.inspectorSessionPath;
            _recordsPath = bootstrap.Config.inspectorJournalPath;
            _episodesPath = Path.Combine(_sessionDirectory, "episodes.jsonl");
            Directory.CreateDirectory(_sessionDirectory);
            BindEnvironment();
            WriteSessionHeader();
            AppendEpisodeRecord("session-start");
            if (bootstrap.Config.inspectorAutoPlay)
            {
                _autoPlay = true;
                Time.timeScale = Mathf.Max(0.1f, _runSpeed);
                _status = "Autoplay frozen inference";
            }
            else Pause("Ready at the first decision boundary");
        }

        public static bool ShouldRequestModelDecision(TrainingEnvironment environment)
        {
            var instance = Instance;
            if (instance == null || instance._environment != environment) return true;
            if (instance._replaying || !instance.IsLiveTip) return false;
            return instance._autoPlay || instance._singleStepPending;
        }

        public static void NotifyDecisionRequested(TrainingEnvironment environment)
        {
            var instance = Instance;
            if (instance == null || instance._environment != environment || instance._replaying) return;
            instance._pending = instance.CaptureCurrentFrame();
            instance._status = "Model inference requested";
        }

        public static void NotifyModelAction(TrainingEnvironment environment, int slot)
        {
            var instance = Instance;
            if (instance == null || instance._environment != environment || instance._replaying) return;
            if (instance._pending == null) instance._pending = instance.CaptureCurrentFrame();
            instance.FillSelectedCandidate(instance._pending, slot);
            instance._status = "Model selected slot " + slot;
        }

        private void BindEnvironment()
        {
            _environment.EpisodeReset -= OnEpisodeReset;
            _environment.EpisodeReset += OnEpisodeReset;
            BindDecisionTrace();
        }

        private void BindDecisionTrace()
        {
            if (_boundOrchestrator != null)
                _boundOrchestrator.DecisionFinished -= OnDecisionFinished;
            _boundOrchestrator = _environment?.Bridge?.Orchestrator;
            if (_boundOrchestrator != null)
                _boundOrchestrator.DecisionFinished += OnDecisionFinished;
        }

        private void OnEpisodeReset(int arenaId, long episodeId)
        {
            if (_environment == null || arenaId != _environment.EnvironmentId) return;
            BindDecisionTrace();
            AppendEpisodeRecord("reset");
        }

        private DecisionRecord CaptureCurrentFrame()
        {
            var frame = _environment?.Bridge?.Frame;
            var record = new DecisionRecord
            {
                index = _history.Count,
                episodeId = _environment?.EpisodeId ?? 0,
                seed = _environment?.Diagnostics?.LastSeed ?? _episodeSeed,
                scenarioId = _environment?.Scenario?.id,
                scenarioStep = _environment?.ScenarioProgress?.StepIndex ?? -1,
                frameSequence = frame?.Sequence ?? 0,
                turn = frame?.Stamp.Turn ?? 0,
                phase = frame?.Stamp.Phase ?? 0,
                rewardBefore = _environment?.Rewards?.TotalReward ?? 0f
            };
            if (frame == null) return record;

            record.observations = frame.Observations.ToArray();
            record.observationHash = HashObservations(record.observations);

            var candidates = new List<CandidateView>();
            for (int i = 0; i < frame.Candidates.Count; i++)
            {
                var c = frame.Candidates[i];
                if (c == null) continue;
                candidates.Add(new CandidateView
                {
                    slot = i,
                    id = c.Id,
                    capability = c.Capability.ToString(),
                    intent = c.Intent.ToString(),
                    actor = c.ActorKey,
                    target = c.TargetKey,
                    x = c.X,
                    y = c.Y,
                    critical = c.Critical,
                    features = c.Features.ToArray()
                });
            }
            record.candidates = candidates.ToArray();
            record.unavailable = frame.Unavailable
                .OrderBy(x => x.Key)
                .Select(x => x.Key + ": " + x.Value)
                .ToArray();
            return record;
        }

        private static string HashObservations(IReadOnlyList<float> values)
        {
            unchecked
            {
                uint hash = 2166136261;
                if (values != null)
                    for (int i = 0; i < values.Count; i++)
                        foreach (byte b in BitConverter.GetBytes(values[i]))
                            hash = (hash ^ b) * 16777619;
                return hash.ToString("x8");
            }
        }

        private void FillSelectedCandidate(DecisionRecord record, int slot)
        {
            if (record == null) return;
            record.selectedSlot = slot;
            var c = record.candidates.FirstOrDefault(x => x.slot == slot);
            if (c == null) return;
            record.selectedCandidateId = c.id;
            record.selectedIntent = c.intent;
            record.selectedCapability = c.capability;
            record.selectedActor = c.actor;
            record.selectedTarget = c.target;
            record.selectedX = c.x;
            record.selectedY = c.y;
        }

        private void OnDecisionFinished(BotDecisionTrace trace)
        {
            if (_replaying) return;

            var record = _pending ?? CaptureCurrentFrame();
            FillSelectedCandidate(record, trace.Slot);
            record.executionResult = trace.Result.ToString();
            record.executionFailure = trace.Failure.ToString();
            record.executionReason = trace.Reason;
            record.latency = trace.Latency;
            record.rewardAfter = _environment.Rewards.TotalReward;
            record.rewardDelta = record.rewardAfter - record.rewardBefore;
            record.index = _history.Count;
            if (!string.IsNullOrEmpty(trace.ObservationHash))
                record.observationHash = trace.ObservationHash;

            _history.Add(record);
            AppendRecord(record);
            _pending = null;
            _cursor = _history.Count;
            _status = string.Format(
                "Decision {0}: slot {1} {2}/{3} -> {4}",
                record.index, record.selectedSlot, record.selectedIntent,
                record.selectedCapability, record.executionResult);

            if (_singleStepPending)
            {
                _singleStepPending = false;
                StartCoroutine(PauseAtStableBoundary());
            }
        }

        private IEnumerator PauseAtStableBoundary()
        {
            float deadline = Time.realtimeSinceStartup + 10f;
            while (_environment != null && _environment.IsReady
                   && !_environment.CanRequestDecision
                   && Time.realtimeSinceStartup < deadline)
                yield return null;
            Pause("Paused after one model decision");
        }

        public void StepModel()
        {
            if (_replaying) return;
            if (!IsLiveTip)
            {
                NavigateTo(Mathf.Min(_history.Count, _cursor + 1));
                return;
            }
            if (_environment == null || !_environment.IsReady)
            {
                _status = "Episode ended. Restart the seed to inspect again.";
                return;
            }
            _autoPlay = false;
            _singleStepPending = true;
            Time.timeScale = Mathf.Max(0.1f, _runSpeed);
            _status = "Running exactly one model decision";
        }

        public void ToggleAutoPlay()
        {
            if (_replaying || !IsLiveTip) return;
            _autoPlay = !_autoPlay;
            _singleStepPending = false;
            if (_autoPlay)
            {
                Time.timeScale = Mathf.Max(0.1f, _runSpeed);
                _status = "Autoplay frozen inference";
            }
            else Pause("Autoplay paused");
        }

        public void Pause(string reason = "Paused")
        {
            _autoPlay = false;
            _singleStepPending = false;
            Time.timeScale = 0f;
            _status = reason;
        }

        public void NavigateTo(int target)
        {
            if (_replaying || _history.Count == 0) return;
            target = Mathf.Clamp(target, 0, _history.Count);
            StopAllCoroutines();
            StartCoroutine(ReplayToBoundary(target));
        }

        private IEnumerator ReplayToBoundary(int target)
        {
            _replaying = true;
            _replayError = null;
            _autoPlay = false;
            _singleStepPending = false;
            _status = "Deterministic replay -> boundary " + target;
            Time.timeScale = 20f;

            try
            {
                _environment.ResetForInspectorReplay(_episodeSeed);
                _environment.BeginEpisode();
                BindDecisionTrace();
            }
            catch (Exception exception)
            {
                FailReplay(exception.Message);
                yield break;
            }

            yield return null;

            for (int i = 0; i < target; i++)
            {
                var expected = _history[i];
                float deadline = Time.realtimeSinceStartup + 15f;
                while (_environment.IsReady && !_environment.CanRequestDecision
                       && Time.realtimeSinceStartup < deadline)
                    yield return null;

                if (!_environment.IsReady)
                {
                    FailReplay("Episode ended before replay reached decision " + i + ".");
                    yield break;
                }
                if (!_environment.CanRequestDecision)
                {
                    FailReplay("Timeout waiting for decision boundary " + i + ".");
                    yield break;
                }

                var frame = _environment.Bridge.Frame;
                var c = frame?.Candidates[expected.selectedSlot];
                string actualId = c?.Id;
                if (c == null || !string.Equals(actualId, expected.selectedCandidateId, StringComparison.Ordinal))
                {
                    FailReplay(
                        "NONDETERMINISTIC_REPLAY at decision " + i +
                        ": expected slot/id " + expected.selectedSlot + "/" + expected.selectedCandidateId +
                        ", actual " + expected.selectedSlot + "/" + (actualId ?? "<masked>") + ".");
                    yield break;
                }

                int before = _environment.Diagnostics.Decisions;
                if (!_environment.Step(expected.selectedSlot))
                {
                    FailReplay("Recorded slot " + expected.selectedSlot + " was rejected at " + i + ".");
                    yield break;
                }

                deadline = Time.realtimeSinceStartup + 15f;
                while (_environment.IsReady
                       && (_environment.Diagnostics.Decisions <= before || !_environment.CanRequestDecision)
                       && Time.realtimeSinceStartup < deadline)
                    yield return null;

                if (Time.realtimeSinceStartup >= deadline)
                {
                    FailReplay("Timeout executing recorded decision " + i + ".");
                    yield break;
                }
            }

            _cursor = target;
            _replaying = false;
            Time.timeScale = 0f;
            _status = target == _history.Count
                ? "Returned to live tip by deterministic replay"
                : "Historical boundary " + target + " reconstructed";
        }

        private void FailReplay(string reason)
        {
            _replaying = false;
            _replayError = reason;
            Time.timeScale = 0f;
            _status = "Replay stopped";
            Debug.LogError("MOYVA_MODEL_INSPECTOR_REPLAY_FAILED: " + reason, this);
        }

        public void RestartSameSeed()
        {
            if (_replaying) return;
            StopAllCoroutines();
            _history.Clear();
            _pending = null;
            _cursor = 0;
            _replayError = null;
            _autoPlay = false;
            _singleStepPending = false;
            _environment.ResetForInspectorReplay(_episodeSeed);
            _environment.BeginEpisode();
            BindDecisionTrace();
            AppendEpisodeRecord("restart");
            Pause("Restarted same deterministic seed " + _episodeSeed);
        }

        public void RestartNextSeed()
        {
            _episodeSeed = unchecked(_episodeSeed + 1);
            RestartSameSeed();
        }

        private void WriteSessionHeader()
        {
            try
            {
                var header = new SessionHeader
                {
                    runId = _bootstrap.Config.inspectorRunId,
                    checkpointId = _bootstrap.Config.inspectorCheckpoint,
                    checkpointStep = _bootstrap.Config.inspectorCheckpointStep,
                    contractHash = _bootstrap.Config.inspectorContractHash,
                    modelSha256 = _bootstrap.Config.inspectorModelSha256,
                    scenarioId = _bootstrap.Config.inspectorScenarioId,
                    seed = _episodeSeed,
                    startedUtc = DateTime.UtcNow.ToString("O")
                };
                File.WriteAllText(
                    Path.Combine(_sessionDirectory, "session.json"),
                    JsonUtility.ToJson(header, true));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Inspector could not write session.json: " + exception.Message);
            }
        }

        private void AppendRecord(DecisionRecord record)
        {
            if (string.IsNullOrWhiteSpace(_recordsPath) || record == null) return;
            try
            {
                string directory = Path.GetDirectoryName(_recordsPath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                File.AppendAllText(_recordsPath, JsonUtility.ToJson(record, false) + Environment.NewLine);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Inspector could not append record: " + exception.Message);
            }
        }

        private void AppendEpisodeRecord(string reason)
        {
            if (string.IsNullOrWhiteSpace(_episodesPath) || _environment == null) return;
            try
            {
                var entry = new EpisodeRecord
                {
                    episodeId = _environment.EpisodeId,
                    seed = _environment.Diagnostics.LastSeed,
                    scenarioId = _environment.Scenario?.id,
                    decisionSequence = _history.Count,
                    reason = reason,
                    utc = DateTime.UtcNow.ToString("O")
                };
                File.AppendAllText(_episodesPath, JsonUtility.ToJson(entry, false) + Environment.NewLine);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Inspector could not append episode record: " + exception.Message);
            }
        }

        private DecisionRecord DisplayedRecord()
        {
            if (_history.Count == 0) return null;
            if (_cursor < _history.Count) return _history[_cursor];
            return _history[_history.Count - 1];
        }

        private BotDecisionFrame CurrentFrame() => _environment?.Bridge?.Frame;

        private float[] DisplayedObservations()
        {
            if (_cursor < _history.Count) return _history[_cursor].observations;
            var frame = CurrentFrame();
            return frame == null ? Array.Empty<float>() : frame.Observations.ToArray();
        }

        private CandidateView[] DisplayedCandidates()
        {
            if (_cursor < _history.Count) return _history[_cursor].candidates;
            var frame = CurrentFrame();
            if (frame == null) return Array.Empty<CandidateView>();
            var result = new List<CandidateView>();
            for (int i = 0; i < frame.Candidates.Count; i++)
            {
                var c = frame.Candidates[i];
                if (c == null) continue;
                result.Add(new CandidateView
                {
                    slot = i, id = c.Id, capability = c.Capability.ToString(),
                    intent = c.Intent.ToString(), actor = c.ActorKey, target = c.TargetKey,
                    x = c.X, y = c.Y, critical = c.Critical, features = c.Features.ToArray()
                });
            }
            return result.ToArray();
        }

        private void OnGUI()
        {
            if (_bootstrap == null || !_bootstrap.Config.inspectorMode) return;
            HandleKeyboard();

            float scale = Mathf.Clamp(Screen.width / 1600f, 0.65f, 1.35f);
            var old = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            DrawTopBar(width);
            DrawMainPanel(width, height);
            GUI.matrix = old;
        }

        private void HandleKeyboard()
        {
            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown) return;
            if (e.keyCode == KeyCode.Space) { ToggleAutoPlay(); e.Use(); }
            else if (e.keyCode == KeyCode.RightArrow) { StepModel(); e.Use(); }
            else if (e.keyCode == KeyCode.LeftArrow)
            {
                if (_history.Count > 0) NavigateTo(Mathf.Max(0, _cursor - 1));
                e.Use();
            }
            else if (e.keyCode == KeyCode.Home)
            {
                if (_history.Count > 0) NavigateTo(0);
                e.Use();
            }
            else if (e.keyCode == KeyCode.End)
            {
                if (_history.Count > 0) NavigateTo(_history.Count);
                e.Use();
            }
            else if (e.keyCode == KeyCode.R) { RestartSameSeed(); e.Use(); }
        }

        private void DrawTopBar(float width)
        {
            GUILayout.BeginArea(new Rect(8, 8, width - 16, 112), GUI.skin.box);
            GUILayout.Label(
                "MOYVA • FROZEN MODEL INSPECTOR | checkpoint " +
                _bootstrap.Config.inspectorCheckpointStep + " | scenario " +
                _bootstrap.Config.inspectorScenarioId + " | seed " + _episodeSeed);

            GUILayout.BeginHorizontal();
            GUI.enabled = !_replaying;
            if (GUILayout.Button(_autoPlay ? "Pause [Space]" : "Play [Space]", GUILayout.Width(120)))
                ToggleAutoPlay();
            if (GUILayout.Button("Model step →", GUILayout.Width(120))) StepModel();
            GUI.enabled = !_replaying && _history.Count > 0;
            if (GUILayout.Button("← Prev", GUILayout.Width(85))) NavigateTo(Mathf.Max(0, _cursor - 1));
            if (GUILayout.Button("Next →", GUILayout.Width(85))) NavigateTo(Mathf.Min(_history.Count, _cursor + 1));
            if (GUILayout.Button("First", GUILayout.Width(70))) NavigateTo(0);
            if (GUILayout.Button("Live tip", GUILayout.Width(80))) NavigateTo(_history.Count);
            GUI.enabled = !_replaying;
            if (GUILayout.Button("Restart seed", GUILayout.Width(105))) RestartSameSeed();
            if (GUILayout.Button("Seed +1", GUILayout.Width(75))) RestartNextSeed();
            GUI.enabled = true;

            GUILayout.Label("Speed", GUILayout.Width(45));
            foreach (float speed in new[] { 0.25f, 0.5f, 1f, 2f, 4f })
            {
                if (GUILayout.Button(speed + "×", GUILayout.Width(46)))
                {
                    _runSpeed = speed;
                    if (_autoPlay) Time.timeScale = speed;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Cursor: " + (_cursor >= _history.Count ? "LIVE TIP" : _cursor.ToString())
                + " / " + _history.Count + " decisions | " + _status);
            if (!string.IsNullOrEmpty(_replayError))
                GUILayout.Label("REPLAY ERROR: " + _replayError);
            GUILayout.EndArea();
        }

        private void DrawMainPanel(float width, float height)
        {
            const float top = 126f;
            GUILayout.BeginArea(new Rect(8, top, width - 16, height - top - 8), GUI.skin.box);

            GUILayout.BeginHorizontal();
            foreach (InspectorTab value in Enum.GetValues(typeof(InspectorTab)))
                if (GUILayout.Toggle(_tab == value, value.ToString(), GUI.skin.button, GUILayout.Width(116)))
                    _tab = value;
            GUILayout.FlexibleSpace();
            _details = GUILayout.Toggle(_details, "details", GUILayout.Width(70));
            _showZeros = GUILayout.Toggle(_showZeros, "zeros", GUILayout.Width(60));
            GUILayout.EndHorizontal();

            _mainScroll = GUILayout.BeginScrollView(_mainScroll);
            switch (_tab)
            {
                case InspectorTab.Overview: DrawOverview(); break;
                case InspectorTab.Observations: DrawObservations(); break;
                case InspectorTab.Spatial: DrawSpatial(); break;
                case InspectorTab.Candidates: DrawCandidates(); break;
                case InspectorTab.Timeline: DrawTimeline(); break;
                default: DrawRaw(); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawOverview()
        {
            var env = _environment;
            var record = DisplayedRecord();
            var frame = CurrentFrame();

            GUILayout.Label("MODEL");
            GUILayout.Label("Mode: ML-Agents InferenceOnly • frozen ONNX • no trainer/optimizer");
            GUILayout.Label("Run: " + _bootstrap.Config.inspectorRunId);
            GUILayout.Label("Checkpoint: " + _bootstrap.Config.inspectorCheckpointStep);
            GUILayout.Label("Model SHA-256: " + _bootstrap.Config.inspectorModelSha256);
            GUILayout.Label("Contract: " + _bootstrap.Config.inspectorContractHash);

            GUILayout.Space(8);
            GUILayout.Label("CURRENT GAME STATE");
            GUILayout.Label("Episode " + env.EpisodeId + " • seed " + env.Diagnostics.LastSeed
                + " • scenario " + (env.Scenario?.id ?? "<none>")
                + " • scenario step " + (env.ScenarioProgress?.StepIndex ?? -1));
            GUILayout.Label("Decisions " + env.Diagnostics.Decisions + " • turns " + env.Diagnostics.Turns
                + " • reward " + env.Rewards.TotalReward.ToString("F4")
                + " • result " + env.Result);
            GUILayout.Label("Can request decision: " + env.CanRequestDecision
                + " • real gameplay: " + env.IsRealGameplay
                + " • candidates " + (frame?.Candidates.Count ?? 0));

            GUILayout.Space(8);
            GUILayout.Label("DISPLAYED DECISION");
            if (record == null)
            {
                GUILayout.Label("No completed model decision yet. Press “Model step →”.");
            }
            else
            {
                GUILayout.Label("#" + record.index + " • frame " + record.frameSequence
                    + " • observation hash " + record.observationHash);
                GUILayout.Label("Selected slot " + record.selectedSlot
                    + " • " + record.selectedIntent + "/" + record.selectedCapability
                    + " • " + record.selectedCandidateId);
                GUILayout.Label("Actor " + record.selectedActor + " • target " + record.selectedTarget
                    + " • cell (" + record.selectedX + "," + record.selectedY + ")");
                GUILayout.Label("Execution " + record.executionResult + " • failure "
                    + record.executionFailure + " • reason " + record.executionReason);
                GUILayout.Label("Reward " + record.rewardBefore.ToString("F4") + " → "
                    + record.rewardAfter.ToString("F4") + " (Δ "
                    + record.rewardDelta.ToString("F4") + ") • latency "
                    + record.latency.ToString("F4"));
            }

            GUILayout.Space(8);
            GUILayout.Label("AUDIT GUARANTEES");
            GUILayout.Label("• exact normalized vector passed to ML-Agents");
            GUILayout.Label("• exact legal candidate/action mask");
            GUILayout.Label("• selected discrete slot returned by the frozen policy");
            GUILayout.Label("• authoritative execution result and reward delta");
            GUILayout.Label("• rewind replays recorded slots from the same seed and validates candidate IDs");
            GUILayout.Label("PPO logits/probabilities are not exposed by this Agent API and are not fabricated.");
        }

        private void DrawObservations()
        {
            var obs = DisplayedObservations();
            if (obs.Length == 0) { GUILayout.Label("No decision frame."); return; }

            GUILayout.Label("Exact policy input: " + obs.Length + " floats");
            GUILayout.Label("Global [0..95] • Spatial [96..735] • Candidates [736..4831]");
            GUILayout.Label("index • BotObservationSchema semantic name • raw vector value • normalized policy value");
            for (int i = 0; i < Math.Min(BotDecisionContract.GlobalFeatureCount, obs.Length); i++)
            {
                if (!_showZeros && Mathf.Approximately(obs[i], 0f)) continue;
                GUILayout.Label(string.Format("[{0,2}] {1,-26} raw={2,10:F6} normalized={2,10:F6}", i, GlobalName(i), obs[i]));
            }
        }

        private void DrawSpatial()
        {
            var obs = DisplayedObservations();
            if (obs.Length < BotObservationSchema.CandidateOffset)
            {
                GUILayout.Label("No spatial observation.");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Spatial channel", GUILayout.Width(100));
            for (int c = 0; c < BotDecisionContract.SpatialChannels; c++)
                if (GUILayout.Toggle(_spatialChannel == c, c.ToString(), GUI.skin.button, GUILayout.Width(42)))
                    _spatialChannel = c;
            GUILayout.EndHorizontal();

            GUILayout.Label("Exact 8×8 values for channel " + _spatialChannel + ".");
            for (int y = 0; y < BotDecisionContract.SpatialSize; y++)
            {
                GUILayout.BeginHorizontal();
                for (int x = 0; x < BotDecisionContract.SpatialSize; x++)
                {
                    int cell = y * BotDecisionContract.SpatialSize + x;
                    int index = BotObservationSchema.SpatialOffset
                        + cell * BotDecisionContract.SpatialChannels + _spatialChannel;
                    GUILayout.Label(obs[index].ToString("0.000"), GUI.skin.box, GUILayout.Width(62));
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawCandidates()
        {
            var candidates = DisplayedCandidates();
            var selected = DisplayedRecord()?.selectedSlot ?? -1;
            GUILayout.Label("Legal actions: " + candidates.Length + " / "
                + BotDecisionContract.MaxCandidateSlots + ". Other slots are masked.");

            _candidateScroll = GUILayout.BeginScrollView(_candidateScroll, GUILayout.Height(520));
            foreach (var c in candidates)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label((c.slot == selected ? "▶ " : "") + "slot " + c.slot
                    + " • " + c.intent + "/" + c.capability + " • " + c.id);
                GUILayout.Label("actor=" + c.actor + " target=" + c.target
                    + " cell=(" + c.x + "," + c.y + ") critical=" + c.critical);

                if (_details && c.features != null)
                {
                    int shown = 0;
                    GUILayout.BeginHorizontal();
                    for (int i = 0; i < c.features.Length; i++)
                    {
                        if (!_showZeros && Mathf.Approximately(c.features[i], 0f)) continue;
                        GUILayout.Label(CandidateFeatureName(i) + "=" + c.features[i].ToString("0.###"),
                            GUILayout.Width(150));
                        shown++;
                        if (shown % 6 == 0) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            string[] unavailable = null;
            if (_cursor < _history.Count) unavailable = _history[_cursor].unavailable;
            else
            {
                var frame = CurrentFrame();
                if (frame != null)
                    unavailable = frame.Unavailable.Select(x => x.Key + ": " + x.Value).ToArray();
            }
            if (unavailable != null && unavailable.Length > 0)
            {
                GUILayout.Label("Unavailable capabilities");
                foreach (var value in unavailable) GUILayout.Label("• " + value);
            }
        }

        private void DrawTimeline()
        {
            GUILayout.Label("Click a decision to reconstruct the world immediately BEFORE that model decision.");
            _timelineScroll = GUILayout.BeginScrollView(_timelineScroll, GUILayout.Height(560));
            for (int i = 0; i < _history.Count; i++)
            {
                var r = _history[i];
                GUILayout.BeginHorizontal(GUI.skin.box);
                if (GUILayout.Button((_cursor == i ? "▶ " : "") + "#" + i, GUILayout.Width(60)))
                    NavigateTo(i);
                GUILayout.Label("slot " + r.selectedSlot, GUILayout.Width(58));
                GUILayout.Label(r.selectedIntent ?? "", GUILayout.Width(90));
                GUILayout.Label(r.selectedCapability ?? "", GUILayout.Width(105));
                GUILayout.Label(r.selectedCandidateId ?? "", GUILayout.Width(360));
                GUILayout.Label(r.executionResult ?? "", GUILayout.Width(90));
                GUILayout.Label("ΔR " + r.rewardDelta.ToString("0.###"), GUILayout.Width(90));
                GUILayout.Label("obs " + r.observationHash, GUILayout.Width(110));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            if (_history.Count > 0 && GUILayout.Button("Return to live tip by replaying all recorded decisions"))
                NavigateTo(_history.Count);
        }

        private void DrawRaw()
        {
            var obs = DisplayedObservations();
            var record = DisplayedRecord();
            GUILayout.Label("RAW / AUDIT");
            GUILayout.Label("Session directory: " + _sessionDirectory);
            GUILayout.Label("Decision JSONL: " + _recordsPath);
            GUILayout.Label("Episodes JSONL: " + _episodesPath);
            GUILayout.Label("Observation floats: " + obs.Length);
            GUILayout.Label("Cursor: " + _cursor + " / " + _history.Count);
            GUILayout.Label("Replay active: " + _replaying);
            if (record != null)
                GUILayout.TextArea(JsonUtility.ToJson(record, true), GUILayout.MinHeight(420));
        }

        private static string GlobalName(int i)
        {
            var feature = BotDecisionContract.Spec.Feature(i);
            if (feature == null)
                return "global." + i;
            if (feature.count <= 1)
                return BotDecisionContract.Spec.Label(i);
            if (feature.index == BotObservationSchema.Capabilities
                && i - feature.index < Enum.GetValues(typeof(BotCapabilityId)).Length)
                return "capability." + ((BotCapabilityId)(i - feature.index)).ToString().ToLowerInvariant();
            return feature.name + "." + (i - feature.index);
        }

        private static string CandidateFeatureName(int i)
        {
            if (i == 0) return "present";
            if (i >= 1 && i <= 12) return "intent." + ((BotIntentType)(i - 1));
            switch (i)
            {
                case 13: return "distance";
                case 14: return "cost";
                case 15: return "ownHp";
                case 16: return "targetHp";
                case 17: return "visible";
                case 18: return "path";
                case 19: return "complete";
                case 20: return "buildingType";
                case 21: return "unitTypeOrRole";
                case 22: return "purpose";
                case 23: return "baseVision";
                case 24: return "attackPower";
                case 25: return "durability";
                case 26: return "terrainHeight";
                case 27: return "x";
                case 28: return "y";
                case 29: return "effectiveVision";
                case 30: return "heightAdvantage";
                case 31: return "rangedCombat";
                default: return "f" + i;
            }
        }

        private void OnDestroy()
        {
            if (_boundOrchestrator != null)
                _boundOrchestrator.DecisionFinished -= OnDecisionFinished;
            if (_environment != null)
                _environment.EpisodeReset -= OnEpisodeReset;
            if (Instance == this) Instance = null;
            Time.timeScale = _previousTimeScale;
        }
    }
}
