using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Shared.Diagnostics
{
    /// <summary>
    /// Opt-in startup frame recorder for e2e visual verification.
    ///
    /// Disabled by default; zero cost in normal builds. Enable by either:
    ///   - env var MOYVA_STARTUP_CAPTURE=1 — output under MOYVA_CAPTURE_DIR
    ///     or Temp/MoyvaCaptures/&lt;runId&gt;/;
    ///   - marker file Temp/moyva-capture.on inside the project (editor).
    ///
    /// Captures the presented frame (UI included) via
    /// ScreenCapture.CaptureScreenshot at a 50 ms real-time cadence —
    /// Unity performs the encode/write asynchronously off the main thread.
    /// A JSONL timeline records run id, frame index, monotonic timestamp,
    /// scene and camera state per frame plus scene-change events.
    /// Bounded by duration/frame/disk caps; skipped frames are counted.
    /// </summary>
    internal static class StartupFrameCaptureBootstrap
    {
        private const string EnableEnvVar = "MOYVA_STARTUP_CAPTURE";
        private const string DirEnvVar = "MOYVA_CAPTURE_DIR";
        private const string MarkerFileName = "moyva-capture.on";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void MaybeStart()
        {
            if (!IsEnabled())
            {
                return;
            }

            var go = new GameObject("MoyvaStartupFrameCapture");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<StartupFrameCapture>();
        }

        private static bool IsEnabled()
        {
            if (!string.IsNullOrEmpty(
                    Environment.GetEnvironmentVariable(EnableEnvVar)))
            {
                return true;
            }

            var marker =
                Path.Combine(
                    Application.dataPath,
                    "..",
                    "Temp",
                    MarkerFileName);

            return File.Exists(marker);
        }

        internal static string ResolveOutputDir()
        {
            var fromEnv = Environment.GetEnvironmentVariable(DirEnvVar);
            if (!string.IsNullOrEmpty(fromEnv))
            {
                return fromEnv;
            }

            return Path.Combine(
                Application.dataPath,
                "..",
                "Temp",
                "MoyvaCaptures");
        }
    }

    internal sealed class StartupFrameCapture : MonoBehaviour
    {
        private const float IntervalSeconds = 0.05f;
        private const float MaxDurationSeconds = 90f;
        private const int MaxFrames = 1700;
        private const long MaxTotalBytes = 380L * 1024 * 1024;
        private const int TimelineFlushThreshold = 64;
        private const int DiskCheckInterval = 60;

        private string _outputDir;
        private string _timelinePath;
        private string _runId;
        private int _frameIndex;
        private int _skipped;
        private long _bytesWritten;
        private float _startedAt;
        private float _nextTick;
        private volatile bool _stopping;
        private float _lastFrameTime;
        private string _sceneName = string.Empty;
        private readonly StringBuilder _pendingLines = new StringBuilder();

        private void Awake()
        {
            _runId =
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")
                + "-"
                + Guid.NewGuid().ToString("N").Substring(0, 6);

            _outputDir =
                Path.Combine(
                    StartupFrameCaptureBootstrap.ResolveOutputDir(),
                    _runId);

            Directory.CreateDirectory(_outputDir);
            _timelinePath = Path.Combine(_outputDir, "timeline.jsonl");
            _startedAt = Time.realtimeSinceStartup;
            _nextTick = _startedAt;
            _sceneName = SceneManager.GetActiveScene().name;

            SceneManager.activeSceneChanged += OnSceneChanged;
            EmitEvent("capture-start", string.Empty);
            StartCoroutine(CaptureLoop());
        }

        private void OnSceneChanged(Scene previous, Scene next)
        {
            _sceneName = next.name;
            EmitEvent("scene-changed", next.name);
        }

        private System.Collections.IEnumerator CaptureLoop()
        {
            var wait = new WaitForEndOfFrame();

            while (!_stopping)
            {
                yield return wait;

                var now = Time.realtimeSinceStartup;
                if (now < _nextTick)
                {
                    continue;
                }

                // Preserve real-time cadence without accumulating drift.
                _nextTick = Mathf.Max(now, _nextTick + IntervalSeconds);
                _lastFrameTime = now;
                CaptureOne(now);

                if (now - _startedAt > MaxDurationSeconds
                    || _frameIndex >= MaxFrames)
                {
                    _stopping = true;
                }
            }
        }

        private void CaptureOne(float now)
        {
            var camera = Camera.main;
            var camPos = camera != null ? camera.transform.position : Vector3.zero;
            var frame = _frameIndex++;

            var path =
                Path.Combine(
                    _outputDir,
                    "frame_" + frame.ToString("D5") + ".png");

            // CaptureScreenshot encodes+writes asynchronously on the render
            // thread; frames the pipe cannot absorb are counted at teardown.
            ScreenCapture.CaptureScreenshot(path);

            if (frame % DiskCheckInterval == 0 && OverDiskBudget())
            {
                _stopping = true;
            }

            _pendingLines.Append(
                "{\"type\":\"frame\",\"run\":\"" + _runId
                + "\",\"frame\":" + frame
                + ",\"t\":" + now.ToString("0.000", CultureInfo.InvariantCulture)
                + ",\"scene\":\"" + _sceneName
                + "\",\"cam\":[" + camPos.x.ToString("0.00", CultureInfo.InvariantCulture)
                + "," + camPos.y.ToString("0.00", CultureInfo.InvariantCulture)
                + "," + camPos.z.ToString("0.00", CultureInfo.InvariantCulture) + "]}\n");
        }

        private void EmitEvent(string kind, string detail)
        {
            _pendingLines.Append(
                "{\"type\":\"event\",\"run\":\"" + _runId
                + "\",\"event\":\"" + kind
                + "\",\"t\":" + Time.realtimeSinceStartup.ToString("0.000", CultureInfo.InvariantCulture)
                + ",\"scene\":\"" + _sceneName
                + "\",\"detail\":\"" + detail + "\"}\n");
        }

        private bool OverDiskBudget()
        {
            long total = 0;
            foreach (var f in new DirectoryInfo(_outputDir).GetFiles("frame_*.png"))
            {
                total += f.Length;
            }

            _bytesWritten = total;
            return total > MaxTotalBytes;
        }

        private void LateUpdate()
        {
            if (_pendingLines.Length > TimelineFlushThreshold * 120)
            {
                FlushTimeline();
            }
        }

        private void FlushTimeline()
        {
            if (_pendingLines.Length == 0)
            {
                return;
            }

            File.AppendAllText(_timelinePath, _pendingLines.ToString());
            _pendingLines.Length = 0;
        }

        private void OnDestroy()
        {
            _stopping = true;
            SceneManager.activeSceneChanged -= OnSceneChanged;
            FlushTimeline();

            // Count frames the async writer never produced.
            OverDiskBudget();
            var written =
                new DirectoryInfo(_outputDir).GetFiles("frame_*.png").Length;
            _skipped = Math.Max(0, _frameIndex - written);

            var duration =
                _lastFrameTime > 0f
                    ? _lastFrameTime - _startedAt
                    : Time.realtimeSinceStartup - _startedAt;
            var summary =
                "{\"type\":\"summary\",\"run\":\"" + _runId
                + "\",\"frames\":" + _frameIndex
                + ",\"written\":" + written
                + ",\"skipped\":" + _skipped
                + ",\"bytes\":" + _bytesWritten
                + ",\"durationSec\":" + duration.ToString("0.00", CultureInfo.InvariantCulture)
                + ",\"achievedFps\":"
                + (_frameIndex / Math.Max(0.01f, duration)).ToString("0.0", CultureInfo.InvariantCulture)
                + "}";
            File.WriteAllText(
                Path.Combine(_outputDir, "summary.json"),
                summary);
        }
    }
}
