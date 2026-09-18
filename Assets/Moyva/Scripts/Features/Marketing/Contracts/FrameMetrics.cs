using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Marketing.Contracts
{
    /// <summary>Measurements taken from one candidate frame. Produced by the
    /// runtime sampler, scored by GoldenFrameEvaluator (pure logic).</summary>
    [Serializable]
    public struct FrameMetrics
    {
        /// <summary>Fraction of frame area occupied by the primary subject (0..1).</summary>
        public float subjectScreenArea;
        /// <summary>Distance of subject center from ideal composition point (0..1,
        /// normalized by half-diagonal).</summary>
        public float subjectCenterOffset;
        /// <summary>Fraction of subject bounds clipped by frame edges (0..1).</summary>
        public float subjectClippedFraction;
        /// <summary>Fraction of frame occluded by foreground blockers (0..1).</summary>
        public float occlusionFraction;
        /// <summary>Fraction of pixels that are magenta (shader error).</summary>
        public float magentaFraction;
        /// <summary>Fraction of pixels near-black.</summary>
        public float blackFraction;
        /// <summary>Fraction of pixels near-white.</summary>
        public float whiteFraction;
        /// <summary>Mean luminance 0..1.</summary>
        public float luminanceMean;
        /// <summary>Luminance variance 0..1 (flat frames score low).</summary>
        public float luminanceVariance;
        /// <summary>Normalized screen-space overlap between hero subjects (0..1).</summary>
        public float subjectOverlapFraction;
        /// <summary>True when the subject sits inside the safe area.</summary>
        public bool subjectInsideSafeArea;
        /// <summary>True when the animation state is mid-transition glitch.</summary>
        public bool animationGlitch;
    }

    [Serializable]
    public sealed class FrameScore
    {
        public float score;                    // 0..1
        public List<string> rejectReasons = new List<string>();
        public bool Rejected => rejectReasons.Count > 0;
    }
}
