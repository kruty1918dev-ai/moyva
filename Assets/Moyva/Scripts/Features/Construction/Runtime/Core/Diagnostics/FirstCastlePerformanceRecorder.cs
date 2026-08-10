using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Unity.Profiling;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Captures the first castle commit and the following 120 frames in Editor
    /// and Development Players. Reports are intentionally stored outside Assets.
    /// </summary>
    internal sealed class FirstCastlePerformanceRecorder : IInitializable, ITickable, IDisposable
    {
        private const int PostCommitFrameCount = 120;
        private readonly SignalBus _signalBus;
        private readonly IBuildingRegistry _registry;
        private readonly List<CastleFrameSample> _samples = new List<CastleFrameSample>(PostCommitFrameCount);

        private ProfilerRecorder _mainThreadRecorder;
        private ProfilerRecorder _renderThreadRecorder;
        private ProfilerRecorder _gcAllocatedRecorder;
        private ProfilerRecorder _gcCollectRecorder;
        private ProfilerRecorder _commitRecorder;
        private BuildingPlacedSignal _trigger;
        private int _framesRemaining;
        private bool _captured;
        private float _commitMillisecondsAtTrigger;

        public FirstCastlePerformanceRecorder(SignalBus signalBus, IBuildingRegistry registry)
        {
            _signalBus = signalBus;
            _registry = registry;
        }

        public void Initialize()
        {
            _mainThreadRecorder = TryStart(ProfilerCategory.Internal, "Main Thread");
            _renderThreadRecorder = TryStart(ProfilerCategory.Internal, "Render Thread");
            _gcAllocatedRecorder = TryStart(ProfilerCategory.Memory, "GC Allocated In Frame");
            _gcCollectRecorder = TryStart(ProfilerCategory.Memory, "GC.Collect");
            _commitRecorder = TryStart(ProfilerCategory.Scripts, "Moyva.BuildCommit.Total");
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
        }

        public void Tick()
        {
            if (_framesRemaining <= 0)
                return;

            _samples.Add(new CastleFrameSample
            {
                Frame = PostCommitFrameCount - _framesRemaining,
                UnscaledFrameMilliseconds = Time.unscaledDeltaTime * 1000f,
                MainThreadMilliseconds = ToMilliseconds(_mainThreadRecorder),
                RenderThreadMilliseconds = ToMilliseconds(_renderThreadRecorder),
                BuildCommitMilliseconds = ToMilliseconds(_commitRecorder),
                GcAllocatedBytes = ReadLastValue(_gcAllocatedRecorder),
                GcCollectionMilliseconds = ToMilliseconds(_gcCollectRecorder),
            });

            _framesRemaining--;
            if (_framesRemaining == 0)
                WriteReport();
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _mainThreadRecorder.Dispose();
            _renderThreadRecorder.Dispose();
            _gcAllocatedRecorder.Dispose();
            _gcCollectRecorder.Dispose();
            _commitRecorder.Dispose();
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            if (_captured)
                return;

            BuildingDefinition definition = _registry.GetById(signal.BuildingId);
            if (!BuildingDefinitionCapabilities.IsCastle(definition))
                return;

            _captured = true;
            _trigger = signal;
            _commitMillisecondsAtTrigger = ToMilliseconds(_commitRecorder);
            _samples.Clear();
            _framesRemaining = PostCommitFrameCount;
        }

        private void WriteReport()
        {
            var report = new FirstCastlePerformanceReport
            {
                Schema = "moyva.first-castle-profile",
                Version = 2,
                TimestampUtc = DateTime.UtcNow.ToString("O"),
                UnityVersion = Application.unityVersion,
                BuildingId = _trigger.BuildingId,
                PositionX = _trigger.Position.x,
                PositionY = _trigger.Position.y,
                ResolutionWidth = Screen.width,
                ResolutionHeight = Screen.height,
                BuildCommitMaxMilliseconds = _commitMillisecondsAtTrigger,
                Frames = _samples,
            };
            report.CalculateSummary();

            string directory = Path.Combine(Application.persistentDataPath, "MoyvaDiagnostics", "FirstCastle");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, $"castle-profile-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log($"[MoyvaCastleProfile] COMPLETE accepted={report.Accepted}, frames={report.Frames.Count}, commitMaxMs={report.BuildCommitMaxMilliseconds:0.###}, p95MainMs={report.MainThreadP95Milliseconds:0.###}, maxMainMs={report.MainThreadMaxMilliseconds:0.###}, gcCollections={report.GcCollectionFrames}, path={path}");
        }

        private static ProfilerRecorder TryStart(ProfilerCategory category, string marker)
        {
            try
            {
                return ProfilerRecorder.StartNew(category, marker, 1);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[MoyvaCastleProfile] Profiler counter '{marker}' is unavailable: {exception.Message}");
                return default;
            }
        }

        private static long ReadLastValue(ProfilerRecorder recorder)
            => recorder.Valid ? recorder.LastValue : 0L;

        private static float ToMilliseconds(ProfilerRecorder recorder)
            => recorder.Valid ? recorder.LastValue * 0.000001f : 0f;
    }

    [Serializable]
    internal sealed class FirstCastlePerformanceReport
    {
        public string Schema;
        public int Version;
        public string TimestampUtc;
        public string UnityVersion;
        public string BuildingId;
        public int PositionX;
        public int PositionY;
        public int ResolutionWidth;
        public int ResolutionHeight;
        public float MainThreadP95Milliseconds;
        public float MainThreadMaxMilliseconds;
        public float RenderThreadP95Milliseconds;
        public float BuildCommitMaxMilliseconds;
        public long TotalGcAllocatedBytes;
        public int GcCollectionFrames;
        public bool Accepted;
        public List<string> AcceptanceViolations;
        public List<CastleFrameSample> Frames;

        public void CalculateSummary()
        {
            if (Frames == null || Frames.Count == 0)
                return;

            var mainThreadValues = new List<float>(Frames.Count);
            var renderThreadValues = new List<float>(Frames.Count);
            for (int index = 0; index < Frames.Count; index++)
            {
                float value = Frames[index].MainThreadMilliseconds > 0f
                    ? Frames[index].MainThreadMilliseconds
                    : Frames[index].UnscaledFrameMilliseconds;
                mainThreadValues.Add(value);
                MainThreadMaxMilliseconds = Mathf.Max(MainThreadMaxMilliseconds, value);
                if (Frames[index].RenderThreadMilliseconds > 0f)
                    renderThreadValues.Add(Frames[index].RenderThreadMilliseconds);
                BuildCommitMaxMilliseconds = Mathf.Max(
                    BuildCommitMaxMilliseconds,
                    Frames[index].BuildCommitMilliseconds);
                TotalGcAllocatedBytes += Math.Max(0L, Frames[index].GcAllocatedBytes);
                if (Frames[index].GcCollectionMilliseconds > 0.0001f)
                    GcCollectionFrames++;
            }

            MainThreadP95Milliseconds = CalculateP95(mainThreadValues);
            RenderThreadP95Milliseconds = CalculateP95(renderThreadValues);
            CalculateAcceptance();
        }

        private void CalculateAcceptance()
        {
            AcceptanceViolations = new List<string>();
            if (Frames == null || Frames.Count != 120)
                AcceptanceViolations.Add($"Expected 120 post-commit frames, got {Frames?.Count ?? 0}.");
            if (BuildCommitMaxMilliseconds > 16.6f)
                AcceptanceViolations.Add($"BuildCommit max {BuildCommitMaxMilliseconds:0.###} ms exceeds 16.6 ms.");
            if (MainThreadP95Milliseconds > 16.6f)
                AcceptanceViolations.Add($"Main Thread p95 {MainThreadP95Milliseconds:0.###} ms exceeds 16.6 ms.");
            if (MainThreadMaxMilliseconds > 33.3f)
                AcceptanceViolations.Add($"Main Thread max {MainThreadMaxMilliseconds:0.###} ms exceeds 33.3 ms.");
            if (GcCollectionFrames > 0)
                AcceptanceViolations.Add($"Managed GC collection was observed in {GcCollectionFrames} frame(s).");

            Accepted = AcceptanceViolations.Count == 0;
        }

        private static float CalculateP95(List<float> values)
        {
            if (values == null || values.Count == 0)
                return 0f;

            values.Sort();
            int p95Index = Mathf.Clamp(
                Mathf.CeilToInt(values.Count * 0.95f) - 1,
                0,
                values.Count - 1);
            return values[p95Index];
        }
    }

    [Serializable]
    internal struct CastleFrameSample
    {
        public int Frame;
        public float UnscaledFrameMilliseconds;
        public float MainThreadMilliseconds;
        public float RenderThreadMilliseconds;
        public float BuildCommitMilliseconds;
        public long GcAllocatedBytes;
        public float GcCollectionMilliseconds;
    }
}
