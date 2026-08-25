using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    /// <summary>
    /// Temporary diagnostic-only tracing for unit selection/grid/movement.
    /// Intentionally aggregates timings/counters instead of logging per tile.
    /// Remove after the movement freeze/input issue is diagnosed.
    /// </summary>
    public static class UnitMovementDiagnostics
    {
        public const string Prefix = "[MOYVA_UNIT_DIAG]";

        private static readonly object Gate = new();
        private static readonly Dictionary<string, long> UnitTraceIds =
            new(StringComparer.Ordinal);

        private static long _nextTraceId;
        private static long _currentTraceId;

        public static long CurrentTraceId
            => Interlocked.Read(ref _currentTraceId);

        public static long BeginInput(
            string button,
            Vector2Int position,
            string selectedUnitId,
            string mode)
        {
            long trace = Interlocked.Increment(ref _nextTraceId);
            Interlocked.Exchange(ref _currentTraceId, trace);

            Log(
                trace,
                "INPUT",
                $"button={button}; pos={position}; " +
                $"selected={Safe(selectedUnitId)}; mode={Safe(mode)}");
            return trace;
        }

        public static void AssociateUnit(string unitId, long traceId = 0)
        {
            string normalized = Normalize(unitId);
            if (string.IsNullOrEmpty(normalized))
                return;

            long trace = traceId > 0 ? traceId : CurrentTraceId;
            if (trace <= 0)
                return;

            lock (Gate)
                UnitTraceIds[normalized] = trace;
        }

        public static long TraceForUnit(string unitId)
        {
            string normalized = Normalize(unitId);
            if (!string.IsNullOrEmpty(normalized))
            {
                lock (Gate)
                {
                    if (UnitTraceIds.TryGetValue(normalized, out long trace))
                        return trace;
                }
            }

            return CurrentTraceId;
        }

        public static void Log(
            long traceId,
            string stage,
            string message)
        {
        }

        public static void Warn(
            long traceId,
            string stage,
            string message)
        {
        }

        public static void Error(
            long traceId,
            string stage,
            string message)
        {
            Debug.LogError(Format(traceId, stage, message));
        }

        public static string FormatPath(
            IReadOnlyList<Vector2Int> path,
            int maxPoints = 18)
        {
            if (path == null)
                return "<null>";
            if (path.Count == 0)
                return "[]";

            int count = Mathf.Min(path.Count, Mathf.Max(1, maxPoints));
            var builder = new StringBuilder(96);
            builder.Append('[');

            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    builder.Append(" -> ");
                builder.Append(path[i]);
            }

            if (path.Count > count)
            {
                builder.Append(" -> ... +");
                builder.Append(path.Count - count);
            }

            builder.Append(']');
            return builder.ToString();
        }

        public static double NowMs()
            => Time.realtimeSinceStartupAsDouble * 1000.0;

        public static string Ms(double value)
            => value.ToString("F2");

        public static string Safe(string value)
            => string.IsNullOrWhiteSpace(value)
                ? "<empty>"
                : value.Trim();

        private static string Format(
            long traceId,
            string stage,
            string message)
        {
            return
                $"{Prefix}[trace={traceId}]" +
                $"[t={Time.realtimeSinceStartupAsDouble:F3}s]" +
                $"[{stage}] {message}";
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
    }
}
