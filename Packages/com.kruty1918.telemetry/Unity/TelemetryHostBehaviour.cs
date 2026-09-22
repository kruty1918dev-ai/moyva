using System;
using UnityEngine;
using Kruty1918.Telemetry.Core;

namespace Kruty1918.Telemetry.Unity
{
    /// <summary>
    /// Unity-side host: owns a <see cref="TelemetryRuntime"/>, emits lifecycle events
    /// (start/pause/resume/quit/fatal-error metadata), drives Tick on a timer, and
    /// flushes on quit. Place one in the boot scene. Gameplay never depends on it —
    /// if absent or disabled, producers just get Disabled results.
    /// </summary>
    [DefaultExecutionOrder(-9000)]
    public sealed class TelemetryHostBehaviour : MonoBehaviour
    {
        public static TelemetryHostBehaviour Instance { get; private set; }
        public static ITelemetrySink Sink => Instance?._runtime?.Sink;
        public static TelemetryRuntime Runtime => Instance?._runtime;

        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private float tickIntervalSeconds = 30f;
        [SerializeField] private bool collectPerfSamples = true;

        private TelemetryRuntime _runtime;
        private float _nextTick;
        private float _perfWindowStart;
        private float _perfMaxFrameMs;
        private int _perfFrames;
        private double _perfSumMs;
        private readonly long _startedTicks = System.Diagnostics.Stopwatch.GetTimestamp();

        /// <summary>Wire an externally composed runtime (DI/test path).</summary>
        public void Initialize(TelemetryRuntime runtime)
        {
            _runtime = runtime;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Application.logMessageReceived += OnLog;
            Emit("started");
        }

        private void Update()
        {
            float frameMs = Time.unscaledDeltaTime * 1000f;
            if (collectPerfSamples)
            {
                _perfFrames++;
                _perfSumMs += frameMs;
                if (frameMs > _perfMaxFrameMs) _perfMaxFrameMs = frameMs;
            }
            if (Time.unscaledTime >= _nextTick)
            {
                _nextTick = Time.unscaledTime + Mathf.Max(1f, tickIntervalSeconds);
                Tick();
            }
        }

        private async void Tick()
        {
            try { if (_runtime != null) await _runtime.Tick(); }
            catch (Exception e) { Debug.unityLogger.LogException(e); }
        }

        private void OnApplicationPause(bool paused) => Emit(paused ? "paused" : "resumed");

        private void OnApplicationQuit()
        {
            Emit("quitting", "clean");
            try { _runtime?.Dispose(); } catch { }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Application.logMessageReceived -= OnLog;
        }

        private void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error) return;
            if (_runtime == null) return;
            var sink = _runtime.Sink;
            var ex = new AppErrorEvent
            {
                ExceptionType = type == LogType.Exception ? "exception" : "error",
                MessageHash = ShortHash(condition ?? ""),
                Condition = Truncate(condition, 200),
                IsFatal = type == LogType.Exception,
                UptimeMs = UptimeMs(),
            };
            sink.Track(in ex);
        }

        private void Emit(string phase, string exitReason = null)
        {
            var sink = _runtime?.Sink;
            if (sink == null) return;
            var e = new AppLifecycleEvent
            {
                Phase = phase,
                UptimeMs = UptimeMs(),
                Platform = Application.platform.ToString(),
                AppVersion = Application.version,
                ExitReason = exitReason,
            };
            sink.Track(in e);
            sink.Flush();
        }

        private long UptimeMs()
            => (long)((System.Diagnostics.Stopwatch.GetTimestamp() - _startedTicks) * 1000.0
                      / System.Diagnostics.Stopwatch.Frequency);

        internal static string ShortHash(string s)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var b = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));
                return BitConverter.ToString(b, 0, 8).Replace("-", "").ToLowerInvariant();
            }
        }

        private static string Truncate(string s, int n)
            => s == null ? null : s.Length <= n ? s : s.Substring(0, n);
    }
}
