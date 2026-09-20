using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.InputSensor.PlayMode
{
    /// <summary>
    /// Deterministic generators that imitate real input hardware: the per-frame event
    /// streams a driver would emit, not scripted "move by N" test data.
    ///
    /// Scroll units are post-normalization device units — what Unity 6 reports with
    /// ScrollDeltaBehavior.UniformAcrossAllPlatforms (the Input System default used by
    /// this project): one mouse wheel notch = 1.0, precision touchpad = small
    /// fractional deltas emitted continuously at the display/poll rate.
    /// </summary>
    internal static class SensorStreams
    {
        /// <summary>
        /// Discrete mouse wheel detents. A fast spin lands several notches in
        /// consecutive frames; each notch is one quantized impulse, like a real wheel.
        /// </summary>
        public static List<Vector2> MouseWheelNotches(int notches, int framesPerNotch, float direction = -1f)
        {
            var frames = new List<Vector2>();
            for (var i = 0; i < notches; i++)
            {
                for (var f = 0; f < framesPerNotch; f++)
                    frames.Add(f == 0 ? new Vector2(0f, direction) : Vector2.zero);
            }
            return frames;
        }

        /// <summary>
        /// Precision touchpad two-finger scroll: a smoothstep velocity envelope of
        /// small fractional deltas, then an exponential inertial tail — the driver
        /// keeps reporting decaying deltas after the fingers lift off.
        /// </summary>
        public static List<Vector2> TouchpadGesture(float totalUnits, int activeFrames, int tailFrames, int seed = 0)
        {
            var random = new System.Random(seed);
            var frames = new List<Vector2>();

            for (var i = 0; i < activeFrames; i++)
            {
                var t0 = (float)i / activeFrames;
                var t1 = (float)(i + 1) / activeFrames;
                var slice = SmoothStep(t1) - SmoothStep(t0);
                var jitter = 1f + (float)(random.NextDouble() - 0.5) * 0.3f;
                frames.Add(new Vector2(0f, -totalUnits * slice * jitter)); // drag down = scroll down
            }

            // Inertial tail: residual motion decays ~e^-t like a flicked gesture.
            if (tailFrames > 0)
            {
                var weights = new float[tailFrames];
                var weightSum = 0f;
                for (var i = 0; i < tailFrames; i++)
                {
                    weights[i] = Mathf.Exp(-3f * i / tailFrames);
                    weightSum += weights[i];
                }
                var tailTotal = totalUnits * 0.12f;
                for (var i = 0; i < tailFrames; i++)
                    frames.Add(new Vector2(0f, -tailTotal * weights[i] / weightSum));
            }

            return frames;
        }

        /// <summary>
        /// Absolute pointer positions for a held-button drag: smooth ramp with
        /// sub-pixel sensor jitter, like a ~125-1000Hz mouse coalesced per frame.
        /// </summary>
        public static List<Vector2> DragPositions(Vector2 start, Vector2 totalDelta, int frames, float jitterPixels = 0.4f, int seed = 0)
        {
            var random = new System.Random(seed);
            var positions = new List<Vector2>();
            for (var i = 0; i <= frames; i++)
            {
                var t = SmoothStep((float)i / frames);
                var jitter = new Vector2(
                    (float)(random.NextDouble() - 0.5) * 2f * jitterPixels,
                    (float)(random.NextDouble() - 0.5) * 2f * jitterPixels);
                positions.Add(start + totalDelta * t + jitter);
            }
            return positions;
        }

        /// <summary>
        /// Per-frame deltas matching <see cref="DragPositions"/> — what mouse.delta /
        /// touch.delta report alongside each absolute position.
        /// </summary>
        public static List<Vector2> DeltasFrom(IReadOnlyList<Vector2> positions)
        {
            var deltas = new List<Vector2> { Vector2.zero };
            for (var i = 1; i < positions.Count; i++)
                deltas.Add(positions[i] - positions[i - 1]);
            return deltas;
        }

        private static float SmoothStep(float t) => t * t * (3f - 2f * t);
    }
}
