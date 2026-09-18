using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Planning
{
    /// <summary>
    /// Scores candidate frames for screenshot/still export. Rejects dead or
    /// broken frames outright; ranks acceptable frames on composition,
    /// readability and lighting. Pure logic — fully unit-testable.
    /// </summary>
    public static class GoldenFrameEvaluator
    {
        public sealed class Thresholds
        {
            public float maxMagentaFraction = 0.001f;
            public float maxBlackFraction = 0.85f;
            public float maxWhiteFraction = 0.85f;
            public float maxOcclusion = 0.55f;
            public float minSubjectArea = 0.01f;
            public float maxSubjectClipped = 0.45f;
            public float minLuminanceVariance = 0.004f;
        }

        public static readonly Thresholds Defaults = new Thresholds();

        public static FrameScore Evaluate(in FrameMetrics m, Thresholds t = null)
        {
            t ??= Defaults;
            var result = new FrameScore();

            if (m.magentaFraction > t.maxMagentaFraction) result.rejectReasons.Add("magenta-pixels");
            if (m.blackFraction > t.maxBlackFraction) result.rejectReasons.Add("black-frame");
            if (m.whiteFraction > t.maxWhiteFraction) result.rejectReasons.Add("white-frame");
            if (m.occlusionFraction > t.maxOcclusion) result.rejectReasons.Add("occluded");
            if (m.subjectScreenArea > 0f && m.subjectScreenArea < t.minSubjectArea)
                result.rejectReasons.Add("subject-too-small");
            if (m.subjectClippedFraction > t.maxSubjectClipped)
                result.rejectReasons.Add("subject-clipped");
            if (m.luminanceVariance < t.minLuminanceVariance)
                result.rejectReasons.Add("flat-frame");
            if (m.animationGlitch) result.rejectReasons.Add("animation-glitch");
            if (!m.subjectInsideSafeArea && m.subjectScreenArea > 0f)
                result.rejectReasons.Add("subject-outside-safe-area");

            if (result.Rejected)
            {
                result.score = 0f;
                return result;
            }

            float composition = 1f - Mathf.Clamp01(m.subjectCenterOffset * 1.6f);
            float presence = m.subjectScreenArea <= 0f
                ? 0.6f // pure landscape shot — presence neutral
                : Mathf.Clamp01(Mathf.InverseLerp(0.02f, 0.35f, m.subjectScreenArea));
            float lighting = Mathf.Clamp01(1f - Mathf.Abs(m.luminanceMean - 0.45f) * 1.4f)
                * Mathf.Clamp01(m.luminanceVariance * 14f);
            float overlap = 1f - Mathf.Clamp01(m.subjectOverlapFraction * 1.8f);
            float clean = 1f - Mathf.Clamp01(m.occlusionFraction * 1.2f);

            result.score = Mathf.Clamp01(
                composition * 0.30f +
                presence * 0.22f +
                lighting * 0.22f +
                overlap * 0.14f +
                clean * 0.12f);
            return result;
        }

        /// <summary>Picks the index of the best non-rejected frame. Returns -1
        /// when every candidate fails.</summary>
        public static int PickBest(IReadOnlyList<FrameMetrics> frames, Thresholds t = null)
        {
            int best = -1;
            float bestScore = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                FrameScore s = Evaluate(frames[i], t);
                if (s.Rejected || s.score <= bestScore)
                    continue;
                bestScore = s.score;
                best = i;
            }
            return best;
        }
    }
}
