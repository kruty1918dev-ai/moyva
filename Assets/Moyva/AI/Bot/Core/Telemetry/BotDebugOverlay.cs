using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Kruty1918.Moyva.AI.Bot
{
    // Player-facing AI X-ray. Toggle with F9 during a bot match.
    // Shows only telemetry the bot already produced — never exposes
    // hidden game state to the policy or the player.
    public sealed class BotDebugOverlay : MonoBehaviour
    {
        private BotTelemetryHub _telemetry;
        private BotRuntimeConfig _config;
        private bool _visible;
        private readonly StringBuilder _intents = new StringBuilder(256);

        [Inject]
        public void Configure(BotTelemetryHub telemetry, BotRuntimeConfig config)
        {
            _telemetry = telemetry;
            _config = config;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f9Key.wasPressedThisFrame)
                _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible || _telemetry == null)
                return;

            float scale = Mathf.Clamp(Screen.width / 1280f, 0.7f, 1.5f);
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            GUILayout.BeginArea(new Rect(10, 10, 560, 430), GUI.skin.box);
            GUILayout.Label("MOYVA AI • X-RAY (F9 to hide)");
            GUILayout.Label($"Profile: {_telemetry.ProfileName ?? _config?.selectedDifficultyName ?? "?"} " +
                $"({_config?.selectedDifficultyId})  •  Mode: {_telemetry.PolicyMode}  •  " +
                $"ε={(_telemetry.ExplorationRate.HasValue ? _telemetry.ExplorationRate.Value.ToString("0.00") : "—")}");
            GUILayout.Label($"Contract: {Short(_telemetry.Last?.ContractHash ?? BotDecisionContract.Hash)}  •  " +
                $"Obs: {_telemetry.Last?.ObservationHash ?? "—"}");
            if (!string.IsNullOrEmpty(_telemetry.FallbackReason))
                GUILayout.Label("FALLBACK: " + _telemetry.FallbackReason);
            if (!string.IsNullOrEmpty(_telemetry.LastError))
                GUILayout.Label("ERROR: " + _telemetry.LastError);

            var last = _telemetry.Last;
            GUILayout.Label(last == null
                ? "Last decision: none yet"
                : $"Last: turn {last.Turn} seq {last.Sequence} • {last.Intent}/{last.Capability} → {last.Result} " +
                  $"• slot {last.Slot}/{last.RealCandidateCount} legal ({last.CandidateCount} slots) • {last.Latency:0.0} ms" +
                  (last.Failure != BotDecisionFailure.None ? $" • FAIL {last.Failure}" : ""));

            var m = _telemetry.Metrics;
            double invalid = m.Decisions == 0 ? 0 : m.Invalid / (double)m.Decisions;
            double stale = m.Decisions == 0 ? 0 : m.Stale / (double)m.Decisions;
            GUILayout.Label($"Decisions {m.Decisions} • invalid {m.Invalid} ({invalid:P1}) • stale {m.Stale} ({stale:P1}) • " +
                $"endTurns {m.EndTurns} • noLegal {_telemetry.NoLegalActionFallbackCount} • nonFinite {_telemetry.NonFiniteValues}");
            GUILayout.Label("Health: " + BotLearningHealthEvaluator.Evaluate(m));

            _intents.Length = 0;
            for (int i = 0; i < m.IntentCounts.Length; i++)
                if (m.IntentCounts[i] > 0)
                    _intents.Append((BotIntentType)i).Append(' ').Append(m.IntentCounts[i]).Append("  ");
            GUILayout.Label("Intents: " + (_intents.Length == 0 ? "—" : _intents.ToString()));

            var recent = _telemetry.Traces.TakeLast(6).Reverse().ToList();
            if (recent.Count > 0)
            {
                GUILayout.Label("Recent:");
                foreach (var t in recent)
                    GUILayout.Label($"  t{t.Turn} {t.Intent}/{t.Result} {(t.Failure == BotDecisionFailure.None ? "" : t.Failure + " ")}" +
                        $"c{t.RealCandidateCount} {t.Latency:0.0}ms");
            }
            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }

        private static string Short(string hash)
            => string.IsNullOrEmpty(hash) ? "—" : hash.Length <= 12 ? hash : hash.Substring(0, 12);
    }
}
