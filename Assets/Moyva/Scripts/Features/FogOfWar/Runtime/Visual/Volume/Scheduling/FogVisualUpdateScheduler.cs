using System;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Resolves when pending fog volume visual work should execute.
    /// </summary>
    internal sealed class FogVisualUpdateScheduler : IFogVisualUpdateScheduleState, IFogVisualUpdateTickGate, IFogVisualUpdateRequestPolicy
    {
        private readonly Func<FogVolumeUpdateMode> _resolveUpdateMode;
        private readonly Func<float> _resolveIntervalSeconds;
        private float _nextIntervalRebuildTime;

        public FogVisualUpdateScheduler(Func<FogVolumeUpdateMode> resolveUpdateMode, Func<float> resolveIntervalSeconds)
        {
            _resolveUpdateMode = resolveUpdateMode;
            _resolveIntervalSeconds = resolveIntervalSeconds;
        }

        public FogVolumeUpdateMode CurrentUpdateMode
            => _resolveUpdateMode != null ? _resolveUpdateMode() : FogVolumeUpdateMode.DebouncePerFrame;

        public float CurrentIntervalSeconds
            => _resolveIntervalSeconds != null ? Mathf.Max(0.02f, _resolveIntervalSeconds()) : 0.1f;

        public bool ShouldExecute(FogVolumePendingWorkSnapshot work, out string waitingMessage)
        {
            waitingMessage = null;
            if (!work.HasPendingWork)
                return false;

            if (CurrentUpdateMode != FogVolumeUpdateMode.Interval)
                return true;

            if (Time.unscaledTime < _nextIntervalRebuildTime)
            {
                waitingMessage = $"Tick waiting for interval: now={Time.unscaledTime:0.###}, next={_nextIntervalRebuildTime:0.###}, pending={work.HasPendingWork}, fullRebuild={work.FullRebuildRequested}, dirty={work.DirtyTileCount}.";
                return false;
            }

            _nextIntervalRebuildTime = Time.unscaledTime + CurrentIntervalSeconds;
            return true;
        }

        public bool ShouldExecuteImmediateRequest()
            => CurrentUpdateMode == FogVolumeUpdateMode.Immediate;

        public bool ShouldExecuteFullRebuildRequestImmediately(bool hasBuiltAtLeastOnce, bool worldContextChangedSinceBuild)
            => !hasBuiltAtLeastOnce || worldContextChangedSinceBuild || ShouldExecuteImmediateRequest();
    }
}
