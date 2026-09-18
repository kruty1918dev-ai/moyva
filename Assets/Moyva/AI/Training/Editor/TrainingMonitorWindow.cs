using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kruty1918.Moyva.AI.Bot;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training.Editor
{
    public sealed class TrainingMonitorWindow : EditorWindow
    {
        private const string ConfigPath = "Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json";
        private TrainingBootstrap _bootstrap;
        private Vector2 _scroll;
        private int _window = 50;
        private TrainingPresentationMode _nextMode;
        [MenuItem("Moyva/AI/Training Monitor")]
        public static void Open() => GetWindow<TrainingMonitorWindow>("Training Monitor");
        private void OnEnable()
        {
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfigPath);
            if (json != null) _nextMode = TrainingConfig.Load(json).presentationMode;
        }
        private void OnInspectorUpdate()
        {
            if (_bootstrap == null) _bootstrap = UnityEngine.Object.FindFirstObjectByType<TrainingBootstrap>();
            if (EditorApplication.isPlaying && _bootstrap?.Config?.enableEditorTelemetry == true
                && _bootstrap.Config.presentationMode != TrainingPresentationMode.HeadlessFast) Repaint();
        }
        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _nextMode = (TrainingPresentationMode)EditorGUILayout.EnumPopup("Mode for next run", _nextMode);
            if (GUILayout.Button("Save next-run mode"))
            {
                var json = File.ReadAllText(ConfigPath);
                json = Regex.Replace(json, "(\"presentationMode\"\\s*:\\s*)[0-9]+", "${1}" + (int)_nextMode);
                File.WriteAllText(ConfigPath, json);
                AssetDatabase.ImportAsset(ConfigPath);
            }
            EditorGUILayout.LabelField("Status", EditorApplication.isPaused ? "Paused (Editor)" : _bootstrap?.Status ?? "Not running");
            var report = _bootstrap?.Readiness;
            EditorGUILayout.HelpBox(report?.ToString() ?? "NOT_READY_FOR_REAL_TRAINING: no initialization evidence.",
                report?.IsReady == true ? MessageType.Info : MessageType.Warning);
            if (_bootstrap?.Config == null) { EditorGUILayout.EndScrollView(); return; }
            var config = _bootstrap.Config;
            EditorGUILayout.LabelField("Mode / behavior", config.presentationMode + " / " + config.behaviorType);
            EditorGUILayout.LabelField("Contract v" + BotDecisionContract.ContractVersion, BotDecisionContract.Hash);
            var presentation = _bootstrap.Presentation;
            if (presentation != null)
            {
                presentation.ObservedEnvironment = EditorGUILayout.IntField("Observed environment", presentation.ObservedEnvironment);
                using (new EditorGUI.DisabledScope(presentation.Mode != TrainingPresentationMode.Visual))
                {
                    presentation.OverlayEnabled = EditorGUILayout.Toggle("Scene overlay", presentation.OverlayEnabled);
                    bool paused = EditorGUILayout.Toggle("Pause visualization only", presentation.VisualizationPaused);
                    if (paused != presentation.VisualizationPaused) presentation.VisualizationPaused = paused;
                }
            }
            float speed = EditorGUILayout.FloatField("Training time scale", Time.timeScale);
            if (speed != Time.timeScale) _bootstrap.Performance?.SetSpeed(speed);
            var metrics = _bootstrap.Environments?.Metrics;
            if (metrics == null) { EditorGUILayout.EndScrollView(); return; }
            EditorGUILayout.LabelField("Environment count", _bootstrap.Environments.Environments.Count.ToString());
            EditorGUILayout.LabelField("Decisions / real second", metrics.DecisionsPerSecond.ToString("F2"));
            EditorGUILayout.LabelField("Episodes / minute", metrics.EpisodesPerMinute.ToString("F2"));
            EditorGUILayout.LabelField("Mean episode seconds", metrics.AverageEpisodeDuration.ToString("F2"));
            var environment = presentation?.Selected;
            if (environment != null) DrawCurrent(environment);
            if (GUILayout.Button("Reset charts")) metrics.History.Clear();
            _window = EditorGUILayout.IntPopup("Recent episodes", _window, new[] { "20", "50", "100", "500" }, new[] { 20, 50, 100, 500 });
            var selected = metrics.History.Snapshot().Where(e => e.EnvironmentId == (presentation?.ObservedEnvironment ?? 0)).ToArray();
            var episodes = selected.Skip(Math.Max(0, selected.Length - _window)).ToArray();
            if (episodes.Length > 0) DrawHistory(episodes);
            else EditorGUILayout.LabelField("No episode history.");
            EditorGUILayout.EndScrollView();
        }
        private static void DrawCurrent(TrainingEnvironment environment)
        {
            var d = environment.Diagnostics;
            var trace = environment.Bridge.Telemetry.Last;
            int candidates = environment.Bridge.Frame?.Candidates.Count ?? 0;
            EditorGUILayout.LabelField("Real simulation", environment.IsRealGameplay ? "CONNECTED" : "SCAFFOLD / NOT REAL GAMEPLAY");
            EditorGUILayout.LabelField("Episode / seed / stage", $"{environment.EpisodeId} / {d.LastSeed} / {environment.Stage}");
            EditorGUILayout.LabelField("Result / real seconds", $"{environment.Result} / {environment.ElapsedSeconds:F2}");
            EditorGUILayout.LabelField("Turns / decisions", $"{d.Turns} / {d.Decisions}");
            EditorGUILayout.LabelField("Reward / shaping", $"{environment.Rewards.TotalReward:F4} / {environment.Rewards.ShapingReward:F4}");
            EditorGUILayout.LabelField("Invalid / stale", $"{d.InvalidActions} / {d.StaleActions}");
            EditorGUILayout.LabelField("Candidates / masked", $"{candidates} / {1 - candidates / (float)BotDecisionContract.MaxCandidateSlots:P1}");
            EditorGUILayout.LabelField("Last intent / capability / result", $"{trace?.Intent} / {trace?.Capability} / {trace?.Result}");
            EditorGUILayout.LabelField("Last error", d.LastError ?? "None");
            if (GUILayout.Button("Dump current environment summary"))
                Debug.Log($"Training env={environment.EnvironmentId} episode={environment.EpisodeId} seed={d.LastSeed} "
                    + $"stage={environment.Stage} result={environment.Result} turns={d.Turns} decisions={d.Decisions} "
                    + $"reward={environment.Rewards.TotalReward} candidates={candidates} last={trace?.Intent}/{trace?.Result} error={d.LastError}");
        }
        private static void DrawHistory(TrainingEpisodeSummary[] episodes)
        {
            var healthMetrics = TrainingMetricsHub.HealthMetrics(episodes);
            EditorGUILayout.LabelField("Learning health", BotLearningHealthEvaluator.Evaluate(healthMetrics).ToString());
            EditorGUILayout.LabelField("Health evidence", $"Invalid {healthMetrics.Invalid / (double)Math.Max(1, healthMetrics.Decisions):P1}; "
                + $"timeouts {episodes.Count(e => e.Result == TrainingEpisodeResult.Timeout) / (double)episodes.Length:P1}. Heuristic only.");
            DrawGraph("Episode reward", episodes.Select(e => e.Reward).ToArray());
            DrawGraph("Rolling average reward (20)", Rolling(episodes, e => e.Reward));
            DrawGraph("Win rate (20)", Rolling(episodes, e => e.Result == TrainingEpisodeResult.Victory ? 1 : 0));
            DrawGraph("Timeout rate (20)", Rolling(episodes, e => e.Result == TrainingEpisodeResult.Timeout ? 1 : 0));
            DrawGraph("Invalid action rate", episodes.Select(e => e.Invalid / (float)Math.Max(1, e.Decisions)).ToArray());
            DrawGraph("Episode length (decisions)", episodes.Select(e => (float)e.Decisions).ToArray());
            EditorGUILayout.LabelField("Completed action distribution", EditorStyles.boldLabel);
            long total = episodes.Sum(e => e.ActionCounts.Sum(v => (long)v));
            var intents = new[] { BotIntentType.EndTurn, BotIntentType.Move, BotIntentType.Attack, BotIntentType.Recruit, BotIntentType.Build, BotIntentType.Capture };
            long accounted = 0;
            foreach (var intent in intents)
            {
                long count = episodes.Sum(e => (long)e.ActionCounts[(int)intent]);
                accounted += count;
                EditorGUILayout.LabelField(intent.ToString(), $"{count} ({count / (double)Math.Max(1, total):P1})");
            }
            EditorGUILayout.LabelField("Wait / Other", $"{total - accounted} ({(total - accounted) / (double)Math.Max(1, total):P1})");
        }
        private static float[] Rolling(TrainingEpisodeSummary[] episodes, Func<TrainingEpisodeSummary, float> value)
        {
            var result = new float[episodes.Length];
            float sum = 0;
            for (int i = 0; i < episodes.Length; i++)
            {
                sum += value(episodes[i]);
                if (i >= 20) sum -= value(episodes[i - 20]);
                result[i] = sum / Math.Min(20, i + 1);
            }
            return result;
        }
        private static void DrawGraph(string label, float[] values)
        {
            EditorGUILayout.LabelField(label + $" | latest {values[values.Length - 1]:F3}");
            var rect = GUILayoutUtility.GetRect(100, 64, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f));
            if (Event.current.type != EventType.Repaint || values.Length < 2) return;
            float min = values.Min(), span = Mathf.Max(0.001f, values.Max() - min);
            var points = new Vector3[values.Length];
            for (int i = 0; i < values.Length; i++)
                points[i] = new Vector3(rect.x + rect.width * i / (values.Length - 1), rect.yMax - 3 - (rect.height - 6) * (values[i] - min) / span);
            Handles.BeginGUI();
            var previous = Handles.color;
            Handles.color = Color.cyan;
            Handles.DrawAAPolyLine(2, points);
            Handles.color = previous;
            Handles.EndGUI();
        }
    }
}
