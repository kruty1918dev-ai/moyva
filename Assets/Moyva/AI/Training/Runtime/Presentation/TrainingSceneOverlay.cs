using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingSceneOverlay : MonoBehaviour
    {
        private TrainingPresentationController _presentation;
        private TrainingBootstrap _bootstrap;
        public void Configure(TrainingPresentationController presentation, TrainingBootstrap bootstrap)
        { _presentation = presentation; _bootstrap = bootstrap; }
        private void OnGUI()
        {
            if (_presentation == null || _presentation.Mode != TrainingPresentationMode.Visual
                || !_presentation.OverlayEnabled || _presentation.VisualizationPaused) return;
            GUILayout.BeginArea(new Rect(10, 10, 540, 340), GUI.skin.box);
            GUILayout.Label(_bootstrap.Status);
            GUILayout.Label(_bootstrap.Readiness?.Verdict ?? "NOT_READY_FOR_REAL_TRAINING");
            var environment = _presentation.Selected;
            if (environment != null)
            {
                var d = environment.Diagnostics;
                var last = environment.Bridge.Telemetry.Last;
                GUILayout.Label($"Environment {environment.EnvironmentId} | Episode {environment.EpisodeId} | Seed {d.LastSeed}");
                GUILayout.Label($"Stage {environment.Stage} | Policy {_bootstrap.Config.behaviorType} | Turn {d.Turns} | Decision {d.Decisions}");
                GUILayout.Label($"Reward {environment.Rewards.TotalReward:F4} | Shaping {environment.Rewards.ShapingReward:F4} | Candidates {environment.Bridge.Frame?.Candidates.Count ?? 0}");
                GUILayout.Label($"Last: {last?.Intent} / {last?.Capability} / {last?.Result}");
                var candidate = environment.LastCandidate;
                if (candidate != null) GUILayout.Label($"Actor {candidate.ActorKey} -> Target {candidate.TargetKey} ({candidate.X}, {candidate.Y})");
                GUILayout.Label(d.LastError ?? environment.Limitation ?? "");
            }
            else if (_bootstrap.Readiness != null) GUILayout.Label(string.Join("\n", _bootstrap.Readiness.Blockers));
            GUILayout.EndArea();
        }
    }
}
