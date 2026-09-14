using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    // Spectator controls change presentation/simulation pacing, never submit learner actions.
    public sealed class TrainingSceneOverlay : MonoBehaviour
    {
        private TrainingPresentationController _presentation;
        private TrainingBootstrap _bootstrap;
        private bool _details = true;
        public void Configure(TrainingPresentationController presentation, TrainingBootstrap bootstrap)
        { _presentation = presentation; _bootstrap = bootstrap; }
        private void OnGUI()
        {
            if (_presentation == null || _presentation.Mode != TrainingPresentationMode.Visual) return;
            float scale = Mathf.Clamp(Screen.width / 1280f, 0.7f, 1.5f);
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            GUILayout.BeginArea(new Rect(10, 10, 570, _details ? 310 : 100), GUI.skin.box);
            GUILayout.Label("MOYVA • LIVE GAMEPLAY  |  Blue: learner  •  Red: heuristic opponent");
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Speed {Time.timeScale:0.0}×", GUILayout.Width(90));
            foreach (float speed in new[] { 0.1f, 1f, 2f, 5f, 10f, 20f })
                if (GUILayout.Button(speed + "×")) _bootstrap.Performance.SetSpeed(speed);
            if (GUILayout.Button(_details ? "Hide details" : "Details")) _details = !_details;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Zoom +")) _presentation.Zoom(0.8f);
            if (GUILayout.Button("Zoom −")) _presentation.Zoom(1.25f);
            if (GUILayout.Button("Fit map")) _presentation.Zoom(0);
            GUILayout.Label("Arrow keys: pan • wheel: zoom");
            GUILayout.EndHorizontal();
            var environment = _presentation.Selected;
            if (_details && environment != null)
            {
                var d = environment.Diagnostics;
                var last = environment.Bridge.Telemetry.Last;
                GUILayout.Label($"{environment.Stage} | {(_bootstrap.Config.learnInitialCastle ? "Goal: place your first castle, then develop your settlement" : "Practice unlocked game capabilities")}");
                GUILayout.Label($"Episode {environment.EpisodeId} • Seed {d.LastSeed} • Turn {d.Turns} • Decisions {d.Decisions}");
                GUILayout.Label($"Reward {environment.Rewards.TotalReward:F3} • Legal candidates {environment.Bridge.Frame?.Candidates.Count ?? 0}");
                GUILayout.Label($"Action: {last?.Intent} / {last?.Capability} / {last?.Result}");
                var candidate = environment.LastCandidate;
                if (candidate != null) GUILayout.Label($"Target: {candidate.TargetKey}  cell ({candidate.X}, {candidate.Y})");
                var world = environment.GameplayEpisode;
                if (world != null)
                {
                    int learner = 0, opponent = 0;
                    foreach (var id in world.Units.GetAllUnitIds())
                        if (world.UnitOwners.GetUnitOwnerId(id) == TrainingGameplayScope.LearnerId) learner++; else opponent++;
                    GUILayout.Label($"Units: learner {learner} / opponent {opponent} • Buildings: {(world.EconomyInstalled ? world.Placements.GetSavedPlacements().Count : 0)}");
                }
                GUILayout.Label(d.LastError ?? "The trainer chooses actions. Watching does not prove this skill has been learned.");
            }
            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
