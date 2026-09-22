using System;
using System.IO;
using System.Linq;
using Kruty1918.Telemetry.Serialization;
using Kruty1918.Telemetry.Unity;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Telemetry.EditorTools
{
    /// <summary>
    /// Minimal dashboard: live diagnostics while playing, local spool inspection,
    /// contract catalog export, and fingerprint diff against a saved baseline.
    /// Read-only tooling — never mutates gameplay.
    /// </summary>
    public sealed class TelemetryDashboardWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _baselineFingerprintPath = "";
        private string _message = "";

        [MenuItem("Tools/Telemetry/Dashboard")]
        public static void Open() => GetWindow<TelemetryDashboardWindow>("Telemetry");

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawRuntimeSection();
            EditorGUILayout.Space(8);
            DrawSpoolSection();
            EditorGUILayout.Space(8);
            DrawFingerprintSection();

            if (!string.IsNullOrEmpty(_message))
                EditorGUILayout.HelpBox(_message, MessageType.None);

            EditorGUILayout.EndScrollView();
            if (Application.isPlaying) Repaint();
        }

        private void DrawRuntimeSection()
        {
            EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
            var rt = TelemetryHostBehaviour.Runtime;
            if (rt == null)
            {
                EditorGUILayout.HelpBox(
                    "No telemetry runtime in this scene. Enter Play mode in a scene that " +
                    "installs telemetry (gameplay bootstrap).",
                    MessageType.Info);
                return;
            }
            var d = rt.Diagnostics;
            EditorGUILayout.LabelField("Session", rt.Session?.ApplicationSessionId ?? "-");
            EditorGUILayout.LabelField("Installation", rt.Session?.InstallationId ?? "-");
            EditorGUILayout.LabelField("Fingerprint", rt.Fingerprint.Value ?? "-");
            EditorGUILayout.LabelField("Events",
                $"tracked={d.EventsTracked} rejected={d.EventsRejected} dropped={d.EventsDropped}");
            EditorGUILayout.LabelField("Batches",
                $"stored={d.BatchesStored} uploaded={d.BatchesUploaded} " +
                $"quarantined={d.BatchesQuarantined} retries={d.UploadRetries}");
            if (d.LastError != null)
                EditorGUILayout.LabelField("Last error", d.LastError, EditorStyles.miniLabel);
            if (GUILayout.Button("Flush now")) rt.Sink.Flush();
            if (GUILayout.Button("Export contract catalog…"))
            {
                var path = EditorUtility.SaveFilePanel("Contract catalog", "", "contract-catalog.json", "json");
                if (!string.IsNullOrEmpty(path))
                    File.WriteAllText(path, Contracts.ContractCatalog.ToJson(rt.Contracts));
            }
        }

        private void DrawSpoolSection()
        {
            EditorGUILayout.LabelField("Local spool", EditorStyles.boldLabel);
            var rt = TelemetryHostBehaviour.Runtime;
            var root = rt?.Store?.RootPath
                ?? Path.Combine(Application.persistentDataPath, "telemetry", "spool");
            EditorGUILayout.LabelField(root, EditorStyles.miniLabel);
            if (!Directory.Exists(root))
            {
                EditorGUILayout.LabelField("(empty)");
                return;
            }
            foreach (var sub in new[] { "pending", "inflight", "quarantine" })
            {
                var dir = Path.Combine(root, sub);
                if (!Directory.Exists(dir)) continue;
                var files = Directory.GetFiles(dir, "*.tbatch");
                EditorGUILayout.LabelField($"{sub}: {files.Length} batch(es)");
                foreach (var f in files.OrderByDescending(File.GetLastWriteTimeUtc).Take(8))
                    DrawBatchRow(f);
            }
        }

        private void DrawBatchRow(string path)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Path.GetFileName(path), EditorStyles.miniLabel);
                if (GUILayout.Button("inspect", GUILayout.Width(60))) InspectBatch(path);
            }
        }

        private void InspectBatch(string path)
        {
            try
            {
                var batch = BatchFileCodec.Decode(File.ReadAllBytes(path));
                _message = $"{batch.BatchId}\n" +
                           $"events={batch.EventCount} payload={batch.Payload?.Length ?? 0}B " +
                           $"uncompressed={batch.UncompressedBytes}B priority={batch.Priority}\n" +
                           $"sha256={batch.PayloadSha256}\n" +
                           $"fingerprint={batch.DatasetFingerprint}";
            }
            catch (Exception e)
            {
                _message = $"{Path.GetFileName(path)}: corrupt — {e.Message}";
            }
        }

        private void DrawFingerprintSection()
        {
            EditorGUILayout.LabelField("Fingerprint diff", EditorStyles.boldLabel);
            _baselineFingerprintPath = EditorGUILayout.TextField("baseline file", _baselineFingerprintPath);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Diff vs baseline")) ComputeDiff();
                if (GUILayout.Button("Save current as baseline")) SaveBaseline();
            }
        }

        private void ComputeDiff()
        {
            var rt = TelemetryHostBehaviour.Runtime;
            if (rt == null) { _message = "no runtime"; return; }
            if (!File.Exists(_baselineFingerprintPath)) { _message = "baseline file not found"; return; }
            var baseline = File.ReadAllText(_baselineFingerprintPath).Trim();
            var current = rt.Fingerprint.Value;
            _message = baseline == current
                ? $"MATCH {current}"
                : $"CHANGED\nbaseline: {baseline}\ncurrent:  {current}\n" +
                  "(semantic inputs differ — creates a new dataset group server-side)";
        }

        private void SaveBaseline()
        {
            var rt = TelemetryHostBehaviour.Runtime;
            if (rt == null) { _message = "no runtime"; return; }
            var path = EditorUtility.SaveFilePanel("Fingerprint baseline", "", "fingerprint.txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, rt.Fingerprint.Value);
                _baselineFingerprintPath = path;
            }
        }
    }
}
