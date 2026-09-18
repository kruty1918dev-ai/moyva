using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training.Editor
{
    public sealed class MoyvaTrainingControlCenter : EditorWindow
    {
        [Serializable] private sealed class Contract { public string hash; public int version; }
        [Serializable] private sealed class Run { public string run_id, state; public int stage, checkpoints; }
        [Serializable] private sealed class Checkpoint { public string relative, run_id; public bool compatible, final; public long bytes; }
        [Serializable] private sealed class Job { public string token, kind, state, run_id; public bool live; }
        [Serializable] private sealed class Dashboard { public string branch, commit; public bool dirty; public int stage; public Contract contract; public Run[] runs; public Checkpoint[] checkpoints; public Job[] processes; }
        private Dashboard _dashboard;
        private Vector2 _scroll;
        private string _preset = "smoke", _run = "", _message = "Refresh to load shared CLI data.", _log = "";
        private bool _busy;
        private double _nextRefresh;
        private int _tab;
        private static string Root => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        [MenuItem("Moyva/AI/Training Control Center")]
        public static void Open() => GetWindow<MoyvaTrainingControlCenter>("Moyva Control Center");
        private void OnInspectorUpdate()
        {
            if (!_busy && EditorApplication.timeSinceStartup > _nextRefresh)
            { _nextRefresh = EditorApplication.timeSinceStartup + 10; RunCli("status"); }
            Repaint();
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("MOYVA / TRAINING CONTROL CENTER", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_dashboard == null ? "Connecting to shared Python services…" :
                $"{_dashboard.branch} · {_dashboard.commit?.Substring(0, Math.Min(10, _dashboard.commit.Length))} · {(_dashboard.dirty ? "modified" : "clean")}");
            if (_dashboard?.contract != null) EditorGUILayout.SelectableLabel($"Contract v{_dashboard.contract.version}: {_dashboard.contract.hash}", GUILayout.Height(20));
            EditorGUILayout.LabelField("Curriculum", _dashboard?.stage.ToString() ?? "Unknown");
            _preset = EditorGUILayout.TextField("Preset", _preset);
            _run = EditorGUILayout.TextField("Run id (blank = generated)", _run);
            using (new EditorGUI.DisabledScope(_busy))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Start Training")) RunCli(string.IsNullOrWhiteSpace(_run) ? new[] { "train", "--preset", _preset } : new[] { "train", "--preset", _preset, "--run-id", _run });
                if (GUILayout.Button("Visual Training")) RunCli("train", "--preset", "visual-debug");
                if (GUILayout.Button("Build Player")) RunCli("build", "training", "--background");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Diagnose")) RunCli("doctor");
                if (GUILayout.Button("Setup / Repair")) RunCli("setup");
                if (GUILayout.Button("Training Monitor")) TrainingMonitorWindow.Open();
                if (GUILayout.Button("Refresh")) RunCli("status");
                EditorGUILayout.EndHorizontal();
            }
            foreach (var job in _dashboard?.processes ?? Array.Empty<Job>())
            {
                if (!job.live) continue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{job.kind}: {job.run_id} / {job.state}");
                if (GUILayout.Button("Stop safely", GUILayout.Width(90))) RunCli("stop", job.token);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.HelpBox(_message, MessageType.Info);
            _tab = GUILayout.Toolbar(_tab, new[] { "Runs", "Checkpoints", "Diagnostics / logs" });
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_tab == 0)
                foreach (var run in _dashboard?.runs ?? Array.Empty<Run>())
                {
                    EditorGUILayout.LabelField($"{run.run_id} — {run.state} · stage {run.stage} · {run.checkpoints} checkpoints", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Resume")) RunCli("run", "resume", run.run_id);
                    if (GUILayout.Button("Details / metrics")) RunCli("run", "show", run.run_id);
                    if (GUILayout.Button("Logs")) RunCli("run", "logs", run.run_id);
                    if (GUILayout.Button("Reveal")) RunCli("run", "reveal", run.run_id);
                    EditorGUILayout.EndHorizontal();
                }
            if (_tab == 1)
                foreach (var checkpoint in _dashboard?.checkpoints ?? Array.Empty<Checkpoint>())
                {
                    EditorGUILayout.LabelField(checkpoint.relative);
                    EditorGUILayout.LabelField($"{(checkpoint.compatible ? "COMPATIBLE" : "INCOMPATIBLE")} · {checkpoint.bytes / 1024} KiB · {(checkpoint.final ? "final" : "checkpoint")}");
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Details")) RunCli("checkpoint", "show", checkpoint.relative);
                    if (GUILayout.Button("Favorite")) RunCli("checkpoint", "favorite", checkpoint.relative);
                    if (GUILayout.Button("Reveal")) RunCli("checkpoint", "reveal", checkpoint.relative);
                    EditorGUILayout.EndHorizontal();
                }
            if (_tab == 2) EditorGUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private async void RunCli(params string[] arguments)
        {
            if (_busy) return;
            _busy = true;
            try
            {
                string python = Environment.GetEnvironmentVariable("MOYVA_PYTHON");
                if (string.IsNullOrWhiteSpace(python))
                {
                    python = Path.Combine(Root, ".venv-training", Application.platform == RuntimePlatform.WindowsEditor ? "Scripts/python.exe" : "bin/python");
                    if (!File.Exists(python)) python = Application.platform == RuntimePlatform.WindowsEditor ? "python" : "python3";
                }
                var start = new ProcessStartInfo(python) { WorkingDirectory = Root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
                start.ArgumentList.Add(Path.Combine(Root, "tools", "ai", "moyva_cli_main.py"));
                foreach (string argument in arguments) start.ArgumentList.Add(argument);
                using var process = Process.Start(start);
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit());
                string output = await outputTask, errors = await errorTask;
                _message = process.ExitCode == 0 ? "Completed: " + string.Join(" ", arguments) : "Failed: " + errors;
                if (arguments[0] == "status" && process.ExitCode == 0) _dashboard = JsonUtility.FromJson<Dashboard>(output);
                else { _log = output + errors; _tab = 2; }
            }
            catch (Exception error) { _message = error.Message + ". Run ./moyva setup in the project terminal."; }
            finally { _busy = false; Repaint(); }
        }
    }
}
