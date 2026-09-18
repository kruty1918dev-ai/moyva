using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Output;
using Kruty1918.Moyva.Marketing.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.EditorTools
{
    /// <summary>
    /// Editor-side run monitor. While a studio play-mode run is active it:
    /// 1) registers the Recorder backend on the runtime bridge (same domain
    ///    during play mode, re-asserted after domain reload);
    /// 2) polls the run status file for progress;
    /// 3) on exit, finalizes (contact sheet, log, batch quit).
    /// </summary>
    [InitializeOnLoad]
    public static class MarketingRunMonitor
    {
        private const string PendingKey = "Moyva.Marketing.RunPending";
        private const string BatchKey = "Moyva.Marketing.BatchRun";
        private const string QueueKey = "Moyva.Marketing.Queue";
        private const string StatusFileKey = "Moyva.Marketing.StatusFile";
        private const string TemplateKey = "Moyva.Marketing.Template";
        private const string StartedKey = "Moyva.Marketing.StartedUtc";

        private static MarketingRecorderPipeline _recorder;
        private static MarketingRunStatus _lastStatus;

        // SessionState survives the play-mode domain reload; plain statics
        // reset to MinValue and would trip the watchdog on the first Tick.
        private static DateTime RunStartedUtc
        {
            get => long.TryParse(SessionState.GetString(StartedKey, "0"), out long b) && b != 0
                ? DateTime.FromBinary(b) : DateTime.UtcNow;
            set => SessionState.SetString(StartedKey, value.ToBinary().ToString());
        }

        public static bool RunPending
        {
            get => SessionState.GetBool(PendingKey, false);
            set => SessionState.SetBool(PendingKey, value);
        }

        public static bool BatchRun
        {
            get => SessionState.GetBool(BatchKey, false);
            set => SessionState.SetBool(BatchKey, value);
        }

        public static MarketingRunStatus LastStatus => _lastStatus;

        static MarketingRunMonitor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += Tick;
        }

        /// <summary>Campaign pack: queue several recipes; each run starts after
        /// the previous finishes. The queue lives in SessionState (survives
        /// domain reload).</summary>
        public static void StartCampaign(IReadOnlyList<string> recipeIds,
            MarketingStudioSession.RunRequest template, bool batch)
        {
            if (recipeIds == null || recipeIds.Count == 0) return;
            var rest = new List<string>(recipeIds);
            string first = rest[0];
            rest.RemoveAt(0);
            SessionState.SetString(QueueKey, string.Join(";", rest));
            var req = template ?? new MarketingStudioSession.RunRequest();
            req.recipeId = first;
            StartRun(req, batch);
        }

        /// <summary>Arm a run: writes request, marks pending, opens the studio
        /// scene and enters play mode.</summary>
        public static void StartRun(MarketingStudioSession.RunRequest request, bool batch)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MarketingStudioSession.RequestFilePath));
            request.statusFile = Path.Combine(
                MarketingOutputLayout.ProjectRoot(), "MarketingOutput", ".cache",
                $"studio-status-{DateTime.UtcNow:HHmmss}.json");
            request.exitPlayModeOnFinish = true;
            MarketingStudioSession.WriteRequest(request);
            SessionState.SetString(StatusFileKey, request.statusFile);
            SessionState.SetString(TemplateKey, JsonConvert.SerializeObject(request));
            RunPending = true;
            BatchRun = batch;
            _lastStatus = null;
            RunStartedUtc = DateTime.UtcNow;

            MarketingSceneBuilder.EnsureSceneExists();
            if (EditorSceneManager.GetActiveScene().path != MarketingSceneBuilder.ScenePath)
                EditorSceneManager.OpenScene(MarketingSceneBuilder.ScenePath);
            EditorApplication.EnterPlaymode();
        }

        public static void CancelRun()
        {
            // Keep RunPending — the controller tears down, exits play mode and
            // FinalizeRun records the cancelled manifest (batch exits non-zero).
            if (EditorApplication.isPlaying)
            {
                var controller = UnityEngine.Object.FindFirstObjectByType<MarketingStudioController>();
                if (controller != null) controller.Cancel();
                else EditorApplication.isPlaying = false;
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode && RunPending)
            {
                _recorder = new MarketingRecorderPipeline();
                MarketingRuntimeBridge.Backend = _recorder;
            }
            if (change == PlayModeStateChange.EnteredEditMode && RunPending)
            {
                RunPending = false;
                FinalizeRun();
            }
        }

        private static void Tick()
        {
            if (!RunPending || !EditorApplication.isPlaying) return;
            // Re-assert backend — survives the domain reload race before the
            // runtime reaches its video phase.
            if (MarketingRuntimeBridge.Backend == null)
            {
                _recorder = new MarketingRecorderPipeline();
                MarketingRuntimeBridge.Backend = _recorder;
            }
            string statusFile = SessionState.GetString(StatusFileKey, string.Empty);
            if (string.IsNullOrEmpty(statusFile)) return;
            var status = MarketingRunStatus.Read(statusFile);
            if (status != null) _lastStatus = status;

            // Safety: runaway runs are cancelled after 30 min.
            if ((DateTime.UtcNow - RunStartedUtc).TotalMinutes > 30)
            {
                Debug.LogError("[MarketingStudio] Run exceeded 30 minutes — cancelling.");
                CancelRun();
            }
        }

        private static void FinalizeRun()
        {
            MarketingRuntimeBridge.Backend = null;
            _recorder = null;

            // Fresh read — the final status write may land after the last Tick.
            string statusFile = SessionState.GetString(StatusFileKey, string.Empty);
            var status = !string.IsNullOrEmpty(statusFile)
                ? MarketingRunStatus.Read(statusFile) ?? _lastStatus
                : _lastStatus;
            _lastStatus = status;

            // Campaign queue: start the next recipe before reporting done.
            string queue = SessionState.GetString(QueueKey, string.Empty);
            if (!string.IsNullOrEmpty(queue) && !(status?.cancelled ?? false))
            {
                var rest = new List<string>(queue.Split(';'));
                string next = rest[0];
                rest.RemoveAt(0);
                SessionState.SetString(QueueKey, string.Join(";", rest));
                var req = ReadTemplate();
                req.recipeId = next;
                req.batch = BatchRun;
                Debug.Log($"[MarketingStudio] Campaign continues → {next}");
                StartRun(req, BatchRun);
                return;
            }
            SessionState.SetString(QueueKey, string.Empty);

            if (status != null && !string.IsNullOrEmpty(status.manifestPath)
                && File.Exists(status.manifestPath))
            {
                var manifest = JsonConvert.DeserializeObject<MarketingManifest>(
                    File.ReadAllText(status.manifestPath));
                string sheet = ContactSheetBuilder.Build(status.runFolder, manifest);
                Debug.Log($"[MarketingStudio] Run '{status.phase}' finished — " +
                    $"status={manifest.status}, files={manifest.filesGenerated.Count}, " +
                    $"run={status.runFolder}" +
                    (sheet != null ? $", contactSheet={sheet}" : ""));
                if (BatchRun)
                {
                    BatchRun = false;
                    EditorApplication.Exit(manifest.status == "ok" || manifest.status == "partial" ? 0 : 1);
                }
            }
            else if (BatchRun)
            {
                BatchRun = false;
                EditorApplication.Exit(2);
            }
            else
            {
                Debug.LogWarning("[MarketingStudio] Run ended without a readable status file.");
            }
        }

        private static MarketingStudioSession.RunRequest ReadTemplate()
        {
            try
            {
                string json = SessionState.GetString(TemplateKey, string.Empty);
                if (!string.IsNullOrEmpty(json))
                    return JsonConvert.DeserializeObject<MarketingStudioSession.RunRequest>(json);
            }
            catch { }
            return new MarketingStudioSession.RunRequest();
        }
    }
}
