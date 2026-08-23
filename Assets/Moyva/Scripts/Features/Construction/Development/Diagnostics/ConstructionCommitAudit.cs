using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal static class ConstructionCommitProfiler
    {
        internal static readonly ProfilerMarker ConfirmMarker =
            new ProfilerMarker("Moyva.BuildCommit.Total");
    }

    /// <summary>
    /// Lightweight phase timer for a single construction confirmation.
    /// It is intentionally allocation-light and runs only on confirmation.
    /// </summary>
    internal sealed class ConstructionCommitAudit : IDisposable
    {
        internal const string LogPrefix = "[MOYVA_BUILD_COMMIT]";

        private readonly Dictionary<string, double> _phaseMilliseconds =
            new Dictionary<string, double>(StringComparer.Ordinal);

        private readonly long _startedAt;
        private long _lastTimestamp;
        private readonly int _pendingAtStart;
        private readonly bool _demolishMode;
        private readonly string _stateAtStart;

        private string _lastBuildingId;
        private Vector2Int _lastPosition;
        private bool _hasObservedPlacement;
        private int _confirmedCount = -1;
        private int _skippedCount = -1;
        private int _remainingCount = -1;
        private bool _disposed;

        private ConstructionCommitAudit(
            int pendingAtStart,
            bool demolishMode,
            string stateAtStart)
        {
            _pendingAtStart =
                pendingAtStart;

            _demolishMode =
                demolishMode;

            _stateAtStart =
                string.IsNullOrWhiteSpace(stateAtStart)
                    ? "<unknown>"
                    : stateAtStart;

            _startedAt =
                Stopwatch.GetTimestamp();

            _lastTimestamp =
                _startedAt;
        }

        public static ConstructionCommitAudit Begin(
            int pendingAtStart,
            bool demolishMode,
            string stateAtStart)
        {
            return new ConstructionCommitAudit(
                pendingAtStart,
                demolishMode,
                stateAtStart);
        }

        public void ObservePlacement(
            string buildingId,
            Vector2Int position)
        {
            _lastBuildingId =
                buildingId;

            _lastPosition =
                position;

            _hasObservedPlacement =
                true;
        }

        public void Step(
            string phase)
        {
            if (_disposed)
                return;

            long now =
                Stopwatch.GetTimestamp();

            double milliseconds =
                ToMilliseconds(
                    now - _lastTimestamp);

            _lastTimestamp =
                now;

            string normalized =
                string.IsNullOrWhiteSpace(phase)
                    ? "unnamed"
                    : phase;

            if (_phaseMilliseconds.TryGetValue(
                    normalized,
                    out double current))
            {
                _phaseMilliseconds[normalized] =
                    current + milliseconds;
            }
            else
            {
                _phaseMilliseconds[normalized] =
                    milliseconds;
            }
        }

        public void SetOutcome(
            int confirmedCount,
            int skippedCount,
            int remainingCount)
        {
            _confirmedCount =
                confirmedCount;

            _skippedCount =
                skippedCount;

            _remainingCount =
                remainingCount;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Step("tail");

            _disposed =
                true;

            long completedAt =
                Stopwatch.GetTimestamp();

            double totalMilliseconds =
                ToMilliseconds(
                    completedAt - _startedAt);

            var builder =
                new StringBuilder(512);

            builder.Append(LogPrefix);
            builder.Append(" totalMs=");
            builder.Append(totalMilliseconds.ToString("0.###"));
            builder.Append(" pendingStart=");
            builder.Append(_pendingAtStart);
            builder.Append(" demolish=");
            builder.Append(_demolishMode);
            builder.Append(" stateStart=");
            builder.Append(_stateAtStart);

            if (_hasObservedPlacement)
            {
                builder.Append(" lastBuilding=");
                builder.Append(
                    string.IsNullOrWhiteSpace(_lastBuildingId)
                        ? "<none>"
                        : _lastBuildingId);

                builder.Append(" lastCell=(");
                builder.Append(_lastPosition.x);
                builder.Append(',');
                builder.Append(_lastPosition.y);
                builder.Append(')');
            }

            if (_confirmedCount >= 0)
            {
                builder.Append(" confirmed=");
                builder.Append(_confirmedCount);
                builder.Append(" skipped=");
                builder.Append(_skippedCount);
                builder.Append(" remaining=");
                builder.Append(_remainingCount);
            }

            builder.Append(" phases={");

            bool first =
                true;

            foreach (KeyValuePair<string, double> phase
                     in _phaseMilliseconds)
            {
                if (!first)
                    builder.Append(',');

                builder.Append(phase.Key);
                builder.Append(':');
                builder.Append(
                    phase.Value.ToString("0.###"));

                first =
                    false;
            }

            builder.Append('}');

            UnityEngine.Debug.Log(
                builder.ToString());
        }

        private static double ToMilliseconds(
            long ticks)
        {
            return ticks
                   * 1000.0
                   / Stopwatch.Frequency;
        }
    }
}
