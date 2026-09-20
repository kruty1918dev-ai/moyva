using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Animations.Runtime.Motion;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.Runtime
{
    /// <summary>
    /// Drives unit world movement as one continuous velocity-profile traversal
    /// (accelerate → cruise → brake) instead of per-tile linear lerps with
    /// dwell between tiles. Facing follows travel direction; bob rides on speed.
    ///
    /// Authority ordering is preserved from the legacy implementation:
    /// <see cref="PathAnimationSettings.CanPerformStep"/> for tile i+1 is
    /// evaluated when the unit reaches tile i, and
    /// <see cref="PathAnimationSettings.OnStepCompleted"/> fires on arrival.
    /// Cancellation snaps the transform back to the last confirmed tile so the
    /// visual never stays on an authoritative-invalid position.
    /// </summary>
    internal sealed class MovementAnimationService : IMovementAnimationService
    {
        public async Task MoveAlongPathAsync(
            Transform target,
            IReadOnlyList<Vector2Int> path,
            PathAnimationSettings settings,
            CancellationToken cancellationToken = default)
        {
            if (target == null || path == null || path.Count <= 1) return;

            // Resolve the whole polyline up front so traversal is continuous
            // through corners — no stop→start per tile.
            var points = new List<Vector3>(path.Count) { target.position };
            for (int i = 1; i < path.Count; i++)
            {
                Vector2Int gridPos = path[i];
                points.Add(settings.ResolveWorldPosition != null
                    ? settings.ResolveWorldPosition(gridPos)
                    : new Vector3(gridPos.x, target.position.y, gridPos.y));
            }

            // Cruise speed derives from the legacy per-tile duration so existing
            // JSON pacing is preserved at cruise while accel/brake shape the feel.
            float totalLength = 0f;
            for (int i = 1; i < points.Count; i++)
                totalLength += Vector3.Distance(points[i - 1], points[i]);
            float averageTileLength = totalLength / (points.Count - 1);
            float cruiseSpeed = averageTileLength / Mathf.Max(0.01f, settings.MoveDurationPerTile);

            var traversal = new PathTraversalMotion(
                points,
                cruiseSpeed,
                settings.Acceleration > 0f ? settings.Acceleration : PathAnimationSettings.Default.Acceleration,
                settings.Deceleration > 0f ? settings.Deceleration : PathAnimationSettings.Default.Deceleration,
                settings.TurnSpeedDegPerSec > 0f
                    ? settings.TurnSpeedDegPerSec
                    : PathAnimationSettings.Default.TurnSpeedDegPerSec,
                settings.FaceTravelDirection,
                settings.BobAmplitude,
                settings.BobFrequency,
                cornerAnticipation: 0.45f,
                initialYawDeg: target.eulerAngles.y,
                canAdvance: settings.CanPerformStep == null
                    ? null
                    : index => settings.CanPerformStep(path[index]),
                onVertexReached: settings.OnStepCompleted == null
                    ? null
                    : index => settings.OnStepCompleted(path[index]));

            try
            {
                while (!traversal.IsComplete)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    traversal.Tick(Time.deltaTime);
                    target.position = traversal.Position;
                    if (settings.FaceTravelDirection)
                        target.rotation = Quaternion.Euler(0f, traversal.YawDegrees, 0f);
                    await Task.Yield();
                }
            }
            catch (OperationCanceledException)
            {
                // Presentation snaps to the last authoritative tile — the visual
                // must not linger mid-segment on a position gameplay never
                // confirmed. Skipped when another lifecycle transition (death)
                // has taken ownership of the transform.
                if (settings.AllowCancelSnap == null || settings.AllowCancelSnap())
                    target.position = points[traversal.ReachedIndex];
                throw;
            }
        }
    }
}
