using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot.Editor
{
    public sealed class BotAIMonitorWindow : EditorWindow
    {
        private Vector2 _scroll;
        [MenuItem("Moyva/AI/Bot Monitor")]
        public static void Open() => GetWindow<BotAIMonitorWindow>("Bot Monitor");
        private void OnInspectorUpdate() { if (EditorApplication.isPlaying) Repaint(); }
        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            var views = Object.FindObjectsByType<BotTelemetryView>();
            if (views.Length == 0) EditorGUILayout.HelpBox("No live bot runtime. Enter Play Mode.", MessageType.Info);
            foreach (var view in views)
            {
                var runtime = view.Orchestrator;
                if (runtime == null) continue;
                var hub = runtime.Telemetry;
                var session = runtime.Session;
                EditorGUILayout.LabelField(hub.EnvironmentId < 0 ? "Production" : "Training #" + hub.EnvironmentId, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Policy / model", hub.PolicyMode + " / " + hub.ProfileName);
                EditorGUILayout.LabelField("Contract v" + BotDecisionContract.ContractVersion, BotDecisionContract.Hash);
                EditorGUILayout.LabelField("Compatibility", hub.FallbackReason ?? "Runtime contract active");
                EditorGUILayout.LabelField("Owner / turn", (session?.Owner ?? "None") + " / " + session?.Turn);
                EditorGUILayout.LabelField("Session / state", session?.Id + " / " + session?.State);
                EditorGUILayout.LabelField("Decisions / invalid / stale", session?.Decisions + " / " + session?.Invalid + " / " + session?.Stale);
                if (!string.IsNullOrEmpty(hub.FallbackReason)) EditorGUILayout.HelpBox("Fallback: " + hub.FallbackReason, MessageType.Warning);
                if (!string.IsNullOrEmpty(hub.LastError)) EditorGUILayout.HelpBox(hub.LastError, MessageType.Warning);
                var last = hub.Last;
                if (last != null)
                {
                    EditorGUILayout.LabelField("Last decision", last.Slot + " / " + last.Intent + " / " + last.Capability + " / " + last.Result);
                    EditorGUILayout.LabelField("Reason", last.Reason ?? "None");
                    EditorGUILayout.LabelField("Confidence", "N/A");
                    if (GUILayout.Button("Dump Last Decision"))
                        Debug.Log($"Bot owner={last.Player} turn={last.Turn} frame={last.Sequence} observations={last.ObservationHash} "
                            + $"candidates={last.CandidateCount} slot={last.Slot} intent={last.Intent} result={last.Result} reason={last.Reason}");
                }
                var frame = runtime.Frame;
                if (frame != null)
                {
                    foreach (var unavailable in frame.Unavailable)
                        EditorGUILayout.LabelField(unavailable.Key + ": unavailable", unavailable.Value);
                    EditorGUILayout.LabelField("Candidates / padding", frame.Candidates.Count + " / " + (128 - frame.Candidates.Count));
                    for (int slot = 0; slot < frame.Candidates.Count; slot++)
                    {
                        var c = frame.Candidates[slot];
                        EditorGUILayout.LabelField((last?.Slot == slot ? "> " : "") + slot + " legal " + c.Intent,
                            c.Capability + " " + c.ActorKey + " → " + c.TargetKey + " (" + c.X + "," + c.Y + ")");
                    }
                }
                var metrics = hub.Metrics;
                var episodes = metrics.Episodes;
                EditorGUILayout.LabelField("Learning health (heuristic)", BotLearningHealthEvaluator.Evaluate(metrics).ToString());
                if (episodes.Count != 0)
                {
                    var episode = episodes[episodes.Count - 1];
                    EditorGUILayout.LabelField("Episode / reward / shaping", episode.Episode + " / " + episode.Reward + " / " + episode.Shaping);
                    EditorGUILayout.LabelField("Rolling reward / win / timeout",
                        episodes.Average(x => x.Reward) + " / " + episodes.Average(x => x.Won ? 1f : 0f) + " / " + episodes.Average(x => x.Timeout ? 1f : 0f));
                }
                else EditorGUILayout.LabelField("Training", "No training signal");
                EditorGUILayout.LabelField("Invalid / stale rate", metrics.Invalid / (double)System.Math.Max(1, metrics.Decisions)
                    + " / " + metrics.Stale / (double)System.Math.Max(1, metrics.Decisions));
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}

